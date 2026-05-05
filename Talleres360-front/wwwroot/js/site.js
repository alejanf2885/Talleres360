// Toast notifications
function showToast(message, type = 'success', duration = 3500) {
    const container = document.getElementById('toast-container');
    if (!container) return;

    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.textContent = message;
    container.appendChild(toast);

    setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transition = 'opacity 0.3s ease';
        setTimeout(() => toast.remove(), 320);
    }, duration);
}

// Auto-mostrar toasts desde atributos data- en el body (inyectados desde _Layout)
document.addEventListener('DOMContentLoaded', () => {
    const success = document.body.dataset.toastSuccess;
    const error   = document.body.dataset.toastError;
    const warning = document.body.dataset.toastWarning;

    if (success) showToast(success, 'success');
    if (error)   showToast(error,   'error');
    if (warning) showToast(warning, 'warning');
});
