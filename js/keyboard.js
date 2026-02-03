/**
 * 仮想キーボードの状態管理用グローバル変数
 */
window.keyboardInputBuffer = "";
window.currentKeyboard = null;

/**
 * C#からいつでも呼び出し可能なクリア関数
 * setupKeyboardの実行状況に関わらず、関数自体は常に存在する
 */
window.clearKeyboard = () => {
    window.keyboardInputBuffer = "";
    if (window.currentKeyboard && typeof window.currentKeyboard.setInput === "function") {
        window.currentKeyboard.setInput("");
    }
};

/**
 * キーボードの初期化関数
 */
window.setupKeyboard = (dotNetHelper) => {
    // 既存のインスタンスがあれば確実に破棄
    if (window.currentKeyboard) {
        window.currentKeyboard.destroy();
        window.currentKeyboard = null;
    }

    const init = () => {
        const el = document.querySelector(".simple-keyboard");
        if (!el) return;

        const Keyboard = window.SimpleKeyboard.default;
        window.currentKeyboard = new Keyboard({
            onKeyPress: button => {
                if (button === "{enter}") {
                    dotNetHelper.invokeMethodAsync('OnKeyboardEnter');
                } else if (button === "{backspace}") {
                    handleBackspace(dotNetHelper);
                } else if (button.length === 1) {
                    handleInput(dotNetHelper, button);
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

    const handleInput = (helper, char) => {
        window.keyboardInputBuffer += char;
        helper.invokeMethodAsync('OnKeyboardInput', window.keyboardInputBuffer);
    };

    const handleBackspace = (helper) => {
        window.keyboardInputBuffer = window.keyboardInputBuffer.slice(0, -1);
        helper.invokeMethodAsync('OnKeyboardInput', window.keyboardInputBuffer);
    };

    init();
};