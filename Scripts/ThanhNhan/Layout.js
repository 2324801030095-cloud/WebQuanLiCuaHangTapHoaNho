/* Layout.js — minimal, defensive, no HTML inside file */
(function () {
    // safe Swiper init if library present and element exists
    try {
        if (typeof Swiper !== 'undefined') {
            try {
                if (document.querySelector('#promoSwiper')) {
                    new Swiper('#promoSwiper', {
                        loop: true,
                        loopAdditionalSlides: 4,
                        speed: 600,
                        autoplay: { delay: 3500, disableOnInteraction: false },
                        spaceBetween: 24,
                        slidesPerView: 1,
                        breakpoints: { 768: { slidesPerView: 2 }, 1200: { slidesPerView: 3 } },
                        navigation: { nextEl: '#promoNext', prevEl: '#promoPrev' }
                    });
                }
            } catch (e) { console.warn('Promo swiper init error', e); }

            try {
                if (document.querySelector('#testiSwiper')) {
                    new Swiper('#testiSwiper', {
                        loop: true,
                        loopAdditionalSlides: 2,
                        speed: 580,
                        autoplay: { delay: 3200, disableOnInteraction: false },
                        spaceBetween: 24,
                        slidesPerView: 1,
                        breakpoints: { 768: { slidesPerView: 2 }, 1200: { slidesPerView: 3 } },
                        navigation: { nextEl: '#testiNext', prevEl: '#testiPrev' }
                    });
                }
            } catch (e) { console.warn('Testi swiper init error', e); }

            try {
                if (document.querySelector('#topCarousel')) {
                    const topSwiper = new Swiper('#topCarousel', {
                        loop: true,
                        speed: 600,
                        autoplay: { delay: 3000, disableOnInteraction: false },
                        slidesPerView: 1.25,
                        centeredSlides: true,
                        spaceBetween: 22,
                        breakpoints: { 768: { slidesPerView: 1.6, spaceBetween: 24 }, 1200: { slidesPerView: 2.2, spaceBetween: 28 } },
                        pagination: { el: '.swiper-pagination', clickable: true }
                    });
                    // expose if needed
                    window.__topSwiper = topSwiper;
                }
            } catch (e) { console.warn('Top swiper init error', e); }
        }
    } catch (e) {
        console.error('Swiper overall init failed', e);
    }

    // flip cards (defensive)
    try {
        document.querySelectorAll('#topCarousel .flip').forEach(card => {
            try {
                const back = card.querySelector('.flip-back p');
                const desc = card.getAttribute('data-desc');
                if (desc && back) back.textContent = desc;
                card.addEventListener('click', () => card.classList.toggle('is-flipped'));
            } catch (e) { /* continue */ }
        });
    } catch (e) { /* no flips present */ }

    // navbar hide-on-scroll (guarded)
    (function () {
        const header = document.getElementById('appHeader');
        if (!header) return;
        let lastY = window.scrollY || 0;
        let scrollTimer = null;
        window.addEventListener('scroll', () => {
            const y = window.scrollY || 0;
            const goingDown = y > lastY;
            if (goingDown && y > 120) header.classList.add('nav-hidden');
            else {
                header.classList.remove('nav-hidden');
                header.classList.add('nav-compact');
            }
            lastY = y;
            clearTimeout(scrollTimer);
            scrollTimer = setTimeout(() => {
                header.classList.remove('nav-hidden');
                header.classList.add('nav-compact');
            }, 300);
        }, { passive: true });

        header.addEventListener('mouseenter', () => { header.classList.remove('nav-hidden'); header.classList.remove('nav-compact'); });
        header.addEventListener('mouseleave', () => { if (window.scrollY > 120) header.classList.add('nav-compact'); });
    })();

    // back to top (guarded)
    (function () {
        const backTop = document.getElementById('backTop');
        if (!backTop) return;
        window.addEventListener('scroll', () => { backTop.style.display = window.scrollY > 400 ? 'flex' : 'none'; });
        backTop.addEventListener('click', () => window.scrollTo({ top: 0, behavior: 'smooth' }));
    })();

})();
