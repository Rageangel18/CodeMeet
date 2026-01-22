// wwwroot/js/auth.js
window.codeMeetAuth = {
    login: async function (email, password) {
        const response = await fetch('/api/auth/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include',
            body: JSON.stringify({ email, password })
        });

        if (!response.ok) {
            const text = await response.text();
            throw new Error(text || ('Login failed. HTTP ' + response.status));
        }
    },

    register: async function (email, displayName, password, confirmPassword) {
        const response = await fetch('/api/auth/register', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include',
            body: JSON.stringify({ email, displayName, password, confirmPassword })
        });

        if (!response.ok) {
            const text = await response.text();
            throw new Error(text || ('Registration failed. HTTP ' + response.status));
        }
    },

    logout: async function () {
        const response = await fetch('/api/auth/logout', {
            method: 'POST',
            credentials: 'include'
        });

        if (!response.ok) {
            const text = await response.text();
            throw new Error(text || ('Logout failed. HTTP ' + response.status));
        }
    }
};
