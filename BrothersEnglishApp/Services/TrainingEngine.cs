// TrainingEngine.cs
using BrothersEnglishApp.Models;

namespace BrothersEnglishApp.Services;

public class TrainingResult
{
    public bool IsCorrect { get; set; }
    public bool ShouldGoToStep2 { get; set; }
    public bool ShouldStartReview { get; set; }
    public bool IsFinished { get; set; }
}

public class Question
{
    public string WordId { get; set; } = "";
    public string QuestionText { get; set; } = "";
    public string CorrectAnswer { get; set; } = "";
    public List<string> Options { get; set; } = [];
    public bool IsEnglishToJapanese { get; set; }
}

public class TrainingEngine
{
    // 前回の単語IDを一時的に保持する変数
    private string? _lastWordId;
    // Studyセッション中に出題したIDとその回数を記録する
    private Dictionary<string, int> _sessionWordCounts = new();

    // セッション開始時（または終了時）に履歴をリセットするメソッド
    public void ResetStudySession()
    {
        _sessionWordCounts.Clear();
        _lastWordId = null;
    }

    // --- Training用ロジック ---
    public EnglishWord? GetNextWord(List<EnglishWord> allWords, UserProgress userProgress, int dailyGoal)
    {
        var today = DateTime.Now.Date;
        var todayCount = userProgress.WordStatuses.Count(s =>
            s.LastReviewed.Date == today && s.Status >= 1);

        if (todayCount >= dailyGoal) return null;

        var candidates = allWords.Where(w =>
            !userProgress.WordStatuses.Any(s =>
                s.WordId == w.Id && s.LastReviewed.Date == today));

        return candidates
            .OrderByDescending(w => w.Level)
            .ThenBy(_ => Guid.NewGuid())
            .FirstOrDefault();
    }

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
                result.ShouldGoToStep2 = true;
            }
            else
            {
                UpdateWordStatus(userProgress, currentWord.Id, true);
                result.IsFinished = true;
            }
        }
        else
        {
            if (isStep2 && !string.IsNullOrEmpty(userInput))
            {
                UpdateWordStatus(userProgress, currentWord.Id, false);
                result.ShouldStartReview = true;
            }
        }
        return result;
    }

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
            status.Status = 1;
            status.CorrectCount++;
            status.LastReviewed = DateTime.UtcNow;
        }
        else
        {
            status.IncorrectCount++;
        }
    }

    // --- Study用ロジック ---
    public Question GenerateStudyQuestion(List<EnglishWord> allWords, UserProgress? userProgress)
    {
        var todayUtc = DateTime.UtcNow.Date;
        EnglishWord? target = null;

        // --- ① 今日の学習単語（未出題分を優先） ---
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
        // --- ② ミスが多い単語（まだ今日のセッションで出してないものを優先） ---
        else
        {
            var weakWords = allWords
                .Where(w => userProgress?.WordStatuses.Any(s => s.WordId == w.Id && s.IncorrectCount > 0) ?? false)
                .OrderByDescending(w => userProgress?.WordStatuses.First(s => s.WordId == w.Id).IncorrectCount)
                .ToList();

            // 未出題の苦手単語があるかチェック
            var freshWeakWords = weakWords.Where(w => !_sessionWordCounts.ContainsKey(w.Id)).ToList();

            if (freshWeakWords.Any())
            {
                target = freshWeakWords.First();
            }
            else
            {
                // --- ③ 【新設】エンドレス・ランダムモード ---
                // 学習済みの全単語から、直前と違うものをランダムに選ぶ
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

        // 万が一ターゲットが見つからない場合のフォールバック
        if (target == null) target = allWords[Random.Shared.Next(allWords.Count)];

        // セッション履歴に記録
        _lastWordId = target.Id;
        if (!_sessionWordCounts.ContainsKey(target.Id)) _sessionWordCounts[target.Id] = 0;
        _sessionWordCounts[target.Id]++;

        // --- 選択肢の作成ロジック（ここは変更なし） ---
        bool isEngToJap = Random.Shared.Next(2) == 0;
        var options = new List<string> { isEngToJap ? target.Meaning : target.Word };
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
    // 学習結果の記録メソッド
    public void RecordStudyResult(UserProgress progress, string wordId, bool isCorrect)
    {
        var status = progress.WordStatuses.FirstOrDefault(s => s.WordId == wordId);

        // もし万が一、学習履歴にない単語を復習した場合は新しく作る
        if (status == null)
        {
            status = new WordStatus { WordId = wordId, Status = 1 }; // 復習に出てる時点で既知扱い
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
}