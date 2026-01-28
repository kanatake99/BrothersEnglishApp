using Microsoft.JSInterop;
using System.Text.Json;
using BrothersEnglishApp.Models;

namespace BrothersEnglishApp.Services;

/// <summary>
/// ブラウザのLocalStorageへのアクセスと、ユーザーデータの永続化を担うサービス
/// </summary>
public class LocalStorageService(IJSRuntime js)
{
    // JSON変換のルールを一箇所に集約（Copilot指摘5の対応）
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true, // 大文字小文字を気にしない
        WriteIndented = false               // 保存容量節約のため改行はなし
    };

    #region 基本的な読み書きメソッド

    /// <summary>汎用的なデータの保存</summary>
    public async Task SaveAsync<T>(string key, T value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            await js.InvokeVoidAsync("localStorage.setItem", key, json);
        }
        catch (Exception ex)
        {
            // 本番では ILogger を使うのが理想だけど、まずは開発用に
            Console.WriteLine($"[LocalStorage Save Error]: {key} - {ex.Message}");
        }
    }

    /// <summary>汎用的なデータの取得</summary>
    public async Task<T?> LoadAsync<T>(string key)
    {
        try
        {
            var json = await js.InvokeAsync<string?>("localStorage.getItem", key);
            if (string.IsNullOrEmpty(json)) return default;

            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LocalStorage Load Error]: {key} - {ex.Message}");
            return default;
        }
    }

    /// <summary>指定したキーのデータを削除</summary>
    public async Task DeleteAsync(string key) =>
        await js.InvokeVoidAsync("localStorage.removeItem", key);

    #endregion

    #region ユーザー進捗（UserProgress）専用メソッド

    // キー名の命名規則を一箇所で管理（Copilot指摘4の対応）
    private string GetUserKey(string userId) => $"User_{userId}";

    /// <summary>特定のユーザーの学習進捗を保存</summary>
    public async Task SaveUserProgressAsync(string userId, UserProgress progress)
    {
        if (string.IsNullOrWhiteSpace(userId)) return;
        await SaveAsync(GetUserKey(userId), progress);
    }

    /// <summary>特定のユーザーの学習進捗を読み込み</summary>
    public async Task<UserProgress?> LoadUserProgressAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return null;
        return await LoadAsync<UserProgress>(GetUserKey(userId));
    }

    #endregion

    #region セッション管理

    /// <summary>ログインセッションの保存</summary>
    public async Task SaveSessionAsync(UserSession session) =>
        await SaveAsync("user_session", session);

    /// <summary>有効期限をチェックしながらセッションを取得</summary>
    public async Task<UserSession?> GetSessionAsync()
    {
        var session = await LoadAsync<UserSession>("user_session");
        if (session == null) return null;

        // 保存は常にUTCで行い、比較もUTCで行う（Copilot指摘3の対応）
        // ※LoginDateが保存時にUTCであることを前提にしているよ
        if (session.LoginDate.AddDays(30) < DateTime.UtcNow)
        {
            await DeleteAsync("user_session");
            return null;
        }
        return session;
    }

    #endregion
}