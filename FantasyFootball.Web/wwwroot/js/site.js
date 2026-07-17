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

/* ============================================
   MY TEAM — two-click swap logic
   ============================================ */
(function () {
    var root = document.querySelector('.ff-myteam');
    if (!root) return;

    var pitch = root.querySelector('[data-role="pitch"]');
    var bench = root.querySelector('[data-role="bench"]');
    var form  = root.querySelector('#ffMyTeamForm');
    var hiddenHost = root.querySelector('[data-role="starters-hidden"]');
    var feedback = root.querySelector('[data-role="swap-feedback"]');
    var saveBtn = root.querySelector('[data-role="save-lineup"]');
    var formationLabel = root.querySelector('[data-role="formation-label"]');

    if (!pitch || !bench || !form || !hiddenHost) return;

    var limits = {
        GK:  { min: +root.dataset.minGk,  max: +root.dataset.maxGk  },
        DEF: { min: +root.dataset.minDef, max: +root.dataset.maxDef },
        MID: { min: +root.dataset.minMid, max: +root.dataset.maxMid },
        FWD: { min: +root.dataset.minFwd, max: +root.dataset.maxFwd }
    };
    var STARTERS_COUNT = +root.dataset.startersCount || 11;

    var selectedEl = null;
    var feedbackTimer = null;

    function getPlayers(scope) {
        return Array.prototype.slice.call(scope.querySelectorAll('.ff-pitch-player'));
    }
    function countStartersByPos() {
        var counts = { GK: 0, DEF: 0, MID: 0, FWD: 0 };
        getPlayers(pitch).forEach(function (el) {
            var p = el.dataset.position;
            if (counts.hasOwnProperty(p)) counts[p]++;
        });
        return counts;
    }
    function isValidCounts(counts) {
        return Object.keys(limits).every(function (k) {
            return counts[k] >= limits[k].min && counts[k] <= limits[k].max;
        }) && (counts.GK + counts.DEF + counts.MID + counts.FWD === STARTERS_COUNT);
    }
    function updateChips(counts) {
        var allValid = true;
        Object.keys(limits).forEach(function (k) {
            var num = root.querySelector('[data-counter-num="' + k + '"]');
            var chip = root.querySelector('[data-counter="' + k + '"]');
            var lim = limits[k];
            var ok = counts[k] >= lim.min && counts[k] <= lim.max;
            if (!ok) allValid = false;
            // Counter chipovi su uklonjeni iz UI-a — guard protiv null DOM elemenata.
            if (num) { num.textContent = counts[k]; }
            if (chip) { chip.classList.toggle('is-invalid', !ok); }
        });
        if (formationLabel) {
            formationLabel.textContent = counts.DEF + '-' + counts.MID + '-' + counts.FWD;
        }
        if (saveBtn) {
            saveBtn.disabled = !allValid;
            saveBtn.setAttribute('aria-disabled', allValid ? 'false' : 'true');
            saveBtn.title = allValid ? '' : 'Formacija nije valjana';
        }
    }
    function rebuildHiddenInputs() {
        var ids = getPlayers(pitch).map(function (el) { return el.dataset.playerId; });
        hiddenHost.innerHTML = '';
        ids.forEach(function (id) {
            var input = document.createElement('input');
            input.type = 'hidden';
            input.name = 'starterIds';
            input.value = id;
            hiddenHost.appendChild(input);
        });
    }
    function refreshState() {
        var counts = countStartersByPos();
        updateChips(counts);
        rebuildHiddenInputs();
    }
    function showFeedback(message) {
        if (!feedback) return;
        feedback.innerHTML = '<i class="bi bi-exclamation-circle-fill" aria-hidden="true"></i><span>' + message + '</span>';
        clearTimeout(feedbackTimer);
        feedbackTimer = setTimeout(function () { feedback.innerHTML = ''; }, 3000);
    }
    function clearFeedback() {
        if (!feedback) return;
        feedback.innerHTML = '';
    }
    function isStarter(el) { return el.dataset.starter === 'true'; }
    function findRow(pos) {
        return pitch.querySelector('.ff-pitch__row[data-row="' + pos + '"]');
    }

    /**
     * Determines if swapping A and B keeps the formation valid.
     * A and B are both .ff-pitch-player elements (one or both may be on bench).
     */
    function isLegalSwap(a, b) {
        if (a === b) return false;
        var aStarter = isStarter(a);
        var bStarter = isStarter(b);
        // Same position swaps are always legal — formation unchanged
        if (a.dataset.position === b.dataset.position) return true;
        // Both starters with different positions: just reordering on pitch — count doesn't change
        if (aStarter && bStarter) return true;
        // Bench <-> Bench (different position): bench composition irrelevant for starter validity
        if (!aStarter && !bStarter) return true;

        // One is a starter, the other is on bench, different positions:
        // Effectively starter's position count goes -1, bench player's position count goes +1.
        var starterEl = aStarter ? a : b;
        var benchEl   = aStarter ? b : a;
        var counts = countStartersByPos();
        counts[starterEl.dataset.position] -= 1;
        counts[benchEl.dataset.position]   += 1;
        return isValidCounts(counts);
    }

    function highlightValidTargets(source) {
        getPlayers(root).forEach(function (el) {
            el.classList.remove('is-valid-target');
            if (el === source) return;
            if (isLegalSwap(source, el)) {
                el.classList.add('is-valid-target');
            }
        });
    }
    function clearHighlights() {
        getPlayers(root).forEach(function (el) {
            el.classList.remove('is-valid-target', 'is-selected', 'ff-pitch-player--invalid-target');
            el.setAttribute('aria-pressed', 'false');
        });
    }

    /**
     * Swap two player tiles in the DOM.
     * - If both are pitch players in the same row, swap their order within the row.
     * - Otherwise: each tile moves into the other's parent slot, and starter flag flips.
     */
    function swapTiles(a, b) {
        var aParent = a.parentNode;
        var bParent = b.parentNode;
        var aNext = a.nextSibling === b ? a : a.nextSibling;
        var bNext = b.nextSibling === a ? b : b.nextSibling;

        // Detach
        aParent.removeChild(a);
        bParent.removeChild(b);

        // Determine new parents based on whether each tile remains pitch or bench
        var aWasStarter = isStarter(a);
        var bWasStarter = isStarter(b);

        function placeInto(el, targetParent, refNode) {
            if (refNode && refNode.parentNode === targetParent) {
                targetParent.insertBefore(el, refNode);
            } else {
                targetParent.appendChild(el);
            }
        }

        if (aWasStarter === bWasStarter) {
            // Both on same side: keep zone, but if same row keep slots; if pitch but different rows,
            // each goes into the *other*'s parent (their row) to preserve formation visualization.
            placeInto(a, bParent, bNext);
            placeInto(b, aParent, aNext);
        } else {
            // One was starter, the other bench. They cross sides AND switch starter status.
            // Starter slot is the pitch row matching the *new* starter's position.
            var newStarter = aWasStarter ? b : a;
            var newBench   = aWasStarter ? a : b;

            var targetRow = findRow(newStarter.dataset.position) || pitch.querySelector('.ff-pitch__row');
            targetRow.appendChild(newStarter);
            newStarter.dataset.starter = 'true';
            newStarter.classList.remove('ff-pitch-player--bench');
            newStarter.classList.add('ff-pitch-player--starter');

            bench.appendChild(newBench);
            newBench.dataset.starter = 'false';
            newBench.classList.remove('ff-pitch-player--starter');
            newBench.classList.add('ff-pitch-player--bench');
        }
    }

    function attemptSwap(a, b) {
        if (!isLegalSwap(a, b)) {
            b.classList.add('ff-pitch-player--invalid-target');
            setTimeout(function () {
                b.classList.remove('ff-pitch-player--invalid-target');
            }, 450);
            var reason = 'Nelegalan swap — formacija ne bi zadovoljila minimalne uvjete (1 GK, ' +
                limits.DEF.min + ' DEF, ' + limits.MID.min + ' MID, ' + limits.FWD.min + ' FWD).';
            showFeedback(reason);
            return;
        }
        swapTiles(a, b);
        clearHighlights();
        selectedEl = null;
        clearFeedback();
        refreshState();
    }

    root.addEventListener('click', function (e) {
        var tile = e.target.closest('.ff-pitch-player');
        if (tile && root.contains(tile)) {
            e.preventDefault();
            if (selectedEl === tile) {
                clearHighlights();
                selectedEl = null;
                clearFeedback();
                return;
            }
            if (!selectedEl) {
                selectedEl = tile;
                getPlayers(root).forEach(function (el) {
                    el.classList.remove('is-selected');
                    el.setAttribute('aria-pressed', 'false');
                });
                tile.classList.add('is-selected');
                tile.setAttribute('aria-pressed', 'true');
                highlightValidTargets(tile);
                return;
            }
            // We have a selected one already — try to swap.
            attemptSwap(selectedEl, tile);
            return;
        }

        // Click outside any tile clears selection
        if (selectedEl) {
            clearHighlights();
            selectedEl = null;
        }

        // Toast dismiss
        var dismiss = e.target.closest('[data-dismiss-toast]');
        if (dismiss) {
            var toast = dismiss.closest('.ff-myteam-toast');
            if (toast) toast.remove();
        }
    });

    // Keyboard support: Enter / Space activates a tile (button), Esc clears selection
    root.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && selectedEl) {
            clearHighlights();
            selectedEl = null;
            clearFeedback();
        }
    });

    // Block submit if formation invalid (defensive — server validates anyway)
    form.addEventListener('submit', function (e) {
        var counts = countStartersByPos();
        if (!isValidCounts(counts)) {
            e.preventDefault();
            showFeedback('Formacija nije valjana — provjeri brojeve po poziciji.');
        }
    });

    // Initial sync
    refreshState();
})();
