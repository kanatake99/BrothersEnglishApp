/**
 * wwwroot/js/keyboard.js
 */
window.currentKeyboard = null;

// バッファ管理用の変数は削除してOK（トラブルの元だぜ！）

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
            // 文字列を組み立てずに、押された「ボタン名」だけをそのまま送る！
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

// 破棄もシンプルに
window.destroyKeyboard = () => {
    if (window.currentKeyboard) {
        window.currentKeyboard.destroy();
        window.currentKeyboard = null;
    }
};

// クリア関数も、C#側で UserInput="" にすれば済むから、JS側は何もしなくてよくなるぜ
window.clearKeyboard = () => {
    if (window.currentKeyboard) {
        window.currentKeyboard.setInput("");
    }
};