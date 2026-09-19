/* =========================================================
   Tech Nova — auth pages.
   Password visibility toggles, injected rather than hand-written
   into every password field.
   ========================================================= */
(function () {
    "use strict";

    function icon(name) {
        return '<svg class="ico ico--sm"><use href="#i-' + name + '" /></svg>';
    }

    function guardForms() {
        // One submit per click: the auth pages have no client-side
        // validation library, so the guard is released if the browser
        // blocks the submit (e.g. a required field is empty).
        document.addEventListener("submit", function (e) {
            var form = e.target;
            if (!(form instanceof HTMLFormElement)) return;
            if (e.defaultPrevented) return;   // client-side validation blocked it
            if (form.dataset.submitting === "true") { e.preventDefault(); return; }
            form.dataset.submitting = "true";

            var buttons = form.querySelectorAll('button[type="submit"]');
            Array.prototype.forEach.call(buttons, function (btn) { btn.disabled = true; });

            setTimeout(function () {
                form.dataset.submitting = "false";
                Array.prototype.forEach.call(buttons, function (btn) { btn.disabled = false; });
            }, 8000);
        });
    }

    function init() {
        guardForms();

        var fields = document.querySelectorAll(".field--password");

        Array.prototype.forEach.call(fields, function (field) {
            var input = field.querySelector("input");
            if (!input) return;

            var btn = document.createElement("button");
            btn.type = "button";                 // never submits the form
            btn.className = "pw-toggle";
            btn.innerHTML = icon("eye");
            btn.setAttribute("aria-label", "Show password");

            btn.addEventListener("click", function () {
                var hidden = input.type === "password";
                input.type = hidden ? "text" : "password";
                btn.innerHTML = icon(hidden ? "eye-off" : "eye");
                btn.setAttribute(
                    "aria-label",
                    hidden ? "Hide password" : "Show password"
                );
                input.focus({ preventScroll: true });
            });

            field.appendChild(btn);
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();
