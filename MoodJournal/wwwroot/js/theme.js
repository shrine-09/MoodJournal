(function () {
    const KEY = "mj_theme";

    function apply(theme) {
        document.documentElement.setAttribute("data-theme", theme);

        document.documentElement.style.colorScheme = theme === "dark" ? "dark" : "light";
    }

    function safeGet() {
        try {
            return localStorage.getItem(KEY) || "light";
        } catch {
            return "light";
        }
    }

    function safeSet(theme) {
        try {
            localStorage.setItem(KEY, theme);
        } catch { }
    }

    window.getTheme = function () {
        return safeGet();
    };

    window.setTheme = function (theme) {
        safeSet(theme);
        apply(theme);
    };

    window.toggleTheme = function () {
        const current = safeGet();
        const next = current === "dark" ? "light" : "dark";
        safeSet(next);
        apply(next);
        return next;
    };

    window.moodJournalTheme = {
        getTheme: () => safeGet(),
        setTheme: (theme) => {
            safeSet(theme);
            apply(theme);
        },
        toggleTheme: () => window.toggleTheme()
    };

    document.addEventListener("DOMContentLoaded", () => {
        apply(safeGet());
    });
})();
