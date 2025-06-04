// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Global site functionality
document.addEventListener('DOMContentLoaded', function() {
    initializeSearch();
    initializeProfileDropdown();
    initializeNotifications();
    globalModal = new GlobalModal();
    
    // Make it globally accessible
    window.globalModal = globalModal;
    
    // Add convenience methods to window
    window.showModal = (options) => globalModal.show(options);
    window.showConfirm = (options) => globalModal.confirm(options);
    window.showAlert = (options) => globalModal.alert(options);
    window.showFormModal = (options) => globalModal.form(options);
});

// Search functionality
function initializeSearch() {
    const searchInput = document.querySelector('.search-input');
    const searchContainer = document.querySelector('.search-container');
    
    if (!searchInput || !searchContainer) return;
    
    let searchTimeout;
    let searchResults = null;
    
    // Create search results dropdown
    const createSearchDropdown = () => {
        if (searchResults) return searchResults;
        
        searchResults = document.createElement('div');
        searchResults.className = 'search-results';
        searchResults.style.cssText = `
            position: absolute;
            top: 100%;
            left: 0;
            right: 0;
            background: white;
            border: 1px solid #e1e5e9;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            max-height: 400px;
            overflow-y: auto;
            z-index: 1000;
            margin-top: 4px;
            display: none;
        `;
        
        searchContainer.style.position = 'relative';
        searchContainer.appendChild(searchResults);
        return searchResults;
    };
    
    // Search function
    const performSearch = async (query) => {
        if (query.length < 2) {
            hideSearchResults();
            return;
        }
        
        try {
            const response = await fetch(`/api/profile/search?query=${encodeURIComponent(query)}&limit=8`);
            if (response.ok) {
                const users = await response.json();
                displaySearchResults(users);
            }
        } catch (error) {
            console.error('Search error:', error);
        }
    };
    
    // Display search results
    const displaySearchResults = (users) => {
        const dropdown = createSearchDropdown();
        
        if (users.length === 0) {
            dropdown.innerHTML = '<div style="padding: 16px; text-align: center; color: #6c757d;">No users found</div>';
        } else {
            dropdown.innerHTML = users.map(user => `
                <a href="/ProfileView?username=${user.userName}" style="display: block; padding: 12px 16px; text-decoration: none; color: inherit; border-bottom: 1px solid #f8f9fa;">
                    <div style="display: flex; align-items: center; gap: 12px;">
                        <img src="${user.profileImageUrl || '/images/default-avatar.svg'}" 
                             style="width: 40px; height: 40px; border-radius: 50%; object-fit: cover;">
                        <div>
                            <div style="font-weight: 600; color: #1a1a1a;">${user.userName}</div>
                            <div style="font-size: 14px; color: #6c757d;">${user.displayName}</div>
                        </div>
                        ${user.isPrivate ? '<i class="fas fa-lock" style="color: #6c757d; margin-left: auto;"></i>' : ''}
                    </div>
                </a>
            `).join('');
        }
        
        dropdown.style.display = 'block';
    };
    
    // Hide search results
    const hideSearchResults = () => {
        if (searchResults) {
            searchResults.style.display = 'none';
        }
    };
    
    // Event listeners
    searchInput.addEventListener('input', (e) => {
        clearTimeout(searchTimeout);
        const query = e.target.value.trim();
        
        searchTimeout = setTimeout(() => {
            performSearch(query);
        }, 300);
    });
    
    searchInput.addEventListener('focus', (e) => {
        if (e.target.value.trim().length >= 2) {
            performSearch(e.target.value.trim());
        }
    });
    
    // Hide results when clicking outside
    document.addEventListener('click', (e) => {
        if (!searchContainer.contains(e.target)) {
            hideSearchResults();
        }
    });
    
    // Handle keyboard navigation
    searchInput.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
            hideSearchResults();
            searchInput.blur();
        }
    });
}

// Profile dropdown functionality
function initializeProfileDropdown() {
    const profileDropdown = document.querySelector('.profile-dropdown');
    if (!profileDropdown) return;
    
    const profileIcon = profileDropdown.querySelector('.profile-icon');
    const dropdownMenu = profileDropdown.querySelector('.dropdown-menu');
    
    if (!profileIcon || !dropdownMenu) return;
    
    profileIcon.addEventListener('click', (e) => {
        e.stopPropagation();
        dropdownMenu.classList.toggle('show');
    });
    
    // Close dropdown when clicking outside
    document.addEventListener('click', () => {
        dropdownMenu.classList.remove('show');
    });
}

// Notification functionality (placeholder)
function initializeNotifications() {
    // This is a placeholder for future notification functionality
    const notificationIcon = document.querySelector('.notification-icon');
    if (notificationIcon) {
        notificationIcon.addEventListener('click', () => {
            console.log('Notifications clicked - feature coming soon!');
        });
    }
}

// Utility functions
const showNotification = (message, type = 'info') => {
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.textContent = message;
    
    const bgColors = { success: '#10b981', error: '#ef4444', default: '#3b82f6' };
    
    Object.assign(notification.style, {
        position: 'fixed',
        top: '20px',
        right: '20px',
        padding: '12px 20px',
        borderRadius: '8px',
        color: 'white',
        fontWeight: '500',
        zIndex: '10000',
        backgroundColor: bgColors[type] || bgColors.default,
        transform: 'translateX(400px)',
        transition: 'transform 0.3s ease',
        maxWidth: '300px'
    });
    
    document.body.appendChild(notification);
    
    setTimeout(() => notification.style.transform = 'translateX(0)', 100);
    setTimeout(() => {
        notification.style.transform = 'translateX(400px)';
        setTimeout(() => document.body.contains(notification) && document.body.removeChild(notification), 300);
    }, 4000);
};

// Global utility to get relative time
const getRelativeTime = (date) => {
    const now = new Date();
    const diffTime = Math.abs(now - date);
    const diffDays = Math.floor(diffTime / (1000 * 60 * 60 * 24));
    const diffHours = Math.floor(diffTime / (1000 * 60 * 60));
    const diffMinutes = Math.floor(diffTime / (1000 * 60));
    
    if (diffDays > 0) return `${diffDays}d`;
    if (diffHours > 0) return `${diffHours}h`;
    if (diffMinutes > 0) return `${diffMinutes}m`;
    return 'now';
};

// Global utility to get default avatar URL
const getDefaultAvatar = (name) => {
    const initial = name ? name[0].toUpperCase() : 'U';
    return `https://via.placeholder.com/40x40/6c757d/ffffff?text=${initial}`;
};

// Global Modal System
class GlobalModal {
    constructor() {
        this.modal = null;
        this.modalDialog = null;
        this.modalHeader = null;
        this.modalTitle = null;
        this.modalBody = null;
        this.modalFooter = null;
        this.bootstrapModal = null;
        this.init();
    }

    init() {
        // Initialize modal elements
        this.modal = document.getElementById('globalModal');
        this.modalDialog = document.getElementById('globalModalDialog');
        this.modalHeader = document.getElementById('globalModalHeader');
        this.modalTitle = document.getElementById('globalModalLabel');
        this.modalBody = document.getElementById('globalModalBody');
        this.modalFooter = document.getElementById('globalModalFooter');

        if (this.modal) {
            this.bootstrapModal = new bootstrap.Modal(this.modal);
            
            // Add event listeners
            this.modal.addEventListener('hidden.bs.modal', () => {
                this.reset();
            });
        }
    }

    show(options = {}) {
        if (!this.modal) return;

        const {
            title = 'Modal',
            body = '',
            footer = '',
            size = 'default',
            closable = true,
            backdrop = true,
            keyboard = true,
            onShow = null,
            onHidden = null
        } = options;

        // Set modal properties
        this.modalTitle.textContent = title;
        this.modalBody.innerHTML = body;
        this.modalFooter.innerHTML = footer;

        // Configure modal dialog size
        this.modalDialog.className = 'modal-dialog';
        if (size === 'small') this.modalDialog.classList.add('modal-sm');
        else if (size === 'large') this.modalDialog.classList.add('modal-lg');
        else if (size === 'extra-large') this.modalDialog.classList.add('modal-xl');

        // Configure backdrop and keyboard
        this.modal.setAttribute('data-bs-backdrop', backdrop ? 'true' : 'static');
        this.modal.setAttribute('data-bs-keyboard', keyboard.toString());

        // Handle close button visibility
        const closeButton = this.modalHeader.querySelector('.btn-close');
        if (closeButton) {
            closeButton.style.display = closable ? 'block' : 'none';
        }

        // Show/hide footer based on content
        this.modalFooter.style.display = footer ? 'block' : 'none';

        // Event handlers
        if (onShow) {
            this.modal.addEventListener('shown.bs.modal', onShow, { once: true });
        }
        if (onHidden) {
            this.modal.addEventListener('hidden.bs.modal', onHidden, { once: true });
        }

        // Show the modal
        this.bootstrapModal.show();
    }

    hide() {
        if (this.bootstrapModal) {
            this.bootstrapModal.hide();
        }
    }

    reset() {
        // Reset modal content
        this.modalTitle.textContent = '';
        this.modalBody.innerHTML = '';
        this.modalFooter.innerHTML = '';
        this.modalDialog.className = 'modal-dialog';
        this.modalFooter.style.display = 'block';
        
        // Reset attributes
        this.modal.setAttribute('data-bs-backdrop', 'true');
        this.modal.setAttribute('data-bs-keyboard', 'true');
    }

    // Convenience methods for common modal types
    confirm(options = {}) {
        const {
            title = 'Confirm Action',
            message = 'Are you sure?',
            confirmText = 'Confirm',
            cancelText = 'Cancel',
            confirmClass = 'btn-danger',
            onConfirm = null,
            onCancel = null
        } = options;

        const footer = `
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">${cancelText}</button>
            <button type="button" class="btn ${confirmClass}" id="modalConfirmBtn">${confirmText}</button>
        `;

        this.show({
            title,
            body: `<p class="mb-0">${message}</p>`,
            footer,
            size: 'default'
        });

        // Add confirm button handler
        const confirmBtn = document.getElementById('modalConfirmBtn');
        if (confirmBtn) {
            confirmBtn.addEventListener('click', () => {
                if (onConfirm) onConfirm();
                this.hide();
            }, { once: true });
        }

        // Add cancel handler
        if (onCancel) {
            this.modal.addEventListener('hidden.bs.modal', onCancel, { once: true });
        }
    }

    alert(options = {}) {
        const {
            title = 'Alert',
            message = '',
            buttonText = 'OK',
            buttonClass = 'btn-primary',
            onClose = null
        } = options;

        const footer = `
            <button type="button" class="btn ${buttonClass}" data-bs-dismiss="modal">${buttonText}</button>
        `;

        this.show({
            title,
            body: `<p class="mb-0">${message}</p>`,
            footer,
            size: 'default',
            onHidden: onClose
        });
    }

    form(options = {}) {
        const {
            title = 'Form',
            formContent = '',
            submitText = 'Submit',
            cancelText = 'Cancel',
            onSubmit = null,
            onCancel = null,
            size = 'default'
        } = options;

        const footer = `
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">${cancelText}</button>
            <button type="button" class="btn btn-primary" id="modalSubmitBtn">${submitText}</button>
        `;

        this.show({
            title,
            body: formContent,
            footer,
            size
        });

        // Add submit button handler
        const submitBtn = document.getElementById('modalSubmitBtn');
        if (submitBtn) {
            submitBtn.addEventListener('click', () => {
                if (onSubmit) {
                    const formData = this.getFormData();
                    onSubmit(formData);
                }
            }, { once: true });
        }

        // Add cancel handler
        if (onCancel) {
            this.modal.addEventListener('hidden.bs.modal', onCancel, { once: true });
        }
    }

    getFormData() {
        const forms = this.modalBody.querySelectorAll('form');
        if (forms.length === 0) return null;

        const formData = new FormData(forms[0]);
        const data = {};
        for (let [key, value] of formData.entries()) {
            data[key] = value;
        }
        return data;
    }
}

// Export functions for use in other scripts
window.showNotification = showNotification;
window.getRelativeTime = getRelativeTime;
window.getDefaultAvatar = getDefaultAvatar;
