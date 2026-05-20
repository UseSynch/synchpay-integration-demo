(function () {
    var storageKey = "synchPayStatusEncryptionKey";

    function setEncryptionKey(value) {
        if (value) {
            window.sessionStorage.setItem(storageKey, value);
        } else {
            window.sessionStorage.removeItem(storageKey);
        }
    }

    function getEncryptionKey() {
        return window.sessionStorage.getItem(storageKey) || "";
    }

    function base64ToBytes(value) {
        var normalized = (value || "").trim().replace(/ /g, "+");

        if (normalized.indexOf("-") !== -1 || normalized.indexOf("_") !== -1) {
            normalized = normalized.replace(/-/g, "+").replace(/_/g, "/");
            while (normalized.length % 4) {
                normalized += "=";
            }
        }

        var binary = window.atob(normalized);
        var bytes = new Uint8Array(binary.length);

        for (var i = 0; i < binary.length; i += 1) {
            bytes[i] = binary.charCodeAt(i);
        }

        return bytes;
    }

    async function decryptStatus(encryptedData, encryptionKey) {
        if (!encryptedData) {
            throw new Error("Payment status data was not provided.");
        }

        if (!encryptionKey) {
            throw new Error("Status encryption key is required.");
        }

        var encryptedPayload = base64ToBytes(encryptedData);

        if (encryptedPayload.length <= 28) {
            throw new Error("Payment status data is not a valid encrypted payload.");
        }

        var nonce = encryptedPayload.slice(0, 12);
        var tag = encryptedPayload.slice(12, 28);
        var ciphertext = encryptedPayload.slice(28);
        var ciphertextWithTag = new Uint8Array(ciphertext.length + tag.length);
        ciphertextWithTag.set(ciphertext, 0);
        ciphertextWithTag.set(tag, ciphertext.length);

        var keyHash = await window.crypto.subtle.digest("SHA-256", new TextEncoder().encode(encryptionKey));
        var cryptoKey = await window.crypto.subtle.importKey("raw", keyHash, { name: "AES-GCM" }, false, ["decrypt"]);
        var plaintext = await window.crypto.subtle.decrypt({ name: "AES-GCM", iv: nonce, tagLength: 128 }, cryptoKey, ciphertextWithTag);
        var json = new TextDecoder().decode(plaintext);

        return JSON.stringify(JSON.parse(json), null, 2);
    }

    async function decryptPageStatus() {
        var keyInput = document.getElementById("StatusEncryptionKeyInput");
        var dataInput = document.getElementById("EncryptedDataInput");
        var errorPanel = document.getElementById("ErrorPanel");
        var placeholder = document.getElementById("StatusPlaceholder");
        var statusJson = document.getElementById("StatusJson");

        if (!keyInput || !dataInput || !errorPanel || !placeholder || !statusJson) {
            return;
        }

        errorPanel.hidden = true;
        statusJson.hidden = true;
        placeholder.hidden = false;

        try {
            setEncryptionKey(keyInput.value);
            statusJson.textContent = await decryptStatus(dataInput.value, keyInput.value);
            statusJson.hidden = false;
            placeholder.hidden = true;
        } catch (error) {
            errorPanel.textContent = error.message;
            errorPanel.hidden = false;
        }
    }

    window.synchPayStatus = {
        setEncryptionKey: setEncryptionKey,
        getEncryptionKey: getEncryptionKey,
        decryptStatus: decryptStatus
    };

    document.addEventListener("DOMContentLoaded", function () {
        var keyInput = document.getElementById("StatusEncryptionKeyInput");
        var dataInput = document.getElementById("EncryptedDataInput");
        var decryptButton = document.getElementById("DecryptStatusButton");

        if (!keyInput || !dataInput || !decryptButton) {
            return;
        }

        keyInput.value = getEncryptionKey();
        decryptButton.addEventListener("click", decryptPageStatus);

        if (keyInput.value && dataInput.value) {
            decryptPageStatus();
        }
    });
}());
