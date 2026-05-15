/* =====================================================
   FF Autocomplete — AJAX search dropdown
   Auto-inits all [data-ff-autocomplete] elements.
   Vanilla JS (no jQuery).
   Endpoint must return JSON array: [{ id, label }, ...]
   ===================================================== */
(function () {
    'use strict';

    var DEBOUNCE_MS = 200;

    function debounce(fn, ms) {
        var t = null;
        return function () {
            var args = arguments, self = this;
            clearTimeout(t);
            t = setTimeout(function () { fn.apply(self, args); }, ms);
        };
    }

    function escapeHtml(s) {
        if (s == null) return '';
        return String(s)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function highlight(text, query) {
        if (!query) return escapeHtml(text);
        var safe = escapeHtml(text);
        var qSafe = escapeHtml(query).replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
        var re = new RegExp('(' + qSafe + ')', 'ig');
        return safe.replace(re, '<mark class="ff-ac__mark">$1</mark>');
    }

    function pickLabel(item) {
        if (item == null) return '';
        return item.label || item.name || item.fullName
            || item.Label || item.Name || item.FullName
            || '';
    }
    function pickId(item) {
        if (item == null) return '';
        return item.id != null ? item.id : (item.Id != null ? item.Id : '');
    }

    function setStatus(state, msg) {
        var node = state.popup.querySelector('[data-ff-ac-status]');
        if (!node) return;
        node.textContent = msg || '';
        node.style.display = msg ? '' : 'none';
    }

    function renderResults(state, items, query) {
        var list = state.popup.querySelector('[data-ff-ac-list]');
        list.innerHTML = '';
        state.activeIndex = -1;

        if (!items.length) {
            setStatus(state, 'Nema rezultata.');
            return;
        }
        setStatus(state, '');

        items.forEach(function (item, i) {
            var id = pickId(item);
            var label = pickLabel(item);
            var li = document.createElement('li');
            li.className = 'ff-ac__item';
            li.setAttribute('role', 'option');
            li.setAttribute('data-id', id);
            li.setAttribute('data-label', label);
            li.setAttribute('data-index', i);
            li.innerHTML = '<span class="ff-ac__item-label">' + highlight(label, query) + '</span>';
            list.appendChild(li);
        });
    }

    function openPopup(state) {
        if (state.isOpen) return;
        state.isOpen = true;
        state.popup.hidden = false;
        void state.popup.offsetWidth;
        state.popup.classList.add('ff-ac__popup--visible');
        state.visibleInput.setAttribute('aria-expanded', 'true');
    }
    function closePopup(state) {
        if (!state.isOpen) return;
        state.isOpen = false;
        state.popup.classList.remove('ff-ac__popup--visible');
        state.visibleInput.setAttribute('aria-expanded', 'false');
        setTimeout(function () {
            if (!state.isOpen) state.popup.hidden = true;
        }, 160);
    }

    function selectItem(state, id, label) {
        state.hiddenInput.value = id != null ? id : '';
        state.visibleInput.value = label || '';
        var clearBtn = state.root.querySelector('[data-ff-ac-clear]');
        if (clearBtn) clearBtn.style.display = label ? '' : 'none';
        var ev = new Event('change', { bubbles: true });
        state.hiddenInput.dispatchEvent(ev);
    }

    function fetchResults(state, query) {
        if (!state.endpoint) return;
        if (state.abortController) state.abortController.abort();

        if (typeof AbortController !== 'undefined') {
            state.abortController = new AbortController();
        }

        var url = state.endpoint + (state.endpoint.indexOf('?') > -1 ? '&' : '?') + 'q=' + encodeURIComponent(query);
        setStatus(state, 'Pretraživanje...');

        fetch(url, {
            method: 'GET',
            credentials: 'same-origin',
            headers: { 'Accept': 'application/json', 'X-Requested-With': 'XMLHttpRequest' },
            signal: state.abortController ? state.abortController.signal : undefined
        })
            .then(function (r) { if (!r.ok) throw new Error('HTTP ' + r.status); return r.json(); })
            .then(function (data) {
                var items = Array.isArray(data) ? data : (data && data.items ? data.items : []);
                state.lastResults = items;
                renderResults(state, items, query);
            })
            .catch(function (err) {
                if (err && err.name === 'AbortError') return;
                setStatus(state, 'Greška pri pretraživanju.');
                state.lastResults = [];
                var list = state.popup.querySelector('[data-ff-ac-list]');
                if (list) list.innerHTML = '';
            });
    }

    function setActive(state, idx) {
        var items = state.popup.querySelectorAll('.ff-ac__item');
        if (!items.length) return;
        if (idx < 0) idx = items.length - 1;
        if (idx >= items.length) idx = 0;
        state.activeIndex = idx;
        items.forEach(function (el, i) {
            el.classList.toggle('ff-ac__item--active', i === idx);
            if (i === idx) el.scrollIntoView({ block: 'nearest' });
        });
    }

    function initOne(root) {
        if (root.__ffAcInit) return;
        root.__ffAcInit = true;

        var visibleInput = root.querySelector('.ff-ac__input');
        var hiddenInput = root.querySelector('input[type="hidden"]');
        var popup = root.querySelector('.ff-ac__popup');
        if (!visibleInput || !hiddenInput || !popup) return;

        var state = {
            root: root,
            visibleInput: visibleInput,
            hiddenInput: hiddenInput,
            popup: popup,
            endpoint: root.getAttribute('data-endpoint') || '',
            freeText: root.getAttribute('data-free-text') === 'true',
            isOpen: false,
            activeIndex: -1,
            lastResults: [],
            abortController: null
        };

        var doFetch = debounce(function (q) { fetchResults(state, q); }, DEBOUNCE_MS);

        visibleInput.addEventListener('focus', function () {
            openPopup(state);
            if (!visibleInput.value.trim()) setStatus(state, 'Počni tipkati za pretragu...');
            else doFetch(visibleInput.value.trim());
        });

        visibleInput.addEventListener('input', function () {
            var q = visibleInput.value.trim();
            // If user clears, also clear hidden value unless FreeText
            if (!q) {
                state.hiddenInput.value = '';
                var clearBtn = root.querySelector('[data-ff-ac-clear]');
                if (clearBtn) clearBtn.style.display = 'none';
                setStatus(state, 'Počni tipkati za pretragu...');
                var list = popup.querySelector('[data-ff-ac-list]');
                if (list) list.innerHTML = '';
                openPopup(state);
                return;
            }

            if (state.freeText) {
                state.hiddenInput.value = visibleInput.value;
            }

            openPopup(state);
            doFetch(q);
        });

        visibleInput.addEventListener('keydown', function (e) {
            if (e.key === 'ArrowDown') {
                e.preventDefault();
                openPopup(state);
                setActive(state, state.activeIndex + 1);
            } else if (e.key === 'ArrowUp') {
                e.preventDefault();
                setActive(state, state.activeIndex - 1);
            } else if (e.key === 'Enter') {
                if (state.isOpen) {
                    var items = popup.querySelectorAll('.ff-ac__item');
                    if (state.activeIndex >= 0 && items[state.activeIndex]) {
                        e.preventDefault();
                        items[state.activeIndex].click();
                    } else if (state.freeText) {
                        // accept current text as value
                        e.preventDefault();
                        selectItem(state, visibleInput.value, visibleInput.value);
                        closePopup(state);
                    }
                }
            } else if (e.key === 'Escape') {
                if (state.isOpen) { e.preventDefault(); closePopup(state); }
            }
        });

        // Click on result
        popup.addEventListener('mousedown', function (e) {
            // mousedown to fire before blur
            var item = e.target.closest('.ff-ac__item');
            if (!item) return;
            e.preventDefault();
            var id = item.getAttribute('data-id');
            var label = item.getAttribute('data-label');
            selectItem(state, id, label);
            closePopup(state);
            visibleInput.focus();
        });

        // Hover highlight
        popup.addEventListener('mouseover', function (e) {
            var item = e.target.closest('.ff-ac__item');
            if (!item) return;
            var i = parseInt(item.getAttribute('data-index'), 10);
            if (!isNaN(i)) setActive(state, i);
        });

        // Blur — close popup; if not freetext and no valid selection, revert
        visibleInput.addEventListener('blur', function () {
            setTimeout(function () {
                if (document.activeElement && root.contains(document.activeElement)) return;
                if (!state.freeText) {
                    // If hidden has no id, clear visible to avoid mismatch
                    if (!state.hiddenInput.value) {
                        visibleInput.value = '';
                        var clearBtn = root.querySelector('[data-ff-ac-clear]');
                        if (clearBtn) clearBtn.style.display = 'none';
                    }
                }
                closePopup(state);
            }, 120);
        });

        // Clear button
        var clearBtn = root.querySelector('[data-ff-ac-clear]');
        if (clearBtn) {
            clearBtn.addEventListener('click', function (e) {
                e.preventDefault();
                selectItem(state, '', '');
                visibleInput.focus();
            });
        }

        // Outside click closes popup
        document.addEventListener('click', function (e) {
            if (!state.isOpen) return;
            if (root.contains(e.target)) return;
            closePopup(state);
        });
    }

    function init() {
        var nodes = document.querySelectorAll('[data-ff-autocomplete]');
        Array.prototype.forEach.call(nodes, initOne);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    window.FFAutocomplete = { init: init, initOne: initOne };
})();
