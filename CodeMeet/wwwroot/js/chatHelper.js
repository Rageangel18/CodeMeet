window.chatHelper = {
    scrollToBottom: function (element) {
        if (!element) return;
        try {
            element.scrollTop = element.scrollHeight;
        } catch { }
    }
};
