namespace BrothersEnglishApp.Models
{
    public class UserProgress
    {
        public string UserName { get; set; } = "";
        public List<WordStatus> WordStatuses { get; set; } = new();
        public int DailyGoal { get; set; } = 10;      // 「覚える」用
        public int StudyGoal { get; set; } = 15;      // 「復習」用
        public Dictionary<string, DayActivity> ActivityLog { get; set; } = new(); // 日付（"yyyyMMdd"形式）をキーにして、その日の活動を保存する
        public bool IsAutoSpeechEnabled { get; set; } = true;
        public string GetTodayKey() => DateTime.Now.ToString("yyyyMMdd");
    }
    public class DayActivity
    {
        public bool TrainingDone { get; set; }
        public bool StudyDone { get; set; }

    }
}
