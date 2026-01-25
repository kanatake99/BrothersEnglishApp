namespace BrothersEnglishApp.Models
{
    public class UserProgress
    {
        public string UserName { get; set; } = ""; // ユーザー名
        public List<WordStatus> WordStatuses { get; set; } = new List<WordStatus>();
    }
}
