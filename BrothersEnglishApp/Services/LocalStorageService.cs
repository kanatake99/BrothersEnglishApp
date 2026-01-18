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
}