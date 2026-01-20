// Tracker - Site JavaScript

// Toast notification helper
function showToast(message, type = 'success') {
    const container = document.querySelector('.toast-container') || createToastContainer();
    
    const toastEl = document.createElement('div');
    toastEl.className = `toast align-items-center text-white bg-${type} border-0`;
    toastEl.setAttribute('role', 'alert');
    toastEl.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">${message}</div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>
    `;
    
    container.appendChild(toastEl);
    const toast = new bootstrap.Toast(toastEl, { autohide: true, delay: 3000 });
    toast.show();
    
    toastEl.addEventListener('hidden.bs.toast', () => toastEl.remove());
}

function createToastContainer() {
    const container = document.createElement('div');
    container.className = 'toast-container';
    document.body.appendChild(container);
    return container;
}

// Loading state helper
function setLoading(element, loading) {
    if (loading) {
        element.classList.add('loading');
        element.setAttribute('disabled', 'disabled');
    } else {
        element.classList.remove('loading');
        element.removeAttribute('disabled');
    }
}

// Form validation helper
function validateForm(form) {
    const requiredFields = form.querySelectorAll('[required]');
    let valid = true;
    
    requiredFields.forEach(field => {
        if (!field.value.trim()) {
            field.classList.add('is-invalid');
            valid = false;
        } else {
            field.classList.remove('is-invalid');
        }
    });
    
    return valid;
}

// Date formatting helper
function formatDate(dateString) {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { 
        year: 'numeric', 
        month: 'short', 
        day: 'numeric' 
    });
}

// Number formatting helper
function formatNumber(num, decimals = 1) {
    if (num === null || num === undefined) return '';
    return parseFloat(num).toFixed(decimals);
}

// Debounce helper for search inputs
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Auto-submit search on typing (with debounce)
document.querySelectorAll('input[name="search"]').forEach(input => {
    const form = input.closest('form');
    if (form) {
        input.addEventListener('input', debounce(() => {
            if (input.value.length >= 2 || input.value.length === 0) {
                // form.submit(); // Uncomment to enable auto-submit
            }
        }, 500));
    }
});

// Keyboard shortcuts
document.addEventListener('keydown', function(e) {
    // Escape key closes modals
    if (e.key === 'Escape') {
        const openModal = document.querySelector('.modal.show');
        if (openModal) {
            bootstrap.Modal.getInstance(openModal)?.hide();
        }
    }
    
    // Ctrl+S to save in modals
    if ((e.ctrlKey || e.metaKey) && e.key === 's') {
        const openModal = document.querySelector('.modal.show');
        if (openModal) {
            e.preventDefault();
            const submitBtn = openModal.querySelector('button[type="submit"]');
            if (submitBtn) submitBtn.click();
        }
    }
});

// Initialize tooltips
document.addEventListener('DOMContentLoaded', function() {
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function(tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});

// Handle AJAX errors globally
window.addEventListener('unhandledrejection', function(event) {
    console.error('Unhandled promise rejection:', event.reason);
    showToast('An error occurred. Please try again.', 'danger');
});

// Confirm before leaving page with unsaved changes
let formChanged = false;

document.querySelectorAll('form').forEach(form => {
    form.addEventListener('change', () => formChanged = true);
    form.addEventListener('submit', () => formChanged = false);
});

window.addEventListener('beforeunload', function(e) {
    if (formChanged) {
        e.preventDefault();
        e.returnValue = '';
    }
});

// Copy to clipboard helper
function copyToClipboard(text) {
    navigator.clipboard.writeText(text).then(() => {
        showToast('Copied to clipboard!', 'success');
    }).catch(() => {
        showToast('Failed to copy', 'danger');
    });
}

console.log('Tracker application loaded');
