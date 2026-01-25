using BrothersEnglishApp.Models;
using BrothersEnglishApp.Services;

public class UserContext(LocalStorageService localStorage)
{
    public UserSession? CurrentSession { get; private set; }

    public event Action? OnChange;

    public bool IsSelected => CurrentSession != null; // IsLoggedIn と同じ意味
    public string CurrentUser => CurrentSession?.UserName ?? "ゲスト";
    public string CurrentUserId => CurrentSession?.UserId ?? "";
    public bool IsLoggedIn => CurrentSession != null;
    public bool IsAdmin => CurrentSession?.UserId == "admin";

    // 初期化処理：保存されたセッション情報を読み込む
    public async Task InitializeAsync()
    {
        CurrentSession = await localStorage.LoadAsync<UserSession>("user_session");
        if (CurrentSession != null && CurrentSession.LoginDate.AddDays(30) < DateTime.UtcNow)
        {
            await LogoutAsync();
        }
    }

    // ログイン処理
    public async Task LoginAsync(UserSession session)
    {
        CurrentSession = session;
        await localStorage.SaveAsync("user_session", session);

        NotifyStateChanged();
    }

    // ログアウト処理
    public async Task LogoutAsync()
    {
        CurrentSession = null;
        await localStorage.DeleteAsync("user_session");
        NotifyStateChanged();
    }

    // 状態変更を通知するためのヘルパーメソッド
    private void NotifyStateChanged() => OnChange?.Invoke();
}