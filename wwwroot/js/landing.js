/* =========================================================
   Tech Nova — landing page interactions.
   Nav scroll state, role dropdown, mobile drawer.
   ========================================================= */
(function () {
    "use strict";

    var nav = document.getElementById("siteNav");
    var ctaWrap = document.getElementById("navCtaWrap");
    var ctaBtn = document.getElementById("navCtaBtn");
    var burger = document.getElementById("navBurger");
    var drawer = document.getElementById("navDrawer");

    /* ---- Navbar flips from transparent-on-hero to solid ---- */

    if (nav) {
        var ticking = false;

        function syncNav() {
            nav.classList.toggle("is-scrolled", window.scrollY > 40);
            ticking = false;
        }

        window.addEventListener("scroll", function () {
            if (!ticking) {
                ticking = true;
                requestAnimationFrame(syncNav);
            }
        }, { passive: true });

        syncNav();
    }

    /* ---- "Get Started" role dropdown ---- */

    function closeMenu() {
        if (!ctaWrap) return;
        ctaWrap.classList.remove("is-open");
        ctaBtn.setAttribute("aria-expanded", "false");
    }

    if (ctaBtn && ctaWrap) {
        ctaBtn.addEventListener("click", function (e) {
            e.stopPropagation();
            var open = ctaWrap.classList.toggle("is-open");
            ctaBtn.setAttribute("aria-expanded", open ? "true" : "false");
        });

        document.addEventListener("click", function (e) {
            if (!ctaWrap.contains(e.target)) closeMenu();
        });
    }

    /* ---- Mobile drawer ---- */

    function closeDrawer() {
        if (!drawer) return;
        drawer.classList.remove("is-open");
        burger.setAttribute("aria-expanded", "false");
        burger.setAttribute("aria-label", "Open menu");
        burger.innerHTML = '<svg class="ico"><use href="#i-menu" /></svg>';
    }

    if (burger && drawer) {
        burger.addEventListener("click", function (e) {
            e.stopPropagation();

            var open = drawer.classList.toggle("is-open");
            burger.setAttribute("aria-expanded", open ? "true" : "false");
            burger.setAttribute("aria-label", open ? "Close menu" : "Open menu");
            burger.innerHTML = open
                ? '<svg class="ico"><use href="#i-x" /></svg>'
                : '<svg class="ico"><use href="#i-menu" /></svg>';
        });

        // Any link inside the drawer dismisses it.
        drawer.addEventListener("click", function (e) {
            if (e.target.closest("a")) closeDrawer();
        });
    }

    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape") {
            closeMenu();
            closeDrawer();
        }
    });

    /* =====================================================
       Scroll-driven heading.

       Vanilla port of the framer-motion CharacterV1 transform:
       each character starts offset by (index - centreIndex) * SPREAD
       and converges on its resting position as the element scrolls
       into view. framer's useTransform([0, 0.5]) is reproduced by
       mapping raw progress through a window that completes early.
       ===================================================== */

    /* The source component scattered by (index - centre) * 50, which is
       fine for its 14-character string but explodes on a real heading:
       33 characters reach ±16 steps, i.e. ±800px and two full rotations.
       Normalising the offset to [-1, 1] keeps the effect identical in
       feel while staying bounded at any heading length. */
    var MAX_SHIFT = 300;   // px of horizontal scatter at the outermost char
    var MAX_TILT = 80;     // deg of rotateX at the outermost char

    function buildChars(el) {
        var text = el.textContent.replace(/\s+/g, " ").trim();
        var total = text.length;
        var centre = Math.floor(total / 2);

        el.textContent = "";

        var chars = [];
        var words = text.split(" ");
        var i = 0;

        words.forEach(function (word, w) {
            // Words stay intact so the heading still wraps normally.
            var wordEl = document.createElement("span");
            wordEl.className = "scroll-word";

            for (var c = 0; c < word.length; c++) {
                var span = document.createElement("span");
                span.className = "scroll-char";
                span.textContent = word[c];
                span.dataset.n = centre ? ((i - centre) / centre).toFixed(4) : 0;
                wordEl.appendChild(span);
                chars.push(span);
                i++;
            }

            el.appendChild(wordEl);

            if (w < words.length - 1) {
                el.appendChild(document.createTextNode(" "));
                i++;
            }
        });

        el.classList.add("is-ready");
        return chars;
    }

    function initScrollChars() {
        var targets = document.querySelectorAll("[data-scroll-chars]");
        if (!targets.length) return;

        var reduced = window.matchMedia &&
            window.matchMedia("(prefers-reduced-motion: reduce)").matches;

        var groups = [];

        targets.forEach(function (el) {
            if (reduced) return;           // leave the heading at rest
            groups.push({ el: el, chars: buildChars(el) });
        });

        if (!groups.length) return;

        function apply(group) {
            var rect = group.el.getBoundingClientRect();
            var vh = window.innerHeight || 1;

            // Scatter must stay narrower than the screen, or the effect
            // reads as characters flying in from nowhere on a phone.
            var shift = Math.min(MAX_SHIFT, window.innerWidth * 0.42);

            // Converge over the stretch where the heading rises from
            // just below the fold to comfortably inside the viewport.
            var start = vh * 0.95;
            var end = vh * 0.34;

            var p = (start - rect.top) / (start - end);
            if (p < 0) p = 0;
            if (p > 1) p = 1;

            var rest = 1 - p;

            group.chars.forEach(function (span) {
                var n = +span.dataset.n;          // normalised, -1 .. 1
                var x = n * shift * rest;
                var tilt = n * MAX_TILT * rest;

                span.style.transform =
                    "translate3d(" + x.toFixed(2) + "px,0,0) " +
                    "rotateX(" + tilt.toFixed(2) + "deg)";

                span.style.opacity = (0.2 + 0.8 * p).toFixed(3);
            });
        }

        function update() {
            for (var g = 0; g < groups.length; g++) apply(groups[g]);
            charTicking = false;
        }

        var charTicking = false;

        function request() {
            if (!charTicking) {
                charTicking = true;
                requestAnimationFrame(update);
            }
        }

        window.addEventListener("scroll", request, { passive: true });
        window.addEventListener("resize", request);

        update();
    }

    initScrollChars();

    /* =====================================================
       Final CTA — the headline collapses into a role picker.

       Progressive enhancement: the reveal is visible by default
       and only becomes a hidden panel once this runs, so the
       signup links are never unreachable without JS.
       ===================================================== */

    function initFinalCta() {
        var section = document.querySelector(".final");
        var trigger = document.getElementById("ctaTrigger");
        var back = document.getElementById("ctaBack");
        if (!section || !trigger) return;

        section.setAttribute("data-cta-ready", "");

        function open() {
            section.classList.add("is-revealed");
            trigger.setAttribute("aria-expanded", "true");

            // move focus to the first choice so keyboard users land there
            var first = section.querySelector(".final__ctas a");
            if (first) first.focus({ preventScroll: true });
        }

        function close() {
            section.classList.remove("is-revealed");
            trigger.setAttribute("aria-expanded", "false");
            trigger.focus({ preventScroll: true });
        }

        trigger.addEventListener("click", open);

        if (back) back.addEventListener("click", close);

        section.addEventListener("keydown", function (e) {
            if (e.key === "Escape" && section.classList.contains("is-revealed")) {
                close();
            }
        });
    }

    initFinalCta();
})();
