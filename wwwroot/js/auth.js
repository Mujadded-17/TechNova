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

    function init() {
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
