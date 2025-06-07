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
    @keyframes slideIn {
        from { transform: translateX(100%); opacity: 0; }
        to { transform: translateX(0); opacity: 1; }
    }
    @keyframes slideOut {
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
    .post-grid-item:hover .hover-overlay {
        opacity: 1 !important;
    }
`;
document.head.appendChild(style);

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
                likeIcon.style.color = '#e91e63';
            } else {
                likeIcon.classList.remove('fas');
                likeIcon.classList.add('far');
                likeIcon.style.color = '';
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

function loadComments() {
    const commentsSection = document.getElementById('commentsSection');
    if (!commentsSection || !currentPostData) return;
    
    commentsSection.innerHTML = '';
    
    if (currentPostData.comments.length === 0) {
        commentsSection.innerHTML = '<p class="text-muted small">No comments yet.</p>';
        return;
    }
    
    currentPostData.comments.forEach(comment => {
        const commentHtml = `
            <div class="mb-2">
                <div class="d-flex">
                    ${comment.user.profilePictureUrl ? 
                        `<img src="${comment.user.profilePictureUrl}" alt="Avatar" class="rounded-circle me-2" style="width: 30px; height: 30px; object-fit: cover;" />` :
                        '<div class="rounded-circle bg-secondary me-2 d-flex align-items-center justify-content-center" style="width: 30px; height: 30px;"><i class="fas fa-user text-white small"></i></div>'
                    }
                    <div class="flex-grow-1">
                        <div class="bg-light rounded p-2">
                            <strong class="small">${comment.user.firstName} ${comment.user.lastName}</strong>
                            <p class="mb-0 small">${comment.content}</p>
                        </div>
                        <small class="text-muted">${comment.createdAt}</small>
                    </div>
                </div>
            </div>
        `;
        commentsSection.innerHTML += commentHtml;
    });
}

function toggleLike() {
    if (!currentPostId) return;
    
    const token = document.querySelector('input[name="__RequestVerificationToken"]');
    if (!token) {
        alert('Security token not found');
        return;
    }
    
    fetch('/Account/Profile?handler=LikePost', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token.value
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
                        icon.style.color = '#e91e63';
                    } else {
                        icon.classList.remove('fas');
                        icon.classList.add('far');
                        icon.style.color = '';
                    }
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
        } else {
            alert('Failed to like/unlike post: ' + data.message);
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('An error occurred while toggling like');
    });
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
                    ${like.user?.profilePictureUrl ? 
                        `<img src="${like.user.profilePictureUrl}" alt="Profile" class="rounded-circle" style="width: 40px; height: 40px; object-fit: cover;" />` :
                        '<div class="rounded-circle bg-secondary d-flex align-items-center justify-content-center" style="width: 40px; height: 40px;"><i class="fas fa-user text-white"></i></div>'
                    }
                </div>
                <div class="flex-grow-1">
                    <div class="fw-bold" style="color: var(--text-primary);">${like.user?.firstName || 'Unknown'} ${like.user?.lastName || 'User'}</div>
                    <small class="text-muted">${like.createdAt}</small>
                </div>
            </div>
        `).join('');
    }
    
    const likesModal = document.getElementById('likesModal');
    if (likesModal && window.bootstrap) {
        new bootstrap.Modal(likesModal).show();
    }
}

function editPost() {
    alert('Edit functionality not implemented yet');
}

function deletePost() {
    if (!confirm('Are you sure you want to delete this post?')) return;
    
    const token = document.querySelector('input[name="__RequestVerificationToken"]');
    if (!token) {
        alert('Security token not found');
        return;
    }
    
    fetch('/Account/Profile?handler=DeletePost', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token.value
        },
        body: `postId=${currentPostId}`
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Close modal and refresh page
            const postModal = document.getElementById('postModal');
            if (postModal && window.bootstrap) {
                bootstrap.Modal.getInstance(postModal).hide();
            }
            location.reload();
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
    
    const token = document.querySelector('input[name="__RequestVerificationToken"]');
    if (!token) {
        alert('Security token not found');
        return;
    }
    
    fetch('/Account/Profile?handler=RemoveProfilePicture', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token.value
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
    // Comment form submission
    const commentForm = document.getElementById('commentForm');
    if (commentForm) {
        commentForm.addEventListener('submit', function(e) {
            e.preventDefault();
            
            const commentInput = document.getElementById('commentInput');
            const content = commentInput.value.trim();
            
            if (!content) return;
            
            const token = document.querySelector('input[name="__RequestVerificationToken"]');
            if (!token) {
                alert('Security token not found');
                return;
            }
            
            fetch('/Account/Profile?handler=AddComment', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': token.value
                },
                body: `CommentInput.PostId=${currentPostId}&CommentInput.Content=${encodeURIComponent(content)}`
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Add comment to current post data
                    if (currentPostData) {
                        currentPostData.comments.push(data.comment);
                    }
                    
                    // Reload comments
                    loadComments();
                    
                    // Clear input
                    commentInput.value = '';
                } else {
                    alert('Failed to add comment: ' + data.message);
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('An error occurred while adding the comment');
            });
        });
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
});