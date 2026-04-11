using Microsoft.JSInterop;

namespace BrothersEnglishApp.Services
{
    public class SpeechService
    {
        private readonly IJSRuntime _js;

        public SpeechService(IJSRuntime js)
        {
            _js = js;
        }

        /// <summary>指定したテキストを読み上げる（rate は任意）</summary>
        public async Task SpeakAsync(string text, double rate = 1.0)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            await _js.InvokeVoidAsync("speechHandlers.speak", text, rate);
        }

        /// <summary>再生中の読み上げを停止する</summary>
        public async Task CancelAsync()
        {
            await _js.InvokeVoidAsync("speechHandlers.cancel");
        }
    }
}