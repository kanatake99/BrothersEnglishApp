namespace BrothersEnglishApp.Models
{
    public class SentenceItem
    {
        public string Id { get; set; } = "";
        public int Level { get; set; }
        public string English { get; set; } = "";
        public string Japanese { get; set; } = "";
        public string Target { get; set; } = ""; // 穴埋めの答え
        public string Note { get; set; } = "";
    }

    // --- クイズ用の一時的な型 ---
    public class SentenceQuestion
    {
        public SentenceItem Sentence { get; set; } = new();
        public List<string> Chips { get; set; } = new();
        public string CorrectAnswer { get; set; } = "";

        public List<string> TargetParts { get; set; } = new();
    }

    // --- 文を「普通の文字」と「穴埋め」に分けるための型 ---
    public class SentenceSegment
    {
        public string Text { get; set; } = "";
        public bool IsTarget { get; set; } = false;
    }
}