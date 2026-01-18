namespace BrothersEnglishApp.Models
{
    public class EnglishWord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Word { get; set; } = ""; // 単語
        public string IPA { get; set; } = ""; // 発音記号
        public string PartOfSpeech { get; set; } = ""; // 品詞
        public string Meaning { get; set; } = ""; // 意味
        public int Level { get; set; } = 3;
    }
}
