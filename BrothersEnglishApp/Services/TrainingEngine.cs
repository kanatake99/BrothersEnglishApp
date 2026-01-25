// Services/TrainingEngine.cs
// 英単語学習アプリのトレーニングエンジン
using BrothersEnglishApp.Models;

namespace BrothersEnglishApp.Services;

public class TrainingResult
{
    public bool IsCorrect { get; set; }
    public bool ShouldGoToStep2 { get; set; }
    public bool ShouldStartReview { get; set; }
    public bool IsFinished { get; set; }
}


    public class TrainingEngine
{

    public EnglishWord? GetNextWord(
        List<EnglishWord> allWords,
        UserProgress userProgress,
        int dailyGoal)
    {
        // 1. 今日の達成目標をチェック
        var todayUtc = DateTime.UtcNow.Date; // UTCでの今日

        var todayCount = userProgress.WordStatuses.Count(s =>
            s.LastReviewed.Date == todayUtc && s.Status >= 1);

        if (todayCount >= dailyGoal) return null;

        // 2. 出題候補を絞り込む
        // まだ今日学習していない単語だけを対象にする
        var candidates = allWords.Where(w =>
            !userProgress.WordStatuses.Any(s =>
                s.WordId == w.Id && s.LastReviewed.Date == todayUtc));

        // 3. レベル順（5→1）で並べ、同じレベル内ではランダムにする
        return candidates
            .OrderByDescending(w => w.Level) // レベルが高い順
            .ThenBy(_ => Guid.NewGuid())     // 同じレベル内はランダム
            .FirstOrDefault();
    }

    // 回答をチェックして進捗を更新するロジック
    public TrainingResult ProcessAnswer(string userInput, EnglishWord currentWord, UserProgress userProgress, bool isStep2)
    {
        var result = new TrainingResult();
        bool isCorrect = userInput.Trim().ToLower() == currentWord.Word.ToLower();
        result.IsCorrect = isCorrect;

        if (isCorrect)
        {
            if (!isStep2)
            {
                result.ShouldGoToStep2 = true;
            }
            else
            {
                // Step2正解なら進捗を更新
                UpdateWordStatus(userProgress, currentWord.Id, true);
                result.IsFinished = true;
            }
        }
        else
        {
            if (isStep2 && !string.IsNullOrEmpty(userInput))
            {
                // Step2で間違えたら不正解カウントを増やして復習モードへ
                UpdateWordStatus(userProgress, currentWord.Id, false);
                result.ShouldStartReview = true;
            }
        }
        return result;
    }

    // 単語の進捗状況を更新するヘルパーメソッド
    private void UpdateWordStatus(UserProgress progress, string wordId, bool isCorrect)
    {
        // 2. ここで探す時も string 同士の比較になるよ
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
}