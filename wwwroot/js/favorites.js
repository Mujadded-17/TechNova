/* =========================================================
   Tech Nova — save / unsave startups from the card grids.

   Posts a normal form-encoded request with the anti-forgery
   token (the controller is [ValidateAntiForgeryToken]) and
   updates the heart in place. On the Favorites page the card
   is removed instead, since it no longer belongs there.
   ========================================================= */
(function () {
    "use strict";

    var ENDPOINT = "/Investor/ToggleFavorite";

    function token() {
        var el = document.querySelector('input[name="__RequestVerificationToken"]');
        return el ? el.value : "";
    }

    function notify(message, isError) {
        var n = document.createElement("div");
        n.className = "alert " + (isError ? "alert-danger" : "alert-success") + " app-toast";
        n.setAttribute("role", "status");
        n.textContent = message;
        document.body.appendChild(n);
        setTimeout(function () { n.classList.add("app-toast--hide"); }, 2200);
        setTimeout(function () { n.remove(); }, 2600);
    }

    function setState(btn, saved) {
        btn.classList.toggle("favorited", saved);
        btn.textContent = saved ? "♥" : "♡";
        btn.setAttribute("aria-pressed", saved ? "true" : "false");
        var label = saved ? "Remove from saved" : "Save startup";
        btn.setAttribute("aria-label", label);
        btn.title = label;
    }

    async function toggle(btn) {
        if (btn.disabled) return;
        btn.disabled = true;

        var body = new URLSearchParams();
        body.set("startupId", btn.dataset.startupId);
        body.set("__RequestVerificationToken", token());

        try {
            var res = await fetch(ENDPOINT, {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded", "X-Requested-With": "fetch" },
                body: body.toString(),
                credentials: "same-origin"
            });

            if (!res.ok) throw new Error("HTTP " + res.status);

            var data = await res.json();
            var saved = data.action === "added";

            if (btn.dataset.removeOnUnsave === "true" && !saved) {
                var card = btn.closest("[data-favorite-card]");
                if (card) card.remove();
                if (!document.querySelector("[data-favorite-card]")) location.reload();
            } else {
                setState(btn, saved);
            }

            notify(saved ? "Saved to your list." : "Removed from your saved startups.", false);
        } catch (err) {
            notify("Couldn't update your saved startups. Please try again.", true);
        } finally {
            btn.disabled = false;
        }
    }

    document.addEventListener("click", function (e) {
        var btn = e.target.closest(".favorite-btn[data-startup-id]");
        if (!btn) return;
        e.preventDefault();
        toggle(btn);
    });
})();
