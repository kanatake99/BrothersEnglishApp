// wwwroot/js/app.js

// ブラウザの読み上げ機能を呼び出す関数
window.speechHandlers = {
    speak: function (text) {
        if (!text) return;
        window.speechSynthesis.cancel();
        const msg = new SpeechSynthesisUtterance(text);
        msg.lang = 'en-US';
        window.speechSynthesis.speak(msg);
    },

    // 指定した要素にフォーカスを当てる（Training用）
    focusElement: function (element) {
        if (element && typeof element.focus === 'function') {
            element.focus();
        }
    },
    // UserAgent取得
    getUserAgent: () => navigator.userAgent,

    // 要素存在チェック
    elementExists: (selector) => document.querySelector(selector) !== null,


    // 現在のフォーカスを解除する（Studyのボタン選択時用）
    blurActiveElement: function () {
        if (document.activeElement && typeof document.activeElement.blur === 'function') {
            document.activeElement.blur();
        }
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

window.quizHandlers = {
    resetButtons: () => {
        // 全てのクイズボタンからフォーカスを外す
        const buttons = document.querySelectorAll('.quiz-button');
        buttons.forEach(btn => btn.blur());
    }
};