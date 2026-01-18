namespace BrothersEnglishApp.Services;

public class UserContext // ユーザーの状態を管理するクラス
{
    public string CurrentUser { get; set; } = "";
    public bool IsAdmin => CurrentUser == "管理人"; // 管理人かどうかを判定
    public bool IsSelected => !string.IsNullOrEmpty(CurrentUser);
}