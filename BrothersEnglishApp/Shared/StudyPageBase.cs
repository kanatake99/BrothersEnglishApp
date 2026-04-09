using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using BrothersEnglishApp.Shared;
using BrothersEnglishApp.Services;

namespace BrothersEnglishApp.Pages
{
    public abstract class StudyPageBase : ComponentBase, IAsyncDisposable
    {
        // 【注入】新設したキーボード専用サービス
        [Inject] protected KeyboardService KeyboardService { get; set; } = default!;
        // 【注入】UA取得や要素チェック用
        [Inject] protected IJSRuntime JS { get; set; } = default!;

        protected bool IsMobile { get; private set; }
        protected VirtualKeyboard? Keyboard;
        protected string UserInput = "";

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // [連携] app.js: speechHandlers.getUserAgent を使用
                var ua = await JS.InvokeAsync<string>("speechHandlers.getUserAgent");
                IsMobile = ua.Contains("iPhone") || ua.Contains("Android") || ua.Contains("iPad");

                StateHasChanged();
                await OnPageFirstRenderAsync();
            }
        }

        protected virtual Task OnInputChangedAsync() => Task.CompletedTask;

        // 【共通処理】キーボード入力の受け皿
        protected virtual async Task HandleVirtualKey(string key)
        {
            if (key == "{backspace}")
            {
                if (UserInput.Length > 0) UserInput = UserInput[..^1];
            }
            else
            {
                UserInput += key;
            }

            // [通知] 子クラス（SentenceTraining等）で入力監視が必要な場合に実行
            await OnInputChangedAsync();
            StateHasChanged();
        }

        protected virtual Task OnPageFirstRenderAsync() => Task.CompletedTask;

        // 【共通処理】入力リセット
        protected async Task ResetInputAsync()
        {
            UserInput = "";
            if (Keyboard != null)
            {
                // [連携] VirtualKeyboard.razor 経由で KeyboardService.ClearAsync を実行
                await Keyboard.ClearAsync();
            }
            StateHasChanged();
        }

        // 【終了処理】ページ離脱時のクリーンアップ
        public virtual async ValueTask DisposeAsync()
        {
            if (Keyboard != null)
            {
                // [連携] KeyboardService.cs: keyboardHandlers.clear を実行
                await KeyboardService.ClearAsync();
            }

            await OnDisposeAsync();
        }

        protected virtual ValueTask OnDisposeAsync() => ValueTask.CompletedTask;
    }
}