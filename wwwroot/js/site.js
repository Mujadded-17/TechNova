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

            // A form can carry several submit buttons that mean different things
            // (e.g. the admin Verify / Suspend actions share name="status"). The
            // browser only submits the clicked one's value, and a *disabled*
            // control is dropped entirely \u2014 so preserve the submitter's value in
            // a hidden field, and disable only on the next tick, after the form
            // has been serialised for submission.
            var submitter = e.submitter;
            if (submitter && submitter.name) {
                var keep = document.createElement("input");
                keep.type = "hidden";
                keep.name = submitter.name;
                keep.value = submitter.value;
                keep.setAttribute("data-guard-keep", "");
                form.appendChild(keep);
            }

            var buttons = form.querySelectorAll('button[type="submit"], input[type="submit"]');

            setTimeout(function () {
                Array.prototype.forEach.call(buttons, function (btn) {
                    btn.disabled = true;
                    if (btn.tagName === "BUTTON" && btn === submitter && !btn.dataset.keepLabel) {
                        btn.dataset.originalLabel = btn.innerHTML;
                        btn.innerHTML = btn.dataset.busyLabel || "Please wait\u2026";
                    }
                });
            }, 0);

            // If the browser stays on the page (validation error, back nav) release the lock.
            setTimeout(function () {
                form.dataset.submitting = "false";
                Array.prototype.forEach.call(buttons, function (btn) {
                    btn.disabled = false;
                    if (btn.dataset.originalLabel) btn.innerHTML = btn.dataset.originalLabel;
                });
                var kept = form.querySelector('input[data-guard-keep]');
                if (kept) kept.remove();
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
