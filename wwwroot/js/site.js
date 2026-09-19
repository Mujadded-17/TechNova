/* =========================================================
   Tech Nova — shared behaviour for the signed-in app shell.

   1. Double-submit guard: any <form> submit disables its submit
      buttons and swaps the label, so a slow request cannot be
      posted twice. Opt out with data-no-submit-guard on the form.
   2. Auto-dismiss the post-redirect alerts after a few seconds.
   ========================================================= */
(function () {
    "use strict";

    function guardForms() {
        document.addEventListener("submit", function (e) {
            var form = e.target;
            if (!(form instanceof HTMLFormElement) || form.hasAttribute("data-no-submit-guard")) return;
            if (e.defaultPrevented) return;   // client-side validation blocked it
            if (form.dataset.submitting === "true") { e.preventDefault(); return; }

            form.dataset.submitting = "true";

            var buttons = form.querySelectorAll('button[type="submit"], input[type="submit"]');
            Array.prototype.forEach.call(buttons, function (btn) {
                btn.disabled = true;
                if (btn.tagName === "BUTTON" && !btn.dataset.keepLabel) {
                    btn.dataset.originalLabel = btn.innerHTML;
                    btn.innerHTML = btn.dataset.busyLabel || "Please wait\u2026";
                }
            });

            // If the browser stays on the page (validation error, back nav) release the lock.
            setTimeout(function () {
                form.dataset.submitting = "false";
                Array.prototype.forEach.call(buttons, function (btn) {
                    btn.disabled = false;
                    if (btn.dataset.originalLabel) btn.innerHTML = btn.dataset.originalLabel;
                });
            }, 8000);
        });
    }

    function autoDismissAlerts() {
        var alerts = document.querySelectorAll(".app-main > .container > .alert.alert-dismissible");
        Array.prototype.forEach.call(alerts, function (el) {
            setTimeout(function () {
                if (window.bootstrap && bootstrap.Alert) {
                    bootstrap.Alert.getOrCreateInstance(el).close();
                } else {
                    el.remove();
                }
            }, 6000);
        });
    }

    function init() {
        guardForms();
        autoDismissAlerts();
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();
