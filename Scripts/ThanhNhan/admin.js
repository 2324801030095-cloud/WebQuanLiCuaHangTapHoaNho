/* ===========================================================
   SIDEBAR TOGGLE (PC + Mobile) + Collapse behavior
   Based on your uploaded admin.js, refined to fix reported bugs.
=========================================================== */
(function () {
    const sidebar = document.getElementById("sidebar");
    const toggleBtn = document.getElementById("toggleBtn");
    const header = document.querySelector(".header-bar");
    const content = document.querySelector(".content");
    const themeSwitch = document.getElementById("themeSwitch");

    // guard
    if (!sidebar || !toggleBtn || !header || !content) return;

    function setCollapsed(val) {
        if (val) {
            sidebar.classList.add("collapsed");
            header.classList.add("collapsed");
            content.classList.add("collapsed");
        } else {
            sidebar.classList.remove("collapsed");
            header.classList.remove("collapsed");
            content.classList.remove("collapsed");
        }
    }

    toggleBtn.addEventListener("click", () => {
        // toggle collapsed
        if (window.innerWidth > 992) {
            const now = sidebar.classList.contains("collapsed");
            setCollapsed(!now);
        } else {
            sidebar.classList.toggle("active");
        }
    });

    /* ===========================================================
       HIDE / SHOW HEADER ON SCROLL
    ============================================================ */
    let lastScrollTop = 0;
    let isHidden = false;
    window.addEventListener("scroll", () => {
        let current = window.scrollY || document.documentElement.scrollTop;
        if (current > lastScrollTop && current > 80) {
            if (!isHidden) {
                header.classList.remove("show");
                header.classList.add("hidden");
                isHidden = true;
            }
        } else {
            if (isHidden) {
                header.classList.remove("hidden");
                header.classList.add("show");
                isHidden = false;
            }
        }
        lastScrollTop = current <= 0 ? 0 : current;
    });

    /* ===========================================================
       ACTIVE SIDEBAR ITEM — Controller + MULTI-ACTION (robust)
       - Uses body data-ctrl and data-act provided in layout.
       - Matches any part equals controller (case-insensitive)
       - Supports data-actions on anchors (comma separated)
    ============================================================ */
    (function markActive() {
        const currentController = (document.body.dataset.ctrl || "").toString().toLowerCase();
        const currentAction = (document.body.dataset.act || "").toString().toLowerCase();

        // select links that are our menu items
        const links = Array.from(document.querySelectorAll(".sidebar .menu-link"));

        // clear existing active
        links.forEach(a => a.classList.remove("active"));

        // find matches
        links.forEach(a => {
            const href = a.getAttribute("href") || "";
            let url;
            try {
                url = new URL(href, window.location.origin);
            } catch (e) {
                // relative fallback
                url = { pathname: href };
            }

            // get segments (filter empty)
            const segs = (url.pathname || "").split("/").filter(Boolean).map(s => s.toLowerCase());

            // Check controller presence in any path segment
            const controllerMatch = currentController && segs.some(s => s === currentController);

            // Multi-action support via data-actions attribute on anchor (e.g. data-actions="Index,Details,Edit")
            const actionsAttr = (a.dataset.actions || "").toString().toLowerCase().split(",").map(s => s.trim()).filter(Boolean);
            let actionMatch = false;
            if (actionsAttr.length > 0 && currentAction) {
                actionMatch = actionsAttr.includes(currentAction);
            } else {
                // fallback: determine action segment candidate (last segment or second)
                const actionSeg = segs.length > 1 ? segs[segs.length - 1] : "";
                actionMatch = !currentAction || actionSeg === currentAction;
            }

            if (controllerMatch) {
                if (actionsAttr.length > 0) {
                    if (actionMatch) a.classList.add("active");
                } else {
                    // mark active by controller match
                    a.classList.add("active");
                }
            }
        });

        // ensure only one active has neon markup (keep first)
        const active = document.querySelector(".sidebar .menu-link.active");
        document.querySelectorAll(".sidebar .neon-bar").forEach(n => n.remove());
        if (active) {
            const neon = document.createElement("div");
            neon.className = "neon-bar";
            active.appendChild(neon);
        }
    })();

    /* expose markActive if you need to re-run after SPA/partial updates */
    window.__tn_markActive = function () {
        const ev = new Event('tn-mark-active');
        window.dispatchEvent(ev);
    };

    /* ===========================================================
       COLLAPSED MODE: clicking icon should show text before nav
       - If sidebar is collapsed (desktop) and user clicks .menu-link,
         first expand visually for a short time so label appears, then navigate.
    ============================================================ */
    (function collapsedClickBehavior() {
        const navLinks = Array.from(document.querySelectorAll(".sidebar .menu-link"));
        navLinks.forEach(a => {
            a.addEventListener("click", function (ev) {
                // if mobile active state (sidebar.active), allow immediate nav
                if (window.innerWidth <= 992) return;

                // if collapsed, expand briefly then follow link
                if (sidebar.classList.contains("collapsed")) {
                    ev.preventDefault();

                    // temporary expansion classes so CSS shows labels
                    sidebar.classList.add("expanded-temp");
                    header.classList.add("expanded-temp");
                    content.classList.add("expanded-temp");

                    // small delay so user sees expanded menu, then follow link
                    setTimeout(() => {
                        // Remove temporary expansion classes and navigate to href
                        sidebar.classList.remove("expanded-temp");
                        header.classList.remove("expanded-temp");
                        content.classList.remove("expanded-temp");
                        window.location.href = a.href;
                    }, 220); // 220ms feels snappy
                }
            });
        });
    })();

    /* ===========================================================
       THEME SWITCH (dark/light) - keep UX consistent
    ============================================================ */
    (function themeSwitch() {
        if (!themeSwitch) return;
        const knob = themeSwitch.querySelector(".knob i");
        const clickSound = new Audio("https://cdn.pixabay.com/audio/2022/03/15/audio_c139e5ce37.mp3");
        const saved = localStorage.getItem("theme") || "light";

        function applyTheme(mode, animate = true) {
            if (mode === "dark") {
                document.body.classList.add("dark");
                themeSwitch.dataset.mode = "dark";
                if (knob) knob.className = "fa-solid fa-moon";
            } else {
                document.body.classList.remove("dark");
                themeSwitch.dataset.mode = "light";
                if (knob) knob.className = "fa-solid fa-sun";
            }
            if (animate) {
                themeSwitch.classList.add("switch-animate");
                setTimeout(() => themeSwitch.classList.remove("switch-animate"), 350);
            }
            localStorage.setItem("theme", mode);
        }

        // initial
        applyTheme(saved, false);

        themeSwitch.addEventListener("click", () => {
            const mode = document.body.classList.contains("dark") ? "light" : "dark";
            try { clickSound.currentTime = 0; clickSound.play(); } catch (e) { /* ignore autoplay blocks */ }
            themeSwitch.classList.remove("ripple");
            void themeSwitch.offsetWidth;
            themeSwitch.classList.add("ripple");
            applyTheme(mode, true);
        });
    })();

})();
