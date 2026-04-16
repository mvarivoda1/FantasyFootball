// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// --- Navbar: hide on scroll down, show on scroll up ---
(function () {
    'use strict';

    var navbar = document.getElementById('ffNavbar');
    if (!navbar) return;

    var lastScrollY = window.pageYOffset || document.documentElement.scrollTop;
    var ticking = false;
    var threshold = 80;       // ne skrivaj u prvih 80px
    var delta = 6;            // minimum delta za promjenu smjera (debounce)

    function onScroll() {
        var currentY = window.pageYOffset || document.documentElement.scrollTop;

        // Scrolled class (za subtle shadow boost kad nije na vrhu)
        if (currentY > 4) {
            navbar.classList.add('ff-navbar--scrolled');
        } else {
            navbar.classList.remove('ff-navbar--scrolled');
        }

        // Ispod thresholda — uvijek prikazan
        if (currentY <= threshold) {
            navbar.classList.remove('ff-navbar--hidden');
            lastScrollY = currentY;
            ticking = false;
            return;
        }

        // Ako je otvoren mobile collapse, ne skrivaj navbar
        var collapse = document.getElementById('navbarMain');
        if (collapse && collapse.classList.contains('show')) {
            lastScrollY = currentY;
            ticking = false;
            return;
        }

        var diff = currentY - lastScrollY;
        if (Math.abs(diff) < delta) {
            ticking = false;
            return;
        }

        if (diff > 0) {
            // Scroll prema dolje — sakrij
            navbar.classList.add('ff-navbar--hidden');
        } else {
            // Scroll prema gore — prikazi
            navbar.classList.remove('ff-navbar--hidden');
        }

        lastScrollY = currentY;
        ticking = false;
    }

    window.addEventListener('scroll', function () {
        if (!ticking) {
            window.requestAnimationFrame(onScroll);
            ticking = true;
        }
    }, { passive: true });
})();


// --- Deadline Countdown ---
(function () {
    'use strict';

    function pad(n) {
        return n < 10 ? '0' + n : '' + n;
    }

    function updateCountdown(el) {
        var raw = el.getAttribute('data-deadline');
        if (!raw) return;

        var target = new Date(raw).getTime();
        if (isNaN(target)) return;

        var now = new Date().getTime();
        var diff = Math.max(0, target - now);

        var days = Math.floor(diff / (1000 * 60 * 60 * 24));
        var hours = Math.floor((diff / (1000 * 60 * 60)) % 24);
        var minutes = Math.floor((diff / (1000 * 60)) % 60);
        var seconds = Math.floor((diff / 1000) % 60);

        var setUnit = function (unit, value) {
            var node = el.querySelector('[data-unit="' + unit + '"]');
            if (node) node.textContent = pad(value);
        };

        setUnit('days', days);
        setUnit('hours', hours);
        setUnit('minutes', minutes);
        setUnit('seconds', seconds);

        if (diff === 0) {
            el.classList.add('ff-countdown--expired');
        }
    }

    function initCountdowns() {
        var countdowns = document.querySelectorAll('.ff-countdown[data-deadline]');
        if (!countdowns.length) return;

        countdowns.forEach(function (el) {
            updateCountdown(el);
        });

        setInterval(function () {
            countdowns.forEach(function (el) {
                updateCountdown(el);
            });
        }, 1000);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initCountdowns);
    } else {
        initCountdowns();
    }
})();

// --- Ranking Table: klikabilni red + sort toggle ---
(function () {
    'use strict';

    function renumberRows(tbody) {
        var rows = tbody.querySelectorAll('tr');
        rows.forEach(function (row, i) {
            var cell = row.querySelector('[data-rank]');
            if (cell) cell.textContent = (i + 1);
        });
    }

    function updateMedalClasses() {
        // CSS nth-child handles vizual, nista za JS
    }

    function sortByPoints(table, direction) {
        var tbody = table.querySelector('tbody');
        if (!tbody) return;
        var rows = Array.prototype.slice.call(tbody.querySelectorAll('tr'));
        rows.sort(function (a, b) {
            var av = parseInt(a.querySelector('[data-points]').getAttribute('data-points'), 10) || 0;
            var bv = parseInt(b.querySelector('[data-points]').getAttribute('data-points'), 10) || 0;
            return direction === 'asc' ? av - bv : bv - av;
        });
        rows.forEach(function (r) { tbody.appendChild(r); });
        renumberRows(tbody);
        updateMedalClasses();
    }

    function initRankingTable() {
        var table = document.getElementById('ff-ranking-table');
        if (!table) return;

        // Klikabilni red
        table.addEventListener('click', function (e) {
            // Zanemari klik unutar sort-header celije
            if (e.target.closest('.ff-ranking-table__sort')) return;
            var row = e.target.closest('.ff-ranking-row');
            if (!row) return;
            var href = row.getAttribute('data-href');
            if (href) window.location.href = href;
        });

        // Keyboard activacija reda
        table.addEventListener('keydown', function (e) {
            var row = e.target.closest('.ff-ranking-row');
            if (!row) return;
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                var href = row.getAttribute('data-href');
                if (href) window.location.href = href;
            }
        });

        // Sort handler na header-u (event delegation)
        var sortHeader = table.querySelector('.ff-ranking-table__sort[data-sort="points"]');
        if (!sortHeader) return;

        function toggleSort() {
            var current = sortHeader.getAttribute('aria-sort') || 'descending';
            var next = current === 'descending' ? 'ascending' : 'descending';
            sortHeader.setAttribute('aria-sort', next);
            sortHeader.classList.add('ff-ranking-table__sort--active');

            var icon = sortHeader.querySelector('.ff-ranking-table__sort-icon');
            if (icon) {
                icon.classList.remove('bi-arrow-down', 'bi-arrow-up');
                icon.classList.add(next === 'descending' ? 'bi-arrow-down' : 'bi-arrow-up');
            }

            sortByPoints(table, next === 'descending' ? 'desc' : 'asc');
        }

        sortHeader.addEventListener('click', toggleSort);
        sortHeader.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                toggleSort();
            }
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initRankingTable);
    } else {
        initRankingTable();
    }
})();

// --- TOTW widget: AJAX navigacija izmedu gameweekova (bez skrolanja na vrh) ---
(function () {
    'use strict';

    var widget = document.getElementById('totwWidget');
    if (!widget) return;

    var inFlight = false;

    function getGwFromLink(link) {
        var dataGw = link.getAttribute('data-gw');
        if (dataGw) return dataGw;

        var href = link.getAttribute('href') || '';
        var match = href.match(/[?&]gw=(\d+)/);
        return match ? match[1] : null;
    }

    function replaceContent(html) {
        // Koristimo template da parsiramo HTML bez izvrsavanja skripti
        var tpl = document.createElement('template');
        tpl.innerHTML = html.trim();
        // Obrisi postojece header + body, ubaci novi
        widget.innerHTML = '';
        widget.appendChild(tpl.content);
    }

    function updateUrl(gw) {
        if (!window.history || !window.history.replaceState) return;
        try {
            var url = new URL(window.location.href);
            url.searchParams.set('gw', gw);
            window.history.replaceState(window.history.state, '', url.toString());
        } catch (e) {
            // Noop ako URL API nije dostupan
        }
    }

    function loadGameweek(gw) {
        if (inFlight || !gw) return;
        inFlight = true;
        widget.classList.add('ff-totw--loading');

        var url = '/Home/TotwPartial?gw=' + encodeURIComponent(gw);

        fetch(url, {
            method: 'GET',
            credentials: 'same-origin',
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'Accept': 'text/html'
            }
        })
            .then(function (response) {
                if (!response.ok) throw new Error('Network response not OK');
                return response.text();
            })
            .then(function (html) {
                replaceContent(html);
                updateUrl(gw);
            })
            .catch(function () {
                // Tihi fallback — ne ruinirati stranicu; korisnik moze pokusati opet
            })
            .then(function () {
                widget.classList.remove('ff-totw--loading');
                inFlight = false;
            });
    }

    widget.addEventListener('click', function (e) {
        var link = e.target.closest('.ff-totw-nav a');
        if (!link) return;
        if (!widget.contains(link)) return;
        if (link.classList.contains('ff-totw-nav__arrow--disabled')) return;

        e.preventDefault();
        var gw = getGwFromLink(link);
        if (gw) loadGameweek(gw);
    });

    widget.addEventListener('keydown', function (e) {
        if (e.key !== 'Enter' && e.key !== ' ') return;
        var link = e.target.closest('.ff-totw-nav a');
        if (!link) return;
        e.preventDefault();
        var gw = getGwFromLink(link);
        if (gw) loadGameweek(gw);
    });
})();
