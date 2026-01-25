namespace BrothersEnglishApp.Models
{
    public class UserProgress
    {
        public string UserName { get; set; } = "";
        public List<WordStatus> WordStatuses { get; set; } = new();

        // ↓ これを追加！日付をキーにして、何をやったか記録する
        public Dictionary<string, DayActivity> ActivityLog { get; set; } = new();
    }
    public class DayActivity
    {
        public bool TrainingDone { get; set; }
        public bool StudyDone { get; set; }
    }
}
