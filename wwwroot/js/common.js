(function (global) {
  "use strict";

  var API_BASE = "/api/v1";

  function escapeHtml(value) {
    return String(value == null ? "" : value).replace(/[&<>'"]/g, function (char) {
      return { "&": "&amp;", "<": "&lt;", ">": "&gt;", "'": "&#39;", '"': "&quot;" }[char];
    });
  }

  function toast(message) {
    var el = document.getElementById("toast");
    el.textContent = message;
    el.classList.add("show");
    setTimeout(function () {
      el.classList.remove("show");
    }, 5000);
  }

  global.Argo = {
    API_BASE: API_BASE,
    escapeHtml: escapeHtml,
    toast: toast
  };
})(window);
