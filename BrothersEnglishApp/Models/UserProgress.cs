namespace BrothersEnglishApp.Models
{
    public class UserProgress
    {
        public string UserName { get; set; } = "";
        public List<WordStatus> WordStatuses { get; set; } = new();

        // 日付（"yyyyMMdd"形式）をキーにして、その日の活動を保存する
        public Dictionary<string, DayActivity> ActivityLog { get; set; } = new();
    }
    public class DayActivity
    {
        public bool TrainingDone { get; set; }
        public bool StudyDone { get; set; }
    }
}
