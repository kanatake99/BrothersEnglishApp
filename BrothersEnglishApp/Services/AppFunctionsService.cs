using Microsoft.JSInterop;

namespace BrothersEnglishApp.Services
{
    public class AppFunctionsService
    {
        private readonly IJSRuntime _js;
        public AppFunctionsService(IJSRuntime js) => _js = js;

        // 【復活】効果音のパス定数
        public const string SoundCorrect = "data/quiz-dingdong.mp3";
        public const string SoundIncorrect = "data/quiz-buzzer.mp3";

        public ValueTask<string> GetUserAgentAsync() =>
            _js.InvokeAsync<string>("speechHandlers.getUserAgent");

        public ValueTask PlaySoundAsync(string path) =>
            _js.InvokeVoidAsync("appFunctions.playAudio", path);

        // 【復活】ショートカットメソッド
        public async Task PlayCorrectSoundAsync() => await PlaySoundAsync(SoundCorrect);
        public async Task PlayIncorrectSoundAsync() => await PlaySoundAsync(SoundIncorrect);

        public ValueTask ScrollToElementAsync(string id) =>
            _js.InvokeVoidAsync("appFunctions.scrollToElement", id);

        public ValueTask ScrollToTopAsync() =>
            _js.InvokeVoidAsync("appFunctions.scrollToTop");

        /// ブラウザ標準のアラートダイアログを表示します
        public ValueTask AlertAsync(string message) =>
            _js.InvokeVoidAsync("alert", message);

        public ValueTask BlurActiveElementAsync() =>
            _js.InvokeVoidAsync("speechHandlers.blurActiveElement");
    }
}