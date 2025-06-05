class Modal {
    constructor() {
        this.modal = null;
        this.modalDialog = null;
        this.modalHeader = null;
        this.modalTitle = null;
        this.modalBody = null;
        this.modalFooter = null;
        this.modalClose = null;
        this.modalLoader = null;
        this.bootstrapModal = null;
        this.confirmModal = null;
        this.alertModal = null;
        this.init();
    }

    init() {
        // initialize main modal elements
        this.modal = document.getElementById('appModal');
        this.modalDialog = document.getElementById('appModalDialog');
        this.modalHeader = document.getElementById('appModalHeader');
        this.modalTitle = document.getElementById('appModalLabel');
        this.modalBody = document.getElementById('appModalBody');
        this.modalFooter = document.getElementById('appModalFooter');
        this.modalClose = document.getElementById('appModalClose');
        this.modalLoader = document.getElementById('appModalLoader');

        // initialize confirm and alert modals
        this.confirmModal = document.getElementById('confirmModal');
        this.alertModal = document.getElementById('alertModal');

        if (this.modal) {
            this.bootstrapModal = new bootstrap.Modal(this.modal);
            
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
            centered = true,
            onShow = null,
            onHidden = null,
            loading = false
        } = options;

        // set modal properties
        this.modalTitle.textContent = title;
        
        // handle loading state
        if (loading) {
            this.showLoader();
            this.modalBody.innerHTML = '';
        } else {
            this.hideLoader();
            this.modalBody.innerHTML = body;
        }
        
        this.modalFooter.innerHTML = footer;

        // configure modal dialog size and centering
        this.modalDialog.className = 'modal-dialog';
        if (centered) this.modalDialog.classList.add('modal-dialog-centered');
        if (size === 'small') this.modalDialog.classList.add('modal-sm');
        else if (size === 'large') this.modalDialog.classList.add('modal-lg');
        else if (size === 'extra-large') this.modalDialog.classList.add('modal-xl');
        else if (size === 'fullscreen') this.modalDialog.classList.add('modal-fullscreen');

        // configure backdrop and keyboard
        this.modal.setAttribute('data-bs-backdrop', backdrop ? 'true' : 'static');
        this.modal.setAttribute('data-bs-keyboard', keyboard.toString());

        // handle close button visibility
        if (this.modalClose) {
            this.modalClose.style.display = closable ? 'block' : 'none';
        }

        // show/hide footer based on content
        this.modalFooter.style.display = footer ? 'block' : 'none';

        // event handlers
        if (onShow) {
            this.modal.addEventListener('shown.bs.modal', onShow, { once: true });
        }
        if (onHidden) {
            this.modal.addEventListener('hidden.bs.modal', onHidden, { once: true });
        }

        // show the modal
        this.bootstrapModal.show();
        return this;
    }

    hide() {
        if (this.bootstrapModal) {
            this.bootstrapModal.hide();
        }
        return this;
    }

    showLoader() {
        if (this.modalLoader) {
            this.modalLoader.style.display = 'block';
        }
        return this;
    }

    hideLoader() {
        if (this.modalLoader) {
            this.modalLoader.style.display = 'none';
        }
        return this;
    }

    updateContent(body, footer = null) {
        this.hideLoader();
        this.modalBody.innerHTML = body;
        if (footer !== null) {
            this.modalFooter.innerHTML = footer;
            this.modalFooter.style.display = footer ? 'block' : 'none';
        }
        return this;
    }

    reset() {
        // reset modal content
        this.modalTitle.textContent = '';
        this.modalBody.innerHTML = '';
        this.modalFooter.innerHTML = '';
        this.modalDialog.className = 'modal-dialog modal-dialog-centered';
        this.modalFooter.style.display = 'block';
        this.hideLoader();
        
        // reset attributes
        this.modal.setAttribute('data-bs-backdrop', 'true');
        this.modal.setAttribute('data-bs-keyboard', 'true');
        
        // close button
        if (this.modalClose) {
            this.modalClose.style.display = 'block';
        }
    }

    // convenience methods for common modal types
    confirm(options = {}) {
        return new Promise((resolve) => {
            const {
                title = 'Confirm Action',
                message = 'Are you sure?',
                confirmText = 'Confirm',
                cancelText = 'Cancel',
                confirmClass = 'btn-danger',
                icon = 'fas fa-question-circle'
            } = options;

            const confirmModalEl = this.confirmModal;
            const confirmModalInstance = new bootstrap.Modal(confirmModalEl);

            document.getElementById('confirmModalLabel').textContent = title;
            document.getElementById('confirmModalBody').innerHTML = `
                <div class="d-flex align-items-center">
                    <i class="${icon} text-warning me-3" style="font-size: 1.5rem;"></i>
                    <span>${message}</span>
                </div>
            `;

            const cancelBtn = document.getElementById('confirmCancelBtn');
            const confirmBtn = document.getElementById('confirmActionBtn');
            
            cancelBtn.textContent = cancelText;
            confirmBtn.textContent = confirmText;
            confirmBtn.className = `btn ${confirmClass}`;

            const handleConfirm = () => {
                confirmModalInstance.hide();
                resolve(true);
                cleanup();
            };

            const handleCancel = () => {
                confirmModalInstance.hide();
                resolve(false);
                cleanup();
            };

            const cleanup = () => {
                confirmBtn.removeEventListener('click', handleConfirm);
                cancelBtn.removeEventListener('click', handleCancel);
                confirmModalEl.removeEventListener('hidden.bs.modal', handleCancel);
            };

            confirmBtn.addEventListener('click', handleConfirm);
            cancelBtn.addEventListener('click', handleCancel);
            confirmModalEl.addEventListener('hidden.bs.modal', handleCancel, { once: true });

            confirmModalInstance.show();
        });
    }

    alert(options = {}) {
        return new Promise((resolve) => {
            const {
                title = 'Alert',
                message = '',
                buttonText = 'OK',
                buttonClass = 'btn-primary',
                icon = 'fas fa-info-circle'
            } = options;

            const alertModalEl = this.alertModal;
            const alertModalInstance = new bootstrap.Modal(alertModalEl);

            document.getElementById('alertModalLabel').textContent = title;
            document.getElementById('alertModalBody').innerHTML = `
                <div class="d-flex align-items-center">
                    <i class="${icon} text-info me-3" style="font-size: 1.5rem;"></i>
                    <span>${message}</span>
                </div>
            `;

            const okBtn = document.getElementById('alertOkBtn');
            okBtn.textContent = buttonText;
            okBtn.className = `btn ${buttonClass}`;

            const handleOk = () => {
                alertModalInstance.hide();
                resolve();
                okBtn.removeEventListener('click', handleOk);
            };

            okBtn.addEventListener('click', handleOk);
            alertModalEl.addEventListener('hidden.bs.modal', handleOk, { once: true });

            alertModalInstance.show();
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
            size = 'default',
            validateForm = true
        } = options;

        const footer = `
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal" id="modalCancelBtn">${cancelText}</button>
            <button type="button" class="btn btn-primary" id="modalSubmitBtn">${submitText}</button>
        `;

        this.show({
            title,
            body: formContent,
            footer,
            size
        });

        setTimeout(() => {
            const submitBtn = document.getElementById('modalSubmitBtn');
            const cancelBtn = document.getElementById('modalCancelBtn');
            
            if (submitBtn) {
                submitBtn.addEventListener('click', () => {
                    if (validateForm && !this.validateModalForm()) {
                        return;
                    }
                    
                    const formData = this.getFormData();
                    if (onSubmit) {
                        const result = onSubmit(formData);
                        if (result !== false) {
                            this.hide();
                        }
                    } else {
                        this.hide();
                    }
                }, { once: true });
            }

            if (cancelBtn && onCancel) {
                cancelBtn.addEventListener('click', onCancel, { once: true });
            }
        }, 100);

        return this;
    }

    validateModalForm() {
        const forms = this.modalBody.querySelectorAll('form');
        if (forms.length === 0) return true;

        const form = forms[0];
        const inputs = form.querySelectorAll('input[required], textarea[required], select[required]');
        let isValid = true;

        inputs.forEach(input => {
            if (!input.value.trim()) {
                input.classList.add('is-invalid');
                isValid = false;
            } else {
                input.classList.remove('is-invalid');
            }
        });

        return isValid;
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

    // Image/Media viewer
    showImage(options = {}) {
        const {
            src = '',
            title = 'Image',
            caption = '',
            showDownload = false
        } = options;

        const downloadBtn = showDownload ? 
            `<a href="${src}" download class="btn btn-outline-primary">
                <i class="fas fa-download me-1"></i>Download
            </a>` : '';

        const body = `
            <div class="text-center">
                <img src="${src}" alt="${title}" class="img-fluid" style="max-height: 70vh;">
                ${caption ? `<p class="mt-3 text-muted">${caption}</p>` : ''}
            </div>
        `;

        this.show({
            title,
            body,
            footer: downloadBtn,
            size: 'large'
        });

        return this;
    }

    showLoading(title = 'Loading...', message = 'Please wait...') {
        this.show({
            title,
            body: `
                <div class="text-center py-4">
                    <div class="spinner-border text-primary mb-3" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    <p class="mb-0">${message}</p>
                </div>
            `,
            closable: false,
            backdrop: 'static',
            keyboard: false
        });

        return this;
    }
}

// search functionality
function initializeSearch() {
    const searchInput = document.querySelector('.search-input');
    const searchContainer = document.querySelector('.search-container');
    
    if (!searchInput || !searchContainer) return;
    
    let searchTimeout;
    let searchResults = null;
    
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
    
    // search
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
    
    const hideSearchResults = () => {
        if (searchResults) {
            searchResults.style.display = 'none';
        }
    };
    
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
    
    // hide results when clicking outside
    document.addEventListener('click', (e) => {
        if (!searchContainer.contains(e.target)) {
            hideSearchResults();
        }
    });
    
    // keyboard navigation
    searchInput.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
            hideSearchResults();
            searchInput.blur();
        }
    });
}

// profile dropdown
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
    
    // close dropdown when clicking outside
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

const getDefaultAvatar = (name) => {
    const initial = name ? name[0].toUpperCase() : 'U';
    return `https://via.placeholder.com/40x40/6c757d/ffffff?text=${initial}`;
};

let modal;
document.addEventListener('DOMContentLoaded', function() {
    modal = new Modal();
    
    window.modal = modal;
    
    window.showModal = (options) => modal.show(options);
    window.showConfirm = (options) => modal.confirm(options);
    window.showAlert = (options) => modal.alert(options);
    window.showFormModal = (options) => modal.form(options);
    window.showImageModal = (options) => modal.showImage(options);
    window.showLoadingModal = (title, message) => modal.showLoading(title, message);
    
    window.showNotification = showNotification;
    window.getRelativeTime = getRelativeTime;
    window.getDefaultAvatar = getDefaultAvatar;
    
    initializeSearch();
    initializeProfileDropdown();
    initializeNotifications();
});
