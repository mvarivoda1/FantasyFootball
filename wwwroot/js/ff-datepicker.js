/* =====================================================
   FF Datepicker — custom vanilla JS popup calendar
   Auto-inits all [data-ff-datepicker] elements.
   No jQuery, no external plugins, no native date controls.
   Supports hr (dd.MM.yyyy) and en (MM/dd/yyyy) locales.
   ===================================================== */
(function () {
    'use strict';

    var LOCALES = {
        hr: {
            firstDay: 1, // ponedjeljak
            months: ['Siječanj', 'Veljača', 'Ožujak', 'Travanj', 'Svibanj', 'Lipanj',
                     'Srpanj', 'Kolovoz', 'Rujan', 'Listopad', 'Studeni', 'Prosinac'],
            weekdaysShort: ['Pon', 'Uto', 'Sri', 'Čet', 'Pet', 'Sub', 'Ned'],
            today: 'Danas',
            clear: 'Obriši',
            close: 'Zatvori',
            invalid: 'Neispravan datum ili vrijeme.',
            am: '', pm: ''
        },
        en: {
            firstDay: 0, // sunday
            months: ['January', 'February', 'March', 'April', 'May', 'June',
                     'July', 'August', 'September', 'October', 'November', 'December'],
            weekdaysShort: ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
            today: 'Today',
            clear: 'Clear',
            close: 'Close',
            invalid: 'Invalid date or time.',
            am: 'AM', pm: 'PM'
        }
    };

    function pad2(n) { return n < 10 ? '0' + n : '' + n; }

    function detectLocale(root) {
        var l = root.getAttribute('data-locale');
        if (l && LOCALES[l]) return l;
        var html = document.documentElement.lang || '';
        if (html.toLowerCase().indexOf('hr') === 0) return 'hr';
        return 'en';
    }

    function formatDate(d, format, locale) {
        if (!d) return '';
        var loc = LOCALES[locale];
        var hh = d.getHours();
        var ampm = hh >= 12 ? 'PM' : 'AM';
        var hh12 = hh % 12; if (hh12 === 0) hh12 = 12;

        return format
            .replace('dd', pad2(d.getDate()))
            .replace('MM', pad2(d.getMonth() + 1))
            .replace('yyyy', d.getFullYear())
            .replace('HH', pad2(d.getHours()))
            .replace('hh', pad2(hh12))
            .replace('mm', pad2(d.getMinutes()))
            .replace('tt', ampm);
    }

    function parseDate(text, format, includeTime) {
        if (!text) return null;
        text = text.trim();
        if (!text) return null;

        // Build regex from format substituting tokens
        var tokenRe = /(dd|MM|yyyy|HH|hh|mm|tt)/g;
        var tokens = [];
        var pattern = '^' + format.replace(/[.*+?^${}()|[\]\\\/]/g, '\\$&');
        pattern = pattern.replace(tokenRe, function (tok) {
            tokens.push(tok);
            switch (tok) {
                case 'yyyy': return '(\\d{4})';
                case 'dd':
                case 'MM':
                case 'HH':
                case 'hh':
                case 'mm':  return '(\\d{1,2})';
                case 'tt':  return '(AM|PM|am|pm)';
            }
            return tok;
        }) + '$';

        var re = new RegExp(pattern);
        var m = text.match(re);
        if (!m) return null;

        var year = 0, month = 0, day = 1, hour = 0, minute = 0, isPm = false, is12 = false;
        for (var i = 0; i < tokens.length; i++) {
            var val = m[i + 1];
            switch (tokens[i]) {
                case 'yyyy': year = parseInt(val, 10); break;
                case 'MM':   month = parseInt(val, 10) - 1; break;
                case 'dd':   day = parseInt(val, 10); break;
                case 'HH':   hour = parseInt(val, 10); if (hour < 0 || hour > 23) return null; break;
                case 'hh':   hour = parseInt(val, 10); is12 = true; if (hour < 1 || hour > 12) return null; break;
                case 'mm':   minute = parseInt(val, 10); if (minute < 0 || minute > 59) return null; break;
                case 'tt':   isPm = val.toUpperCase() === 'PM'; break;
            }
        }
        if (is12) {
            if (hour === 12) hour = 0;
            if (isPm) hour += 12;
        }

        var d = new Date(year, month, day, hour, minute, 0, 0);
        if (isNaN(d.getTime())) return null;
        if (d.getFullYear() !== year || d.getMonth() !== month || d.getDate() !== day) return null;
        return d;
    }

    function toIso(d, includeTime) {
        if (!d) return '';
        var base = d.getFullYear() + '-' + pad2(d.getMonth() + 1) + '-' + pad2(d.getDate());
        if (!includeTime) return base;
        return base + 'T' + pad2(d.getHours()) + ':' + pad2(d.getMinutes()) + ':00';
    }

    function fromIso(iso) {
        if (!iso) return null;
        var d;
        if (iso.indexOf('T') > -1) {
            d = new Date(iso);
        } else {
            // Pure date: parse as local to avoid timezone shifts
            var parts = iso.split('-');
            if (parts.length !== 3) return null;
            d = new Date(parseInt(parts[0], 10), parseInt(parts[1], 10) - 1, parseInt(parts[2], 10));
        }
        return isNaN(d.getTime()) ? null : d;
    }

    function sameDay(a, b) {
        return a && b
            && a.getFullYear() === b.getFullYear()
            && a.getMonth() === b.getMonth()
            && a.getDate() === b.getDate();
    }

    function buildPopup(state) {
        var loc = LOCALES[state.locale];
        var popup = document.createElement('div');
        popup.className = 'ff-dp__popup';
        popup.setAttribute('role', 'dialog');
        popup.setAttribute('aria-label', 'Kalendar');

        // Header (nav)
        var header = document.createElement('div');
        header.className = 'ff-dp__header';
        header.innerHTML =
            '<button type="button" class="ff-dp__nav-btn" data-nav="prev" aria-label="Prethodni mjesec"><i class="bi bi-chevron-left"></i></button>' +
            '<div class="ff-dp__title"><span data-role="month-label"></span> <span data-role="year-label"></span></div>' +
            '<button type="button" class="ff-dp__nav-btn" data-nav="next" aria-label="Sljedeći mjesec"><i class="bi bi-chevron-right"></i></button>';
        popup.appendChild(header);

        // Weekday row
        var wkRow = document.createElement('div');
        wkRow.className = 'ff-dp__weekdays';
        var wdays = loc.weekdaysShort.slice();
        if (loc.firstDay === 1) {
            // shift Sunday to end
            wdays.push(wdays.shift());
        }
        wdays.forEach(function (n) {
            var c = document.createElement('span');
            c.className = 'ff-dp__weekday';
            c.textContent = n;
            wkRow.appendChild(c);
        });
        popup.appendChild(wkRow);

        // Days grid
        var grid = document.createElement('div');
        grid.className = 'ff-dp__days';
        grid.setAttribute('role', 'grid');
        popup.appendChild(grid);

        // Time row (optional)
        if (state.includeTime) {
            var timeRow = document.createElement('div');
            timeRow.className = 'ff-dp__time';
            timeRow.innerHTML =
                '<span class="ff-dp__time-label"><i class="bi bi-clock"></i></span>' +
                '<input type="number" class="ff-dp__time-input" data-role="hour" min="0" max="23" aria-label="Sat" />' +
                '<span class="ff-dp__time-sep">:</span>' +
                '<input type="number" class="ff-dp__time-input" data-role="minute" min="0" max="59" aria-label="Minuta" />';
            popup.appendChild(timeRow);
        }

        // Footer (actions)
        var footer = document.createElement('div');
        footer.className = 'ff-dp__footer';
        footer.innerHTML =
            '<button type="button" class="ff-dp__action" data-action="today">' + loc.today + '</button>' +
            '<button type="button" class="ff-dp__action" data-action="clear">' + loc.clear + '</button>';
        popup.appendChild(footer);

        return popup;
    }

    function renderMonth(state) {
        var loc = LOCALES[state.locale];
        var year = state.viewYear, month = state.viewMonth;

        state.popup.querySelector('[data-role="month-label"]').textContent = loc.months[month];
        state.popup.querySelector('[data-role="year-label"]').textContent = year;

        var first = new Date(year, month, 1);
        var firstDow = first.getDay(); // 0=Ned
        var offset = (firstDow - loc.firstDay + 7) % 7;
        var daysInMonth = new Date(year, month + 1, 0).getDate();
        var prevMonthDays = new Date(year, month, 0).getDate();

        var grid = state.popup.querySelector('.ff-dp__days');
        grid.innerHTML = '';

        var today = new Date();
        var totalCells = Math.ceil((offset + daysInMonth) / 7) * 7;

        for (var i = 0; i < totalCells; i++) {
            var dayNum, isOther = false, cellDate;
            if (i < offset) {
                dayNum = prevMonthDays - offset + i + 1;
                isOther = true;
                cellDate = new Date(year, month - 1, dayNum);
            } else if (i >= offset + daysInMonth) {
                dayNum = i - offset - daysInMonth + 1;
                isOther = true;
                cellDate = new Date(year, month + 1, dayNum);
            } else {
                dayNum = i - offset + 1;
                cellDate = new Date(year, month, dayNum);
            }

            var btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'ff-dp__day';
            if (isOther) btn.classList.add('ff-dp__day--other');
            if (sameDay(cellDate, today)) btn.classList.add('ff-dp__day--today');
            if (state.selected && sameDay(cellDate, state.selected)) {
                btn.classList.add('ff-dp__day--selected');
                btn.setAttribute('aria-selected', 'true');
            }
            btn.textContent = dayNum;
            btn.setAttribute('data-date', cellDate.getFullYear() + '-' + (cellDate.getMonth() + 1) + '-' + cellDate.getDate());
            btn.setAttribute('aria-label', formatDate(cellDate, state.format.split(' ')[0], state.locale));
            grid.appendChild(btn);
        }

        // Update time inputs if present
        if (state.includeTime && state.selected) {
            var hInput = state.popup.querySelector('[data-role="hour"]');
            var mInput = state.popup.querySelector('[data-role="minute"]');
            if (hInput) hInput.value = pad2(state.selected.getHours());
            if (mInput) mInput.value = pad2(state.selected.getMinutes());
        }
    }

    function openPopup(state) {
        if (state.isOpen) return;
        state.isOpen = true;
        state.visibleInput.setAttribute('aria-expanded', 'true');
        state.root.classList.add('ff-dp--open');

        // Sync view to selected or today
        var nav = state.selected || new Date();
        state.viewYear = nav.getFullYear();
        state.viewMonth = nav.getMonth();

        renderMonth(state);
        state.popup.hidden = false;
        // Force reflow before animation class
        void state.popup.offsetWidth;
        state.popup.classList.add('ff-dp__popup--visible');
    }

    function closePopup(state) {
        if (!state.isOpen) return;
        state.isOpen = false;
        state.visibleInput.setAttribute('aria-expanded', 'false');
        state.root.classList.remove('ff-dp--open');
        state.popup.classList.remove('ff-dp__popup--visible');
        // Hide after transition
        setTimeout(function () {
            if (!state.isOpen) state.popup.hidden = true;
        }, 180);
    }

    function showError(state) {
        if (!state.errorEl) return;
        state.errorEl.textContent = LOCALES[state.locale].invalid;
        state.errorEl.hidden = false;
        state.root.classList.add('ff-dp--invalid');
        state.visibleInput.setAttribute('aria-invalid', 'true');
        state.visibleInput.setAttribute('aria-describedby', state.errorId);
    }

    function clearError(state) {
        if (!state.errorEl) return;
        if (!state.root.classList.contains('ff-dp--invalid')) return;
        state.errorEl.hidden = true;
        state.errorEl.textContent = '';
        state.root.classList.remove('ff-dp--invalid');
        state.visibleInput.removeAttribute('aria-invalid');
        state.visibleInput.removeAttribute('aria-describedby');
    }

    function commitSelection(state, d) {
        clearError(state);
        state.selected = d;
        if (d) {
            state.hiddenInput.value = toIso(d, state.includeTime);
            state.visibleInput.value = formatDate(d, state.format, state.locale);
        } else {
            state.hiddenInput.value = '';
            state.visibleInput.value = '';
        }
        // Fire change event so any external code can react
        var ev = new Event('change', { bubbles: true });
        state.hiddenInput.dispatchEvent(ev);
    }

    function attemptParseFromVisible(state) {
        var text = state.visibleInput.value.trim();
        if (!text) {
            commitSelection(state, null);
            return;
        }
        var parsed = parseDate(text, state.format, state.includeTime);
        if (parsed) {
            commitSelection(state, parsed);
        } else {
            // Invalid input: keep the user's text so they can fix it, but
            // ensure no stale/alternative value is submitted, and show an error.
            state.hiddenInput.value = '';
            state.hiddenInput.dispatchEvent(new Event('change', { bubbles: true }));
            showError(state);
        }
    }

    function initOne(root) {
        if (root.__ffDpInit) return;
        root.__ffDpInit = true;

        var visibleInput = root.querySelector('.ff-dp__input');
        var hiddenInput = root.querySelector('input[type="hidden"]');
        if (!visibleInput || !hiddenInput) return;

        var state = {
            root: root,
            visibleInput: visibleInput,
            hiddenInput: hiddenInput,
            locale: detectLocale(root),
            format: root.getAttribute('data-format') || 'dd.MM.yyyy',
            includeTime: root.getAttribute('data-include-time') === 'true',
            isOpen: false,
            selected: fromIso(hiddenInput.value),
            viewYear: 0,
            viewMonth: 0,
            popup: null,
            errorEl: null,
            errorId: null
        };

        // Build popup
        state.popup = buildPopup(state);
        root.appendChild(state.popup);
        state.popup.hidden = true;

        // Inline error element (below the field)
        state.errorId = (root.id || 'ffdp') + '_error';
        var errorEl = document.createElement('div');
        errorEl.className = 'ff-dp__error';
        errorEl.id = state.errorId;
        errorEl.setAttribute('role', 'alert');
        errorEl.hidden = true;
        root.appendChild(errorEl);
        state.errorEl = errorEl;

        // Toggle button
        var toggleBtn = root.querySelector('[data-ff-dp-toggle]');
        if (toggleBtn) {
            toggleBtn.addEventListener('click', function (e) {
                e.preventDefault();
                if (state.isOpen) closePopup(state); else openPopup(state);
            });
        }

        // Visible input handling
        visibleInput.addEventListener('focus', function () { clearError(state); openPopup(state); });
        visibleInput.addEventListener('input', function () { clearError(state); });
        visibleInput.addEventListener('blur', function () {
            setTimeout(function () { attemptParseFromVisible(state); }, 50);
        });
        visibleInput.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') { e.preventDefault(); closePopup(state); }
            else if (e.key === 'ArrowDown' && state.isOpen) {
                e.preventDefault();
                var firstDay = state.popup.querySelector('.ff-dp__day:not(.ff-dp__day--other)');
                if (firstDay) firstDay.focus();
            }
        });

        // Nav buttons
        state.popup.addEventListener('click', function (e) {
            var navBtn = e.target.closest('[data-nav]');
            if (navBtn) {
                e.preventDefault();
                var dir = navBtn.getAttribute('data-nav');
                if (dir === 'prev') {
                    state.viewMonth--;
                    if (state.viewMonth < 0) { state.viewMonth = 11; state.viewYear--; }
                } else {
                    state.viewMonth++;
                    if (state.viewMonth > 11) { state.viewMonth = 0; state.viewYear++; }
                }
                renderMonth(state);
                return;
            }

            var dayBtn = e.target.closest('.ff-dp__day');
            if (dayBtn) {
                e.preventDefault();
                var parts = dayBtn.getAttribute('data-date').split('-');
                var h = 0, m = 0;
                if (state.includeTime) {
                    var hInp = state.popup.querySelector('[data-role="hour"]');
                    var mInp = state.popup.querySelector('[data-role="minute"]');
                    h = parseInt(hInp && hInp.value, 10); if (isNaN(h)) h = 0;
                    m = parseInt(mInp && mInp.value, 10); if (isNaN(m)) m = 0;
                    if (h < 0) h = 0; if (h > 23) h = 23;
                    if (m < 0) m = 0; if (m > 59) m = 59;
                }
                var newDate = new Date(
                    parseInt(parts[0], 10),
                    parseInt(parts[1], 10) - 1,
                    parseInt(parts[2], 10),
                    h, m, 0, 0
                );
                commitSelection(state, newDate);
                if (!state.includeTime) closePopup(state);
                else renderMonth(state); // refresh selected highlight
                return;
            }

            var action = e.target.closest('[data-action]');
            if (action) {
                e.preventDefault();
                if (action.getAttribute('data-action') === 'today') {
                    var t = new Date();
                    if (!state.includeTime) { t.setHours(0,0,0,0); }
                    commitSelection(state, t);
                    state.viewYear = t.getFullYear();
                    state.viewMonth = t.getMonth();
                    renderMonth(state);
                    if (!state.includeTime) closePopup(state);
                } else if (action.getAttribute('data-action') === 'clear') {
                    commitSelection(state, null);
                    renderMonth(state);
                    closePopup(state);
                }
            }
        });

        // Time input changes
        if (state.includeTime) {
            state.popup.addEventListener('input', function (e) {
                var t = e.target;
                if (t.matches('[data-role="hour"], [data-role="minute"]')) {
                    if (!state.selected) return;
                    var h = parseInt(state.popup.querySelector('[data-role="hour"]').value, 10);
                    var m = parseInt(state.popup.querySelector('[data-role="minute"]').value, 10);
                    if (isNaN(h)) h = 0; if (isNaN(m)) m = 0;
                    h = Math.max(0, Math.min(23, h));
                    m = Math.max(0, Math.min(59, m));
                    var d = new Date(state.selected);
                    d.setHours(h, m, 0, 0);
                    commitSelection(state, d);
                }
            });
        }

        // Keyboard nav within grid
        state.popup.addEventListener('keydown', function (e) {
            var focused = document.activeElement;
            if (!focused || !focused.classList.contains('ff-dp__day')) return;
            var idx = Array.prototype.indexOf.call(focused.parentNode.children, focused);
            var move = 0;
            if (e.key === 'ArrowLeft') move = -1;
            else if (e.key === 'ArrowRight') move = 1;
            else if (e.key === 'ArrowUp') move = -7;
            else if (e.key === 'ArrowDown') move = 7;
            else if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); focused.click(); return; }
            else if (e.key === 'Escape') { e.preventDefault(); closePopup(state); state.visibleInput.focus(); return; }
            else return;
            e.preventDefault();
            var target = focused.parentNode.children[idx + move];
            if (target) target.focus();
        });

        // Outside click closes
        document.addEventListener('click', function (e) {
            if (!state.isOpen) return;
            if (root.contains(e.target)) return;
            closePopup(state);
        });
    }

    function init() {
        var nodes = document.querySelectorAll('[data-ff-datepicker]');
        Array.prototype.forEach.call(nodes, initOne);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // Expose minimal API for dynamic init
    window.FFDatePicker = { init: init, initOne: initOne };
})();
