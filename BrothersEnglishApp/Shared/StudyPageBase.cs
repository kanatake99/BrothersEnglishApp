using Microsoft.AspNetCore.Components;
using BrothersEnglishApp.Shared;
using BrothersEnglishApp.Services;

namespace BrothersEnglishApp.Pages
{
    public abstract class StudyPageBase : ComponentBase, IAsyncDisposable
    {
        [Inject] protected KeyboardService KeyboardService { get; set; } = default!;

        // 【修正】読み上げ用
        [Inject] protected SpeechService SpeechService { get; set; } = default!;

        // 【追加】ブラウザ共通操作（UA取得など）用
        [Inject] protected AppFunctionsService AppService { get; set; } = default!;

        protected bool IsMobile { get; private set; }
        protected VirtualKeyboard? Keyboard;
        protected string UserInput = "";

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // 【変更】AppService 経由で UA を取得する
                var ua = await AppService.GetUserAgentAsync();
                IsMobile = ua.Contains("iPhone") || ua.Contains("Android") || ua.Contains("iPad");

                StateHasChanged();
                await OnPageFirstRenderAsync();
            }
        }

        protected virtual Task OnInputChangedAsync() => Task.CompletedTask;

        protected virtual async Task HandleVirtualKey(string key)
        {
            if (key == "{backspace}")
            {
                if (UserInput.Length > 0) UserInput = UserInput[..^1];
            }
            else if (key == "{space}")
            {
                UserInput += " ";
            }
            else if (key.StartsWith("{") && key.EndsWith("}"))
            {
                return;
            }
            else
            {
                UserInput += key;
            }

            await OnInputChangedAsync();
            StateHasChanged();
        }

        protected virtual Task OnPageFirstRenderAsync() => Task.CompletedTask;

        protected async Task ResetInputAsync()
        {
            UserInput = "";
            if (Keyboard != null)
            {
                await Keyboard.ClearAsync();
            }
            StateHasChanged();
        }

        public virtual async ValueTask DisposeAsync()
        {
            if (Keyboard != null)
            {
                await KeyboardService.ClearAsync();
            }

            await OnDisposeAsync();
        }

        protected virtual ValueTask OnDisposeAsync() => ValueTask.CompletedTask;
    }
}