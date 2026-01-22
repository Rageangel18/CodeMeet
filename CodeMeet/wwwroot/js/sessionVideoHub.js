(function () {
    let connection = null;
    let started = false;
    let currentSessionId = null;

    let videoCallback = null;
    let audioCallback = null;

    // для событий run
    let execDotNetRef = null;

    function ensureSignalR() {
        if (!window.signalR) {
            console.error("[sessionVideoHub] window.signalR is missing. Make sure SignalR script is loaded.");
            throw new Error("SignalR not loaded");
        }
    }

    function buildConnectionIfNeeded() {
        ensureSignalR();

        if (connection) return;

        connection = new signalR.HubConnectionBuilder()
            .withUrl("/hubs/session")
            .withAutomaticReconnect()
            .build();

        // видео
        connection.on("VideoFrameReceived", function (base64) {
            if (videoCallback && typeof videoCallback === "function") {
                videoCallback(base64);
            }
        });

        // аудио
        connection.on("AudioChunkReceived", function (dataUrl) {
            if (audioCallback && typeof audioCallback === "function") {
                audioCallback(dataUrl);
            }
        });

        // exec - run requested
        connection.on("RunRequested", function (payload) {
            try {
                if (execDotNetRef) {
                    execDotNetRef.invokeMethodAsync("OnRunRequested", JSON.stringify(payload));
                }
            } catch (e) {
                console.warn("[sessionVideoHub] RunRequested handler error", e);
            }
        });

        // exec - run completed
        connection.on("RunCompleted", function (payload) {
            try {
                if (execDotNetRef) {
                    execDotNetRef.invokeMethodAsync("OnRunCompleted", JSON.stringify(payload));
                }
            } catch (e) {
                console.warn("[sessionVideoHub] RunCompleted handler error", e);
            }
        });

        // exec - run failed
        connection.on("RunFailed", function (payload) {
            try {
                if (execDotNetRef) {
                    execDotNetRef.invokeMethodAsync("OnRunFailed", JSON.stringify(payload));
                }
            } catch (e) {
                console.warn("[sessionVideoHub] RunFailed handler error", e);
            }
        });
    }

    async function ensureStarted() {
        buildConnectionIfNeeded();

        if (started) return;

        await connection.start();
        started = true;
        console.log("[sessionVideoHub] connection started");
    }

    async function ensureJoined(sessionId) {
        await ensureStarted();

        if (currentSessionId === sessionId) return;

        currentSessionId = sessionId;

        try {
            await connection.invoke("JoinSession", sessionId);
            console.log("[sessionVideoHub] joined session", sessionId);
        } catch (err) {
            console.error("[sessionVideoHub] JoinSession failed", err);
            throw err;
        }
    }

    async function startAsync(sessionId, onVideoFrame, onAudioChunk) {
        videoCallback = onVideoFrame || null;
        audioCallback = onAudioChunk || null;

        await ensureJoined(sessionId);
    }

    async function stopAsync() {
        if (!connection || !started || !currentSessionId) {
            videoCallback = null;
            audioCallback = null;
            return;
        }

        try {
            await connection.invoke("LeaveSession", currentSessionId);
            console.log("[sessionVideoHub] left session", currentSessionId);
        } catch (err) {
            console.warn("[sessionVideoHub] LeaveSession error", err);
        }

        currentSessionId = null;
        videoCallback = null;
        audioCallback = null;
    }

    async function sendFrameAsync(dataUrl) {
        if (!connection || !started || !currentSessionId) return;

        try {
            await connection.invoke("SendVideoFrame", currentSessionId, dataUrl);
        } catch (err) {
            console.warn("[sessionVideoHub] SendVideoFrame error", err);
        }
    }

    async function sendAudioChunkAsync(dataUrl) {
        if (!connection || !started || !currentSessionId) return;

        try {
            await connection.invoke("SendAudioChunk", currentSessionId, dataUrl);
        } catch (err) {
            console.warn("[sessionVideoHub] SendAudioChunk error", err);
        }
    }

    function setExecDotNetRef(dotNetRef) {
        execDotNetRef = dotNetRef || null;
    }

    window.sessionVideoHub = {
        startAsync,
        stopAsync,
        sendFrameAsync,
        sendAudioChunkAsync,
        ensureJoined,
        setExecDotNetRef
    };
})();
