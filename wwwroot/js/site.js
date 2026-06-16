(function () {
    'use strict';

    /* ── Toast ── */
    function showToast(msg, type) {
        var container = document.getElementById('toast-container');
        if (!container) return;
        var id = 'toast-' + Date.now();
        var icon = type === 'danger' ? '✕' : '✓';
        var borderColor = type === 'danger' ? 'rgba(220,53,69,.5)' : 'var(--jira-border)';
        container.insertAdjacentHTML('beforeend',
            '<div id="' + id + '" class="toast" role="alert" style="background:var(--jira-surface);border:1px solid ' + borderColor + ';color:var(--jira-text)">' +
            '<div class="d-flex align-items-center"><div class="toast-body">' + icon + ' ' + msg + '</div>' +
            '<button type="button" class="btn-close me-2 m-auto" data-bs-dismiss="toast" style="filter:invert(.6)"></button>' +
            '</div></div>');
        var el = document.getElementById(id);
        var toast = new bootstrap.Toast(el, { delay: 2500 });
        toast.show();
        el.addEventListener('hidden.bs.toast', function () { el.remove(); });
    }

    /* ── Board: count visible cards per column ── */
    function updateCounts() {
        document.querySelectorAll('.board-column').forEach(function (col) {
            var visible = Array.from(col.querySelectorAll('.issue-card-wrapper'))
                .filter(function (w) { return w.style.display !== 'none'; }).length;
            var badge = col.querySelector('.col-count');
            if (badge) badge.textContent = visible;
        });
    }

    /* ── Drag & Drop ── */
    var dragging = null;

    document.querySelectorAll('.issue-card-wrapper').forEach(function (card) {
        card.addEventListener('dragstart', function (e) {
            dragging = card;
            e.dataTransfer.effectAllowed = 'move';
            e.dataTransfer.setData('text/plain', card.dataset.issueId);
            setTimeout(function () { card.classList.add('is-dragging'); }, 0);
        });
        card.addEventListener('dragend', function () {
            card.classList.remove('is-dragging');
            document.querySelectorAll('.drop-zone').forEach(function (z) { z.classList.remove('drag-over'); });
            dragging = null;
        });
    });

    document.querySelectorAll('.drop-zone').forEach(function (zone) {
        zone.addEventListener('dragenter', function (e) { e.preventDefault(); });
        zone.addEventListener('dragover', function (e) {
            e.preventDefault();
            e.dataTransfer.dropEffect = 'move';
            zone.classList.add('drag-over');
        });
        zone.addEventListener('dragleave', function (e) {
            if (!zone.contains(e.relatedTarget)) zone.classList.remove('drag-over');
        });
        zone.addEventListener('drop', function (e) {
            e.preventDefault();
            zone.classList.remove('drag-over');
            if (!dragging) return;

            var oldStatusNum = dragging.dataset.statusNum;
            var newStatusNum = zone.dataset.statusNum;
            if (oldStatusNum === newStatusNum) return;

            var movedCard = dragging;
            var issueId = parseInt(movedCard.dataset.issueId);
            var newStatus = parseInt(newStatusNum);

            var addBtn = zone.querySelector('.add-issue-btn');
            zone.insertBefore(movedCard, addBtn);
            movedCard.dataset.statusNum = newStatusNum;
            updateCounts();

            var tokenEl = document.querySelector('[name=__RequestVerificationToken]');
            var token = tokenEl ? tokenEl.value : '';

            fetch('/Issues/MoveStatus', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
                body: JSON.stringify({ id: issueId, status: newStatus })
            }).then(function (res) {
                if (!res.ok) throw new Error('error');
                showToast('Statut mis à jour');
            }).catch(function () {
                var origZone = document.querySelector('.drop-zone[data-status-num="' + oldStatusNum + '"]');
                if (origZone) {
                    origZone.insertBefore(movedCard, origZone.querySelector('.add-issue-btn'));
                    movedCard.dataset.statusNum = oldStatusNum;
                }
                updateCounts();
                showToast('Erreur lors de la mise à jour', 'danger');
            });
        });
    });

    /* ── Board filters ── */
    var searchInput = document.getElementById('board-search');
    var priorityFilter = document.getElementById('board-priority');
    var typeFilter = document.getElementById('board-type');

    function applyFilters() {
        var q = searchInput ? searchInput.value.toLowerCase().trim() : '';
        var priority = priorityFilter ? priorityFilter.value : '';
        var type = typeFilter ? typeFilter.value : '';
        document.querySelectorAll('.issue-card-wrapper').forEach(function (card) {
            var ok = true;
            if (q && card.dataset.title.indexOf(q) === -1) ok = false;
            if (priority && card.dataset.priority !== priority) ok = false;
            if (type && card.dataset.type !== type) ok = false;
            card.style.display = ok ? '' : 'none';
        });
        updateCounts();
    }

    function clearFilters() {
        if (searchInput) searchInput.value = '';
        if (priorityFilter) priorityFilter.value = '';
        if (typeFilter) typeFilter.value = '';
        applyFilters();
    }

    if (searchInput) searchInput.addEventListener('input', applyFilters);
    if (priorityFilter) priorityFilter.addEventListener('change', applyFilters);
    if (typeFilter) typeFilter.addEventListener('change', applyFilters);

    /* ── Keyboard shortcuts ── */
    document.addEventListener('keydown', function (e) {
        var tag = document.activeElement ? document.activeElement.tagName : '';
        if (tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT') return;

        if (e.key === '?') {
            var modal = document.getElementById('shortcutsModal');
            if (modal) bootstrap.Modal.getOrCreateInstance(modal).show();
            return;
        }
        if ((e.key === 'c' || e.key === 'C') && !e.ctrlKey && !e.metaKey) {
            var header = document.querySelector('[data-create-url]');
            if (header) window.location.href = header.dataset.createUrl;
            return;
        }
        if (e.key === '/') {
            e.preventDefault();
            var el = document.querySelector('.jira-search-input') || document.getElementById('board-search');
            if (el) el.focus();
            return;
        }
        if (e.key === 'Escape') { clearFilters(); }
    });
})();
