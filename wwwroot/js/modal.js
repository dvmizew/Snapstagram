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
        this.currentOptions = {};
        this.templates = {};
        this.init();
        this.initTemplates();
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

    initTemplates() {
        // Define reusable modal templates
        this.templates = {
            profile: {
                editProfile: {
                    title: 'Edit Profile',
                    size: 'default',
                    form: true
                },
                changePassword: {
                    title: 'Change Password',
                    size: 'default',
                    form: true
                },
                deleteAccount: {
                    title: 'Delete Account',
                    size: 'default',
                    confirmAction: true,
                    confirmClass: 'btn-danger'
                }
            },
            post: {
                create: {
                    title: 'Create Post',
                    size: 'large',
                    form: true
                },
                edit: {
                    title: 'Edit Post',
                    size: 'default',
                    form: true
                },
                delete: {
                    title: 'Delete Post',
                    size: 'small',
                    confirmAction: true,
                    confirmClass: 'btn-danger'
                },
                share: {
                    title: 'Share Post',
                    size: 'default'
                }
            },
            message: {
                compose: {
                    title: 'New Message',
                    size: 'default',
                    form: true
                },
                viewImage: {
                    title: 'Image',
                    size: 'large',
                    centered: true
                }
            },
            settings: {
                privacy: {
                    title: 'Privacy Settings',
                    size: 'default',
                    form: true
                },
                notifications: {
                    title: 'Notification Settings',
                    size: 'default',
                    form: true
                }
            },
            generic: {
                info: {
                    title: 'Information',
                    size: 'default'
                },
                error: {
                    title: 'Error',
                    size: 'small',
                    icon: 'fas fa-exclamation-triangle text-danger'
                },
                success: {
                    title: 'Success',
                    size: 'small',
                    icon: 'fas fa-check-circle text-success'
                }
            }
        };
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
        this.currentOptions = options;
        return this;
    }

    hide() {
        if (this.bootstrapModal) {
            this.bootstrapModal.hide();
        }
        return this;
    }

    // Enhanced template-based modal creation
    fromTemplate(category, type, customOptions = {}) {
        const template = this.templates[category]?.[type];
        if (!template) {
            console.error(`Modal template not found: ${category}.${type}`);
            return this;
        }

        const options = { ...template, ...customOptions };
        return this.show(options);
    }

    // AJAX content loading
    async loadContent(url, options = {}) {
        const { 
            method = 'GET', 
            data = null,
            headers = {},
            onSuccess = null,
            onError = null,
            showLoader = true
        } = options;

        if (showLoader) {
            this.showLoader();
        }

        try {
            const fetchOptions = {
                method,
                headers: {
                    'Content-Type': 'application/json',
                    ...headers
                }
            };

            if (data && method !== 'GET') {
                fetchOptions.body = JSON.stringify(data);
            }

            const response = await fetch(url, fetchOptions);
            
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const content = await response.text();
            this.updateContent(content);

            if (onSuccess) {
                onSuccess(content, response);
            }

        } catch (error) {
            console.error('Error loading modal content:', error);
            
            const errorContent = `
                <div class="text-center py-4">
                    <i class="fas fa-exclamation-triangle text-danger mb-3" style="font-size: 2rem;"></i>
                    <h5>Error Loading Content</h5>
                    <p class="text-muted">Failed to load the requested content. Please try again.</p>
                </div>
            `;
            
            this.updateContent(errorContent);

            if (onError) {
                onError(error);
            }
        }

        return this;
    }

    // Dynamic form creation
    createForm(fields = [], options = {}) {
        const {
            submitText = 'Submit',
            cancelText = 'Cancel',
            onSubmit = null,
            onCancel = null,
            validation = true,
            formClass = '',
            includeCSRF = true
        } = options;

        let formHTML = `<form class="modal-form ${formClass}">`;
        
        // Add CSRF token if needed (for ASP.NET Core)
        if (includeCSRF) {
            const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]');
            if (csrfToken) {
                formHTML += `<input type="hidden" name="__RequestVerificationToken" value="${csrfToken.value}">`;
            }
        }

        // Generate form fields
        fields.forEach(field => {
            formHTML += this.generateFormField(field);
        });

        formHTML += '</form>';

        // Create footer with buttons
        const footer = `
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">${cancelText}</button>
            <button type="button" class="btn btn-primary modal-submit-btn">${submitText}</button>
        `;

        this.modalBody.innerHTML = formHTML;
        this.modalFooter.innerHTML = footer;
        this.modalFooter.style.display = 'block';

        // Add event listeners
        setTimeout(() => {
            const submitBtn = this.modalFooter.querySelector('.modal-submit-btn');
            const form = this.modalBody.querySelector('form');

            if (submitBtn && form) {
                submitBtn.addEventListener('click', (e) => {
                    e.preventDefault();
                    
                    if (validation && !this.validateForm(form)) {
                        return;
                    }

                    const formData = this.getFormData(form);
                    
                    if (onSubmit) {
                        const result = onSubmit(formData, form);
                        if (result !== false) {
                            this.hide();
                        }
                    } else {
                        this.hide();
                    }
                });
            }

            if (onCancel) {
                const cancelBtn = this.modalFooter.querySelector('.btn-secondary');
                if (cancelBtn) {
                    cancelBtn.addEventListener('click', onCancel);
                }
            }
        }, 100);

        return this;
    }

    // Generate individual form fields
    generateFormField(field) {
        const {
            type = 'text',
            name = '',
            label = '',
            placeholder = '',
            value = '',
            required = false,
            options = [],
            attributes = {},
            helpText = '',
            validation = {},
            colClass = 'col-12'
        } = field;

        const fieldId = `modal-field-${name}`;
        const requiredAttr = required ? 'required' : '';
        const requiredLabel = required ? '<span class="text-danger">*</span>' : '';
        
        // Build attributes string
        let attrString = '';
        Object.entries(attributes).forEach(([key, val]) => {
            attrString += ` ${key}="${val}"`;
        });

        let fieldHTML = `<div class="${colClass} mb-3">`;
        
        if (label) {
            fieldHTML += `<label for="${fieldId}" class="form-label">${label}${requiredLabel}</label>`;
        }

        switch (type) {
            case 'textarea':
                fieldHTML += `<textarea id="${fieldId}" name="${name}" class="form-control" placeholder="${placeholder}" ${requiredAttr}${attrString}>${value}</textarea>`;
                break;
            
            case 'select':
                fieldHTML += `<select id="${fieldId}" name="${name}" class="form-select" ${requiredAttr}${attrString}>`;
                if (placeholder) {
                    fieldHTML += `<option value="">${placeholder}</option>`;
                }
                options.forEach(option => {
                    const optValue = typeof option === 'string' ? option : option.value;
                    const optText = typeof option === 'string' ? option : option.text;
                    const selected = optValue === value ? 'selected' : '';
                    fieldHTML += `<option value="${optValue}" ${selected}>${optText}</option>`;
                });
                fieldHTML += '</select>';
                break;
            
            case 'checkbox':
                fieldHTML += `
                    <div class="form-check">
                        <input type="checkbox" id="${fieldId}" name="${name}" class="form-check-input" value="true" ${value ? 'checked' : ''} ${requiredAttr}${attrString}>
                        <label class="form-check-label" for="${fieldId}">
                            ${placeholder || label}
                        </label>
                    </div>
                `;
                break;
            
            case 'file':
                fieldHTML += `<input type="file" id="${fieldId}" name="${name}" class="form-control" ${requiredAttr}${attrString}>`;
                break;
            
            default:
                fieldHTML += `<input type="${type}" id="${fieldId}" name="${name}" class="form-control" placeholder="${placeholder}" value="${value}" ${requiredAttr}${attrString}>`;
        }

        if (helpText) {
            fieldHTML += `<div class="form-text">${helpText}</div>`;
        }

        fieldHTML += '</div>';
        return fieldHTML;
    }

    // Enhanced form validation
    validateForm(form) {
        const inputs = form.querySelectorAll('input[required], textarea[required], select[required]');
        let isValid = true;

        inputs.forEach(input => {
            this.clearFieldError(input);
            
            if (!input.value.trim()) {
                this.showFieldError(input, `${this.getFieldLabel(input)} is required`);
                isValid = false;
            } else {
                // Custom validation based on input type
                const validationResult = this.validateField(input);
                if (!validationResult.isValid) {
                    this.showFieldError(input, validationResult.message);
                    isValid = false;
                }
            }
        });

        return isValid;
    }

    validateField(input) {
        const type = input.type;
        const value = input.value.trim();

        switch (type) {
            case 'email':
                const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                if (!emailRegex.test(value)) {
                    return { isValid: false, message: 'Please enter a valid email address' };
                }
                break;
            
            case 'password':
                if (value.length < 6) {
                    return { isValid: false, message: 'Password must be at least 6 characters long' };
                }
                break;
            
            case 'url':
                try {
                    new URL(value);
                } catch {
                    return { isValid: false, message: 'Please enter a valid URL' };
                }
                break;
        }

        // Check custom validation attributes
        const minLength = input.getAttribute('minlength');
        if (minLength && value.length < parseInt(minLength)) {
            return { isValid: false, message: `Minimum length is ${minLength} characters` };
        }

        const maxLength = input.getAttribute('maxlength');
        if (maxLength && value.length > parseInt(maxLength)) {
            return { isValid: false, message: `Maximum length is ${maxLength} characters` };
        }

        const pattern = input.getAttribute('pattern');
        if (pattern) {
            const regex = new RegExp(pattern);
            if (!regex.test(value)) {
                return { isValid: false, message: 'Please match the required format' };
            }
        }

        return { isValid: true };
    }

    showFieldError(input, message) {
        input.classList.add('is-invalid');
        
        // Remove existing error message
        const existingError = input.parentNode.querySelector('.invalid-feedback');
        if (existingError) {
            existingError.remove();
        }

        // Add new error message
        const errorDiv = document.createElement('div');
        errorDiv.className = 'invalid-feedback';
        errorDiv.textContent = message;
        input.parentNode.appendChild(errorDiv);
    }

    clearFieldError(input) {
        input.classList.remove('is-invalid');
        const errorDiv = input.parentNode.querySelector('.invalid-feedback');
        if (errorDiv) {
            errorDiv.remove();
        }
    }

    getFieldLabel(input) {
        const label = input.parentNode.querySelector('label');
        return label ? label.textContent.replace('*', '').trim() : input.name;
    }

    // Enhanced form data extraction
    getFormData(form = null) {
        const targetForm = form || this.modalBody.querySelector('form');
        if (!targetForm) return null;

        const formData = new FormData(targetForm);
        const data = {};
        
        for (let [key, value] of formData.entries()) {
            // Handle multiple values (checkboxes with same name)
            if (data[key]) {
                if (Array.isArray(data[key])) {
                    data[key].push(value);
                } else {
                    data[key] = [data[key], value];
                }
            } else {
                data[key] = value;
            }
        }

        return data;
    }

    // Quick utility methods for common use cases
    showSuccess(message, title = 'Success') {
        return this.alert({
            title,
            message,
            icon: 'fas fa-check-circle text-success',
            buttonClass: 'btn-success'
        });
    }

    showError(message, title = 'Error') {
        return this.alert({
            title,
            message,
            icon: 'fas fa-exclamation-triangle text-danger',
            buttonClass: 'btn-danger'
        });
    }

    showInfo(message, title = 'Information') {
        return this.alert({
            title,
            message,
            icon: 'fas fa-info-circle text-info',
            buttonClass: 'btn-info'
        });
    }

    showWarning(message, title = 'Warning') {
        return this.alert({
            title,
            message,
            icon: 'fas fa-exclamation-triangle text-warning',
            buttonClass: 'btn-warning'
        });
    }

    // Profile-specific modals
    showEditProfile(userData = {}) {
        const fields = [
            { type: 'text', name: 'username', label: 'Username', value: userData.username || '', required: true },
            { type: 'email', name: 'email', label: 'Email', value: userData.email || '', required: true },
            { type: 'text', name: 'firstName', label: 'First Name', value: userData.firstName || '' },
            { type: 'text', name: 'lastName', label: 'Last Name', value: userData.lastName || '' },
            { type: 'textarea', name: 'bio', label: 'Bio', value: userData.bio || '', attributes: { rows: 3 } },
            { type: 'url', name: 'website', label: 'Website', value: userData.website || '' }
        ];

        this.show({
            title: 'Edit Profile',
            size: 'default',
            body: '',
            footer: ''
        });

        this.createForm(fields, {
            submitText: 'Save Changes',
            onSubmit: (data) => {
                // This will be handled by the calling code
                if (window.updateProfile) {
                    return window.updateProfile(data);
                }
                return false;
            }
        });

        return this;
    }

    showChangePassword() {
        const fields = [
            { type: 'password', name: 'currentPassword', label: 'Current Password', required: true },
            { type: 'password', name: 'newPassword', label: 'New Password', required: true, attributes: { minlength: 6 } },
            { type: 'password', name: 'confirmPassword', label: 'Confirm New Password', required: true }
        ];

        this.show({
            title: 'Change Password',
            size: 'default',
            body: '',
            footer: ''
        });

        this.createForm(fields, {
            submitText: 'Change Password',
            validation: true,
            onSubmit: (data) => {
                if (data.newPassword !== data.confirmPassword) {
                    this.showFieldError(
                        this.modalBody.querySelector('input[name="confirmPassword"]'),
                        'Passwords do not match'
                    );
                    return false;
                }

                if (window.changePassword) {
                    return window.changePassword(data);
                }
                return false;
            }
        });

        return this;
    }

    // Post-specific modals
    showCreatePost() {
        this.show({
            title: 'Create New Post',
            size: 'large',
            body: `
                <div class="row">
                    <div class="col-md-6">
                        <div class="upload-area border rounded p-4 text-center mb-3">
                            <i class="fas fa-cloud-upload-alt fa-3x text-muted mb-3"></i>
                            <p class="mb-0">Drop your image or video here, or click to browse</p>
                            <input type="file" class="form-control mt-3" accept="image/*,video/*" id="postMedia">
                        </div>
                        <div id="mediaPreview" style="display: none;">
                            <img id="previewImage" class="img-fluid rounded" style="max-height: 300px;">
                            <video id="previewVideo" class="w-100 rounded" style="max-height: 300px; display: none;" controls></video>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <div class="mb-3">
                            <label for="postCaption" class="form-label">Caption</label>
                            <textarea id="postCaption" class="form-control" rows="5" placeholder="Write a caption..."></textarea>
                            <div class="form-text">
                                <span id="captionCount">0</span>/2200 characters
                            </div>
                        </div>
                        <div class="mb-3">
                            <label for="postTags" class="form-label">Tags</label>
                            <input type="text" id="postTags" class="form-control" placeholder="#hashtag #anothertag">
                        </div>
                        <div class="form-check">
                            <input type="checkbox" class="form-check-input" id="allowComments" checked>
                            <label class="form-check-label" for="allowComments">
                                Allow comments
                            </label>
                        </div>
                    </div>
                </div>
            `,
            footer: `
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                <button type="button" class="btn btn-primary" id="sharePostBtn" disabled>Share Post</button>
            `
        });

        // Add event listeners for post creation
        setTimeout(() => {
            this.initializePostCreation();
        }, 100);

        return this;
    }

    initializePostCreation() {
        const fileInput = document.getElementById('postMedia');
        const previewContainer = document.getElementById('mediaPreview');
        const previewImage = document.getElementById('previewImage');
        const previewVideo = document.getElementById('previewVideo');
        const captionTextarea = document.getElementById('postCaption');
        const captionCount = document.getElementById('captionCount');
        const shareBtn = document.getElementById('sharePostBtn');

        // File upload handling
        fileInput.addEventListener('change', (e) => {
            const file = e.target.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = (e) => {
                    previewContainer.style.display = 'block';
                    
                    if (file.type.startsWith('image/')) {
                        previewImage.src = e.target.result;
                        previewImage.style.display = 'block';
                        previewVideo.style.display = 'none';
                    } else if (file.type.startsWith('video/')) {
                        previewVideo.src = e.target.result;
                        previewVideo.style.display = 'block';
                        previewImage.style.display = 'none';
                    }
                    
                    shareBtn.disabled = false;
                };
                reader.readAsDataURL(file);
            }
        });

        // Caption character counting
        captionTextarea.addEventListener('input', (e) => {
            const length = e.target.value.length;
            captionCount.textContent = length;
            captionCount.style.color = length > 2200 ? '#dc3545' : '';
        });

        // Share button handling
        shareBtn.addEventListener('click', () => {
            const file = fileInput.files[0];
            const caption = captionTextarea.value;
            const tags = document.getElementById('postTags').value;
            const allowComments = document.getElementById('allowComments').checked;

            if (!file) {
                this.showError('Please select an image or video');
                return;
            }

            if (window.createPost) {
                window.createPost({
                    file,
                    caption,
                    tags,
                    allowComments
                });
            }
        });
    }

    // Message-specific modals
    showComposeMessage(recipientUsername = '') {
        const fields = [
            { 
                type: 'text', 
                name: 'recipient', 
                label: 'To', 
                value: recipientUsername, 
                required: true,
                placeholder: 'Enter username' 
            },
            { 
                type: 'textarea', 
                name: 'message', 
                label: 'Message', 
                required: true,
                attributes: { rows: 4 },
                placeholder: 'Type your message...' 
            }
        ];

        this.show({
            title: 'New Message',
            size: 'default',
            body: '',
            footer: ''
        });

        this.createForm(fields, {
            submitText: 'Send Message',
            onSubmit: (data) => {
                if (window.sendMessage) {
                    return window.sendMessage(data);
                }
                return false;
            }
        });

        return this;
    }

    // Settings-specific modals
    showPrivacySettings(currentSettings = {}) {
        const fields = [
            {
                type: 'select',
                name: 'profileVisibility',
                label: 'Profile Visibility',
                value: currentSettings.profileVisibility || 'public',
                options: [
                    { value: 'public', text: 'Public' },
                    { value: 'private', text: 'Private' }
                ]
            },
            {
                type: 'checkbox',
                name: 'allowTagging',
                label: 'Allow others to tag you in posts',
                value: currentSettings.allowTagging !== false
            },
            {
                type: 'checkbox',
                name: 'showOnlineStatus',
                label: 'Show when you\'re online',
                value: currentSettings.showOnlineStatus !== false
            }
        ];

        this.show({
            title: 'Privacy Settings',
            size: 'default',
            body: '',
            footer: ''
        });

        this.createForm(fields, {
            submitText: 'Save Settings',
            onSubmit: (data) => {
                if (window.updatePrivacySettings) {
                    return window.updatePrivacySettings(data);
                }
                return false;
            }
        });

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

// Global modal instance initialization
document.addEventListener('DOMContentLoaded', function() {
    // Create global modal instance
    window.modal = new Modal();
    
    // Legacy showModal function for backward compatibility
    window.showModal = function(options) {
        return window.modal.show(options);
    };
    
    // Legacy alert/confirm functions for backward compatibility
    window.showAlert = function(options) {
        if (typeof options === 'string') {
            options = { message: options };
        }
        return window.modal.alert(options);
    };
    
    window.showConfirm = function(options) {
        if (typeof options === 'string') {
            options = { message: options };
        }
        return window.modal.confirm(options);
    };

    // Profile modal functions
    window.openEditProfileModal = function() {
        const fields = [
            { 
                type: 'text', 
                name: 'displayName', 
                label: 'Display Name', 
                value: document.querySelector('[data-profile-displayname]')?.textContent || '',
                required: true 
            },
            { 
                type: 'textarea', 
                name: 'bio', 
                label: 'Bio', 
                value: document.querySelector('[data-profile-bio]')?.textContent || '',
                attributes: { rows: 3 }
            },
            { 
                type: 'checkbox', 
                name: 'isPrivate', 
                label: 'Private Account',
                value: document.querySelector('[data-profile-private]')?.value === 'true'
            }
        ];

        window.modal.show({
            title: 'Edit Profile',
            size: 'default'
        });

        window.modal.createForm(fields, {
            submitText: 'Save Changes',
            onSubmit: async (data) => {
                try {
                    const response = await fetch('/api/profile/update', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                        },
                        body: JSON.stringify({
                            displayName: data.displayName,
                            bio: data.bio,
                            isPrivate: data.isPrivate === 'on' || data.isPrivate === true
                        })
                    });

                    if (response.ok) {
                        window.showNotification && window.showNotification('Profile updated successfully!', 'success');
                        location.reload();
                        return true;
                    } else {
                        const error = await response.json();
                        window.showNotification && window.showNotification(error.message || 'Failed to update profile', 'error');
                        return false;
                    }
                } catch (error) {
                    console.error('Error updating profile:', error);
                    window.showNotification && window.showNotification('Error updating profile. Please try again.', 'error');
                    return false;
                }
            }
        });
    };

    window.openChangePasswordModal = function() {
        const fields = [
            { 
                type: 'password', 
                name: 'currentPassword', 
                label: 'Current Password', 
                required: true 
            },
            { 
                type: 'password', 
                name: 'newPassword', 
                label: 'New Password', 
                required: true,
                attributes: { minlength: 6 }
            },
            { 
                type: 'password', 
                name: 'confirmPassword', 
                label: 'Confirm New Password', 
                required: true 
            }
        ];

        window.modal.show({
            title: 'Change Password',
            size: 'default'
        });

        window.modal.createForm(fields, {
            submitText: 'Change Password',
            onSubmit: async (data) => {
                if (data.newPassword !== data.confirmPassword) {
                    alert('Passwords do not match');
                    return false;
                }

                try {
                    const response = await fetch('/api/profile/change-password', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                        },
                        body: JSON.stringify({
                            currentPassword: data.currentPassword,
                            newPassword: data.newPassword,
                            confirmPassword: data.confirmPassword
                        })
                    });

                    if (response.ok) {
                        window.showNotification && window.showNotification('Password changed successfully!', 'success');
                        return true;
                    } else {
                        const error = await response.json();
                        window.showNotification && window.showNotification(error.message || 'Failed to change password', 'error');
                        return false;
                    }
                } catch (error) {
                    console.error('Error changing password:', error);
                    window.showNotification && window.showNotification('Error changing password. Please try again.', 'error');
                    return false;
                }
            }
        });
    };

    window.openProfileImageModal = function() {
        window.modal.show({
            title: 'Update Profile Picture',
            size: 'default',
            body: `
                <div class="text-center">
                    <div class="mb-3">
                        <input type="file" class="form-control" id="profileImageInput" accept="image/*">
                    </div>
                    <div id="imagePreview" style="display: none;">
                        <img id="previewImg" class="img-fluid rounded" style="max-height: 200px;">
                    </div>
                </div>
            `,
            footer: `
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                <button type="button" class="btn btn-primary" id="uploadImageBtn" disabled>Update Picture</button>
            `
        });

        // Add event listeners for image upload
        setTimeout(() => {
            const fileInput = document.getElementById('profileImageInput');
            const preview = document.getElementById('imagePreview');
            const previewImg = document.getElementById('previewImg');
            const uploadBtn = document.getElementById('uploadImageBtn');

            fileInput.addEventListener('change', (e) => {
                const file = e.target.files[0];
                if (file) {
                    const reader = new FileReader();
                    reader.onload = (e) => {
                        previewImg.src = e.target.result;
                        preview.style.display = 'block';
                        uploadBtn.disabled = false;
                    };
                    reader.readAsDataURL(file);
                }
            });

            uploadBtn.addEventListener('click', () => {
                // In a real app, you'd upload the file to a server first
                window.showNotification && window.showNotification('Profile picture updated!', 'success');
                window.modal.hide();
            });
        }, 100);
    };
});
