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
    public string QuestionText { get; set; } = "";
    public string CorrectAnswer { get; set; } = "";
    public List<string> Options { get; set; } = [];
    public bool IsEnglishToJapanese { get; set; }
}

public class TrainingEngine
{
    // 前回の単語IDを一時的に保持する変数
    private string? _lastWordId;

    // --- Training用ロジック ---
    public EnglishWord? GetNextWord(List<EnglishWord> allWords, UserProgress userProgress, int dailyGoal)
    {
        var todayUtc = DateTime.UtcNow.Date;
        var todayCount = userProgress.WordStatuses.Count(s =>
            s.LastReviewed.Date == todayUtc && s.Status >= 1);

        if (todayCount >= dailyGoal) return null;

        var candidates = allWords.Where(w =>
            !userProgress.WordStatuses.Any(s =>
                s.WordId == w.Id && s.LastReviewed.Date == todayUtc));

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

        // 1. 出題候補（pool）の作成
        var todayWords = allWords.Where(w =>
            userProgress?.WordStatuses.Any(s => s.WordId == w.Id && s.LastReviewed.Date == todayUtc && s.Status >= 1) ?? false
        ).ToList();

        List<EnglishWord> pool = todayWords.Count < 3
                    ? (allWords.Where(w => userProgress?.WordStatuses.Any(s => s.WordId == w.Id && s.Status >= 1) ?? false).ToList())
                    : todayWords;

        if (pool.Count == 0) pool = allWords.Take(20).ToList();

        // ★ ここがポイント：前回と違う単語を選ぶロジック
        // poolが2件以上あれば、前回出したID以外のものに絞り込む
        var finalPool = (pool.Count > 1 && _lastWordId != null)
            ? pool.Where(w => w.Id != _lastWordId).ToList()
            : pool;

        // 2. ターゲットの決定
        var target = finalPool[Random.Shared.Next(finalPool.Count)];

        // ★ 今回のIDを「前回のID」として保存しておく
        _lastWordId = target.Id;

        bool isEngToJap = Random.Shared.Next(2) == 0;

        var options = new List<string> { isEngToJap ? target.Meaning : target.Word };
        var wrongCandidates = allWords.Where(w => w.Id != target.Id).OrderBy(_ => Guid.NewGuid()).Take(3);
        foreach (var w in wrongCandidates)
        {
            options.Add(isEngToJap ? w.Meaning : w.Word);
        }

        return new Question
        {
            QuestionText = isEngToJap ? target.Word : target.Meaning,
            CorrectAnswer = isEngToJap ? target.Meaning : target.Word,
            Options = options.OrderBy(_ => Guid.NewGuid()).ToList(),
            IsEnglishToJapanese = isEngToJap
        };
    }
}