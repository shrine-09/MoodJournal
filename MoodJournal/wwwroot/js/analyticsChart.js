(function () {
    const store = (window.mjCharts = window.mjCharts || {});

    function getCanvas(id) {
        const el = document.getElementById(id);
        if (!el) return null;
        return el.getContext("2d");
    }

    function ensureChart(id, config) {
        const ctx = getCanvas(id);
        if (!ctx) return;

        if (store[id]) {
            store[id].data.labels = config.data.labels;
            store[id].data.datasets = config.data.datasets;
            store[id].options = config.options || store[id].options;
            store[id].update();
            return;
        }

        store[id] = new Chart(ctx, config);
    }

    window.mjChartsInitOrUpdate = function (chartId, chartType, labels, data, label, optionsJson) {
        const options = optionsJson ? JSON.parse(optionsJson) : {};

        const config = {
            type: chartType,
            data: {
                labels: labels,
                datasets: [
                    {
                        label: label || "",
                        data: data,
                        borderWidth: 1,
                        tension: 0.25
                    }
                ]
            },
            options: Object.assign(
                {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: { display: chartType !== "bar" },
                        tooltip: { enabled: true }
                    },
                    scales: chartType === "pie" || chartType === "doughnut" ? {} : {
                        y: { beginAtZero: true }
                    }
                },
                options
            )
        };

        ensureChart(chartId, config);
    };

    window.mjChartsDestroy = function (chartId) {
        if (store[chartId]) {
            store[chartId].destroy();
            delete store[chartId];
        }
    };
})();
