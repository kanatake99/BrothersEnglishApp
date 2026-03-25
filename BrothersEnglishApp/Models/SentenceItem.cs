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
}