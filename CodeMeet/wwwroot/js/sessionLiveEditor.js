const editors = new Map();

function normalizeLanguage(lang) {
    if (!lang) return "csharp";
    const v = ("" + lang).trim().toLowerCase();
    if (v === "c#" || v === "csharp" || v === "c_sharp") return "csharp";
    if (v === "javascript" || v === "js") return "javascript";
    return "csharp";
}

function debounce(fn, ms) {
    let t = null;
    return (...args) => {
        if (t) clearTimeout(t);
        t = setTimeout(() => fn(...args), ms);
    };
}

async function ensureMonacoLoaded() {
    if (window.monacoLoaded && typeof window.monacoLoaded.then === "function") {
        await window.monacoLoaded;
    }

    if (window.monaco && window.monaco.editor) {
        return;
    }

    throw new Error("Monaco is not loaded. Ensure loader.js + editor.main are included and monacoLoaded is initialized.");
}

export async function create(elementId, value, language, dotNetRef) {
    const el = document.getElementById(elementId);
    if (!el) throw new Error(`Editor element not found: ${elementId}`);

    if (editors.has(elementId)) {
        return true;
    }

    await ensureMonacoLoaded();

    const lang = normalizeLanguage(language);

    const editor = window.monaco.editor.create(el, {
        value: value ?? "",
        language: lang,
        automaticLayout: true,
        minimap: { enabled: false },
        fontSize: 13,
        wordWrap: "on",
        scrollBeyondLastLine: false
    });

    const notify = debounce(async () => {
        try {
            const text = editor.getValue();
            await dotNetRef.invokeMethodAsync("OnEditorChanged", text);
        } catch {
        }
    }, 350);

    const sub = editor.onDidChangeModelContent(() => notify());

    editors.set(elementId, { editor, sub });

    return true;
}

export async function isReady() {
    try {
        await ensureMonacoLoaded();
        return true;
    } catch {
        return false;
    }
}

export function setValue(elementId, value) {
    const item = editors.get(elementId);
    if (!item) return;

    const model = item.editor.getModel();
    if (!model) {
        item.editor.setValue(value ?? "");
        return;
    }

    const text = value ?? "";

    if (model.getValue() === text) return;

    item.editor.pushUndoStop();
    model.pushEditOperations(
        [],
        [{ range: model.getFullModelRange(), text }],
        () => null
    );
    item.editor.pushUndoStop();
}

export function getValue(elementId) {
    const item = editors.get(elementId);
    if (!item) return "";
    return item.editor.getValue();
}

export function setLanguage(elementId, language) {
    const item = editors.get(elementId);
    if (!item) return;

    const lang = normalizeLanguage(language);
    const model = item.editor.getModel();
    if (model) window.monaco.editor.setModelLanguage(model, lang);
}

export function dispose(elementId) {
    const item = editors.get(elementId);
    if (!item) return;

    try { item.sub && item.sub.dispose(); } catch { }
    try { item.editor && item.editor.dispose(); } catch { }

    editors.delete(elementId);
}
