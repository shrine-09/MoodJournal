var quill = null;

window.initQuill = function () {
    var host = document.getElementById("quillEditor");
    if (!host) return;

    if (host.__quill) {
        quill = host.__quill;
        return;
    }

    var wrapper = host.closest(".editor-card") || host.parentElement;
    if (wrapper) {
        wrapper.querySelectorAll(".ql-toolbar").forEach(function (t) { t.remove(); });
    }

    var clean = host.cloneNode(false);
    clean.innerHTML = "";
    host.parentNode.replaceChild(clean, host);

    var toolbar = [
        [{ header: 1 }, { header: 2 }, { header: 3 }, { header: false }],
        ["bold", "italic", "underline", "strike"],
        [{ list: "ordered" }, { list: "bullet" }],
        [{ indent: "-1" }, { indent: "+1" }],
        [{ align: [] }],
        ["blockquote", "code-block", "link"],
        ["clean"]
    ];

    quill = new Quill(clean, {
        theme: "snow",
        modules: { toolbar: toolbar }
    });
};

window.getQuillContent = function () {
    if (!quill) return "";
    return quill.root.innerHTML;
};

window.setQuillContent = function (html) {
    if (!quill) return;
    quill.root.innerHTML = html || "";
};

window.getQuillText = function () {
    if (!quill) return "";
    return (quill.getText() || "").trim();
};

window.setQuillReadOnly = function (isReadOnly) {
    if (!quill) return;
    quill.enable(!isReadOnly);
};
