// wwwroot/js/app.js

/**
 * 汎用ヘルパー (speechHandlers)
 * Blazor側からは "speechHandlers.関数名" で呼び出す
 */
window.speechHandlers = {
    // 読み上げ
    speak: function (text) {
        if (!text) return;
        window.speechSynthesis.cancel();
        const msg = new SpeechSynthesisUtterance(text);
        msg.lang = 'en-US';
        window.speechSynthesis.speak(msg);
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

// --- 以下、Simple Keyboard 関連（グローバル関数として定義） ---

window.keyboardInputBuffer = "";
window.currentKeyboard = null;

window.clearKeyboard = () => {
    window.keyboardInputBuffer = "";
    if (window.currentKeyboard && typeof window.currentKeyboard.setInput === "function") {
        window.currentKeyboard.setInput("");
    }
};

window.setupKeyboard = (dotNetHelper) => {
    if (window.currentKeyboard) {
        window.currentKeyboard.destroy();
        window.currentKeyboard = null;
    }

    const el = document.querySelector(".simple-keyboard");
    if (!el) return;

    const Keyboard = window.SimpleKeyboard.default;
    window.currentKeyboard = new Keyboard({
        onKeyPress: button => {
            if (button === "{enter}") {
                dotNetHelper.invokeMethodAsync('OnKeyboardEnter');
            } else if (button === "{backspace}") {
                window.keyboardInputBuffer = window.keyboardInputBuffer.slice(0, -1);
                dotNetHelper.invokeMethodAsync('OnKeyboardInput', window.keyboardInputBuffer);
            } else if (button.length === 1) {
                window.keyboardInputBuffer += button;
                dotNetHelper.invokeMethodAsync('OnKeyboardInput', window.keyboardInputBuffer);
            }
        },
        layout: {
            'default': [
                'q w e r t y u i o p',
                'a s d f g h j k l',
                'z x c v b n m {backspace}',
                '{enter}'
            ]
        },
        display: { '{enter}': '決定', '{backspace}': '⌫' }
    });
};

window.destroyKeyboard = () => {
    if (window.currentKeyboard) {
        window.currentKeyboard.destroy();
        window.currentKeyboard = null;
    }
    window.keyboardInputBuffer = "";
};