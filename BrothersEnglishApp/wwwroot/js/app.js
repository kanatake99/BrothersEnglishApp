// wwwroot/js/app.js

/**
 * 汎用ヘルパー (speechHandlers)
 * Blazor側からは "speechHandlers.関数名" で呼び出す
 */
window.speechHandlers = {
    // 読み上げ
    // rate 引数を追加（デフォルトは 1.0）
    speak: function (text, rate = 1.0) {
        if (!text) return;
        // すべての読み上げをキャンセルしてから開始
        window.speechSynthesis.cancel();

        const utterance = new SpeechSynthesisUtterance(text);
        utterance.lang = 'en-US';
        utterance.rate = rate; // ここで速度を設定！ (0.1 〜 10.0)
        window.speechSynthesis.speak(utterance);
    },

    // フォーカス・UI操作
    focusElement: function (element) {
        if (element && typeof element.focus === 'function') {
            element.focus();
        }
    },
    blurActiveElement: function () {
        if (document.activeElement && typeof document.activeElement.blur === 'function') {
            document.activeElement.blur();
        }
    },

    // 判定用（今回エラーになっていた場所）
    getUserAgent: () => navigator.userAgent || "",
    elementExists: (selector) => document.querySelector(selector) !== null
};

/**
 * スクロール操作 (appFunctions)
 */
window.appFunctions = {
    scrollToElement: function (elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            const offset = 70;
            const bodyRect = document.body.getBoundingClientRect().top;
            const elementRect = element.getBoundingClientRect().top;
            const elementPosition = elementRect - bodyRect;
            const offsetPosition = elementPosition - offset;

            window.scrollTo({ top: offsetPosition, behavior: 'smooth' });
        }
    },
    scrollToTop: function () {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }
};

/**
 * クイズ専用 (quizHandlers)
 */
window.quizHandlers = {
    resetButtons: () => {
        const buttons = document.querySelectorAll('.quiz-button');
        buttons.forEach(btn => btn.blur());
    }
};

/**
 * 仮想キーボードの状態管理用グローバル変数
 */
window.keyboardInputBuffer = "";
window.currentKeyboard = null;

/**
 * C#から呼び出し可能なクリア関数
 */
window.clearKeyboard = () => {
    window.keyboardInputBuffer = "";
    if (window.currentKeyboard && typeof window.currentKeyboard.setInput === "function") {
        window.currentKeyboard.setInput("");
    }
};

/**
 * キーボードの初期化関数
 * 単語・文の両方に対応した「フルセット」レイアウト
 */
window.setupKeyboard = (dotNetHelper) => {
    // 既存のキーボードを破棄
    if (window.currentKeyboard) {
        window.currentKeyboard.destroy();
        window.currentKeyboard = null;
    }

    const el = document.querySelector(".simple-keyboard");
    if (!el) {
        console.error("Keyboard container (.simple-keyboard) not found!");
        return;
    }

    // ライブラリが存在するか最終確認
    if (!window.SimpleKeyboard) {
        console.error("SimpleKeyboard library is not loaded!");
        return;
    }

    const Keyboard = window.SimpleKeyboard.default;
    window.currentKeyboard = new Keyboard({
        onKeyPress: button => {
            // JS側で文字列を組み立てるのをやめ、押されたキーの名前(q, w, {space}など)を直接送る
            dotNetHelper.invokeMethodAsync('OnKeyboardInput', button);
        },
        layout: {
            'default': [
                'q w e r t y u i o p',
                'a s d f g h j k l \'',
                'z x c v b n m {backspace}',
                '{space} {enter}'
            ]
        },
        display: {
            '{enter}': '決定',
            '{backspace}': '⌫',
            '{space}': 'Space'
        }
    });
};

window.destroyKeyboard = () => {
    if (window.currentKeyboard) {
        window.currentKeyboard.destroy();
        window.currentKeyboard = null;
    }
    window.keyboardInputBuffer = "";
};
