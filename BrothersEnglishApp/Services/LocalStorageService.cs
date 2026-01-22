using Microsoft.JSInterop;
using System.Text.Json;
using BrothersEnglishApp.Models;

namespace BrothersEnglishApp.Services;

// クラス名のすぐ横で (IJSRuntime js) を受け取る！これが「プライマリコンストラクター」だよ。
// これで、クラス内のどこでも「js」という名前でJavaScriptが呼べるようになるんだ。
public class LocalStorageService(IJSRuntime js)
{
    // データを保存する
    public async Task SaveAsync<T>(string key, T value)
    {
        try
        {
            // value を JSON という文字列形式に変換して、ブラウザの引き出しにしまうよ
            var json = JsonSerializer.Serialize(value);
            await js.InvokeVoidAsync("localStorage.setItem", key, json);
        }
        catch (Exception ex)
        {
            // 万が一保存できなかった時のための保険だね
            Console.WriteLine($"保存エラー: {ex.Message}");
        }
    }

    // データを取り出す
    public async Task<T?> LoadAsync<T>(string key)
    {
        try
        {
            // 引き出しから JSON 文字列を取り出す
            var json = await js.InvokeAsync<string?>("localStorage.getItem", key);

            // 何も入っていなければ、空っぽ（デフォルト値）を返すよ
            if (string.IsNullOrEmpty(json)) return default;

            // 文字列を元のデータ形式（T）に戻してあげる
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"読み込みエラー: {ex.Message}");
            return default;
        }
    }

    // セッション情報を保存・取得するメソッド
    public async Task SaveSessionAsync(UserSession session)
    {
        // セッション情報を保存
        await js.InvokeVoidAsync("localStorage.setItem", "user_session", JsonSerializer.Serialize(session));
    }

    public async Task<UserSession?> GetSessionAsync()
    {
        var json = await js.InvokeAsync<string>("localStorage.getItem", "user_session");
        if (string.IsNullOrEmpty(json)) return null;

        var session = JsonSerializer.Deserialize<UserSession>(json);

        // 1ヶ月（30日）経過しているかチェック
        if (session != null && session.LoginDate.AddDays(30) < DateTime.Now)
        {
            await js.InvokeVoidAsync("localStorage.removeItem", "user_session");
            return null;
        }
        return session;
    }

    // データを削除する
    public async Task DeleteAsync(string key)
    {
        // JavaScript の localStorage.removeItem を呼び出す
        await js.InvokeVoidAsync("localStorage.removeItem", key);
    }
}

