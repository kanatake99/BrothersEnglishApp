window.setupKeyboard = (dotNetHelper) => {
    // 既存のインスタンスがあれば破棄（二重生成防止）
    if (window.currentKeyboard) {
        window.currentKeyboard.destroy();
    }

    const init = () => {
        const el = document.querySelector(".simple-keyboard");
        if (!el) return;

        const Keyboard = window.SimpleKeyboard.default;
        window.currentKeyboard = new Keyboard({
            // onChange は無限ループの元になりやすいので、
            // ボタンが押されたときだけ処理する方針に変える
            onKeyPress: button => {
                if (button === "{enter}") {
                    dotNetHelper.invokeMethodAsync('OnKeyboardEnter');
                } else if (button === "{backspace}") {
                    // C#側の入力を一文字消す処理を呼ぶか、JS側で処理して送る
                    handleBackspace(dotNetHelper);
                } else if (button.length === 1) { // 普通の文字（q, w, e...）
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

    // 入力処理用の変数をトップレベル（またはwindow）で保持
    window.keyboardInputBuffer = "";

    const handleInput = (helper, char) => {
        window.keyboardInputBuffer += char;
        helper.invokeMethodAsync('OnKeyboardInput', window.keyboardInputBuffer);
    };

    const handleBackspace = (helper) => {
        window.keyboardInputBuffer = window.keyboardInputBuffer.slice(0, -1);
        helper.invokeMethodAsync('OnKeyboardInput', window.keyboardInputBuffer);
    };

    // 公開：C#からキーボードをクリアできるようにする
    window.clearKeyboard = () => {
        window.keyboardInputBuffer = ""; // JS側のバッファをクリア
        if (window.currentKeyboard) {
            window.currentKeyboard.setInput(""); // 仮想キーボードの内部状態をクリア
        }
    };
    init();
};