// wwwroot/js/app.js

/**
 * 汎用ヘルパー (speechHandlers)
 */
window.speechHandlers = {
    speak: function (text, rate = 1.0) {
        if (!text) return;
        window.speechSynthesis.cancel();
        const utterance = new SpeechSynthesisUtterance(text);
        utterance.lang = 'en-US';
        utterance.rate = rate;
        window.speechSynthesis.speak(utterance);
    },
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
    getUserAgent: () => navigator.userAgent || "",
    elementExists: (selector) => document.querySelector(selector) !== null
};

/**
 * アプリ共通操作 (appFunctions)
 * ここにスクロールと音声再生
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
    },
    playAudio: function (path) {
        // パスの先頭にスラッシュがない場合などを考慮して調整
        var audio = new Audio(path);
        audio.play().catch(e => console.error("Audio play error:", e));
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

// --- 以下、キーボード関連のコード ---
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
    if (!window.SimpleKeyboard) return;
    const Keyboard = window.SimpleKeyboard.default;
    window.currentKeyboard = new Keyboard({
        onKeyPress: button => {
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
};