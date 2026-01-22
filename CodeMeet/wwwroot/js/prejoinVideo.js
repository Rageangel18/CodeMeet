// prejoinVideo.js
// Только превью камеры, без SignalR / отправки кадров.

window.prejoinVideo = (function () {
    let previewStream = null;

    async function startPreview(deviceId) {
        const videoEl = document.getElementById("prejoinVideo");
        if (!videoEl) {
            console.warn("[prejoinVideo] prejoinVideo element not found");
            return;
        }

        // Отключаем старый стрим, если был
        if (previewStream) {
            previewStream.getTracks().forEach(t => t.stop());
            previewStream = null;
        }

        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            alert("Your browser does not support getUserMedia");
            return;
        }

        const constraints = {
            video: deviceId
                ? { deviceId: { exact: deviceId } }
                : true,
            audio: false
        };

        try {
            previewStream = await navigator.mediaDevices.getUserMedia(constraints);
            videoEl.srcObject = previewStream;
            const playPromise = videoEl.play();
            if (playPromise && playPromise.catch) {
                playPromise.catch(() => { });
            }
        } catch (err) {
            console.error("[prejoinVideo] Error getting media:", err);
            alert("Could not access camera: " + err.message);
        }
    }

    async function changeCamera(deviceId) {
        await startPreview(deviceId);
    }

    async function stopPreview() {
        if (previewStream) {
            previewStream.getTracks().forEach(t => t.stop());
            previewStream = null;
        }
    }

    return {
        startPreview,
        changeCamera,
        stopPreview
    };
})();
