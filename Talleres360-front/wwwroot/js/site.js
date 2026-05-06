// Sidebar toggle (mobile)
function openSidebar() {
    const sidebar  = document.getElementById('sidebar');
    const overlay  = document.getElementById('sidebar-overlay');
    if (!sidebar) return;
    sidebar.classList.add('open');
    overlay?.classList.remove('d-none');
    document.body.style.overflow = 'hidden';
}

function closeSidebar() {
    const sidebar  = document.getElementById('sidebar');
    const overlay  = document.getElementById('sidebar-overlay');
    if (!sidebar) return;
    sidebar.classList.remove('open');
    overlay?.classList.add('d-none');
    document.body.style.overflow = '';
}

// Cerrar sidebar con Escape
document.addEventListener('keydown', e => {
    if (e.key === 'Escape') closeSidebar();
});

// Toast notifications
function showToast(message, type = 'success', duration = 3500) {
    const container = document.getElementById('toast-container');
    if (!container) return;

    const toast = document.createElement('div');
    toast.className = `t360-toast t360-toast-${type}`;
    toast.textContent = message;
    container.appendChild(toast);

    setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transition = 'opacity 0.3s ease';
        setTimeout(() => toast.remove(), 320);
    }, duration);
}

// ── Confirm Modal ─────────────────────────────────────────
let _confirmCallback = null;

const CONFIRM_ICONS = {
    danger: `<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/><path d="M10 11v6"/><path d="M14 11v6"/><path d="M9 6V4h6v2"/></svg>`,
    primary: `<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="12" y1="18" x2="12" y2="12"/><line x1="9" y1="15" x2="15" y2="15"/></svg>`
};

function askConfirm(title, msg, onConfirm, variant = 'danger') {
    const modal    = document.getElementById('confirmModal');
    const okBtn    = document.getElementById('confirmModalOk');
    const iconEl   = document.getElementById('confirmModalIcon');

    if (!modal) { if (onConfirm) onConfirm(); return; }

    document.getElementById('confirmModalTitle').textContent = title;
    document.getElementById('confirmModalMsg').textContent   = msg || '';

    iconEl.className = `confirm-modal-icon confirm-modal-icon--${variant}`;
    iconEl.innerHTML = CONFIRM_ICONS[variant] || CONFIRM_ICONS.danger;

    okBtn.className = `btn btn-${variant === 'primary' ? 'primary' : 'danger'} btn-md`;
    okBtn.textContent = variant === 'primary' ? 'Confirmar' : 'Eliminar';

    _confirmCallback = onConfirm;
    bootstrap.Modal.getOrCreateInstance(modal).show();
}

document.addEventListener('DOMContentLoaded', () => {
    const okBtn = document.getElementById('confirmModalOk');
    okBtn?.addEventListener('click', () => {
        bootstrap.Modal.getInstance(document.getElementById('confirmModal'))?.hide();
        _confirmCallback?.();
        _confirmCallback = null;
    });

    // Interceptar formularios con data-confirm-title
    document.addEventListener('submit', e => {
        const form = e.target;
        const title = form.dataset.confirmTitle;
        if (!title) return;
        e.preventDefault();
        const variant = form.dataset.confirmVariant || 'danger';
        askConfirm(title, form.dataset.confirmMsg || '', () => {
            delete form.dataset.confirmTitle;
            form.requestSubmit ? form.requestSubmit() : form.submit();
        }, variant);
    });
});
