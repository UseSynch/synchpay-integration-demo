window.synchPayStatus = {
    setEncryptionKey: function (value) {
        if (value) {
            window.sessionStorage.setItem("synchPayStatusEncryptionKey", value);
        } else {
            window.sessionStorage.removeItem("synchPayStatusEncryptionKey");
        }
    },
    getEncryptionKey: function () {
        return window.sessionStorage.getItem("synchPayStatusEncryptionKey") || "";
    }
};
