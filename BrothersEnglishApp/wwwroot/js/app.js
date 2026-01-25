// wwwroot/js/app.js

// ブラウザの読み上げ機能を呼び出す関数
window.speechHandlers = {
    speak: function (text) {
        if (!text) return;

        // 前回の読み上げがあればキャンセル（声が重ならないように）
        window.speechSynthesis.cancel();

        const msg = new SpeechSynthesisUtterance(text);
        msg.lang = 'en-US';
        window.speechSynthesis.speak(msg);
    },

    // 他にもJSが必要な処理（フォーカス移動など）があればここに追加できる
    focusElement: function (element) {
        if (element) element.focus();
    }
};