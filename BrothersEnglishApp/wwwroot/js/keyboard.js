/**
 * wwwroot/js/keyboard.js
 * 仮想キーボードの状態管理用グローバル変数
 */
window.keyboardInputBuffer = "";
window.currentKeyboard = null;

/**
 * 入力バッファとキーボード本体をクリアする
 */
window.clearKeyboard = () => {
    window.keyboardInputBuffer = "";
    if (window.currentKeyboard) {
        window.currentKeyboard.setInput("");
    }
};

/**
 * キーボードの初期化関数
 * 単語でも文でも使える「フルセット」レイアウトだぜ！
 */
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
            } else if (button === "{space}") {
                window.keyboardInputBuffer += " ";
                dotNetHelper.invokeMethodAsync('OnKeyboardInput', window.keyboardInputBuffer);
            } else {
                // 通常の文字入力（アポストロフィ含む）
                window.keyboardInputBuffer += button;
                dotNetHelper.invokeMethodAsync('OnKeyboardInput', window.keyboardInputBuffer);
            }
        },
        layout: {
            'default': [
                'q w e r t y u i o p',
                'a s d f g h j k l \'', // アポストロフィを追加
                'z x c v b n m {backspace}',
                '{space} {enter}' // スペースと決定を最下段に
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