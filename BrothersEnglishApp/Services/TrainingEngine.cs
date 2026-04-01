using BrothersEnglishApp.Models;

namespace BrothersEnglishApp.Services;

/// <summary>
/// トレーニングの結果を保持するクラス
/// </summary>
public class TrainingResult
{
    public bool IsCorrect { get; set; }
    public bool ShouldGoToStep2 { get; set; } // お手本入力後の「見ないで入力」へ進むか
    public bool ShouldStartReview { get; set; } // ミス時に復習（お手本確認）へ戻るか
    public bool IsFinished { get; set; } // その単語の学習が完了したか
}

/// <summary>
/// クイズ（Study用）の出題データを保持するクラス
/// </summary>
public class Question
{
    public string WordId { get; set; } = "";
    public string QuestionText { get; set; } = "";
    public string CorrectAnswer { get; set; } = "";
    public List<string> Options { get; set; } = new List<string>();
    public bool IsEnglishToJapanese { get; set; }
}

/// <summary>
/// 学習ロジックの中核を担うエンジン
/// </summary>
public class TrainingEngine
{
    private string? _lastWordId; // 直前の出題と同じ単語が連続しないように保持
    private Dictionary<string, int> _sessionWordCounts = new(); // セッション内での出題回数管理

    /// <summary>
    /// Studyセッションの履歴をリセット
    /// </summary>
    public void ResetStudySession()
    {
        _sessionWordCounts.Clear();
        _lastWordId = null;
    }

    // --- Training（新規学習）用ロジック ---

    /// <summary>
    /// 次に学習すべき単語を取得する
    /// </summary>
    public EnglishWord? GetNextWord(List<EnglishWord> allWords, UserProgress userProgress, int dailyGoal)
    {
        // 重要：サーバー・クライアント間の時差による進捗の不整合を防ぐため、
        // 判定はすべて UTC(世界標準時) の日付で統一する。
        var todayUtc = DateTime.UtcNow.Date;

        // 今日の学習完了数をカウント（今日ステータスが1以上になったもの）
        var todayCount = userProgress.WordStatuses.Count(s =>
            s.LastReviewed.Date == todayUtc && s.Status >= 1);

        if (todayCount >= dailyGoal) return null;

        // まだ今日一度も触れていない単語を候補にする
        var candidates = allWords.Where(w =>
            !userProgress.WordStatuses.Any(s =>
                s.WordId == w.Id && s.LastReviewed.Date == todayUtc));

        return candidates
            .OrderByDescending(w => w.Level) // レベルが高い（難しい）ものから
            .ThenBy(_ => Guid.NewGuid())     // 同レベル内ではランダム
            .FirstOrDefault();
    }

    /// <summary>
    /// 入力された回答を検証し、次のアクションを決定する
    /// </summary>
    public TrainingResult ProcessAnswer(string userInput, EnglishWord currentWord, UserProgress userProgress, bool isStep2)
    {
        var result = new TrainingResult();
        bool isCorrect = string.Equals(
            userInput.Trim(),
            currentWord.Word,
            StringComparison.OrdinalIgnoreCase);

        result.IsCorrect = isCorrect;

        if (isCorrect)
        {
            if (!isStep2)
            {
                // Step1（お手本あり）正解なら Step2（見ないで入力）へ
                result.ShouldGoToStep2 = true;
            }
            else
            {
                // Step2 正解でその単語の学習ステータスを更新
                UpdateWordStatus(userProgress, currentWord.Id, true);
                result.IsFinished = true;
            }
        }
        else
        {
            // Step2（見ないで入力）で間違えた場合は、強制復習モードへ
            if (isStep2 && !string.IsNullOrEmpty(userInput))
            {
                UpdateWordStatus(userProgress, currentWord.Id, false);
                result.ShouldStartReview = true;
            }
        }
        return result;
    }

    /// <summary>
    /// 単語の学習ステータス（習熟度や履歴）を更新する
    /// </summary>
    private void UpdateWordStatus(UserProgress progress, string wordId, bool isCorrect)
    {
        var status = progress.WordStatuses.FirstOrDefault(s => s.WordId == wordId);
        if (status == null)
        {
            status = new WordStatus { WordId = wordId };
            progress.WordStatuses.Add(status);
        }

        if (isCorrect)
        {
            status.Status = 1; // 1: 学習済み
            status.CorrectCount++;
            // 保存は UTC で行い、判定ロジックと整合性を保つ
            status.LastReviewed = DateTime.UtcNow;
        }
        else
        {
            status.IncorrectCount++;
        }
    }

    // --- Study（復習クイズ）用ロジック ---

    /// <summary>
    /// 復習用のクイズを生成する
    /// </summary>
    public Question GenerateStudyQuestion(List<EnglishWord> allWords, UserProgress? userProgress)
    {
        var todayUtc = DateTime.UtcNow.Date;
        EnglishWord? target = null;

        // ① 優先：今日新しく覚えた単語（未出題分）
        var todayLearned = allWords.Where(w =>
            userProgress?.WordStatuses.Any(s =>
                s.WordId == w.Id &&
                s.LastReviewed.Date == todayUtc &&
                s.Status >= 1) ?? false
        ).ToList();

        var freshWords = todayLearned
            .Where(w => !_sessionWordCounts.ContainsKey(w.Id))
            .ToList();

        if (freshWords.Any())
        {
            target = freshWords[Random.Shared.Next(freshWords.Count)];
        }
        // ② 次点：過去に間違えたことがある苦手単語
        else
        {
            var weakWords = allWords
        .Where(w => userProgress?.WordStatuses.Any(s => s.WordId == w.Id && s.IncorrectCount > 0) ?? false)
        .OrderByDescending(w => userProgress?.WordStatuses.FirstOrDefault(s => s.WordId == w.Id)?.IncorrectCount ?? 0)
        .ToList();

            var freshWeakWords = weakWords.Where(w => !_sessionWordCounts.ContainsKey(w.Id)).ToList();

            if (freshWeakWords.Any())
            {
                target = freshWeakWords.First();
            }
            else
            {
                // ③ 最終：学習済みプールからランダム（無限モード）
                var masteredPool = allWords.Where(w =>
                    userProgress?.WordStatuses.Any(s => s.WordId == w.Id && s.Status >= 1) ?? false
                ).ToList();

                if (masteredPool.Any())
                {
                    var finalPool = (masteredPool.Count > 1 && _lastWordId != null)
                        ? masteredPool.Where(w => w.Id != _lastWordId).ToList()
                        : masteredPool;
                    target = finalPool[Random.Shared.Next(finalPool.Count)];
                }
            }
        }

        // フォールバック（未学習含む完全ランダム）
        if (target == null) target = allWords[Random.Shared.Next(allWords.Count)];

        _lastWordId = target.Id;
        if (!_sessionWordCounts.ContainsKey(target.Id)) _sessionWordCounts[target.Id] = 0;
        _sessionWordCounts[target.Id]++;

        // 出題形式（英→日 / 日→英）をランダム決定
        bool isEngToJap = Random.Shared.Next(2) == 0;
        var options = new List<string> { isEngToJap ? target.Meaning : target.Word };

        // 不正解の選択肢を3つ作成
        var wrongCandidates = allWords.Where(w => w.Id != target.Id).OrderBy(_ => Guid.NewGuid()).Take(3);
        foreach (var w in wrongCandidates)
        {
            options.Add(isEngToJap ? w.Meaning : w.Word);
        }

        return new Question
        {
            WordId = target.Id,
            QuestionText = isEngToJap ? target.Word : target.Meaning,
            CorrectAnswer = isEngToJap ? target.Meaning : target.Word,
            Options = options.OrderBy(_ => Guid.NewGuid()).ToList(),
            IsEnglishToJapanese = isEngToJap
        };
    }

    /// <summary>
    /// Studyセッションでの回答結果を記録する
    /// </summary>
    public void RecordStudyResult(UserProgress progress, string wordId, bool isCorrect)
    {
        var status = progress.WordStatuses.FirstOrDefault(s => s.WordId == wordId);

        if (status == null)
        {
            status = new WordStatus { WordId = wordId, Status = 1 };
            progress.WordStatuses.Add(status);
        }

        status.LastReviewed = DateTime.UtcNow;

        if (isCorrect)
        {
            status.CorrectCount++;
        }
        else
        {
            status.IncorrectCount++;
        }
    }

    // --- Sentence Training（文章入力）用ロジック ---
    public List<string> GenerateSentenceChips(string target, List<EnglishWord> allWords)
    {
        var chips = new List<string> { target };
        var random = new Random();

        // 1. 固定の「機能語」リスト
        var prepositions = new[] { "in", "on", "at", "for", "with", "to", "by", "of", "from", "up", "about" };
        var auxiliaries = new[] { "can", "will", "shall", "may", "must", "do", "does", "did", "is", "am", "are", "was", "were" };

        // 前置詞・助動詞判定
        if (prepositions.Contains(target.ToLower()))
        {
            var dummies = prepositions.Where(x => x != target.ToLower())
                                      .OrderBy(_ => random.Next()).Take(2);
            chips.AddRange(dummies);
        }
        else if (auxiliaries.Contains(target.ToLower()))
        {
            var dummies = auxiliaries.Where(x => x != target.ToLower())
                                     .OrderBy(_ => random.Next()).Take(2);
            chips.AddRange(dummies);
        }
        else
        {
            // 2. master_words.json (allWords) から品詞一致で探す
            var targetWordData = allWords.FirstOrDefault(w => w.Word.Equals(target, StringComparison.OrdinalIgnoreCase));

            IEnumerable<string> dummies;
            if (targetWordData != null)
            {
                // 同じ品詞（PartOfSpeech）を含む単語を抽出
                dummies = allWords.Where(w => w.Word != target && w.PartOfSpeech.Contains(targetWordData.PartOfSpeech.Trim()))
                                  .OrderBy(_ => random.Next()).Take(2)
                                  .Select(w => w.Word);
            }
            else
            {
                // 辞書にない場合は完全ランダム
                dummies = allWords.OrderBy(_ => random.Next()).Take(2).Select(w => w.Word);
            }
            chips.AddRange(dummies);
        }

        // 最後にシャッフルして返す
        return chips.OrderBy(_ => random.Next()).ToList();
    }

    public SentenceQuestion GenerateSentenceTraining(SentenceItem sentence, List<EnglishWord> allWords)
    {
        // 1. チップを生成（さっきのロジックを使用）
        var chips = GenerateSentenceChips(sentence.Target, allWords);

        return new SentenceQuestion
        {
            Sentence = sentence,
            Chips = chips,
            CorrectAnswer = sentence.Target
        };
    }

    // --- 文を分割するロジック ---
    public List<SentenceSegment> SplitSentence(string english, string target)
    {
        var segments = new List<SentenceSegment>();
        // ターゲット単語の場所で分割する（簡易版）
        var parts = english.Split(new[] { target }, StringSplitOptions.None);

        for (int i = 0; i < parts.Length; i++)
        {
            if (!string.IsNullOrEmpty(parts[i]))
            {
                segments.Add(new SentenceSegment { Text = parts[i], IsTarget = false });
            }

            // 最後のパーツ以外、またはパーツが1つでターゲットがある場合
            if (i < parts.Length - 1)
            {
                segments.Add(new SentenceSegment { Text = target, IsTarget = true });
            }
        }
        return segments;
    }

    // --- 次の問題を選ぶロジック（とりあえずランダム版） ---
    public SentenceItem? GetNextSentenceToTrain(UserProgress? progress)
    {
        if (progress == null || progress.SentenceStatuses.Count == 0)
        {
            return new SentenceItem { English = "I go to the park.", Target = "go", Japanese = "私は公園に行きます。" };
        }

        // KeyValuePair の ?.Value を使うのではなく、一回変数に受けてから中身を取り出すぜ
        var entry = progress.SentenceStatuses.OrderBy(_ => Guid.NewGuid()).FirstOrDefault();

        // Dictionary の要素を FirstOrDefault すると KeyValuePair が返るから、
        // その中の Value (SentenceStatus) から Sentence を取り出すんだ。
        return entry.Value?.Sentence;
    }
}