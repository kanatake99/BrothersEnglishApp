using BrothersEnglishApp.Models;

namespace BrothersEnglishApp.Services;

// ============================================================
// データクラス：Training / Study の各モードが
// 処理結果や問題データを呼び出し元へ返すために使用する。
// ============================================================

/// <summary>
/// Training（新規学習）モードでの1回分の回答処理結果を保持する。
/// ProcessAnswer() の戻り値として使用し、次にどの画面へ遷移するかを示す。
/// </summary>
public class TrainingResult
{
    /// <summary>回答が正解だったか</summary>
    public bool IsCorrect { get; set; }

    /// <summary>
    /// Step1（お手本あり入力）が正解だったとき true になる。
    /// UI はこのフラグを見て Step2（見ないで入力）画面へ遷移する。
    /// </summary>
    public bool ShouldGoToStep2 { get; set; }

    /// <summary>
    /// Step2 で不正解だったとき true になる。
    /// UI はこのフラグを見てお手本確認（復習）画面へ戻る。
    /// </summary>
    public bool ShouldStartReview { get; set; }

    /// <summary>
    /// Step2 が正解でその単語の学習が完了したとき true になる。
    /// UI はこのフラグを見て次の単語へ進む。
    /// </summary>
    public bool IsFinished { get; set; }
}

/// <summary>
/// Study（復習クイズ）モードでの1問分のデータを保持する。
/// GenerateStudyQuestion() の戻り値として使用する。
/// </summary>
public class Question
{
    /// <summary>出題対象の単語 ID（マスターデータの EnglishWord.Id と対応）</summary>
    public string WordId { get; set; } = "";

    /// <summary>画面に表示する問題文（英語 or 日本語）</summary>
    public string QuestionText { get; set; } = "";

    /// <summary>正解の文字列</summary>
    public string CorrectAnswer { get; set; } = "";

    /// <summary>4択の選択肢リスト（正解1つ＋不正解3つをシャッフル済み）</summary>
    public List<string> Options { get; set; } = new List<string>();

    /// <summary>true = 英語→日本語問題、false = 日本語→英語問題</summary>
    public bool IsEnglishToJapanese { get; set; }
}

// ============================================================
// TrainingEngine：学習ロジックの中核クラス
//
// 3つのモードを担当する：
//   1. Training  — 新規単語を Step1(お手本あり) → Step2(見ないで) の流れで学習
//   2. Study     — 学習済み単語を4択クイズで復習
//   3. Sentence  — 文章の中の空欄単語をチップ選択で入力
//
// ※ インスタンスはセッション単位で管理すること。
//    _sessionWordCounts と _lastWordId はセッション状態を保持する。
// ============================================================
public class TrainingEngine
{
    // ---- セッション状態フィールド ----

    /// <summary>
    /// Study モードで直前に出題した単語 ID。
    /// 同じ単語が連続して出題されないよう除外フィルタに使う。
    /// </summary>
    private string? _lastWordId;

    /// <summary>
    /// Study モードでのセッション内出題回数。
    /// Key = WordId、Value = 出題回数。
    /// "まだ今セッションで出題していない単語" を優先するために使用。
    /// </summary>
    private Dictionary<string, int> _sessionWordCounts = new();

    // ============================================================
    // Study セッション管理
    // ============================================================

    /// <summary>
    /// Study セッションの状態をリセットする。
    /// 画面を離れたり「もう一度」ボタンを押したりしたときに呼び出す。
    /// </summary>
    public void ResetStudySession()
    {
        _sessionWordCounts.Clear();
        _lastWordId = null;
    }

    // ============================================================
    // Training モード（新規学習）
    // ============================================================

    /// <summary>
    /// 今日学習すべき次の単語を1件返す。
    /// 日次目標 <paramref name="dailyGoal"/> に達していれば null を返す。
    /// </summary>
    /// <remarks>
    /// 選定基準：
    ///   - 今日（UTC）まだ1度も触れていない単語を候補にする。
    ///   - 候補の中で Level が高い（難しい）ものを優先し、
    ///     同レベル内はランダムに選ぶ。
    ///
    /// ⚠ 日付判定はすべて UTC で統一している。
    ///    サーバー・クライアント間の時差による不整合を防ぐため、
    ///    ローカル時刻（DateTime.Now）は使わないこと。
    /// </remarks>
    public EnglishWord? GetNextWord(List<EnglishWord> allWords, UserProgress userProgress, int dailyGoal)
    {
        var todayUtc = DateTime.UtcNow.Date;

        // 今日すでに学習ステータスが 1 以上になった単語数を日次カウントとする
        var todayCount = userProgress.WordStatuses.Count(s =>
            s.LastReviewed.Date == todayUtc && s.Status >= 1);

        if (todayCount >= dailyGoal) return null;

        // 今日まだ出題していない単語を候補にし、難易度降順→ランダムで選ぶ
        var candidates = allWords.Where(w =>
            !userProgress.WordStatuses.Any(s =>
                s.WordId == w.Id && s.LastReviewed.Date == todayUtc));

        return candidates
            .OrderByDescending(w => w.Level)
            .ThenBy(_ => Guid.NewGuid())
            .FirstOrDefault();
    }

    /// <summary>
    /// ユーザーの入力を検証し、次のアクション（Step2へ進む / 復習へ戻る / 完了）を決定する。
    /// </summary>
    /// <param name="userInput">ユーザーが入力した文字列</param>
    /// <param name="currentWord">現在出題中の単語</param>
    /// <param name="userProgress">現在のユーザー進捗データ</param>
    /// <param name="isStep2">false = Step1（お手本あり）、true = Step2（見ないで入力）</param>
    /// <returns>次の画面遷移に必要なフラグを持つ <see cref="TrainingResult"/></returns>
    public TrainingResult ProcessAnswer(
        string userInput,
        EnglishWord currentWord,
        UserProgress userProgress,
        bool isStep2)
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
                // Step1 正解 → Step2（見ないで入力）へ進む
                result.ShouldGoToStep2 = true;
            }
            else
            {
                // Step2 正解 → 学習完了。ステータスを更新してから完了フラグを立てる
                UpdateWordStatus(userProgress, currentWord.Id, isCorrect: true);
                result.IsFinished = true;
            }
        }
        else
        {
            if (isStep2 && !string.IsNullOrEmpty(userInput))
            {
                // Step2 で不正解かつ空欄でない場合 → 強制復習モードへ
                // ※ 空欄（未入力）は誤答カウントしない設計
                UpdateWordStatus(userProgress, currentWord.Id, isCorrect: false);
                result.ShouldStartReview = true;
            }
            else if (!isStep2 && !string.IsNullOrEmpty(userInput))
            {
                // Step1 の不正解も IncorrectCount に記録する（苦手単語判定に使用）
                // ただし LastReviewed は更新せず、今日の学習済み判定には影響させない
                UpdateWordStatus(userProgress, currentWord.Id, isCorrect: false);
            }
        }

        return result;
    }

    /// <summary>
    /// 単語の学習ステータス（習熟度・正誤カウント・最終学習日）を更新する。
    /// WordStatus が存在しない場合は新規作成して追加する。
    /// </summary>
    /// <param name="progress">更新対象のユーザー進捗</param>
    /// <param name="wordId">更新対象の単語 ID</param>
    /// <param name="isCorrect">正解なら true、誤答なら false</param>
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
            status.Status = 1;          // 1 = 学習済み
            status.CorrectCount++;
            status.LastReviewed = DateTime.UtcNow;  // UTC で保存（判定ロジックと統一）
        }
        else
        {
            status.IncorrectCount++;
            // ⚠ 誤答時は LastReviewed を更新しない。
            //    これにより「今日まだ触れていない単語」判定に影響しない。
        }
    }

    // ============================================================
    // Study モード（復習クイズ）
    // ============================================================

    /// <summary>
    /// 復習用の4択クイズを1問生成して返す。
    /// </summary>
    /// <remarks>
    /// 出題優先順位：
    ///   ① 今日新しく覚えた単語のうち、今セッションで未出題のもの
    ///   ② 過去に誤答したことがある単語のうち、今セッションで未出題のもの（誤答数降順）
    ///   ③ 学習済みプールからランダム（無限ループ用。直前と同じ単語は除外）
    ///   ④ フォールバック：未学習を含む完全ランダム
    ///
    /// 出題形式（英→日 / 日→英）は各呼び出しでランダムに決定する。
    /// </remarks>
    public Question GenerateStudyQuestion(List<EnglishWord> allWords, UserProgress? userProgress)
    {
        var todayUtc = DateTime.UtcNow.Date;
        EnglishWord? target = null;

        // ① 今日学習した単語（セッション内未出題）
        var todayLearnedIds = userProgress?.WordStatuses
            .Where(s => s.LastReviewed.Date == todayUtc && s.Status >= 1)
            .Select(s => s.WordId)
            .ToHashSet() ?? new HashSet<string>();

        var freshWords = allWords
            .Where(w => todayLearnedIds.Contains(w.Id) && !_sessionWordCounts.ContainsKey(w.Id))
            .ToList();

        if (freshWords.Any())
        {
            target = freshWords[Random.Shared.Next(freshWords.Count)];
        }
        else
        {
            // ② 真の苦手単語 または 直近1週間以内にミスした単語
            var todayUtcData = DateTime.UtcNow.Date;
            var oneWeekAgo = todayUtcData.AddDays(-7);

            var weakWords = allWords
                .Where(w => {
                    var s = userProgress?.WordStatuses.FirstOrDefault(x => x.WordId == w.Id);
                    if (s == null) return false;

                    // 条件A: 長期的な苦手（正解数がミス数以下）
                    bool isLongTermWeak = s.IncorrectCount > 0 && s.CorrectCount <= s.IncorrectCount;

                    // 条件B: 短期的なミス（最終レビューが1週間以内で、かつミスがある）
                    // ※LastReviewedがUTCであることを考慮
                    bool isRecentlyMissed = s.LastReviewed.Date >= oneWeekAgo && s.IncorrectCount > 0;

                    // A または B を満たす単語を抽出
                    return isLongTermWeak || isRecentlyMissed;
                })
                .Where(w => !_sessionWordCounts.ContainsKey(w.Id)) // 今セッション未出題
                .OrderByDescending(w => {
                    var s = userProgress?.WordStatuses.FirstOrDefault(x => x.WordId == w.Id);
                    return s?.IncorrectCount ?? 0;
                })
                .ToList();

            if (weakWords.Any())
            {
                target = weakWords.First();
            }
            else
            {
                // ③ 学習済みプールからランダム（該当がなければ即ここに来る）
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

        // ④ フォールバック：未学習含む完全ランダム
        if (target == null)
            target = allWords[Random.Shared.Next(allWords.Count)];

        // セッション出題記録を更新
        _lastWordId = target.Id;
        if (!_sessionWordCounts.ContainsKey(target.Id))
            _sessionWordCounts[target.Id] = 0;
        _sessionWordCounts[target.Id]++;

        // 出題形式をランダム決定（50%ずつ）
        bool isEngToJap = Random.Shared.Next(2) == 0;

        // 正解選択肢と不正解3択を作成してシャッフル
        // ガード：単語数が4未満のときは取れるだけ取る（最低1択＝正解のみ になることもある）
        var correctOption = isEngToJap ? target.Meaning : target.Word;
        var wrongOptions = allWords
            .Where(w => w.Id != target.Id)
            .OrderBy(_ => Guid.NewGuid())
            .Take(3)
            .Select(w => isEngToJap ? w.Meaning : w.Word)
            .ToList();

        if (wrongOptions.Count < 3)
        {
            // 単語リストが足りない場合は重複を避けつつフォールバック
            // （本来は allWords が十分な件数あることを前提とする）
        }

        var options = new List<string> { correctOption };
        options.AddRange(wrongOptions);

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
    /// Study モードでの回答結果を UserProgress に記録する。
    /// WordStatus が存在しない場合は Status=1（学習済み）で新規作成する。
    /// </summary>
    public void RecordStudyResult(UserProgress progress, string wordId, bool isCorrect)
    {
        var status = progress.WordStatuses.FirstOrDefault(s => s.WordId == wordId);
        if (status == null)
        {
            // Study で初めて出題された場合は学習済みとして登録
            status = new WordStatus { WordId = wordId, Status = 1 };
            progress.WordStatuses.Add(status);
        }

        status.LastReviewed = DateTime.UtcNow;

        if (isCorrect)
            status.CorrectCount++;
        else
            status.IncorrectCount++;
    }

}