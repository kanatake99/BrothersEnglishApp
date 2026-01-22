namespace BrothersEnglishApp.Models;

/// <summary>
/// ユーザーのログインセッション情報を保持するレコード
/// </summary>
/// <param name="UserId">ユーザーを識別するID（tsukasa, mamoru 等）</param>
/// <param name="UserName">表示用の名前</param>
/// <param name="LoginDate">ログインした日時</param>
// Models/UserSession.cs
public record UserSession(string UserId, string UserName, string Pin, DateTime LoginDate);