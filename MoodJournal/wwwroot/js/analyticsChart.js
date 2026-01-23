(function () {
    window.mjCharts = window.mjCharts || {};

    window.mjChartsDestroy = function (id) {
        try {
            if (window.mjCharts[id]) {
                window.mjCharts[id].destroy();
                delete window.mjCharts[id];
            }
        } catch (e) { }
    };

    window.mjChartsInitOrUpdate = function (canvasId, type, labels, data, label, optionsJson) {
        // If Chart.js isn't available, don't crash the app.
        if (!window.Chart) return;

        const el = document.getElementById(canvasId);
        if (!el) return;

        // destroy old chart if exists
        if (window.mjCharts[canvasId]) {
            window.mjCharts[canvasId].destroy();
            delete window.mjCharts[canvasId];
        }

        let parsedOptions = {};
        try { parsedOptions = optionsJson ? JSON.parse(optionsJson) : {}; } catch (e) { parsedOptions = {}; }

        const cfg = {
            type: type,
            data: {
                labels: labels || [],
                datasets: [{
                    label: label || "",
                    data: data || []
                }]
            },
            options: Object.assign({
                responsive: true,
                maintainAspectRatio: false
            }, parsedOptions)
        };

        const ctx = el.getContext("2d");
        window.mjCharts[canvasId] = new Chart(ctx, cfg);
    };
})();
