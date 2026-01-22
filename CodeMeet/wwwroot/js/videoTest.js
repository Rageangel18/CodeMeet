// videoTest.js
// Зависит от sessionVideoHub.js и signalr.js

window.videoTest = (function () {
    const FRAME_INTERVAL_MS = 250;    // ~4 fps
    const AUDIO_CHUNK_MS = 500;       // длительность одного аудио-чанка
    const AUDIO_MIME = "audio/webm";  // без codecs, чтобы меньше шансов на NotSupported

    let localVideoEl = null;
    let remoteImgEl = null;
    let remoteAudioEl = null;

    let mediaStream = null;      // общий (видео + аудио)
    let audioStream = null;      // отдельный стрим только из аудио-треков
    let audioTracks = [];

    let captureIntervalId = null;

    // для аудио
    let audioLoopActive = false;
    let currentAudioRecorder = null;
    let localMuted = false;

    function ensureElements() {
        if (!localVideoEl) {
            localVideoEl = document.getElementById("localVideo");
        }
        if (!remoteImgEl) {
            remoteImgEl = document.getElementById("remoteVideo");
        }
        if (!remoteAudioEl) {
            remoteAudioEl = document.getElementById("remoteAudio");
        }
    }

    async function start(sessionId, deviceId) {
        ensureElements();

        if (!window.sessionVideoHub || typeof window.sessionVideoHub.startAsync !== "function") {
            console.error("[videoTest] sessionVideoHub is not available");
            return;
        }

        // Подключаемся к хабу и подписываемся на видео+аудио
        await window.sessionVideoHub.startAsync(
            sessionId,
            function onFrame(base64) {
                if (remoteImgEl) {
                    remoteImgEl.src = base64;
                }
            },
            function onAudioChunk(dataUrl) {
                playRemoteAudioChunk(dataUrl);
            }
        );

        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            alert("Your browser does not support getUserMedia");
            return;
        }

        const constraints = {
            video: deviceId ? { deviceId: { exact: deviceId } } : true,
            audio: true
        };

        try {
            mediaStream = await navigator.mediaDevices.getUserMedia(constraints);

            // локальное видео
            if (localVideoEl) {
                localVideoEl.srcObject = mediaStream;
                const playPromise = localVideoEl.play();
                if (playPromise && playPromise.catch) {
                    playPromise.catch(() => { });
                }
            }

            // отдельный стрим только из аудио-треков
            audioTracks = mediaStream.getAudioTracks();
            if (audioTracks.length > 0) {
                audioStream = new MediaStream(audioTracks);
                startAudioLoop();
            } else {
                console.warn("[videoTest] no audio tracks in mediaStream");
            }
        } catch (err) {
            console.error("[videoTest] Error getting media:", err);
            alert("Could not access camera/microphone: " + err.message);
            return;
        }

        // отправка кадров видео
        const canvas = document.createElement("canvas");
        const ctx = canvas.getContext("2d");

        captureIntervalId = setInterval(() => {
            if (!localVideoEl || localVideoEl.readyState < 2) return;

            const width = localVideoEl.videoWidth;
            const height = localVideoEl.videoHeight;
            if (!width || !height) return;

            const targetWidth = 320;
            const ratio = width / height;
            const targetHeight = Math.round(targetWidth / ratio);

            canvas.width = targetWidth;
            canvas.height = targetHeight;

            ctx.drawImage(localVideoEl, 0, 0, targetWidth, targetHeight);

            const dataUrl = canvas.toDataURL("image/webp", 0.4);

            if (window.sessionVideoHub && typeof window.sessionVideoHub.sendFrameAsync === "function") {
                window.sessionVideoHub.sendFrameAsync(dataUrl);
            }
        }, FRAME_INTERVAL_MS);
    }

    // --------------------- АУДИО-ЛУП ---------------------

    function startAudioLoop() {
        if (!audioStream) return;
        if (audioLoopActive) return;

        audioLoopActive = true;
        runAudioRecorderOnce();
    }

    function runAudioRecorderOnce() {
        if (!audioLoopActive || !audioStream) return;

        let options = {};
        try {
            if (typeof MediaRecorder !== "undefined" &&
                MediaRecorder.isTypeSupported &&
                MediaRecorder.isTypeSupported(AUDIO_MIME)) {
                options.mimeType = AUDIO_MIME;
            }
        } catch {
            // оставляем options пустым
        }

        let recorder;
        try {
            recorder = new MediaRecorder(audioStream, options);
        } catch (err) {
            console.warn("[videoTest] MediaRecorder init failed, audio disabled:", err);
            audioLoopActive = false;
            return;
        }

        currentAudioRecorder = recorder;

        recorder.ondataavailable = (ev) => {
            if (!ev.data || ev.data.size === 0) return;
            if (localMuted) return;

            if (!window.sessionVideoHub || typeof window.sessionVideoHub.sendAudioChunkAsync !== "function") {
                return;
            }

            const reader = new FileReader();
            reader.onloadend = () => {
                const dataUrl = reader.result;
                if (typeof dataUrl === "string") {
                    window.sessionVideoHub.sendAudioChunkAsync(dataUrl);
                }
            };
            reader.readAsDataURL(ev.data);
        };

        recorder.onerror = (ev) => {
            console.warn("[videoTest] mediaRecorder error", ev.error || ev);
        };

        recorder.onstop = () => {
            currentAudioRecorder = null;
            if (!audioLoopActive) return;
            // сразу запускаем следующий рекордер
            setTimeout(runAudioRecorderOnce, 0);
        };

        try {
            recorder.start();
            // Останавливаем через AUDIO_CHUNK_MS,
            // чтобы каждый чанк был отдельным WebM-файлом.
            setTimeout(() => {
                if (recorder.state === "recording") {
                    try {
                        recorder.stop();
                    } catch { }
                }
            }, AUDIO_CHUNK_MS);
        } catch (err) {
            console.warn("[videoTest] mediaRecorder.start failed, audio disabled:", err);
            audioLoopActive = false;
            currentAudioRecorder = null;
        }
    }

    function stopAudioLoop() {
        audioLoopActive = false;
        if (currentAudioRecorder && currentAudioRecorder.state === "recording") {
            try {
                currentAudioRecorder.stop();
            } catch { }
        }
        currentAudioRecorder = null;
    }

    function playRemoteAudioChunk(dataUrl) {
        ensureElements();
        if (!remoteAudioEl) {
            console.warn("[videoTest] remoteAudio element not found");
            return;
        }

        remoteAudioEl.src = dataUrl;

        const p = remoteAudioEl.play();
        if (p && p.catch) {
            p.catch((e) => {
                console.warn("[videoTest] remote audio play error", e);
            });
        }
    }

    // --------------------- ПУБЛИЧНЫЕ API ---------------------

    async function stop() {
        if (captureIntervalId) {
            clearInterval(captureIntervalId);
            captureIntervalId = null;
        }

        stopAudioLoop();

        if (mediaStream) {
            mediaStream.getTracks().forEach(t => t.stop());
            mediaStream = null;
        }

        audioStream = null;
        audioTracks = [];

        if (remoteAudioEl) {
            try {
                remoteAudioEl.removeAttribute("src");
                remoteAudioEl.load();
            } catch { }
        }

        if (window.sessionVideoHub && typeof window.sessionVideoHub.stopAsync === "function") {
            await window.sessionVideoHub.stopAsync();
        }
    }

    function setMuted(muted) {
        localMuted = !!muted;

        if (audioTracks && audioTracks.length > 0) {
            audioTracks.forEach(t => {
                t.enabled = !localMuted;
            });
        }
    }

    async function getVideoDevices() {
        if (!navigator.mediaDevices || !navigator.mediaDevices.enumerateDevices) {
            return [];
        }

        let devices = await navigator.mediaDevices.enumerateDevices();
        let cameras = devices.filter(d => d.kind === "videoinput");

        const hasLabels = cameras.some(d => d.label && d.label.length > 0);

        if (!hasLabels) {
            try {
                const tmpStream = await navigator.mediaDevices.getUserMedia({
                    video: true,
                    audio: false
                });
                tmpStream.getTracks().forEach(t => t.stop());

                devices = await navigator.mediaDevices.enumerateDevices();
                cameras = devices.filter(d => d.kind === "videoinput");
            } catch (e) {
                console.warn("[videoTest] getVideoDevices permission error", e);
            }
        }

        return cameras.map((d, i) => ({
            deviceId: d.deviceId,
            label: d.label || `Camera ${i + 1}`
        }));
    }

    return {
        start,
        stop,
        getVideoDevices,
        setMuted
    };
})();
