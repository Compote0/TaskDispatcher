(function () {
  "use strict";

  var STATUS_COLORS = {
    0: "#6c757d",
    1: "#0052CC",
    2: "#00B8D9",
    3: "#FFAB00",
    4: "#36B37E",
  };
  var STATUS_LABELS = {
    0: "Backlog",
    1: "À faire",
    2: "En cours",
    3: "En revue",
    4: "Terminé",
  };

  function showToast(msg, type) {
    var container = document.getElementById("toast-container");
    if (!container) return;
    var id = "toast-" + Date.now();
    var icon = type === "danger" ? "✕" : "✓";
    var borderColor =
      type === "danger" ? "rgba(220,53,69,.5)" : "var(--jira-border)";
    container.insertAdjacentHTML(
      "beforeend",
      '<div id="' +
        id +
        '" class="toast" role="alert" style="background:var(--jira-surface);border:1px solid ' +
        borderColor +
        ';color:var(--jira-text)">' +
        '<div class="d-flex align-items-center"><div class="toast-body">' +
        icon +
        " " +
        msg +
        "</div>" +
        '<button type="button" class="btn-close me-2 m-auto" data-bs-dismiss="toast" style="filter:invert(.6)"></button>' +
        "</div></div>",
    );
    var el = document.getElementById(id);
    var toast = new bootstrap.Toast(el, { delay: 2500 });
    toast.show();
    el.addEventListener("hidden.bs.toast", function () {
      el.remove();
    });
  }

  function getToken() {
    var tokenEl = document.querySelector("[name=__RequestVerificationToken]");
    return tokenEl ? tokenEl.value : "";
  }

  function updateCounts() {
    document.querySelectorAll(".board-column").forEach(function (col) {
      var visible = Array.from(
        col.querySelectorAll(".issue-card-wrapper"),
      ).filter(function (w) {
        return w.style.display !== "none";
      }).length;
      var badge = col.querySelector(".col-count");
      if (badge) badge.textContent = visible;
    });
  }

  function updatePickerUI(picker, statusNum) {
    var num = String(statusNum);
    var label = STATUS_LABELS[statusNum] || num;
    var color = STATUS_COLORS[statusNum] || "#6c757d";

    picker.dataset.statusNum = num;
    var btnDot = picker.querySelector(".status-picker-btn .status-dot");
    var btnLabel = picker.querySelector(".status-picker-label");
    if (btnDot) btnDot.style.background = color;
    if (btnLabel) btnLabel.textContent = label;

    picker.querySelectorAll(".status-picker-option").forEach(function (opt) {
      var isActive = opt.dataset.statusNum === num;
      opt.classList.toggle("active", isActive);
      var check = opt.querySelector(".status-picker-check");
      if (check) check.remove();
      if (isActive) {
        opt.insertAdjacentHTML(
          "beforeend",
          '<span class="status-picker-check ms-auto">✓</span>',
        );
      }
    });
  }

  function moveCardToColumn(issueId, newStatusNum) {
    var wrapper = document.querySelector(
      '.issue-card-wrapper[data-issue-id="' + issueId + '"]',
    );
    if (!wrapper) return null;
    var oldStatusNum = wrapper.dataset.statusNum;
    if (oldStatusNum === String(newStatusNum)) return null;

    var zone = document.querySelector(
      '.drop-zone[data-status-num="' + newStatusNum + '"]',
    );
    if (!zone) return null;

    var addBtn = zone.querySelector(".add-issue-btn");
    zone.insertBefore(wrapper, addBtn);
    wrapper.dataset.statusNum = String(newStatusNum);
    updateCounts();
    return oldStatusNum;
  }

  function moveIssueStatus(issueId, newStatusNum, onSuccess, onError) {
    fetch("/Issues/MoveStatus", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        RequestVerificationToken: getToken(),
      },
      body: JSON.stringify({ id: issueId, status: newStatusNum }),
    })
      .then(function (res) {
        if (!res.ok) throw new Error("error");
        if (onSuccess) onSuccess();
      })
      .catch(function () {
        if (onError) onError();
      });
  }

  document.querySelectorAll(".status-picker").forEach(function (picker) {
    picker.addEventListener("mousedown", function (e) {
      e.stopPropagation();
    });
    picker.addEventListener("dragstart", function (e) {
      e.preventDefault();
    });
  });

  document.addEventListener("click", function (e) {
    var opt = e.target.closest(".status-picker-option");
    if (!opt) return;

    e.preventDefault();
    e.stopPropagation();

    var issueId = parseInt(opt.dataset.issueId);
    var newStatusNum = parseInt(opt.dataset.statusNum);
    var picker = opt.closest(".status-picker");
    var oldStatusNum = parseInt(picker.dataset.statusNum);

    if (oldStatusNum === newStatusNum) {
      bootstrap.Dropdown.getOrCreateInstance(
        picker.querySelector('[data-bs-toggle="dropdown"]'),
      ).hide();
      return;
    }

    document
      .querySelectorAll('.status-picker[data-issue-id="' + issueId + '"]')
      .forEach(function (p) {
        updatePickerUI(p, newStatusNum);
      });

    var cardOldStatus = moveCardToColumn(issueId, newStatusNum);

    bootstrap.Dropdown.getOrCreateInstance(
      picker.querySelector('[data-bs-toggle="dropdown"]'),
    ).hide();

    moveIssueStatus(
      issueId,
      newStatusNum,
      function () {
        showToast("Statut → " + (STATUS_LABELS[newStatusNum] || newStatusNum));
      },
      function () {
        document
          .querySelectorAll('.status-picker[data-issue-id="' + issueId + '"]')
          .forEach(function (p) {
            updatePickerUI(p, oldStatusNum);
          });
        if (cardOldStatus !== null) moveCardToColumn(issueId, cardOldStatus);
        showToast("Erreur lors de la mise à jour", "danger");
      },
    );
  });

  var dragging = null;

  document.querySelectorAll(".issue-card-wrapper").forEach(function (card) {
    card.addEventListener("dragstart", function (e) {
      if (e.target.closest(".status-picker")) {
        e.preventDefault();
        return;
      }
      dragging = card;
      e.dataTransfer.effectAllowed = "move";
      e.dataTransfer.setData("text/plain", card.dataset.issueId);
      setTimeout(function () {
        card.classList.add("is-dragging");
      }, 0);
    });
    card.addEventListener("dragend", function () {
      card.classList.remove("is-dragging");
      document.querySelectorAll(".drop-zone").forEach(function (z) {
        z.classList.remove("drag-over");
      });
      dragging = null;
    });
  });

  document.querySelectorAll(".drop-zone").forEach(function (zone) {
    zone.addEventListener("dragenter", function (e) {
      e.preventDefault();
    });
    zone.addEventListener("dragover", function (e) {
      e.preventDefault();
      e.dataTransfer.dropEffect = "move";
      zone.classList.add("drag-over");
    });
    zone.addEventListener("dragleave", function (e) {
      if (!zone.contains(e.relatedTarget)) zone.classList.remove("drag-over");
    });
    zone.addEventListener("drop", function (e) {
      e.preventDefault();
      zone.classList.remove("drag-over");
      if (!dragging) return;

      var oldStatusNum = parseInt(dragging.dataset.statusNum);
      var newStatusNum = parseInt(zone.dataset.statusNum);
      if (oldStatusNum === newStatusNum) return;

      var movedCard = dragging;
      var issueId = parseInt(movedCard.dataset.issueId);

      moveCardToColumn(issueId, newStatusNum);
      document
        .querySelectorAll('.status-picker[data-issue-id="' + issueId + '"]')
        .forEach(function (p) {
          updatePickerUI(p, newStatusNum);
        });

      moveIssueStatus(
        issueId,
        newStatusNum,
        function () {
          showToast("Statut mis à jour");
        },
        function () {
          moveCardToColumn(issueId, oldStatusNum);
          document
            .querySelectorAll('.status-picker[data-issue-id="' + issueId + '"]')
            .forEach(function (p) {
              updatePickerUI(p, oldStatusNum);
            });
          showToast("Erreur lors de la mise à jour", "danger");
        },
      );
    });
  });

  var searchInput = document.getElementById("board-search");
  var priorityFilter = document.getElementById("board-priority");
  var typeFilter = document.getElementById("board-type");

  function applyFilters() {
    var q = searchInput ? searchInput.value.toLowerCase().trim() : "";
    var priority = priorityFilter ? priorityFilter.value : "";
    var type = typeFilter ? typeFilter.value : "";
    document.querySelectorAll(".issue-card-wrapper").forEach(function (card) {
      var ok = true;
      if (q && card.dataset.title.indexOf(q) === -1) ok = false;
      if (priority && card.dataset.priority !== priority) ok = false;
      if (type && card.dataset.type !== type) ok = false;
      card.style.display = ok ? "" : "none";
    });
    updateCounts();
  }

  function clearFilters() {
    if (searchInput) searchInput.value = "";
    if (priorityFilter) priorityFilter.value = "";
    if (typeFilter) typeFilter.value = "";
    applyFilters();
  }

  if (searchInput) searchInput.addEventListener("input", applyFilters);
  if (priorityFilter) priorityFilter.addEventListener("change", applyFilters);
  if (typeFilter) typeFilter.addEventListener("change", applyFilters);

  // ── Inline comment editing ──
  window.editComment = function (id) {
    var view = document.getElementById("comment-view-" + id);
    var form = document.getElementById("comment-edit-" + id);
    if (!view || !form) return;
    view.style.display = "none";
    form.style.display = "block";
    var ta = form.querySelector("textarea");
    if (ta) { ta.focus(); ta.selectionStart = ta.value.length; }
  };

  window.cancelCommentEdit = function (id) {
    var view = document.getElementById("comment-view-" + id);
    var form = document.getElementById("comment-edit-" + id);
    if (!view || !form) return;
    view.style.display = "";
    form.style.display = "none";
  };

  // ── Add-comment expand/collapse ──
  var addCommentInput = document.querySelector(".add-comment-input");
  var commentSubmit = document.querySelector(".comment-submit");
  if (addCommentInput && commentSubmit) {
    addCommentInput.addEventListener("focus", function () {
      commentSubmit.classList.remove("d-none");
      addCommentInput.rows = 3;
    });
    var cancelBtn = document.querySelector(".js-comment-cancel");
    if (cancelBtn) {
      cancelBtn.addEventListener("click", function () {
        addCommentInput.value = "";
        addCommentInput.rows = 1;
        commentSubmit.classList.add("d-none");
        addCommentInput.blur();
      });
    }
  }

  document.addEventListener("keydown", function (e) {
    var tag = document.activeElement ? document.activeElement.tagName : "";
    if (tag === "INPUT" || tag === "TEXTAREA" || tag === "SELECT") return;

    if (e.key === "?") {
      var modal = document.getElementById("shortcutsModal");
      if (modal) bootstrap.Modal.getOrCreateInstance(modal).show();
      return;
    }
    if ((e.key === "c" || e.key === "C") && !e.ctrlKey && !e.metaKey) {
      var header = document.querySelector("[data-create-url]");
      if (header) window.location.href = header.dataset.createUrl;
      return;
    }
    if (e.key === "/") {
      e.preventDefault();
      var el =
        document.querySelector(".jira-search-input") ||
        document.getElementById("board-search");
      if (el) el.focus();
      return;
    }
    if (e.key === "Escape") {
      clearFilters();
    }
  });
})();
