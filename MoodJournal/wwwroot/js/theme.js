(function () {
    const KEY = "mj_theme";

    function apply(theme) {
        //theme = "dark" | "light"
        document.documentElement.setAttribute("data-theme", theme);
    }

    window.getTheme = function () {
        try {
            return localStorage.getItem(KEY) || "light";
        } catch {
            return "light";
        }
    };

    window.setTheme = function (theme) {
        try {
            localStorage.setItem(KEY, theme);
        } catch { }
        apply(theme);
    };

    window.toggleTheme = function () {
        const current = window.getTheme();
        const next = current === "dark" ? "light" : "dark";
        window.setTheme(next);
        return next;
    };

    //applying automatically on load
    document.addEventListener("DOMContentLoaded", () => {
        apply(window.getTheme());
    });
})();
