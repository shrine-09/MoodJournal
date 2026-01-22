var quill;

window.initQuill = function () {
    var el = document.getElementById("quillEditor");
    if (!el) return;

    quill = new Quill("#quillEditor", { theme: "snow" });
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
