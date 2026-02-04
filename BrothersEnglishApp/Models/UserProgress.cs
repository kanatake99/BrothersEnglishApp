namespace BrothersEnglishApp.Models
{
    public class UserProgress
    {
        public string UserName { get; set; } = "";
        public List<WordStatus> WordStatuses { get; set; } = new();

        public int DailyGoal { get; set; } = 10;  // 「Study（新しい単語）」用
        public int TrainingGoal { get; set; } = 15; // 「Training（復習クイズ）」用

        public Dictionary<string, DayActivity> ActivityLog { get; set; } = new();
        public bool IsAutoSpeechEnabled { get; set; } = true;

        // --- 修正：UTCベースに統一 ---
        // これでサーバーとクライアント、どこの国で動かしても日付がズレなくなるぜ
        public string GetTodayKey() => DateTime.UtcNow.ToString("yyyyMMdd");
    }

    public class DayActivity
    {
        // --- 修正：画面名に合わせてプロパティ名も変更 ---
        public bool StudyDone { get; set; }    // 新規学習
        public bool TrainingDone { get; set; } // 復習トレーニング
    }
}