using Microsoft.JSInterop;

namespace BrothersEnglishApp.Services
{
    public class SpeechService
    {
        private readonly IJSRuntime _js;

        // 効果音のパス
        public const string SoundCorrect = "data/quiz-dingdong.mp3";
        public const string SoundIncorrect = "data/quiz-buzzer.mp3";

        public SpeechService(IJSRuntime js)
        {
            _js = js;
        }

        /// <summary>
        /// 指定したテキストを音声で読み上げます。
        /// </summary>
        /// <param name="text">読み上げる英文</param>
        /// <param name="rate">再生速度 (0.1 ~ 2.0)</param>
        public async Task SpeakAsync(string text, double rate = 1.0)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            // JS側の speechHandlers.speak を呼び出す
            // 第2引数は再生速度だぜ
            await _js.InvokeVoidAsync("speechHandlers.speak", text, rate);
        }

        /// <summary>
        /// MP3ファイルを再生します（★新機能）
        /// </summary>
        public async Task PlaySoundAsync(string path)
        {
            await _js.InvokeVoidAsync("appFunctions.playAudio", path);
        }

        /// <summary>
        /// 正解音を鳴らします（★便利ショートカット）
        /// </summary>
        public async Task PlayCorrectSoundAsync() => await PlaySoundAsync(SoundCorrect);

        /// <summary>
        /// 不正解音を鳴らします（★便利ショートカット）
        /// </summary>
        public async Task PlayIncorrectSoundAsync() => await PlaySoundAsync(SoundIncorrect);

        /// <summary>
        /// 再生中の音声を停止します。
        /// </summary>
        public async Task CancelAsync()
        {
            await _js.InvokeVoidAsync("speechHandlers.cancel");
        }
    }
}