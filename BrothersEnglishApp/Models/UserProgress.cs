namespace BrothersEnglishApp.Models
{
    public class UserProgress
    {
        public string UserName { get; set; } = "";

        // --- 単語データ ---
        public List<WordStatus> WordStatuses { get; set; } = new();

        // --- 文（イディオム）データ ---
        // IDをキーにして、各文の学習状態を管理するぜ
        public Dictionary<string, SentenceStatus> SentenceStatuses { get; set; } = new();

        // --- 設定（Settings） ---
        public UserSettings Settings { get; set; } = new();

        // --- 活動記録（ActivityLog） ---
        public Dictionary<string, DayActivity> ActivityLog { get; set; } = new();

        // --- ユーティリティ ---
        public string GetTodayKey() => DateTime.UtcNow.ToString("yyyyMMdd");

        // 旧プロパティとの互換性のために残すか、整理するかだが、
        // 今後は Settings 側を参照するようにしていくぜ。
    }

    public class UserSettings
    {
        // 単語設定
        public int WordStudyGoal { get; set; } = 10;
        public int WordTrainingGoal { get; set; } = 15;
        public float WordSpeechRate { get; set; } = 1.0f;

        // 文（Sentence）設定
        public int SentenceStudyGoal { get; set; } = 3;
        public int SentenceTrainingGoal { get; set; } = 3;
        public float SentenceSpeechRate { get; set; } = 0.8f;

        // 共通
        public bool IsAutoSpeechEnabled { get; set; } = true;
    }

    public class DayActivity
    {
        public bool StudyDone { get; set; }          // 単語・新規
        public bool TrainingDone { get; set; }       // 単語・復習
        public bool SentenceStudyDone { get; set; }  // 文・新規 (NEW)
        public bool SentenceTrainingDone { get; set; } // 文・復習 (NEW)
    }

    public class SentenceStatus
    {
        public string Id { get; set; } = "";
        public int Status { get; set; } = 0; // 0:未学習, 1:学習済み
        public DateTime LastReviewed { get; set; } = DateTime.UtcNow;
        public int Level { get; set; }
    }
}