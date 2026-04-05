// StudyPageBase.cs
// 学習ページの共通基底クラス。スマホ判定やキーボード管理など、全学習ページで共通の処理をここにまとめる。

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using BrothersEnglishApp.Shared;

namespace BrothersEnglishApp.Pages
{
    public abstract class StudyPageBase : ComponentBase, IAsyncDisposable
    {
        [Inject] protected IJSRuntime JS { get; set; } = default!;

        protected bool IsMobile { get; private set; }
        protected VirtualKeyboard? Keyboard;
        protected string UserInput = "";

        // 最初の描画が終わった後に、スマホ判定と必要な初期処理を行う
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // 全学習ページ共通の「スマホ判定」をここで一括実行
                var ua = await JS.InvokeAsync<string>("speechHandlers.getUserAgent");
                IsMobile = ua.Contains("iPhone") || ua.Contains("Android") || ua.Contains("iPad");

                // 判定が終わったら再描画して、UIをスマホ用に切り替える
                StateHasChanged();

                // 1問目の読み上げが必要な場合は、各ページでこのメソッドをオーバーライドする
                await OnPageFirstRenderAsync();
            }
        }

        // StudyPageBase.cs に以下を追加してくれ
        protected virtual Task OnInputChangedAsync() => Task.CompletedTask;

        // キーボードからの入力を受け取る共通メソッド
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

            // 入力が変わったことを子クラス（SentenceTrainingなど）に通知するぜ！
            await OnInputChangedAsync();
            StateHasChanged();
        }

        // ページごとの「最初の1回だけの処理（音声再生など）」
        protected virtual Task OnPageFirstRenderAsync() => Task.CompletedTask;

        // ユーザーの入力をリセットする共通処理。キーボードもリセットして、UIを更新する。
        protected async Task ResetInputAsync()
        {
            UserInput = "";
            if (Keyboard != null) await Keyboard.ClearAsync();
            StateHasChanged();
        }

        // ページを離れるときの共通処理（キーボードのリセットなど）をここでまとめて行う
        public virtual async ValueTask DisposeAsync()
        {
            // ページを離れるときに、キーボードのリセットなどを共通で行う
            if (Keyboard != null)
            {
                await Keyboard.ClearAsync();
            }

            // 子クラスで追加の片付けをしたい場合は、ここを override できるようにしておく
            await OnDisposeAsync();
        }

        // 子クラスで追加の片付けをしたい場合は、ここを override できるようにしておく
        protected virtual ValueTask OnDisposeAsync() => ValueTask.CompletedTask;
    }
}