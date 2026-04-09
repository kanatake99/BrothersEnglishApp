using Microsoft.JSInterop;

namespace BrothersEnglishApp.Services;

/// <summary>
/// 【役割】
/// C#側から JavaScript (app.js) のキーボード操作を呼び出すための「専用窓口」。
/// 各 Razor ページで IJSRuntime を直接叩く代わりに、このサービスを経由することで
/// 呼び出し名の打ち間違いを防ぎ、安全に操作できるようにしているんだ。
/// </summary>
public class KeyboardService : IAsyncDisposable
{
    private readonly IJSRuntime _js;

    // コンストラクタ：Blazor標準の IJSRuntime を受け取って保持
    public KeyboardService(IJSRuntime js) => _js = js;

    /// <summary>
    /// 【実行内容】JS側(app.js)の「window.keyboardHandlers.setup」を呼び出す
    /// 【引数】dotNetRef: C#側のインスタンス参照。JS側から C# を呼び戻すために使用
    /// 【使われる場所】VirtualKeyboard.razor の OnAfterRenderAsync 内
    /// </summary>
    public async ValueTask SetupAsync<T>(DotNetObjectReference<T> dotNetRef) where T : class
    {
        // 呼び出し先：wwwroot/js/app.js 内の keyboardHandlers.setup
        await _js.InvokeVoidAsync("keyboardHandlers.setup", dotNetRef);
    }

    /// <summary>
    /// 【実行内容】JS側(app.js)の「window.keyboardHandlers.clear」を呼び出す
    /// 【使われる場所】各学習ページ（Study.razorなど）で「次の問題へ行く時」や「リセットボタン」
    /// </summary>
    public async ValueTask ClearAsync()
    {
        // 呼び出し先：wwwroot/js/app.js 内の keyboardHandlers.clear
        await _js.InvokeVoidAsync("keyboardHandlers.clear");
    }

    /// <summary>
    /// 【実行内容】JS側(app.js)の「window.keyboardHandlers.destroy」を呼び出す
    /// 【使われる場所】VirtualKeyboard.razor の DisposeAsync 内（コンポーネント破棄時）
    /// </summary>
    public async ValueTask DestroyAsync()
    {
        try
        {
            // 呼び出し先：wwwroot/js/app.js 内の keyboardHandlers.destroy
            await _js.InvokeVoidAsync("keyboardHandlers.destroy");
        }
        catch (JSException)
        {
            // ブラウザのページが閉じられた後に C# が動こうとした時のエラーを無視するぜ
        }
    }

    /// <summary>
    /// 【役割】このサービス自体が不要になった時に呼ばれる（DIコンテナ用）
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await DestroyAsync();
    }
}