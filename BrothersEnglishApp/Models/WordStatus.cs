namespace BrothersEnglishApp.Models
{
    public class WordStatus
    {
        public string WordId { get; set; } = "";
        public int Status { get; set; } = 0;
        public int CorrectCount { get; set; } = 0;   // 正解数
        public int IncorrectCount { get; set; } = 0; // 不正解数
        public DateTime LastReviewed { get; set; } = DateTime.UtcNow;
    }
}
