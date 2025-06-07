
        // Instagram-like File Upload with Enhanced UX
        document.addEventListener('DOMContentLoaded', function() {
            const uploadZone = document.getElementById('uploadZone');
            const photoInput = document.getElementById('photoInput');
            const contentTextarea = document.querySelector('textarea[name="PostInput.Content"]');
            const createPostModal = document.getElementById('createPostModal');
            
            // Set up drag and drop events
            if (uploadZone) {
                setupDragAndDrop(uploadZone, photoInput);
            }
            
            // Set up content change tracking
            if (contentTextarea) {
                contentTextarea.addEventListener('input', updatePostTypeHint);
                contentTextarea.addEventListener('focus', function() {
                    this.parentElement.classList.add('focused');
                });
                contentTextarea.addEventListener('blur', function() {
                    this.parentElement.classList.remove('focused');
                });
            }
            
            // Reset modal state when it's opened
            if (createPostModal) {
                createPostModal.addEventListener('shown.bs.modal', function() {
                    resetModalState();
                    updatePostTypeHint();
                });
                
                createPostModal.addEventListener('hidden.bs.modal', function() {
                    resetModalState();
                });
            }
            
            // Initial state
            updatePostTypeHint();
        });
        
        function resetModalState() {
            const photoInput = document.getElementById('photoInput');
            const preview = document.getElementById('imagePreview');
            const uploadZone = document.getElementById('uploadZone');
            const contentTextarea = document.querySelector('textarea[name="PostInput.Content"]');
            
            if (photoInput) photoInput.value = '';
            if (preview) preview.style.display = 'none';
            if (uploadZone) {
                uploadZone.style.display = 'flex';
                uploadZone.style.opacity = '1';
            }
            if (contentTextarea) contentTextarea.value = '';
        }
        
        function setupDragAndDrop(uploadZone, fileInput) {
            ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
                uploadZone.addEventListener(eventName, preventDefaults, false);
            });
            
            function preventDefaults(e) {
                e.preventDefault();
                e.stopPropagation();
            }
            
            ['dragenter', 'dragover'].forEach(eventName => {
                uploadZone.addEventListener(eventName, highlight, false);
            });
            
            ['dragleave', 'drop'].forEach(eventName => {
                uploadZone.addEventListener(eventName, unhighlight, false);
            });
            
            function highlight(e) {
                uploadZone.classList.add('drag-over');
            }
            
            function unhighlight(e) {
                uploadZone.classList.remove('drag-over');
            }
            
            uploadZone.addEventListener('drop', handleDrop, false);
            
            function handleDrop(e) {
                const dt = e.dataTransfer;
                const files = dt.files;
                
                if (files.length > 0) {
                    const file = files[0];
                    if (isValidImageFile(file)) {
                        fileInput.files = files;
                        previewImage(fileInput);
                    } else {
                        showError('Please upload a valid image file (JPG, PNG, GIF)');
                        uploadZone.classList.add('upload-error');
                        setTimeout(() => {
                            uploadZone.classList.remove('upload-error');
                        }, 3000);
                    }
                }
            }
        }
        
        function isValidImageFile(file) {
            const validTypes = ['image/jpeg', 'image/png', 'image/gif'];
            const maxSize = 5 * 1024 * 1024; // 5MB
            
            if (!validTypes.includes(file.type)) {
                return false;
            }
            
            if (file.size > maxSize) {
                showError('File size must be less than 5MB');
                return false;
            }
            
            return true;
        }
        
        function previewImage(input) {
            const file = input.files[0];
            
            if (file && isValidImageFile(file)) {
                const uploadZone = document.getElementById('uploadZone');
                const preview = document.getElementById('imagePreview');
                const previewImg = document.getElementById('previewImg');
                
                uploadZone.classList.add('upload-success');
                setTimeout(() => {
                    uploadZone.classList.remove('upload-success');
                }, 1000);
                
                const reader = new FileReader();
                
                reader.onload = function(e) {
                    previewImg.src = e.target.result;
                    
                    // Smooth transition
                    uploadZone.style.opacity = '0';
                    setTimeout(() => {
                        uploadZone.style.display = 'none';
                        preview.style.display = 'block';
                        preview.style.opacity = '0';
                        setTimeout(() => {
                            preview.style.opacity = '1';
                        }, 50);
                    }, 300);
                    
                    updatePostTypeHint();
                };
                
                reader.readAsDataURL(file);
            }
        }
        
        function removeImage() {
            const photoInput = document.getElementById('photoInput');
            const preview = document.getElementById('imagePreview');
            const uploadZone = document.getElementById('uploadZone');
            
            photoInput.value = '';
            
            // Smooth transition back
            preview.style.opacity = '0';
            setTimeout(() => {
                preview.style.display = 'none';
                uploadZone.style.display = 'flex';
                uploadZone.style.opacity = '0';
                setTimeout(() => {
                    uploadZone.style.opacity = '1';
                }, 50);
            }, 300);
            
            updatePostTypeHint();
        }
        
        function updatePostTypeHint() {
            const postTypeHint = document.getElementById('postTypeHint');
            const postTypeHintContainer = postTypeHint?.parentElement;
            const contentTextarea = document.querySelector('textarea[name="PostInput.Content"]');
            const hasPhoto = document.getElementById('imagePreview').style.display !== 'none';
            const hasText = contentTextarea && contentTextarea.value.trim().length > 0;
            
            if (postTypeHint && postTypeHintContainer) {
                // Remove all classes
                postTypeHintContainer.classList.remove('has-photo', 'has-text', 'has-both');
                
                if (hasPhoto && hasText) {
                    postTypeHint.innerHTML = '<i class="fas fa-check-circle me-1"></i>Photo with caption ready to post!';
                    postTypeHintContainer.classList.add('has-both');
                } else if (hasPhoto) {
                    postTypeHint.innerHTML = '<i class="fas fa-camera me-1"></i>Photo ready! Add a caption or post as is.';
                    postTypeHintContainer.classList.add('has-photo');
                } else if (hasText) {
                    postTypeHint.innerHTML = '<i class="fas fa-font me-1"></i>Text post ready! Add a photo if you like.';
                    postTypeHintContainer.classList.add('has-text');
                } else {
                    postTypeHint.innerHTML = '<i class="fas fa-info-circle me-1"></i>Add a photo, write something, or both!';
                }
            }
        }
        
        function showError(message) {
            // Create a toast-like notification
            const toast = document.createElement('div');
            toast.className = 'alert alert-danger position-fixed';
            toast.style.cssText = `
                top: 20px;
                right: 20px;
                z-index: 9999;
                min-width: 300px;
                animation: slideIn 0.3s ease-out;
                box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
                border-radius: 12px;
                border: none;
            `;
            toast.innerHTML = `
                <i class="fas fa-exclamation-triangle me-2"></i>
                ${message}
                <button type="button" class="btn-close float-end" onclick="this.parentElement.remove()"></button>
            `;
            
            document.body.appendChild(toast);
            
            // Auto remove after 5 seconds
            setTimeout(() => {
                if (toast.parentElement) {
                    toast.style.animation = 'slideOut 0.3s ease-out';
                    setTimeout(() => {
                        toast.remove();
                    }, 300);
                }
            }, 5000);
        }
        
        // Add CSS for enhanced animations
        const style = document.createElement('style');
        style.textContent = `
            @@keyframes slideIn {
                from { transform: translateX(100%); opacity: 0; }
                to { transform: translateX(0); opacity: 1; }
            }
            @@keyframes slideOut {
                from { transform: translateX(0); opacity: 1; }
                to { transform: translateX(100%); opacity: 0; }
            }
            .upload-zone {
                transition: all 0.3s ease;
            }
            .image-preview-container {
                transition: all 0.3s ease;
            }
            .focused .form-control {
                transform: translateY(-2px);
                box-shadow: 0 8px 25px rgba(0, 149, 246, 0.15);
            }
        `;
        document.head.appendChild(style);