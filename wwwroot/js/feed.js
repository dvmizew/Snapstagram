// Feed page JavaScript functionality
let currentPostId = null;
let currentPostData = null;
let postsData = window.postsData || [];
let currentPage = 1;
const pageSize = 10;

document.addEventListener('DOMContentLoaded', function() {
    console.log('Feed page loaded with', postsData.length, 'posts');
    initializeFeed();
});

function initializeFeed() {
    // Add any feed-specific initialization here
    addPostAnimations();
    setupInfiniteScroll();
}

function addPostAnimations() {
    // Add smooth animations to post cards
    const postCards = document.querySelectorAll('.post-card');
    postCards.forEach((card, index) => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(20px)';
        setTimeout(() => {
            card.style.transition = 'all 0.5s ease';
            card.style.opacity = '1';
            card.style.transform = 'translateY(0)';
        }, index * 100);
    });
}

function setupInfiniteScroll() {
    // Setup infinite scroll for feed
    window.addEventListener('scroll', function() {
        if ((window.innerHeight + window.scrollY) >= document.body.offsetHeight - 1000) {
            loadMorePosts();
        }
    });
}

function toggleLikeFromFeed(postId) {
    console.log('Toggling like for post:', postId);
    
    const token = getSecurityToken();
    if (!token) {
        console.error('No security token found');
        return;
    }
    
    fetch('/Feed?handler=LikePost', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
        },
        body: `__RequestVerificationToken=${encodeURIComponent(token)}&postId=${postId}`
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Update the UI
            const postCard = document.querySelector(`[data-post-id="${postId}"]`);
            const likeBtn = postCard?.querySelector('.fa-heart').closest('button');
            const likeIcon = likeBtn?.querySelector('i');
            const likeCount = likeBtn?.querySelector('span');
            
            if (likeIcon && likeCount) {
                if (data.liked) {
                    likeIcon.classList.remove('far');
                    likeIcon.classList.add('fas');
                    likeBtn.classList.add('animate-like');
                } else {
                    likeIcon.classList.remove('fas');
                    likeIcon.classList.add('far');
                }
                likeCount.textContent = data.likeCount;
                
                // Remove animation class
                setTimeout(() => {
                    likeBtn?.classList.remove('animate-like');
                }, 300);
            }
            
            // Update the posts data
            const postIndex = postsData.findIndex(p => p.id === postId);
            if (postIndex !== -1) {
                postsData[postIndex].isLikedByCurrentUser = data.liked;
                postsData[postIndex].likeCount = data.likeCount;
            }
        } else {
            showNotification('Failed to like post: ' + data.message, 'error');
        }
    })
    .catch(error => {
        console.error('Error liking post:', error);
        showNotification('An error occurred while liking the post', 'error');
    });
}

function openPostModal(postId) {
    currentPostId = postId;
    currentPostData = postsData.find(p => p.id === postId);
    
    if (!currentPostData) {
        console.error('Post data not found for ID:', postId);
        return;
    }
    
    console.log('Opening post modal for:', currentPostData);
    
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

function loadComments() {
    const commentsSection = document.getElementById('commentsSection');
    if (!commentsSection || !currentPostData) return;
    
    commentsSection.innerHTML = '';
    
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
    
    currentPostData.comments.forEach(comment => {
        const commentElement = createCommentElement(comment);
        commentsSection.appendChild(commentElement);
    });
}

function createCommentElement(comment) {
    const commentDiv = document.createElement('div');
    commentDiv.className = 'comment-item mb-3 p-2 rounded';
    commentDiv.setAttribute('data-comment-id', comment.id);
    
    const timeAgo = getTimeAgo(comment.createdAt);
    
    commentDiv.innerHTML = `
        <div class="d-flex">
            <div class="me-2 flex-shrink-0">
                ${createAvatarElement(comment.user.profilePictureUrl, '32px')}
            </div>
            <div class="flex-grow-1">
                <div class="comment-content rounded px-3 py-2">
                    <strong class="comment-author small" style="color: white;">${comment.user.firstName} ${comment.user.lastName}</strong>
                    <p class="comment-text mb-1 small" style="color: white;">${escapeHtml(comment.content)}</p>
                </div>
                <div class="comment-actions d-flex align-items-center mt-1">
                    <small class="text-muted me-3">${timeAgo}</small>
                    <button class="btn btn-link btn-sm p-0 me-3 text-muted comment-like-btn" title="Like comment">
                        <i class="far fa-heart"></i>
                        ${comment.likeCount > 0 ? `<span class="ms-1">${comment.likeCount}</span>` : ''}
                    </button>
                </div>
            </div>
        </div>
    `;
    
    return commentDiv;
}

function toggleLike() {
    if (!currentPostId) return;
    
    const token = getSecurityToken();
    if (!token) return;
    
    fetch('/Feed?handler=LikePost', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
        },
        body: `__RequestVerificationToken=${encodeURIComponent(token)}&postId=${currentPostId}`
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Update modal UI
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
                    
                    setTimeout(() => {
                        likeBtn.classList.remove('animate-like');
                    }, 300);
                }
            }
            
            if (likeCount) {
                likeCount.textContent = `${data.likeCount} like${data.likeCount !== 1 ? 's' : ''}`;
            }
            
            // Update feed UI
            toggleLikeFromFeed(currentPostId);
        } else {
            showNotification('Failed to like/unlike post: ' + data.message, 'error');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showNotification('An error occurred while toggling like', 'error');
    });
}

// Utility functions
function getSecurityToken() {
    let token = document.querySelector('input[name="__RequestVerificationToken"]');
    
    if (!token) {
        token = document.querySelector('form input[name="__RequestVerificationToken"]');
    }
    
    if (!token) {
        console.error('Security token not found');
        return null;
    }
    
    return token.value;
}

function createAvatarElement(profilePictureUrl, size = '30px', additionalClasses = '') {
    if (profilePictureUrl) {
        return `<img src="${profilePictureUrl}" alt="Profile" class="rounded-circle ${additionalClasses}" style="width: ${size}; height: ${size}; object-fit: cover;" />`;
    } else {
        const iconSize = size === '40px' ? '' : 'small';
        return `<div class="rounded-circle bg-secondary d-flex align-items-center justify-content-center ${additionalClasses}" style="width: ${size}; height: ${size};"><i class="fas fa-user text-white ${iconSize}"></i></div>`;
    }
}

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

function showNotification(message, type = 'info') {
    const toast = document.createElement('div');
    const isError = type === 'error';
    const isSuccess = type === 'success';
    const className = isError ? 'alert-danger' : isSuccess ? 'alert-success' : 'alert-info';
    const icon = isError ? 'fa-exclamation-circle' : isSuccess ? 'fa-check-circle' : 'fa-info-circle';
    
    toast.className = `alert ${className} alert-dismissible fade show position-fixed`;
    toast.style.cssText = 'top: 20px; right: 20px; z-index: 1060; min-width: 320px; border-radius: 12px; box-shadow: 0 8px 25px rgba(0,0,0,0.15);';
    toast.innerHTML = `
        <div class="d-flex align-items-center">
            <i class="fas ${icon} me-2"></i>
            <span>${message}</span>
            <button type="button" class="btn-close ms-auto" data-bs-dismiss="alert"></button>
        </div>
    `;
    
    document.body.appendChild(toast);
    
    setTimeout(() => {
        if (toast.parentElement) {
            toast.remove();
        }
    }, 5000);
}

// Add CSS animations
const style = document.createElement('style');
style.textContent = `
    .animate-like {
        animation: likeAnimation 0.3s ease-out;
    }
    
    @keyframes likeAnimation {
        0% { transform: scale(1); }
        50% { transform: scale(1.2); }
        100% { transform: scale(1); }
    }
    
    .post-card {
        transition: transform 0.2s ease, box-shadow 0.2s ease;
    }
    
    .post-card:hover {
        transform: translateY(-2px);
        box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1) !important;
    }
`;
document.head.appendChild(style);
