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


// スクロール操作を行う関数
window.appFunctions = {
    // 指定したIDの要素までスクロール
    scrollToElement: function (elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            // ヘッダーが固定されている場合は、その分少し上に余裕を持たせる
            const offset = 70;
            const bodyRect = document.body.getBoundingClientRect().top;
            const elementRect = element.getBoundingClientRect().top;
            const elementPosition = elementRect - bodyRect;
            const offsetPosition = elementPosition - offset;

            window.scrollTo({
                top: offsetPosition,
                behavior: 'smooth'
            });
        }
    },
    // 一番上までスクロール
    scrollToTop: function () {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    }
};
