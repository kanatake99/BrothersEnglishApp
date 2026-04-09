/**
 * 汎用・UIヘルパー (speechHandlers)
 */
window.speechHandlers = {
    // 読み上げ
    speak: function (text, rate = 1.0) {
        if (!text) return;
        window.speechSynthesis.cancel();
        const utterance = new SpeechSynthesisUtterance(text);
        utterance.lang = 'en-US';
        utterance.rate = rate;
        window.speechSynthesis.speak(utterance);
    },
    // フォーカス操作
    focusElement: function (element) {
        if (element && typeof element.focus === 'function') {
            element.focus();
        }
    },
    // フォーカス解除
    blurActiveElement: function () {
        if (document.activeElement && typeof document.activeElement.blur === 'function') {
            document.activeElement.blur();
        }
    },
    // ブラウザ情報取得
    getUserAgent: () => navigator.userAgent || "",
    // 要素の存在チェック
    elementExists: (selector) => document.querySelector(selector) !== null
};

/**
 * アプリ共通操作 (appFunctions)
 */
window.appFunctions = {
    scrollToElement: function (elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            const offset = 70;
            const elementPosition = element.getBoundingClientRect().top + window.pageYOffset;
            window.scrollTo({ top: elementPosition - offset, behavior: 'smooth' });
        }
    },
    scrollToTop: function () {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    },
    playAudio: function (path) {
        const audio = new Audio(path);
        audio.play().catch(e => console.error("Audio play error:", e));
    }
};

/**
 * キーボード操作 (keyboardHandlers)
 */
window.keyboardHandlers = {
    current: null,

    setup: function (dotNetHelper) {
        if (this.current) {
            this.current.destroy();
        }

        const el = document.querySelector(".simple-keyboard");
        if (!el || !window.SimpleKeyboard) return;

        const Keyboard = window.SimpleKeyboard.default;
        this.current = new Keyboard({
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
    },

    destroy: function () {
        if (this.current) {
            this.current.destroy();
            this.current = null;
        }
    },

    clear: function () {
        if (this.current) {
            this.current.setInput("");
        }
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