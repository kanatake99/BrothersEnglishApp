namespace BrothersEnglishApp.Models
{
    public class DictationProgress
    {
        public string SentenceId { get; set; } = "";
        public double Accuracy { get; set; }
        // 保存・判定はUTCに統一だぜ
        public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;
    }

    // UserProgress.cs の中にこれを追加してくれ
    // public List<DictationProgress> DictationHistory { get; set; } = new();
}