/* ============================================================
   ff-search.js — Globalna pretraga (navbar live dropdown)
   - debounce na unos (~250ms)
   - fetch /Search/Suggest?q=... -> renderira prijedloge
   - tipkovnica: strelice gore/dolje, Enter, Esc
   - klik izvan zatvara; Enter bez odabira submita formu na /Search
   ============================================================ */
(function () {
    'use strict';

    var form = document.getElementById('ffNavSearch');
    if (!form) return;

    var input = document.getElementById('ffNavSearchInput');
    var results = document.getElementById('ffNavSearchResults');
    if (!input || !results) return;

    var MIN_CHARS = 2;
    var DEBOUNCE_MS = 250;

    var debounceTimer = null;
    var activeIndex = -1;          // trenutno označena stavka (tipkovnica)
    var currentItems = [];         // zadnji renderirani prijedlozi
    var lastQuery = '';
    var abortController = null;

    /* --- Helpers --- */
    function escapeHtml(value) {
        if (value == null) return '';
        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function openDropdown() {
        results.hidden = false;
        input.setAttribute('aria-expanded', 'true');
    }

    function closeDropdown() {
        results.hidden = true;
        results.innerHTML = '';
        input.setAttribute('aria-expanded', 'false');
        input.removeAttribute('aria-activedescendant');
        activeIndex = -1;
        currentItems = [];
    }

    function showMessage(text) {
        results.innerHTML =
            '<div class="ff-navsearch__empty" role="status">' +
            '<i class="bi bi-search" aria-hidden="true"></i>' +
            '<span>' + escapeHtml(text) + '</span>' +
            '</div>';
        openDropdown();
    }

    function render(items) {
        currentItems = items || [];
        activeIndex = -1;
        input.removeAttribute('aria-activedescendant');

        if (!currentItems.length) {
            showMessage('Nema prijedloga za "' + input.value.trim() + '".');
            return;
        }

        var html = '';
        for (var i = 0; i < currentItems.length; i++) {
            var item = currentItems[i] || {};
            var icon = (item.icon || 'bi-dot');
            var label = escapeHtml(item.label || '');
            var sublabel = escapeHtml(item.sublabel || '');
            var type = escapeHtml(item.type || '');
            var url = escapeHtml(item.url || '#');

            html +=
                '<a class="ff-navsearch__item" role="option" id="ffNavSearchOpt' + i + '"' +
                ' href="' + url + '" data-index="' + i + '" tabindex="-1">' +
                '<span class="ff-navsearch__item-icon"><i class="bi ' + escapeHtml(icon) + '" aria-hidden="true"></i></span>' +
                '<span class="ff-navsearch__item-body">' +
                '<span class="ff-navsearch__item-label">' + label + '</span>' +
                (sublabel ? '<span class="ff-navsearch__item-sub">' + sublabel + '</span>' : '') +
                '</span>' +
                (type ? '<span class="ff-navsearch__item-type">' + type + '</span>' : '') +
                '</a>';
        }
        results.innerHTML = html;
        openDropdown();
    }

    function setActive(index) {
        var options = results.querySelectorAll('.ff-navsearch__item');
        if (!options.length) return;

        if (index < 0) index = options.length - 1;
        if (index >= options.length) index = 0;
        activeIndex = index;

        for (var i = 0; i < options.length; i++) {
            var isActive = (i === activeIndex);
            options[i].classList.toggle('ff-navsearch__item--active', isActive);
            options[i].setAttribute('aria-selected', isActive ? 'true' : 'false');
        }

        var active = options[activeIndex];
        input.setAttribute('aria-activedescendant', active.id);
        // Zadrži označenu stavku vidljivom unutar scrollabilnog dropdowna.
        if (active.scrollIntoView) {
            active.scrollIntoView({ block: 'nearest' });
        }
    }

    /* --- Dohvat prijedloga --- */
    function fetchSuggestions(query) {
        if (abortController) {
            abortController.abort();
        }
        abortController = (typeof AbortController !== 'undefined') ? new AbortController() : null;

        fetch('/Search/Suggest?q=' + encodeURIComponent(query), {
            credentials: 'same-origin',
            headers: { 'Accept': 'application/json', 'X-Requested-With': 'XMLHttpRequest' },
            signal: abortController ? abortController.signal : undefined
        })
            .then(function (r) {
                if (!r.ok) throw new Error('HTTP ' + r.status);
                return r.json();
            })
            .then(function (data) {
                // Ignoriraj zakašnjele odgovore koji ne odgovaraju trenutnom unosu.
                if (input.value.trim() !== query) return;
                render(Array.isArray(data) ? data : []);
            })
            .catch(function (err) {
                if (err && err.name === 'AbortError') return;
                showMessage('Pretraga trenutno nije dostupna.');
            });
    }

    /* --- Events --- */
    function onInput() {
        var query = input.value.trim();
        clearTimeout(debounceTimer);

        if (query.length < MIN_CHARS) {
            lastQuery = '';
            closeDropdown();
            return;
        }

        if (query === lastQuery && !results.hidden) return;
        lastQuery = query;

        debounceTimer = setTimeout(function () {
            fetchSuggestions(query);
        }, DEBOUNCE_MS);
    }

    function onKeydown(e) {
        var hasItems = !results.hidden && currentItems.length > 0;

        switch (e.key) {
            case 'ArrowDown':
                if (hasItems) {
                    e.preventDefault();
                    setActive(activeIndex + 1);
                }
                break;
            case 'ArrowUp':
                if (hasItems) {
                    e.preventDefault();
                    setActive(activeIndex - 1);
                }
                break;
            case 'Enter':
                // Ako je stavka označena strelicama -> idi na nju; inače pusti submit na /Search.
                if (hasItems && activeIndex >= 0 && currentItems[activeIndex]) {
                    e.preventDefault();
                    var url = currentItems[activeIndex].url;
                    if (url) window.location.href = url;
                }
                break;
            case 'Escape':
                if (!results.hidden) {
                    e.preventDefault();
                    closeDropdown();
                }
                break;
            default:
                break;
        }
    }

    input.addEventListener('input', onInput);
    input.addEventListener('keydown', onKeydown);

    input.addEventListener('focus', function () {
        // Ponovno otvori ako već imamo rezultate za trenutni upit.
        if (input.value.trim().length >= MIN_CHARS && currentItems.length) {
            openDropdown();
        }
    });

    // Mouse hover sinkronizira označenu stavku s tipkovnicom.
    results.addEventListener('mousemove', function (e) {
        var item = e.target.closest('.ff-navsearch__item');
        if (!item) return;
        var idx = parseInt(item.getAttribute('data-index'), 10);
        if (!isNaN(idx) && idx !== activeIndex) setActive(idx);
    });

    // Klik izvan forme zatvara dropdown.
    document.addEventListener('click', function (e) {
        if (!form.contains(e.target)) {
            closeDropdown();
        }
    });

    // Submit: ako je polje prekratko, ne radi ništa (pusti browser/server odlučiti).
    form.addEventListener('submit', function () {
        closeDropdown();
    });
})();
