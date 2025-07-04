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
    
    // Comment form submission
    const commentForm = document.getElementById('commentForm');
    if (commentForm) {
        commentForm.addEventListener('submit', function(e) {
            e.preventDefault();
            
            const commentInput = document.getElementById('commentInput');
            const submitBtn = commentForm.querySelector('button[type="submit"]');
            const content = commentInput.value.trim();
            
            if (!content) {
                showNotification('Please enter a comment', 'error');
                return;
            }
            
            if (content.length > 2000) {
                showNotification('Comment is too long (max 2000 characters)', 'error');
                return;
            }
            
            if (!currentPostId) {
                showNotification('Please select a post first', 'error');
                return;
            }
            
            const token = getSecurityToken();
            if (!token) return;
            
            // Disable form during submission
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
            commentInput.disabled = true;
            
            fetch('/Account/Profile?handler=AddComment', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': token
                },
                body: `CommentInput.PostId=${currentPostId}&CommentInput.Content=${encodeURIComponent(content)}`
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Add comment to current post data
                    if (currentPostData) {
                        // Add canDelete property based on current user
                        data.comment.canDelete = true; // Current user can delete their own comments
                        data.comment.isLikedByCurrentUser = false;
                        data.comment.likeCount = 0;
                        data.comment.replies = [];
                        
                        currentPostData.comments.push(data.comment);
                    }
                    
                    // Reload comments with animation
                    loadComments();
                    
                    // Clear input
                    commentInput.value = '';
                    
                    // Show success feedback
                    showNotification('Comment added successfully!', 'success');
                    
                    // Scroll to latest comment
                    setTimeout(() => {
                        const commentsSection = document.getElementById('commentsSection');
                        if (commentsSection.firstElementChild) {
                            commentsSection.firstElementChild.scrollIntoView({ behavior: 'smooth' });
                        }
                    }, 100);
                } else {
                    showNotification('Failed to add comment: ' + (data.message || 'Unknown error'), 'error');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                showNotification('An error occurred while adding the comment', 'error');
            })
            .finally(() => {
                // Re-enable form
                submitBtn.disabled = false;
                submitBtn.innerHTML = 'Post';
                commentInput.disabled = false;
                commentInput.focus();
            });
        });
        
        // Auto-resize comment input and add character counter
        const commentInput = document.getElementById('commentInput');
        if (commentInput) {
            // Add character counter
            const charCounter = document.createElement('small');
            charCounter.className = 'text-muted char-counter';
            charCounter.style.display = 'none';
            commentForm.appendChild(charCounter);
            
            commentInput.addEventListener('input', function() {
                const length = this.value.length;
                
                if (length > 1900) {
                    charCounter.textContent = `${length}/2000`;
                    charCounter.style.display = 'block';
                    charCounter.className = length > 2000 ? 'text-danger char-counter' : 'text-muted char-counter';
                } else {
                    charCounter.style.display = 'none';
                }
            });
            
            // Submit on Ctrl+Enter
            commentInput.addEventListener('keydown', function(e) {
                if (e.ctrlKey && e.key === 'Enter') {
                    commentForm.dispatchEvent(new Event('submit'));
                }
            });
        }
    }
    
    // Handle edit profile form submission
    const editProfileForm = document.getElementById('editProfileForm');
    if (editProfileForm) {
        editProfileForm.addEventListener('submit', function(e) {
            console.log('Edit profile form submitted');
            // Let the form submit normally - don't prevent default
            // This will trigger the OnPostUpdateProfileAsync method
        });
    }
    
    // Initial state
    updatePostTypeHint();
    addCommentAnimations();
    enhanceCommentInteractions();

    // Set up album modal
    const createAlbumModal = document.getElementById('createAlbumModal');
    if (createAlbumModal) {
        createAlbumModal.addEventListener('shown.bs.modal', function() {
            initializeAlbumModal();
        });
        
        createAlbumModal.addEventListener('hidden.bs.modal', function() {
            resetAlbumModal();
        });
    }
    
    // Set up edit album modal
    const editAlbumModal = document.getElementById('editAlbumModal');
    if (editAlbumModal) {
        editAlbumModal.addEventListener('hidden.bs.modal', function() {
            resetEditAlbumModal();
        });
    }
    
    // Add form submission debugging for edit album
    const editAlbumForm = document.getElementById('editAlbumForm');
    if (editAlbumForm) {
        editAlbumForm.addEventListener('submit', function(e) {
            console.log('=== EDIT ALBUM FORM SUBMISSION ===');
            
            // Log all form data
            const formData = new FormData(this);
            for (let [key, value] of formData.entries()) {
                console.log(`Form field: ${key} = ${value}`);
            }
            
            // Specifically log photos to remove
            const photosToRemoveInput = document.getElementById('photosToRemove');
            console.log('Photos to remove input value:', photosToRemoveInput ? photosToRemoveInput.value : 'NOT FOUND');
            console.log('Photos to remove set:', Array.from(photosToRemove));
            
            console.log('=== END FORM SUBMISSION DEBUG ===');
            
            // Let the form submit normally
        });
    }
});

// Enhanced notification system for comments
function showCommentNotification(message, type = 'info', duration = 3000) {
    // Remove any existing comment notifications
    const existingToasts = document.querySelectorAll('.comment-toast');
    existingToasts.forEach(toast => toast.remove());
    
    const toast = document.createElement('div');
    toast.className = `comment-toast alert alert-${type} alert-dismissible fade show position-fixed`;
    toast.style.cssText = `
        top: 20px;
        right: 20px;
        z-index: 1060;
        min-width: 300px;
        max-width: 400px;
        box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
        border: none;
        animation: slideInRight 0.3s ease-out;
    `;
    
    const iconMap = {
        'success': 'fas fa-check-circle',
        'error': 'fas fa-exclamation-circle',
        'warning': 'fas fa-exclamation-triangle',
        'info': 'fas fa-info-circle'
    };
    
    toast.innerHTML = `
        <div class="d-flex align-items-center">
            <i class="${iconMap[type] || iconMap.info} me-2"></i>
            <span>${message}</span>
            <button type="button" class="btn-close ms-auto" data-bs-dismiss="alert"></button>
        </div>
    `;
    
    document.body.appendChild(toast);
    
    // Auto-remove after duration
    setTimeout(() => {
        if (toast.parentNode) {
            toast.classList.remove('show');
            setTimeout(() => toast.remove(), 150);
        }
    }, duration);
}

// Enhanced comment interaction feedback
function enhanceCommentInteractions() {
    // Add loading states for comment forms
    document.addEventListener('submit', function(e) {
        if (e.target.id === 'commentForm') {
            const submitBtn = e.target.querySelector('button[type="submit"]');
            const originalText = submitBtn.innerHTML;
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Posting...';
            submitBtn.disabled = true;
            
            // Re-enable after a delay (will be overridden by actual response)
            setTimeout(() => {
                if (submitBtn.disabled) {
                    submitBtn.innerHTML = originalText;
                    submitBtn.disabled = false;
                }
            }, 5000);
        }
    });
    
    // Add pulse effect to new comments
    const observer = new MutationObserver(function(mutations) {
        mutations.forEach(function(mutation) {
            mutation.addedNodes.forEach(function(node) {
                if (node.nodeType === 1 && node.classList && node.classList.contains('comment-item')) {
                    node.style.animation = 'newCommentPulse 1s ease-out';
                }
            });
        });
    });
    
    const commentsSection = document.getElementById('commentsSection');
    if (commentsSection) {
        observer.observe(commentsSection, { childList: true, subtree: true });
    }
}

// Add the animation keyframe
function addCommentAnimations() {
    if (!document.getElementById('comment-animations')) {
        const style = document.createElement('style');
        style.id = 'comment-animations';
        style.textContent = `
            @keyframes slideInRight {
                from {
                    opacity: 0;
                    transform: translateX(100%);
                }
                to {
                    opacity: 1;
                    transform: translateX(0);
                }
            }
            
            @keyframes newCommentPulse {
                0% {
                    background-color: rgba(40, 167, 69, 0.1);
                    transform: scale(1);
                }
                50% {
                    background-color: rgba(40, 167, 69, 0.05);
                    transform: scale(1.02);
                }
                100% {
                    background-color: transparent;
                    transform: scale(1);
                }
            }
        `;
        document.head.appendChild(style);
    }
}

// Replace the generic showNotification with our enhanced version for comments
window.showNotification = showCommentNotification;

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
    const hasPhoto = document.getElementById('imagePreview')?.style.display !== 'none';
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

// Utility functions
function getSecurityToken() {
    console.log('🔍 Looking for security token...');
    
    // First try the direct selector
    let token = document.querySelector('input[name="__RequestVerificationToken"]');
    console.log('🔍 Direct selector result:', token ? `Found token input (value length: ${token.value?.length})` : 'Not found');
    
    if (!token) {
        console.log('🔍 Trying form-specific selector...');
        token = document.querySelector('form input[name="__RequestVerificationToken"]');
        console.log('🔍 Form selector result:', token ? `Found token input (value length: ${token.value?.length})` : 'Not found');
    }
    
    if (!token) {
        console.log('🔍 Trying to find token in ajaxTokenForm specifically...');
        token = document.querySelector('#ajaxTokenForm input[name="__RequestVerificationToken"]');
        console.log('🔍 ajaxTokenForm selector result:', token ? `Found token input (value length: ${token.value?.length})` : 'Not found');
    }
    
    if (!token) {
        console.log('🔍 Trying to find any forms on the page...');
        const forms = document.querySelectorAll('form');
        console.log('📝 Found forms:', forms.length);
        
        forms.forEach((form, index) => {
            console.log(`📝 Form ${index}:`, {
                id: form.id,
                action: form.action,
                method: form.method,
                inputs: form.querySelectorAll('input').length
            });
            
            const formToken = form.querySelector('input[name="__RequestVerificationToken"]');
            if (formToken) {
                console.log(`🎯 Found token in form ${index}:`, formToken.value?.substring(0, 20) + '...');
                token = formToken;
            }
        });
    }
    
    if (!token) {
        console.error('❌ Security token not found anywhere on the page');
        
        // Debug: Show all inputs on the page
        const allInputs = document.querySelectorAll('input');
        console.log('🔍 All inputs on page:', Array.from(allInputs).map(input => ({
            name: input.name,
            type: input.type,
            id: input.id,
            value: input.value?.substring(0, 20) + (input.value?.length > 20 ? '...' : '')
        })));
        
        alert('Security token not found');
        return null;
    }
    
    console.log('✅ Security token found:', token.value?.substring(0, 20) + '...');
    return token.value;
}

function showNotification(message, type = 'error') {
    const toast = document.createElement('div');
    const isError = type === 'error';
    const icon = isError ? 'fas fa-exclamation-triangle' : 'fas fa-check-circle';
    const className = isError ? 'toast-error' : 'toast-success';
    const timeout = isError ? 5000 : 3000;
    
    toast.className = `alert toast-notification ${className} position-fixed slide-in`;
    toast.innerHTML = `
        <i class="${icon} me-2"></i>
        ${message}
        <button type="button" class="btn-close float-end" onclick="this.parentElement.remove()"></button>
    `;
    
    document.body.appendChild(toast);
    
    setTimeout(() => {
        if (toast.parentElement) {
            toast.classList.remove('slide-in');
            toast.classList.add('slide-out');
            setTimeout(() => {
                toast.remove();
            }, 300);
        }
    }, timeout);
}

function showError(message) {
    showNotification(message, 'error');
}

function showSuccess(message) {
    showNotification(message, 'success');
}

function showModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal && window.bootstrap) {
        new bootstrap.Modal(modal).show();
    }
}

function hideModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal && window.bootstrap) {
        const modalInstance = bootstrap.Modal.getInstance(modal);
        if (modalInstance) {
            modalInstance.hide();
        }
    }
}

function createAvatarElement(profilePictureUrl, size = '30px', additionalClasses = '') {
    if (profilePictureUrl) {
        return `<img src="${profilePictureUrl}" alt="Profile" class="rounded-circle ${additionalClasses}" style="width: ${size}; height: ${size}; object-fit: cover;" />`;
    } else {
        const iconSize = size === '40px' ? '' : 'small';
        return `<div class="rounded-circle bg-secondary d-flex align-items-center justify-content-center ${additionalClasses}" style="width: ${size}; height: ${size};"><i class="fas fa-user text-white ${iconSize}"></i></div>`;
    }
}

// Global variables for post modal functionality
let currentPostId = null;
let currentPostData = null;
let postsData = window.postsData || [];

function openPostModal(postId) {
    currentPostId = postId;
    currentPostData = postsData.find(p => p.id === postId);
    
    if (!currentPostData) return;
    
    // Set user info
    const postUserName = document.getElementById('postUserName');
    const postDate = document.getElementById('postDate');
    
    if (postUserName) {
        postUserName.textContent = `${currentPostData.user.firstName} ${currentPostData.user.lastName}`;
    }
    if (postDate) {
        postDate.textContent = currentPostData.createdAt;
    }
    
    // Set user avatar
    const avatar = document.getElementById('postUserAvatar');
    if (avatar) {
        if (currentPostData.user.profilePictureUrl) {
            avatar.src = currentPostData.user.profilePictureUrl;
            avatar.style.display = 'block';
        } else {
            avatar.style.display = 'none';
        }
    }
    
    // Set image or adjust layout
    const imageSection = document.getElementById('postImageSection');
    const contentSection = document.getElementById('postContentSection');
    
    if (currentPostData.imageUrl) {
        const postImage = document.getElementById('postImage');
        if (postImage) {
            postImage.src = currentPostData.imageUrl;
        }
        if (imageSection) {
            imageSection.style.display = 'block';
        }
        if (contentSection) {
            contentSection.className = 'col-lg-4';
        }
    } else {
        if (imageSection) {
            imageSection.style.display = 'none';
        }
        if (contentSection) {
            contentSection.className = 'col-12';
            contentSection.style.borderLeft = 'none';
        }
    }
    
    // Set caption
    const postCaption = document.getElementById('postCaption');
    if (postCaption) {
        postCaption.textContent = currentPostData.caption;
    }
    
    // Initialize like button state
    const likeBtn = document.getElementById('likeBtn');
    const likeCount = document.getElementById('likeCount');
    
    if (likeBtn) {
        const likeIcon = likeBtn.querySelector('i');
        if (likeIcon) {
            if (currentPostData.isLikedByCurrentUser) {
                likeIcon.classList.remove('far');
                likeIcon.classList.add('fas');
                likeBtn.classList.add('liked');
            } else {
                likeIcon.classList.remove('fas');
                likeIcon.classList.add('far');
                likeBtn.classList.remove('liked');
            }
        }
    }
    
    if (likeCount) {
        likeCount.textContent = `${currentPostData.likeCount} like${currentPostData.likeCount !== 1 ? 's' : ''}`;
    }
    
    // Load comments
    loadComments();
    
    // Show modal
    const postModal = document.getElementById('postModal');
    if (postModal && window.bootstrap) {
        new bootstrap.Modal(postModal).show();
    }
}

function loadComments(page = 1, pageSize = 10) {
    const commentsSection = document.getElementById('commentsSection');
    if (!commentsSection || !currentPostData) return;
    
    // Only clear if it's the first page
    if (page === 1) {
        commentsSection.innerHTML = '';
    }
    
    if (currentPostData.comments.length === 0) {
        commentsSection.innerHTML = `
            <div class="text-center py-4">
                <i class="far fa-comment fa-2x text-muted mb-2"></i>
                <p class="text-muted small mb-0">No comments yet</p>
                <p class="text-muted small">Be the first to comment!</p>
            </div>
        `;
        return;
    }
    
    // Add comment sorting controls (only on first page)
    if (page === 1) {
        const commentCountDiv = document.createElement('div');
        commentCountDiv.className = 'comment-count mb-3';
        commentCountDiv.innerHTML = `
            <small class="text-muted">${currentPostData.comments.length} comment${currentPostData.comments.length !== 1 ? 's' : ''}</small>
        `;
        commentsSection.appendChild(commentCountDiv);
    }
    
    // Calculate pagination
    const startIndex = (page - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const paginatedComments = currentPostData.comments.slice(startIndex, endIndex);
    
    // Create comments container if it doesn't exist
    let commentsContainer = document.getElementById('commentsContainer');
    if (!commentsContainer) {
        commentsContainer = document.createElement('div');
        commentsContainer.id = 'commentsContainer';
        commentsSection.appendChild(commentsContainer);
    }
    
    // Add comments to container
    paginatedComments.forEach(comment => {
        const commentElement = createCommentElement(comment);
        commentsContainer.appendChild(commentElement);
    });
    
    // Add load more button if there are more comments
    const hasMore = endIndex < currentPostData.comments.length;
    let loadMoreContainer = document.getElementById('loadMoreContainer');
    
    if (loadMoreContainer) {
        loadMoreContainer.remove();
    }
    
    if (hasMore) {
        loadMoreContainer = document.createElement('div');
        loadMoreContainer.id = 'loadMoreContainer';
        loadMoreContainer.className = 'text-center mt-3';
        loadMoreContainer.innerHTML = `
            <button class="btn btn-outline-primary btn-sm" onclick="loadMoreComments()">
                <i class="fas fa-chevron-down me-1"></i>
                Load more comments (${currentPostData.comments.length - endIndex} remaining)
            </button>
        `;
        commentsSection.appendChild(loadMoreContainer);
    }
    
    // Update comment count display
    updateCommentCount();
    
    // Store current page for load more functionality
    window.currentCommentPage = page;
}

function loadMoreComments() {
    const nextPage = (window.currentCommentPage || 1) + 1;
    loadComments(nextPage, 10);
}

function createCommentElement(comment) {
    const commentDiv = document.createElement('div');
    commentDiv.className = 'comment-item mb-3 p-2 rounded';
    commentDiv.setAttribute('data-comment-id', comment.id);
    
    // Calculate time ago
    const timeAgo = getTimeAgo(comment.createdAt);
    
    commentDiv.innerHTML = `
        <div class="d-flex">
            <div class="me-2 flex-shrink-0">
                ${createAvatarElement(comment.user.profilePictureUrl, '32px')}
            </div>
            <div class="flex-grow-1">
                <div class="comment-content rounded px-3 py-2">
                    <div class="d-flex justify-content-between align-items-start">
                        <div class="flex-grow-1">
                            <strong class="comment-author small text-dark">${comment.user.firstName} ${comment.user.lastName}</strong>
                            <p class="comment-text mb-1 small" id="commentText-${comment.id}">${escapeHtml(comment.content)}</p>
                            <div class="comment-edit-form mt-2" id="editForm-${comment.id}" style="display: none;">
                                <div class="input-group input-group-sm">
                                    <input type="text" class="form-control edit-comment-input" 
                                           value="${escapeHtml(comment.content)}" 
                                           maxlength="2000">
                                    <button class="btn btn-accent btn-sm" type="button" onclick="saveCommentEdit(${comment.id})">
                                        <i class="fas fa-check"></i>
                                    </button>
                                    <button class="btn btn-secondary btn-sm" type="button" onclick="cancelCommentEdit(${comment.id})">
                                        <i class="fas fa-times"></i>
                                    </button>
                                </div>
                            </div>
                        </div>
                        ${comment.canDelete ? `
                            <div class="comment-actions-buttons">
                                <button class="btn btn-link btn-sm p-0 me-1 text-muted edit-comment-btn" 
                                        onclick="editComment(${comment.id})" 
                                        title="Edit comment">
                                    <i class="fas fa-edit"></i>
                                </button>
                                <button class="btn btn-link btn-sm p-0 text-muted delete-comment-btn" 
                                        onclick="deleteComment(${comment.id})" 
                                        title="Delete comment">
                                    <i class="fas fa-times"></i>
                                </button>
                            </div>
                        ` : ''}
                    </div>
                </div>
                <div class="comment-actions d-flex align-items-center mt-1">
                    <small class="text-muted me-3">${timeAgo}</small>
                    <button class="btn btn-link btn-sm p-0 me-3 text-muted comment-like-btn ${comment.isLikedByCurrentUser ? 'liked' : ''}" 
                            onclick="toggleCommentLike(${comment.id})" 
                            title="Like comment">
                        <i class="${comment.isLikedByCurrentUser ? 'fas' : 'far'} fa-heart"></i>
                        ${comment.likeCount > 0 ? `<span class="ms-1">${comment.likeCount}</span>` : ''}
                    </button>
                    <button class="btn btn-link btn-sm p-0 text-muted reply-btn" 
                            onclick="showReplyForm(${comment.id})" 
                            title="Reply to comment"
                            ${comment.replies && comment.replies.length >= 3 ? 'disabled' : ''}>
                        <i class="far fa-comment"></i> Reply ${comment.replies && comment.replies.length >= 3 ? '(Max depth reached)' : ''}
                    </button>
                </div>
                <div class="reply-form-container mt-2" id="replyForm-${comment.id}" style="display: none;">
                    <div class="d-flex">
                        <div class="me-2 flex-shrink-0">
                            ${createAvatarElement(window.currentUserData.profilePictureUrl, '24px')}
                        </div>
                        <div class="flex-grow-1">
                            <div class="input-group input-group-sm">
                                <input type="text" class="form-control reply-input" 
                                       placeholder="Write a reply..." 
                                       maxlength="500">
                                <button class="btn btn-accent" type="button" onclick="submitReply(${comment.id})">
                                    <i class="fas fa-paper-plane"></i>
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
                ${comment.replies && comment.replies.length > 0 ? createRepliesSection(comment.replies) : ''}
            </div>
        </div>
    `;
    
    return commentDiv;
}

function createRepliesSection(replies) {
    if (!replies || replies.length === 0) return '';
    
    let repliesHtml = '<div class="replies-section mt-2 ms-3">';
    
    // Limit the number of visible replies to prevent overwhelming UI
    const maxVisibleReplies = 5;
    const visibleReplies = replies.slice(0, maxVisibleReplies);
    const hiddenCount = replies.length - maxVisibleReplies;
    
    visibleReplies.forEach((reply, index) => {
        const timeAgo = getTimeAgo(reply.createdAt);
        repliesHtml += `
            <div class="reply-item d-flex mb-2" data-reply-id="${reply.id}">
                <div class="me-2 flex-shrink-0">
                    ${createAvatarElement(reply.user.profilePictureUrl, '24px')}
                </div>
                <div class="flex-grow-1">
                    <div class="reply-content rounded px-2 py-1">
                        <div class="d-flex justify-content-between align-items-start">
                            <div class="flex-grow-1">
                                <strong class="small text-primary">${reply.user.firstName} ${reply.user.lastName}</strong>
                                <span class="small ms-1">${escapeHtml(reply.content)}</span>
                            </div>
                            ${reply.canDelete ? `
                                <button class="btn btn-link btn-sm p-0 ms-1 text-muted delete-reply-btn" 
                                        onclick="deleteReply(${reply.id})" 
                                        title="Delete reply">
                                    <i class="fas fa-times small"></i>
                                </button>
                            ` : ''}
                        </div>
                    </div>
                    <small class="text-muted ms-2">${timeAgo}</small>
                </div>
            </div>
        `;
    });
    
    // Show "View more replies" button if there are hidden replies
    if (hiddenCount > 0) {
        repliesHtml += `
            <div class="text-center mt-2">
                <button class="btn btn-link btn-sm text-muted" onclick="showAllReplies(this)">
                    <i class="fas fa-chevron-down"></i> View ${hiddenCount} more ${hiddenCount === 1 ? 'reply' : 'replies'}
                </button>
            </div>
        `;
    }
    
    repliesHtml += '</div>';
    return repliesHtml;
}

function showAllReplies(button) {
    const repliesSection = button.closest('.replies-section');
    const commentId = repliesSection.closest('[data-comment-id]').getAttribute('data-comment-id');
    
    if (currentPostData) {
        const comment = currentPostData.comments.find(c => c.id == commentId);
        if (comment && comment.replies) {
            // Replace the entire replies section with all replies
            const newRepliesHtml = createFullRepliesSection(comment.replies);
            repliesSection.innerHTML = newRepliesHtml.replace('<div class="replies-section mt-2 ms-3">', '').replace('</div>', '');
        }
    }
}

function createFullRepliesSection(replies) {
    let repliesHtml = '<div class="replies-section mt-2 ms-3">';
    
    replies.forEach(reply => {
        const timeAgo = getTimeAgo(reply.createdAt);
        repliesHtml += `
            <div class="reply-item d-flex mb-2" data-reply-id="${reply.id}">
                <div class="me-2 flex-shrink-0">
                    ${createAvatarElement(reply.user.profilePictureUrl, '24px')}
                </div>
                <div class="flex-grow-1">
                    <div class="reply-content rounded px-2 py-1">
                        <div class="d-flex justify-content-between align-items-start">
                            <div class="flex-grow-1">
                                <strong class="small text-primary">${reply.user.firstName} ${reply.user.lastName}</strong>
                                <span class="small ms-1">${escapeHtml(reply.content)}</span>
                            </div>
                            ${reply.canDelete ? `
                                <button class="btn btn-link btn-sm p-0 ms-1 text-muted delete-reply-btn" 
                                        onclick="deleteReply(${reply.id})" 
                                        title="Delete reply">
                                    <i class="fas fa-times small"></i>
                                </button>
                            ` : ''}
                        </div>
                    </div>
                    <small class="text-muted ms-2">${timeAgo}</small>
                </div>
            </div>
        `;
    });
    
    repliesHtml += '</div>';
    return repliesHtml;
}

function showReplyForm(commentId) {
    // Hide any other open reply forms
    document.querySelectorAll('.reply-form-container').forEach(form => {
        if (form.id !== `replyForm-${commentId}`) {
            form.style.display = 'none';
        }
    });
    
    const replyForm = document.getElementById(`replyForm-${commentId}`);
    if (!replyForm) return;
    
    // Check if comment already has maximum replies
    if (currentPostData) {
        const comment = currentPostData.comments.find(c => c.id == commentId);
        if (comment && comment.replies && comment.replies.length >= 10) {
            showNotification('Maximum number of replies reached for this comment', 'warning');
            return;
        }
    }
    
    if (replyForm.style.display === 'none' || !replyForm.style.display) {
        replyForm.style.display = 'block';
        const input = replyForm.querySelector('.reply-input');
        if (input) {
            input.focus();
        }
    } else {
        replyForm.style.display = 'none';
    }
}

function deleteComment(commentId) {
    if (!confirm('Are you sure you want to delete this comment?')) return;
    
    const token = getSecurityToken();
    if (!token) return;
    
    fetch('/Account/Profile?handler=DeleteComment', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token
        },
        body: `commentId=${commentId}`
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Remove comment from current post data
            if (currentPostData) {
                currentPostData.comments = currentPostData.comments.filter(c => c.id !== commentId);
            }
            
            // Remove comment element with animation
            const commentElement = document.querySelector(`[data-comment-id="${commentId}"]`);
            if (commentElement) {
                commentElement.style.opacity = '0.5';
                commentElement.style.transform = 'translateX(-20px)';
                setTimeout(() => {
                    commentElement.remove();
                    updateCommentCount();
                }, 300);
            }
            
            showNotification('Comment deleted successfully', 'success');
        } else {
            showNotification('Failed to delete comment: ' + (data.message || 'Unknown error'), 'error');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showNotification('An error occurred while deleting the comment', 'error');
    });
}

function toggleCommentLike(commentId) {
    const token = getSecurityToken();
    if (!token) return;
    
    const likeBtn = document.querySelector(`[data-comment-id="${commentId}"] .comment-like-btn`);
    if (!likeBtn) return;
    
    const icon = likeBtn.querySelector('i');
    const countSpan = likeBtn.querySelector('span');
    
    // Optimistic UI update
    const wasLiked = likeBtn.classList.contains('liked');
    likeBtn.classList.toggle('liked');
    icon.classList.toggle('fas', !wasLiked);
    icon.classList.toggle('far', wasLiked);
    
    fetch('/Account/Profile?handler=LikeComment', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token
        },
        body: `commentId=${commentId}`
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Update like count
            if (data.likeCount > 0) {
                if (countSpan) {
                    countSpan.textContent = data.likeCount;
                } else {
                    likeBtn.innerHTML += `<span class="ms-1">${data.likeCount}</span>`;
                }
            } else if (countSpan) {
                countSpan.remove();
            }
            
            // Update post data
            if (currentPostData) {
                const comment = currentPostData.comments.find(c => c.id === commentId);
                if (comment) {
                    comment.isLikedByCurrentUser = data.liked;
                    comment.likeCount = data.likeCount;
                }
            }
        } else {
            // Revert optimistic update
            likeBtn.classList.toggle('liked');
            icon.classList.toggle('fas', wasLiked);
            icon.classList.toggle('far', !wasLiked);
            showNotification('Failed to like comment', 'error');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        // Revert optimistic update
        likeBtn.classList.toggle('liked');
        icon.classList.toggle('fas', wasLiked);
        icon.classList.toggle('far', !wasLiked);
        showNotification('An error occurred while liking the comment', 'error');
    });
}

function submitReply(commentId) {
    const replyForm = document.getElementById(`replyForm-${commentId}`);
    if (!replyForm) return;
    
    const input = replyForm.querySelector('.reply-input');
    const content = input.value.trim();
    
    if (!content) {
        showNotification('Please enter a reply', 'error');
        return;
    }
    
    const token = getSecurityToken();
    if (!token) return;
    
    const submitBtn = replyForm.querySelector('button');
    submitBtn.disabled = true;
    submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
    
    fetch('/Account/Profile?handler=AddReply', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token
        },
        body: `commentId=${commentId}&content=${encodeURIComponent(content)}`
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Add reply to comment data
            if (currentPostData) {
                const comment = currentPostData.comments.find(c => c.id === commentId);
                if (comment) {
                    if (!comment.replies) comment.replies = [];
                    data.reply.canDelete = true;
                    comment.replies.push(data.reply);
                }
            }
            
            // Reload comments to show new reply
            loadComments();
            showNotification('Reply added successfully!', 'success');
        } else {
            showNotification('Failed to add reply: ' + (data.message || 'Unknown error'), 'error');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showNotification('An error occurred while adding the reply', 'error');
    })
    .finally(() => {
        submitBtn.disabled = false;
        submitBtn.innerHTML = '<i class="fas fa-paper-plane"></i>';
    });
}

function deleteReply(replyId) {
    if (!confirm('Are you sure you want to delete this reply?')) return;
    
    const token = getSecurityToken();
    if (!token) return;
    
    fetch('/Account/Profile?handler=DeleteReply', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token
        },
        body: `replyId=${replyId}`
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Remove reply from post data
            if (currentPostData) {
                currentPostData.comments.forEach(comment => {
                    if (comment.replies) {
                        comment.replies = comment.replies.filter(r => r.id !== replyId);
                    }
                });
            }
            
            // Remove reply element
            const replyElement = document.querySelector(`[data-reply-id="${replyId}"]`);
            if (replyElement) {
                replyElement.remove();
            }
            
            showNotification('Reply deleted successfully', 'success');
        } else {
            showNotification('Failed to delete reply: ' + (data.message || 'Unknown error'), 'error');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showNotification('An error occurred while deleting the reply', 'error');
    });
}

function updateCommentCount() {
    if (!currentPostData) return;
    
    const count = currentPostData.comments.length;
    const commentButton = document.querySelector('.fa-comment').closest('button');
    
    if (commentButton) {
        // Update comment button with count
        const existingCount = commentButton.querySelector('.comment-count');
        if (existingCount) {
            existingCount.remove();
        }
        
        if (count > 0) {
            const countSpan = document.createElement('span');
            countSpan.className = 'comment-count ms-1 small';
            countSpan.textContent = count;
            commentButton.appendChild(countSpan);
        }
    }
}

// Utility functions for comments
function getTimeAgo(dateString) {
    const date = new Date(dateString);
    const now = new Date();
    const diffInSeconds = Math.floor((now - date) / 1000);
    
    const intervals = [
        { label: 'year', seconds: 31536000 },
        { label: 'month', seconds: 2592000 },
        { label: 'week', seconds: 604800 },
        { label: 'day', seconds: 86400 },
        { label: 'hour', seconds: 3600 },
        { label: 'minute', seconds: 60 }
    ];
    
    for (const interval of intervals) {
        const count = Math.floor(diffInSeconds / interval.seconds);
        if (count >= 1) {
            return `${count} ${interval.label}${count !== 1 ? 's' : ''} ago`;
        }
    }
    
    return 'Just now';
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function toggleLike() {
    if (!currentPostId) return;
    
    const token = getSecurityToken();
    if (!token) return;
    
    fetch('/Account/Profile?handler=LikePost', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token
        },
        body: `postId=${currentPostId}`
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Update UI
            const likeBtn = document.getElementById('likeBtn');
            const likeCount = document.getElementById('likeCount');
            
            if (likeBtn) {
                const icon = likeBtn.querySelector('i');
                if (icon) {
                    if (data.liked) {
                        icon.classList.remove('far');
                        icon.classList.add('fas');
                        likeBtn.classList.add('liked', 'animate-like');
                    } else {
                        icon.classList.remove('fas');
                        icon.classList.add('far');
                        likeBtn.classList.remove('liked');
                    }
                    
                    // Remove animation class after animation completes
                    setTimeout(() => {
                        likeBtn.classList.remove('animate-like');
                    }, 300);
                }
            }
            
            if (likeCount) {
                likeCount.textContent = `${data.likeCount} like${data.likeCount !== 1 ? 's' : ''}`;
            }
            
            // Update post data with new likes information
            if (currentPostData) {
                currentPostData.isLikedByCurrentUser = data.liked;
                currentPostData.likeCount = data.likeCount;
                currentPostData.likes = data.likes || [];
            }
            
            // Update the post in postsData array
            const postIndex = postsData.findIndex(p => p.id === currentPostId);
            if (postIndex !== -1) {
                postsData[postIndex].isLikedByCurrentUser = data.liked;
                postsData[postIndex].likeCount = data.likeCount;
                postsData[postIndex].likes = data.likes || [];
            }
            
            // Update the grid item like count display
            updateGridItemLikeCount(currentPostId, data.likeCount);
        } else {
            alert('Failed to like/unlike post: ' + data.message);
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('An error occurred while toggling like');
    });
}

function updateGridItemLikeCount(postId, newLikeCount) {
    // Find the grid item for this post
    const gridItem = document.querySelector(`[data-post-id="${postId}"]`);
    if (gridItem) {
        // Find the like count span within the hover overlay
        const likeCountSpan = gridItem.querySelector('.hover-overlay .fa-heart').parentElement;
        if (likeCountSpan) {
            // Update the like count text
            likeCountSpan.innerHTML = `<i class="fas fa-heart"></i> ${newLikeCount}`;
        }
    }
}

function showLikesModal() {
    if (!currentPostId || !currentPostData) return;
    
    const likesContent = document.getElementById('likesContent');
    if (!likesContent) return;
    
    if (currentPostData.likes.length === 0) {
        likesContent.innerHTML = `
            <div class="text-center p-4">
                <i class="far fa-heart fa-3x text-muted mb-3"></i>
                <p class="text-muted">No likes yet</p>
            </div>
        `;
    } else {
        likesContent.innerHTML = currentPostData.likes.map(like => `
            <div class="d-flex align-items-center p-3 border-bottom" style="border-color: var(--border-color) !important;">
                <div class="me-3">
                    ${createAvatarElement(like.user?.profilePictureUrl, '40px')}
                </div>
                <div class="flex-grow-1">
                    <div class="fw-bold" style="color: var(--text-primary);">${like.user?.firstName || 'Unknown'} ${like.user?.lastName || 'User'}</div>
                    <small class="text-muted">${like.createdAt}</small>
                </div>
            </div>
        `).join('');
    }
    
    showModal('likesModal');
}

function editPost() {
    if (!currentPostId || !currentPostData) return;
    
    // Set the post ID in the hidden field
    const editPostId = document.getElementById('editPostId');
    if (editPostId) {
        editPostId.value = currentPostId;
    }
    
    // Set the current content in the textarea
    const editPostContent = document.getElementById('editPostContent');
    if (editPostContent) {
        editPostContent.value = currentPostData.caption || '';
    }
    
    // Show/hide image section
    const editPostImageSection = document.getElementById('editPostImageSection');
    const editPostImage = document.getElementById('editPostImage');
    
    if (currentPostData.imageUrl && editPostImageSection && editPostImage) {
        editPostImage.src = currentPostData.imageUrl;
        editPostImageSection.style.display = 'block';
    } else if (editPostImageSection) {
        editPostImageSection.style.display = 'none';
    }
    
    // Hide the current post modal
    hideModal('postModal');
    
    // Show the edit modal
    showModal('editPostModal');
}

function savePostEdit() {
    const postId = document.getElementById('editPostId')?.value;
    const content = document.getElementById('editPostContent')?.value;
    
    if (!postId) {
        alert('Post ID not found');
        return;
    }

    const token = getSecurityToken();
    if (!token) return;
    
    // Show loading state
    const saveBtn = document.querySelector('#editPostModal .btn-accent');
    const originalText = saveBtn?.innerHTML;
    if (saveBtn) {
        saveBtn.innerHTML = '<span class="btn-text">Saving...</span>';
        saveBtn.classList.add('btn-loading');
        saveBtn.disabled = true;
    }        fetch('/Account/Profile?handler=EditPost', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': token
            },
            body: `postId=${encodeURIComponent(postId)}&content=${encodeURIComponent(content || '')}`
        })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Update the current post data
            if (currentPostData) {
                currentPostData.caption = content || '';
            }
            
            // Update the post in postsData array
            const postIndex = postsData.findIndex(p => p.id == postId);
            if (postIndex !== -1) {
                postsData[postIndex].caption = content || '';
            }
            
            // Update the post caption in the main modal
            const postCaption = document.getElementById('postCaption');
            if (postCaption) {
                postCaption.textContent = content || '';
            }
            
            // Hide edit modal
            hideModal('editPostModal');
            
            // Show success message
            showSuccess('Post updated successfully!');
            
            // Optionally refresh the page to show updated content
            setTimeout(() => {
                location.reload();
            }, 1500);
        } else {
            alert('Failed to update post: ' + data.message);
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('An error occurred while updating the post');
    })
    .finally(() => {
        // Restore button state
        if (saveBtn && originalText) {
            saveBtn.innerHTML = originalText;
            saveBtn.classList.remove('btn-loading');
            saveBtn.disabled = false;
        }
    });
}

function deletePost() {
    if (!currentPostId) return;
    
    // Show a more styled confirmation dialog
    const confirmed = confirm('Are you sure you want to delete this post? This action cannot be undone.');
    if (!confirmed) return;
    
    const token = getSecurityToken();
    if (!token) return;
    
    fetch('/Account/Profile?handler=DeletePost', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token
        },
        body: `postId=${currentPostId}`
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Close modal
            hideModal('postModal');
            
            // Show success message
            showSuccess('Post deleted successfully!');
            
            // Remove the post from the grid
            const postElement = document.querySelector(`[data-post-id="${currentPostId}"]`);
            if (postElement) {
                postElement.classList.add('fade-out');
                setTimeout(() => {
                    postElement.remove();
                }, 300);
            }
            
            // Update postsData array
            postsData = postsData.filter(p => p.id !== currentPostId);
            
            // Reload page after a short delay to update post count
            setTimeout(() => {
                location.reload();
            }, 1500);
        } else {
            alert('Failed to delete post: ' + data.message);
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('An error occurred while deleting the post');
    });
}

// Profile picture preview function
function previewProfilePicture(input) {
    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = function(e) {
            const currentProfilePic = document.getElementById('currentProfilePic');
            if (currentProfilePic) {
                if (currentProfilePic.tagName === 'IMG') {
                    currentProfilePic.src = e.target.result;
                } else {
                    // Replace the placeholder div with an image
                    const newImg = document.createElement('img');
                    newImg.id = 'currentProfilePic';
                    newImg.src = e.target.result;
                    newImg.alt = 'Profile Picture Preview';
                    newImg.className = 'rounded-circle';
                    newImg.style.cssText = 'width: 80px; height: 80px; object-fit: cover;';
                    currentProfilePic.parentNode.replaceChild(newImg, currentProfilePic);
                }
            }
        };
        reader.readAsDataURL(input.files[0]);
    }
}

// Remove profile picture function
function removeProfilePicture() {
    if (!confirm('Are you sure you want to remove your profile picture?')) return;
    
    const token = getSecurityToken();
    if (!token) return;
    
    fetch('/Account/Profile?handler=RemoveProfilePicture', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token
        }
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Replace image with placeholder
            const currentProfilePic = document.getElementById('currentProfilePic');
            if (currentProfilePic) {
                const newDiv = document.createElement('div');
                newDiv.id = 'currentProfilePic';
                newDiv.className = 'rounded-circle bg-secondary d-flex align-items-center justify-content-center';
                newDiv.style.cssText = 'width: 80px; height: 80px;';
                newDiv.innerHTML = '<i class="fas fa-user fa-2x text-white"></i>';
                currentProfilePic.parentNode.replaceChild(newDiv, currentProfilePic);
            }
            
            // Hide the remove button
            const removeBtn = document.querySelector('.btn-outline-danger');
            if (removeBtn) {
                removeBtn.style.display = 'none';
            }
            
            alert('Profile picture removed successfully!');
        } else {
            alert('Failed to remove profile picture: ' + data.message);
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('An error occurred while removing the profile picture');
    });
}

// Initialize everything when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    // Handle edit profile form submission
    const editProfileForm = document.getElementById('editProfileForm');
    if (editProfileForm) {
        editProfileForm.addEventListener('submit', function(e) {
            console.log('Edit profile form submitted');
            // Let the form submit normally - don't prevent default
            // This will trigger the OnPostUpdateProfileAsync method
        });
    }
});

// Functions for editing comments
function editComment(commentId) {
    const commentText = document.getElementById(`commentText-${commentId}`);
    const editForm = document.getElementById(`editForm-${commentId}`);
    const editBtn = document.querySelector(`[onclick="editComment(${commentId})"]`);
    const deleteBtn = document.querySelector(`[onclick="deleteComment(${commentId})"]`);
    
    if (commentText && editForm) {
        commentText.style.display = 'none';
        editForm.style.display = 'block';
        
        // Hide edit/delete buttons while editing
        if (editBtn) editBtn.style.display = 'none';
        if (deleteBtn) deleteBtn.style.display = 'none';
        
        // Focus on the input
        const input = editForm.querySelector('.edit-comment-input');
        if (input) {
            input.focus();
            input.select();
            
            // Add keyboard event listeners
            input.addEventListener('keydown', function(e) {
                if (e.key === 'Enter' && !e.shiftKey) {
                    e.preventDefault();
                    saveCommentEdit(commentId);
                } else if (e.key === 'Escape') {
                    e.preventDefault();
                    cancelCommentEdit(commentId);
                }
            });
        }
    }
}

function cancelCommentEdit(commentId) {
    const commentText = document.getElementById(`commentText-${commentId}`);
    const editForm = document.getElementById(`editForm-${commentId}`);
    const editBtn = document.querySelector(`[onclick="editComment(${commentId})"]`);
    const deleteBtn = document.querySelector(`[onclick="deleteComment(${commentId})"]`);
    
    if (commentText && editForm) {
        commentText.style.display = 'block';
        editForm.style.display = 'none';
        
        // Show edit/delete buttons again
        if (editBtn) editBtn.style.display = 'inline-block';
        if (deleteBtn) deleteBtn.style.display = 'inline-block';
        
        // Reset input value to original
        const input = editForm.querySelector('.edit-comment-input');
        const originalText = commentText.textContent;
        if (input && originalText) {
            input.value = originalText;
        }
    }
}

function saveCommentEdit(commentId) {
    const editForm = document.getElementById(`editForm-${commentId}`);
    const input = editForm?.querySelector('.edit-comment-input');
    const newContent = input?.value.trim();
    
    if (!newContent) {
        showNotification('Comment cannot be empty', 'error');
        return;
    }
    
    if (newContent.length > 2000) {
        showNotification('Comment is too long (max 2000 characters)', 'error');
        return;
    }
    
    const token = getSecurityToken();
    if (!token) return;
    
    // Show loading state
    const saveBtn = editForm.querySelector('.btn-accent');
    const cancelBtn = editForm.querySelector('.btn-secondary');
    const originalSaveText = saveBtn.innerHTML;
    
    saveBtn.disabled = true;
    cancelBtn.disabled = true;
    saveBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
    
    fetch('/Account/Profile?handler=EditComment', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token
        },
        body: `commentId=${commentId}&content=${encodeURIComponent(newContent)}`
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Update the comment text
            const commentText = document.getElementById(`commentText-${commentId}`);
            if (commentText) {
                commentText.textContent = newContent;
            }
            
            // Update the comment in currentPostData
            if (currentPostData) {
                const comment = currentPostData.comments.find(c => c.id == commentId);
                if (comment) {
                    comment.content = newContent;
                }
            }
            
            // Exit edit mode
            cancelCommentEdit(commentId);
            
            showNotification('Comment updated successfully!', 'success');
        } else {
            showNotification('Failed to update comment: ' + (data.message || 'Unknown error'), 'error');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showNotification('An error occurred while updating the comment', 'error');
    })
    .finally(() => {
        // Restore button states
        saveBtn.disabled = false;
        cancelBtn.disabled = false;
        saveBtn.innerHTML = originalSaveText;
    });
}

// Friend Request Functions
function sendFriendRequest(receiverId) {
    console.log('🚀 sendFriendRequest called with receiverId:', receiverId);
    
    const token = getSecurityToken();
    console.log('🔐 Retrieved security token:', token ? `Token found (length: ${token.length})` : 'No token found');
    
    if (!token) {
        console.error('❌ No security token available, aborting friend request');
        return;
    }
    
    const requestBody = `__RequestVerificationToken=${encodeURIComponent(token)}&receiverId=${receiverId}`;
    console.log('📤 Request body:', requestBody);
    
    const fetchOptions = {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
        },
        body: requestBody
    };
    console.log('⚙️ Fetch options:', fetchOptions);
    
    console.log('🌐 Making fetch request to /Account/Profile?handler=SendFriendRequest');
    
    fetch('/Account/Profile?handler=SendFriendRequest', fetchOptions)
    .then(response => {
        console.log('📥 Response received:', {
            status: response.status,
            statusText: response.statusText,
            ok: response.ok,
            headers: Array.from(response.headers.entries())
        });
        
        if (!response.ok) {
            console.error('❌ Response not OK:', response.status, response.statusText);
        }
        
        return response.json();
    })
    .then(data => {
        console.log('📊 Response data:', data);
        
        if (data.success) {
            console.log('✅ Friend request successful:', data.message);
            showNotification(data.message, 'success');
            // Refresh the page to update the UI
            setTimeout(() => {
                console.log('🔄 Reloading page...');
                location.reload();
            }, 1000);
        } else {
            console.error('❌ Friend request failed:', data.message);
            showNotification(data.message, 'error');
        }
    })
    .catch(error => {
        console.error('💥 Fetch error occurred:', error);
        console.error('Error details:', {
            name: error.name,
            message: error.message,
            stack: error.stack
        });
        showNotification('An error occurred while sending friend request', 'error');
    });
}

function respondToFriendRequest(requestId, accept) {
    const token = getSecurityToken();
    if (!token) return;
    
    fetch('/Account/Profile?handler=RespondToFriendRequest', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
        },
        body: `__RequestVerificationToken=${encodeURIComponent(token)}&requestId=${requestId}&accept=${accept}`
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showNotification(data.message, 'success');
            // Refresh the page to update the UI
            setTimeout(() => location.reload(), 1000);
        } else {
            showNotification(data.message, 'error');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showNotification('An error occurred while responding to friend request', 'error');
    });
}

function cancelFriendRequest(requestId) {
    const token = getSecurityToken();
    if (!token) return;
    
    fetch('/Account/Profile?handler=CancelFriendRequest', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
        },
        body: `__RequestVerificationToken=${encodeURIComponent(token)}&requestId=${requestId}`
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showNotification(data.message, 'success');
            // Refresh the page to update the UI
            setTimeout(() => location.reload(), 1000);
        } else {
            showNotification(data.message, 'error');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showNotification('An error occurred while canceling friend request', 'error');
    });
}

function removeFriend(friendId) {
    if (!confirm('Are you sure you want to remove this friend?')) {
        return;
    }
    
    const token = getSecurityToken();
    if (!token) return;
    
    fetch('/Account/Profile?handler=RemoveFriend', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
        },
        body: `__RequestVerificationToken=${encodeURIComponent(token)}&friendId=${friendId}`
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showNotification(data.message, 'success');
            // Refresh the page to update the UI
            setTimeout(() => location.reload(), 1000);
        } else {
            showNotification(data.message, 'error');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showNotification('An error occurred while removing friend', 'error');
    });
}

// Album creation functionality
let selectedExistingPhotos = new Set();

// Initialize album modal when shown
function initializeAlbumModal() {
    // Reset state
    selectedExistingPhotos.clear();
    updateSelectedPhotosDisplay();
    
    // Populate existing photos grid when the "Select from Posts" tab is first clicked
    const existingTab = document.getElementById('existing-tab');
    if (existingTab) {
        existingTab.addEventListener('click', function() {
            if (!this.dataset.loaded) {
                populateExistingPhotosGrid();
                this.dataset.loaded = 'true';
            }
        });
    }
}

function populateExistingPhotosGrid() {
    const grid = document.getElementById('existingPhotosGrid');
    if (!grid || !window.postsData) return;
    
    // Filter posts that have images
    const postsWithImages = window.postsData.filter(post => post.imageUrl);
    
    if (postsWithImages.length === 0) {
        grid.innerHTML = `
            <div class="col-12 text-center py-4">
                <i class="fas fa-images fa-3x text-muted mb-3"></i>
                <p class="text-muted">No photos found in your posts. Upload some photos first!</p>
            </div>
        `;
        return;
    }
    
    grid.innerHTML = '';
    
    postsWithImages.forEach(post => {
        const colDiv = document.createElement('div');
        colDiv.className = 'col-6 col-md-4 col-lg-3';
        
        colDiv.innerHTML = `
            <div class="existing-photo-item position-relative" 
                 onclick="toggleExistingPhotoSelection(${post.id})" 
                 style="cursor: pointer; border-radius: 8px; overflow: hidden; aspect-ratio: 1;">
                <img src="${post.imageUrl}" 
                     alt="Post photo" 
                     class="w-100 h-100" 
                     style="object-fit: cover; transition: all 0.3s ease;" />
                <div class="photo-overlay position-absolute top-0 start-0 w-100 h-100 d-flex align-items-center justify-content-center" 
                     style="background: rgba(0, 0, 0, 0.5); opacity: 0; transition: opacity 0.3s ease;">
                    <i class="fas fa-check-circle text-success fa-2x"></i>
                </div>
                <div class="selection-indicator position-absolute top-2 end-2">
                    <div class="form-check">
                        <input class="form-check-input" type="checkbox" id="photo_${post.id}" style="transform: scale(1.2);">
                    </div>
                </div>
            </div>
        `;
        
        grid.appendChild(colDiv);
    });
}

function toggleExistingPhotoSelection(postId) {
    const post = window.postsData.find(p => p.id === postId);
    if (!post) return;
    
    const photoItem = document.querySelector(`#existingPhotosGrid .existing-photo-item img[src="${post.imageUrl}"]`).closest('.existing-photo-item');
    const checkbox = document.getElementById(`photo_${postId}`);
    const overlay = photoItem.querySelector('.photo-overlay');
    
    if (selectedExistingPhotos.has(postId)) {
        // Deselect
        selectedExistingPhotos.delete(postId);
        checkbox.checked = false;
        photoItem.style.border = 'none';
        overlay.style.opacity = '0';
    } else {
        // Select
        selectedExistingPhotos.add(postId);
        checkbox.checked = true;
        photoItem.style.border = '3px solid #0d6efd';
        overlay.style.opacity = '1';
    }
    
    updateSelectedPhotosDisplay();
    updateSelectedExistingPhotosInput();
}

function updateSelectedPhotosDisplay() {
    const countBadge = document.getElementById('selectedPhotoCount');
    const previewsList = document.getElementById('selectedPhotosList');
    
    if (!countBadge || !previewsList) return;
    
    countBadge.textContent = selectedExistingPhotos.size;
    previewsList.innerHTML = '';
    
    if (selectedExistingPhotos.size === 0) return;
    
    selectedExistingPhotos.forEach(postId => {
        const post = window.postsData.find(p => p.id === postId);
        if (!post) return;
        
        const previewDiv = document.createElement('div');
        previewDiv.className = 'position-relative';
        previewDiv.innerHTML = `
            <img src="${post.imageUrl}" 
                 alt="Selected photo" 
                 class="rounded" 
                 style="width: 40px; height: 40px; object-fit: cover; border: 2px solid #0d6efd;">
            <button type="button" 
                    class="btn btn-sm btn-danger position-absolute top-0 start-100 translate-middle rounded-circle" 
                    style="width: 20px; height: 20px; padding: 0; font-size: 10px; line-height: 1;"
                    onclick="toggleExistingPhotoSelection(${postId})" 
                    title="Remove photo">
                <i class="fas fa-times"></i>
            </button>
        `;
        
        previewsList.appendChild(previewDiv);
    });
}

function updateSelectedExistingPhotosInput() {
    const hiddenInput = document.getElementById('selectedExistingPhotos');
    if (hiddenInput) {
        hiddenInput.value = Array.from(selectedExistingPhotos).join(',');
    }
}

function clearExistingPhotoSelection() {
    selectedExistingPhotos.clear();
    
    // Update UI
    document.querySelectorAll('#existingPhotosGrid .existing-photo-item').forEach(item => {
        item.style.border = 'none';
        const overlay = item.querySelector('.photo-overlay');
        if (overlay) overlay.style.opacity = '0';
    });
    
    document.querySelectorAll('#existingPhotosGrid input[type="checkbox"]').forEach(checkbox => {
        checkbox.checked = false;
    });
    
    updateSelectedPhotosDisplay();
    updateSelectedExistingPhotosInput();
}

function resetAlbumModal() {
    // Reset form
    const form = document.getElementById('createAlbumForm');
    if (form) {
        form.reset();
    }
    
    // Reset selected photos
    selectedExistingPhotos.clear();
    updateSelectedPhotosDisplay();
    updateSelectedExistingPhotosInput();
    
    // Reset existing photos grid
    const existingTab = document.getElementById('existing-tab');
    if (existingTab) {
        existingTab.removeAttribute('data-loaded');
    }
    
    // Reset album image preview
    const albumPreview = document.getElementById('albumImagePreview');
    if (albumPreview) {
        albumPreview.innerHTML = '';
    }
    
    // Reset to first tab
    const uploadTab = document.getElementById('upload-tab');
    const existingTabEl = document.getElementById('existing-tab');
    const uploadPane = document.getElementById('upload-pane');
    const existingPane = document.getElementById('existing-pane');
    
    if (uploadTab && existingTabEl && uploadPane && existingPane) {
        uploadTab.classList.add('active');
        existingTabEl.classList.remove('active');
        uploadPane.classList.add('show', 'active');
        existingPane.classList.remove('show', 'active');
    }
}

// Edit Album functionality
function editAlbum(albumId) {
    console.log('editAlbum called with albumId:', albumId);
    console.log('window.albumsData:', window.albumsData);
    
    // Validate modal elements exist
    const editModalElement = document.getElementById('editAlbumModal');
    const editAlbumIdInput = document.getElementById('editAlbumId');
    const editAlbumNameInput = document.getElementById('editAlbumName');
    const editAlbumDescriptionInput = document.getElementById('editAlbumDescription');
    const currentPhotosGrid = document.getElementById('currentAlbumPhotos');
    
    console.log('Modal elements validation:', {
        editModalElement: !!editModalElement,
        editAlbumIdInput: !!editAlbumIdInput,
        editAlbumNameInput: !!editAlbumNameInput,
        editAlbumDescriptionInput: !!editAlbumDescriptionInput,
        currentPhotosGrid: !!currentPhotosGrid
    });
    
    if (!editModalElement) {
        console.error('Edit modal element not found!');
        showNotification('Edit modal not found', 'error');
        return;
    }
    
    // Find album data from window.albumsData (or fetch from server if needed)
    const album = (window.albumsData || []).find(a => a.id === albumId);
    console.log('Found album:', album);
    
    if (!album) {
        console.error('Album not found for ID:', albumId);
        showNotification('Album not found', 'error');
        return;
    }
    
    // Populate edit modal with current album data
    if (editAlbumIdInput) editAlbumIdInput.value = album.id;
    if (editAlbumNameInput) editAlbumNameInput.value = album.name || '';
    if (editAlbumDescriptionInput) editAlbumDescriptionInput.value = album.description || '';
    
    // Initialize edit album state
    initializeEditAlbumModal(album);
    
    // Show the edit modal
    console.log('Showing edit modal...');
    const editModal = new bootstrap.Modal(editModalElement);
    editModal.show();
    console.log('Modal should be shown now');
}

function initializeEditAlbumModal(album) {
    console.log('initializeEditAlbumModal called with album:', album);
    
    // Reset any previous state
    const currentPhotosGrid = document.getElementById('currentAlbumPhotos');
    const photosToRemoveInput = document.getElementById('photosToRemove');
    
    console.log('Edit modal elements found:', {
        currentPhotosGrid,
        photosToRemoveInput
    });
    
    if (currentPhotosGrid) {
        currentPhotosGrid.innerHTML = '';
        
        // Populate current photos grid
        if (album.photos && album.photos.length > 0) {
            console.log('Populating current photos, count:', album.photos.length);
            album.photos.forEach(photo => {
                const photoDiv = document.createElement('div');
                photoDiv.className = 'col-6 col-md-4 col-lg-3 mb-2';
                photoDiv.innerHTML = `
                    <div class="position-relative current-photo-item" data-photo-id="${photo.id}">
                        <img src="${photo.url}" alt="${photo.name || 'Album photo'}" 
                             class="w-100 rounded" style="aspect-ratio: 1; object-fit: cover;">
                        <div class="position-absolute top-0 end-0 p-1">
                            <button type="button" class="btn btn-sm btn-danger rounded-circle photo-remove-btn" 
                                    onclick="togglePhotoRemoval(${photo.id}, event)" 
                                    title="Remove this photo">
                                <i class="fas fa-times"></i>
                            </button>
                        </div>
                        <div class="position-absolute top-0 start-0 w-100 h-100 bg-danger bg-opacity-50 d-none remove-overlay rounded">
                            <div class="d-flex align-items-center justify-content-center h-100">
                                <i class="fas fa-trash text-white fa-2x"></i>
                            </div>
                        </div>
                    </div>
                `;
                currentPhotosGrid.appendChild(photoDiv);
            });
        } else {
            console.log('No photos in album, showing empty message');
            currentPhotosGrid.innerHTML = `
                <div class="col-12 text-center py-4">
                    <i class="fas fa-images fa-2x text-muted mb-2"></i>
                    <p class="text-muted">No photos in this album</p>
                </div>
            `;
        }
    }
    
    // Reset photos to remove input
    if (photosToRemoveInput) {
        photosToRemoveInput.value = '';
    }
    
    // Reset edit modal existing photos selection
    selectedEditExistingPhotos.clear();
    updateEditSelectedPhotosDisplay();
    
    // Setup existing photos tab for edit modal
    const editExistingTab = document.getElementById('edit-existing-tab');
    if (editExistingTab) {
        editExistingTab.addEventListener('click', function() {
            if (!this.dataset.loaded) {
                populateEditExistingPhotosGrid();
                this.dataset.loaded = 'true';
            }
        });
    }
}

// Photo removal functionality for edit modal
let photosToRemove = new Set();

// Debounce mechanism to prevent double-clicks
let togglePhotoRemovalDebounce = new Map();

function togglePhotoRemoval(photoId, event) {
    console.log('togglePhotoRemoval called with photoId:', photoId);
    
    // Prevent event bubbling to avoid multiple triggers
    if (event) {
        event.preventDefault();
        event.stopPropagation();
    }
    
    // Ensure photoId is a number for consistent comparison
    const numericPhotoId = parseInt(photoId);
    if (isNaN(numericPhotoId)) {
        console.error('Invalid photoId provided:', photoId);
        return;
    }
    
    // Debounce mechanism - prevent multiple rapid calls
    const debounceKey = `photo-${numericPhotoId}`;
    const now = Date.now();
    const lastCall = togglePhotoRemovalDebounce.get(debounceKey) || 0;
    
    if (now - lastCall < 500) { // 500ms debounce
        console.log('Debounced duplicate call for photo:', numericPhotoId);
        return;
    }
    
    togglePhotoRemovalDebounce.set(debounceKey, now);
    
    const photoItem = document.querySelector(`[data-photo-id="${numericPhotoId}"]`);
    console.log('Found photoItem:', photoItem);
    
    if (!photoItem) {
        console.error('Photo item not found for photoId:', numericPhotoId);
        return;
    }
    
    const removeOverlay = photoItem.querySelector('.remove-overlay');
    const removeBtn = photoItem.querySelector('.photo-remove-btn');
    
    console.log('Found removeOverlay:', removeOverlay);
    console.log('Found removeBtn:', removeBtn);
    
    if (!removeOverlay || !removeBtn) {
        console.error('Required elements not found in photo item');
        return;
    }
    
    // Add visual feedback animation
    photoItem.classList.add('removing');
    setTimeout(() => photoItem.classList.remove('removing'), 300);
    
    if (photosToRemove.has(numericPhotoId)) {
        // Unmark for removal
        console.log('Unmarking photo for removal');
        photosToRemove.delete(numericPhotoId);
        removeOverlay.classList.add('d-none');
        removeBtn.innerHTML = '<i class="fas fa-times"></i>';
        removeBtn.classList.remove('btn-success');
        removeBtn.classList.add('btn-danger');
        removeBtn.title = 'Remove this photo';
        photoItem.style.opacity = '1';
    } else {
        // Mark for removal
        console.log('Marking photo for removal');
        photosToRemove.add(numericPhotoId);
        removeOverlay.classList.remove('d-none');
        removeBtn.innerHTML = '<i class="fas fa-undo"></i>';
        removeBtn.classList.remove('btn-danger');
        removeBtn.classList.add('btn-success');
        removeBtn.title = 'Keep this photo';
        photoItem.style.opacity = '0.6';
    }
    
    // Update hidden input
    const hiddenInput = document.getElementById('photosToRemove');
    const photosToRemoveArray = Array.from(photosToRemove);
    console.log('Photos to remove array:', photosToRemoveArray);
    
    if (hiddenInput) {
        hiddenInput.value = photosToRemoveArray.join(',');
        console.log('Updated hidden input value:', hiddenInput.value);
    } else {
        console.error('photosToRemove hidden input not found');
    }
    
    // Provide user feedback
    const removeCount = photosToRemove.size;
    if (removeCount > 0) {
        console.log(`${removeCount} photo(s) marked for removal`);
    }
}

// Edit modal existing photos selection
let selectedEditExistingPhotos = new Set();

function populateEditExistingPhotosGrid() {
    console.log('populateEditExistingPhotosGrid called');
    
    const grid = document.getElementById('editExistingPhotosGrid');
    console.log('Edit existing photos grid found:', !!grid);
    console.log('window.postsData:', window.postsData);
    
    if (!grid || !window.postsData) {
        console.error('Grid or postsData not found');
        return;
    }
    
    // Filter posts that have images
    const postsWithImages = window.postsData.filter(post => post.imageUrl);
    console.log('Posts with images:', postsWithImages.length);
    
    if (postsWithImages.length === 0) {
        console.log('No posts with images found, showing empty message');
        grid.innerHTML = `
            <div class="col-12 text-center py-4">
                <i class="fas fa-images fa-3x text-muted mb-3"></i>
                <p class="text-muted">No photos found in your posts.</p>
            </div>
        `;
        return;
    }
    
    grid.innerHTML = '';
    console.log('Populating grid with', postsWithImages.length, 'posts');
    
    postsWithImages.forEach(post => {
        console.log('Processing post:', post.id, post.imageUrl);
        const colDiv = document.createElement('div');
        colDiv.className = 'col-6 col-md-4 col-lg-3';
        
        colDiv.innerHTML = `
            <div class="existing-photo-item position-relative" 
                 onclick="toggleEditExistingPhotoSelection(${post.id})" 
                 style="cursor: pointer; border-radius: 8px; overflow: hidden; aspect-ratio: 1;">
                <img src="${post.imageUrl}" 
                     alt="Post photo" 
                     class="w-100 h-100" 
                     style="object-fit: cover; transition: all 0.3s ease;" />
                <div class="photo-overlay position-absolute top-0 start-0 w-100 h-100 d-flex align-items-center justify-content-center" 
                     style="background: rgba(0, 0, 0, 0.5); opacity: 0; transition: opacity 0.3s ease;">
                    <i class="fas fa-check-circle text-success fa-2x"></i>
                </div>
                <div class="selection-indicator position-absolute top-2 end-2">
                    <div class="form-check">
                        <input class="form-check-input" type="checkbox" id="editPhoto_${post.id}" style="transform: scale(1.2);">
                    </div>
                </div>
            </div>
        `;
        
        grid.appendChild(colDiv);
    });
    
    console.log('Grid population completed');
}

function toggleEditExistingPhotoSelection(postId) {
    const post = window.postsData.find(p => p.id === postId);
    if (!post) return;
    
    const photoItem = document.querySelector(`#editExistingPhotosGrid .existing-photo-item img[src="${post.imageUrl}"]`).closest('.existing-photo-item');
    const checkbox = document.getElementById(`editPhoto_${postId}`);
    const overlay = photoItem.querySelector('.photo-overlay');
    
    if (selectedEditExistingPhotos.has(postId)) {
        // Deselect
        selectedEditExistingPhotos.delete(postId);
        checkbox.checked = false;
        photoItem.style.border = 'none';
        overlay.style.opacity = '0';
    } else {
        // Select
        selectedEditExistingPhotos.add(postId);
        checkbox.checked = true;
        photoItem.style.border = '3px solid #0d6efd';
        overlay.style.opacity = '1';
    }
    
    updateEditSelectedPhotosDisplay();
    updateEditSelectedExistingPhotosInput();
}

function updateEditSelectedPhotosDisplay() {
    const countBadge = document.getElementById('editSelectedPhotoCount');
    const previewsList = document.getElementById('editSelectedPhotosList');
    
    if (!countBadge || !previewsList) return;
    
    countBadge.textContent = selectedEditExistingPhotos.size;
    previewsList.innerHTML = '';
    
    if (selectedEditExistingPhotos.size === 0) return;
    
    selectedEditExistingPhotos.forEach(postId => {
        const post = window.postsData.find(p => p.id === postId);
        if (!post) return;
        
        const previewDiv = document.createElement('div');
        previewDiv.className = 'position-relative';
        previewDiv.innerHTML = `
            <img src="${post.imageUrl}" 
                 alt="Selected photo" 
                 class="rounded" 
                 style="width: 40px; height: 40px; object-fit: cover; border: 2px solid #0d6efd;">
            <button type="button" 
                    class="btn btn-sm btn-danger position-absolute top-0 start-100 translate-middle rounded-circle" 
                    style="width: 20px; height: 20px; padding: 0; font-size: 10px; line-height: 1;"
                    onclick="toggleEditExistingPhotoSelection(${postId})" 
                    title="Remove photo">
                <i class="fas fa-times"></i>
            </button>
        `;
        
        previewsList.appendChild(previewDiv);
    });
}

function updateEditSelectedExistingPhotosInput() {
    const hiddenInput = document.getElementById('editSelectedExistingPhotos');
    if (hiddenInput) {
        hiddenInput.value = Array.from(selectedEditExistingPhotos).join(',');
    }
}

function clearEditExistingPhotoSelection() {
    selectedEditExistingPhotos.clear();
    
    // Update UI
    document.querySelectorAll('#editExistingPhotosGrid .existing-photo-item').forEach(item => {
        item.style.border = 'none';
        const overlay = item.querySelector('.photo-overlay');
        if (overlay) overlay.style.opacity = '0';
    });
    
    document.querySelectorAll('#editExistingPhotosGrid input[type="checkbox"]').forEach(checkbox => {
        checkbox.checked = false;
    });
    
    updateEditSelectedPhotosDisplay();
    updateEditSelectedExistingPhotosInput();
}

function resetEditAlbumModal() {
    // Reset form
    const form = document.getElementById('editAlbumForm');
    if (form) {
        form.reset();
    }
    
    // Reset selected photos for edit
    selectedEditExistingPhotos.clear();
    photosToRemove.clear();
    updateEditSelectedPhotosDisplay();
    updateEditSelectedExistingPhotosInput();
    
    // Clear debounce tracking
    togglePhotoRemovalDebounce.clear();
    
    // Reset existing photos grid
    const editExistingTab = document.getElementById('edit-existing-tab');
    if (editExistingTab) {
        editExistingTab.removeAttribute('data-loaded');
    }
    
    // Reset edit album image preview
    const editAlbumPreview = document.getElementById('editAlbumImagePreview');
    if (editAlbumPreview) {
        editAlbumPreview.innerHTML = '';
    }
    
    // Clear photos to remove input
    const photosToRemoveInput = document.getElementById('photosToRemove');
    if (photosToRemoveInput) {
        photosToRemoveInput.value = '';
    }
    
    // Reset current album photos visual state
    document.querySelectorAll('.current-photo-item').forEach(item => {
        item.style.opacity = '1';
        const overlay = item.querySelector('.remove-overlay');
        const btn = item.querySelector('.photo-remove-btn');
        if (overlay) overlay.classList.add('d-none');
        if (btn) {
            btn.innerHTML = '<i class="fas fa-times"></i>';
            btn.classList.remove('btn-success');
            btn.classList.add('btn-danger');
            btn.title = 'Remove this photo';
        }
    });
    
    // Reset to first tab with proper Bootstrap 5 syntax
    const uploadTab = document.getElementById('edit-upload-tab');
    const existingTabEl = document.getElementById('edit-existing-tab');
    const uploadPane = document.getElementById('edit-upload-pane');
    const existingPane = document.getElementById('edit-existing-pane');
    
    if (uploadTab && existingTabEl && uploadPane && existingPane) {
        uploadTab.classList.add('active');
        existingTabEl.classList.remove('active');
        uploadPane.classList.add('show', 'active');
        existingPane.classList.remove('show', 'active');
    }
}

// Delete Album functionality
function deleteAlbum(albumId) {
    // Find album data to get the album name for confirmation
    const album = (window.albumsData || []).find(a => a.id === albumId);
    const albumName = album ? album.name : 'this album';
    
    // Populate delete modal with album info
    document.getElementById('deleteAlbumId').value = albumId;
    document.getElementById('deleteAlbumName').textContent = albumName;
    
    // Show the delete confirmation modal
    const deleteModal = new bootstrap.Modal(document.getElementById('deleteAlbumModal'));
    deleteModal.show();
}

// Album image preview functions
function previewAlbumImages(input) {
    const preview = document.getElementById('albumImagePreview');
    if (!preview) return;
    
    preview.innerHTML = '';
    
    if (input.files && input.files.length > 0) {
        Array.from(input.files).forEach((file, index) => {
            if (isValidImageFile(file)) {
                const reader = new FileReader();
                reader.onload = function(e) {
                    const previewDiv = document.createElement('div');
                    previewDiv.className = 'position-relative';
                    previewDiv.innerHTML = `
                        <img src="${e.target.result}" alt="Preview" class="rounded" style="width: 80px; height: 80px; object-fit: cover;">
                        <button type="button" class="btn btn-sm btn-danger position-absolute top-0 end-0 rounded-circle" 
                                style="transform: translate(50%, -50%); width: 20px; height: 20px; padding: 0;" 
                                onclick="removeAlbumPreviewImage(this, ${index})" title="Remove">
                            <i class="fas fa-times" style="font-size: 10px;"></i>
                        </button>
                    `;
                    preview.appendChild(previewDiv);
                };
                reader.readAsDataURL(file);
            }
        });
    }
}

function previewEditAlbumImages(input) {
    console.log('previewEditAlbumImages called with input:', input);
    console.log('Files selected:', input.files ? input.files.length : 'No files');
    
    const preview = document.getElementById('editAlbumImagePreview');
    console.log('Preview element found:', !!preview);
    
    if (!preview) {
        console.error('editAlbumImagePreview element not found!');
        return;
    }
    
    preview.innerHTML = '';
    
    if (input.files && input.files.length > 0) {
        console.log('Processing', input.files.length, 'files');
        Array.from(input.files).forEach((file, index) => {
            console.log(`Processing file ${index}: ${file.name}, size: ${file.size}, type: ${file.type}`);
            
            if (isValidImageFile(file)) {
                console.log(`File ${index} is valid, creating preview`);
                const reader = new FileReader();
                reader.onload = function(e) {
                    console.log(`FileReader loaded for file ${index}`);
                    const previewDiv = document.createElement('div');
                    previewDiv.className = 'position-relative';
                    previewDiv.innerHTML = `
                        <img src="${e.target.result}" alt="Preview" class="rounded" style="width: 80px; height: 80px; object-fit: cover;">
                        <button type="button" class="btn btn-sm btn-danger position-absolute top-0 end-0 rounded-circle" 
                                style="transform: translate(50%, -50%); width: 20px; height: 20px; padding: 0;" 
                                onclick="removeEditAlbumPreviewImage(this, ${index})" title="Remove">
                            <i class="fas fa-times" style="font-size: 10px;"></i>
                        </button>
                    `;
                    preview.appendChild(previewDiv);
                };
                reader.readAsDataURL(file);
            } else {
                console.log(`File ${index} is not valid`);
            }
        });
    } else {
        console.log('No files to process');
    }
}

function removeAlbumPreviewImage(button, index) {
    // Remove the preview
    button.closest('.position-relative').remove();
    
    // Note: Removing individual files from FileList is complex
    // For now, we'll let the user re-select if needed
    // In a production app, you might want to use a more sophisticated file management system
}

function removeEditAlbumPreviewImage(button, index) {
    // Remove the preview
    button.closest('.position-relative').remove();
    
    // Note: Removing individual files from FileList is complex
    // For now, we'll let the user re-select if needed
}