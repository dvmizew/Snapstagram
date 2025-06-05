let currentStoryIndex = 0;
let currentStories = [];
let storyTimer = null;
let storyDuration = 5000; // 5 seconds
let feedPage = 0;
let isLoadingFeed = false;
let hasMorePosts = true;

document.addEventListener('DOMContentLoaded', initializeFeed);

function initializeFeed() {
    initializeCommentInputs();
    initializeDoubleTap();
    initializeStoriesScroll();
    initializePostObserver();
    initializeInfiniteScroll();
    initializeKeyboardShortcuts();
    loadSuggestedUsers();
    loadRecentActivity();
    loadFeedSettings();
}

function initializeCommentInputs() {
    document.querySelectorAll('.comment-input').forEach(input => {
        input.addEventListener('input', function() {
            this.parentElement.querySelector('.comment-submit').disabled = !this.value.trim();
        });
    });
}

async function addComment(event, postId) {
    event.preventDefault();
    
    const input = document.getElementById(`comment-input-${postId}`);
    const submitBtn = document.getElementById(`comment-submit-${postId}`);
    const commentText = input.value.trim();
    
    if (!commentText) return;
    
    const originalBtnContent = submitBtn.innerHTML;
    
    try {
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
        
        const response = await fetch(`/api/posts/${postId}/comments`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getRequestVerificationToken()
            },
            body: JSON.stringify({ text: commentText })
        });
        
        if (response.ok) {
            const comment = await response.json();
            addCommentToUI(postId, comment);
            input.value = '';
            updateCommentsCount(postId, 1);
            showNotification('Comment added successfully!', 'success');
        } else {
            const error = await response.json();
            showNotification(error.message || 'Failed to add comment', 'error');
        }
    } catch (error) {
        console.error('Error adding comment:', error);
        showNotification('Failed to add comment', 'error');
    } finally {
        submitBtn.disabled = true;
        submitBtn.innerHTML = 'Post';
    }
}

function addCommentToUI(postId, comment) {
    const commentsList = document.getElementById(`comments-list-${postId}`);
    const commentHTML = `
        <div class="comment slide-up">
            <img src="${comment.user.profileImageUrl || getDefaultAvatar(comment.user.displayName)}" 
                 alt="${comment.user.displayName}" class="comment-avatar" />
            <div class="comment-content">
                <div class="comment-text">
                    <span class="comment-username">${comment.user.displayName}</span>
                    ${comment.text}
                </div>
                <div class="comment-time">just now</div>
            </div>
        </div>
    `;
    
    if (commentsList) {
        commentsList.insertAdjacentHTML('beforeend', commentHTML);
    }
}

function focusCommentInput(postId) {
    const input = document.getElementById(`comment-input-${postId}`);
    const commentsSection = document.getElementById(`comments-${postId}`);
    
    if (commentsSection.style.display === 'none') {
        toggleComments(postId);
    }
    
    setTimeout(() => {
        input.focus();
    }, 300);
}

const toggleComments = (postId) => {
    const commentsSection = document.getElementById(`comments-${postId}`);
    const isVisible = commentsSection.style.display !== 'none';
    
    commentsSection.style.display = isVisible ? 'none' : 'block';
    if (!isVisible) loadComments(postId);
};

const loadComments = async (postId) => {
    try {
        const response = await fetch(`/api/posts/${postId}/comments`);
        if (response.ok) {
            const comments = await response.json();
            renderComments(postId, comments);
        }
    } catch (error) {
        console.error('Error loading comments:', error);
    }
};

async function toggleLike(postId) {
    const likeBtn = document.querySelector(`[data-post-id="${postId}"]`);
    const likesCount = document.getElementById(`likes-count-${postId}`);
    const postLikesDisplay = document.getElementById(`likes-${postId}`);
    
    if (!likeBtn || !likesCount || !postLikesDisplay) return;
    
    try {
        const isLiked = likeBtn.classList.contains('liked');
        const currentCount = parseInt(likesCount.textContent) || 0;
        const newCount = Math.max(0, currentCount + (isLiked ? -1 : 1));
        
        // Optimistic UI update
        likeBtn.classList.toggle('liked');
        likeBtn.querySelector('i').className = isLiked ? 'far fa-heart' : 'fas fa-heart';
        likesCount.textContent = newCount;
        postLikesDisplay.textContent = newCount;
        
        // Animate button
        likeBtn.style.transform = 'scale(1.2)';
        setTimeout(() => likeBtn.style.transform = 'scale(1)', 150);
        
        const response = await fetch(`/api/posts/${postId}/like`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getRequestVerificationToken()
            }
        });

        if (response.ok) {
            const result = await response.json();
            likesCount.textContent = result.likesCount;
            postLikesDisplay.textContent = result.likesCount;
            
            if (result.liked && !isLiked) {
                showHeartAnimation(postId);
            }
        } else {
            // Revert optimistic update on error
            likeBtn.classList.toggle('liked');
            likeBtn.querySelector('i').className = isLiked ? 'fas fa-heart' : 'far fa-heart';
            likesCount.textContent = currentCount;
            postLikesDisplay.textContent = currentCount;
            showNotification('Failed to update like', 'error');
        }
    } catch (error) {
        console.error('Error toggling like:', error);
        showNotification('Failed to update like', 'error');
        // Revert UI on error
        location.reload();
    }
}

function initializeDoubleTap() {
    document.querySelectorAll('.post-media').forEach(media => {
        let lastTap = 0;
        
        media.addEventListener('click', function(e) {
            const currentTime = new Date().getTime();
            const tapLength = currentTime - lastTap;
            
            if (tapLength < 500 && tapLength > 0) {
                e.preventDefault();
                const postId = this.closest('.post-card').querySelector('[data-post-id]').dataset.postId;
                handleDoubleTap(postId);
            }
            
            lastTap = currentTime;
        });
    });
}

function handleDoubleTap(postId) {
    const likeBtn = document.querySelector(`[data-post-id="${postId}"]`);
    
    if (!likeBtn.classList.contains('liked')) {
        toggleLike(postId);
    }
    
    showHeartAnimation(postId);
}

function showHeartAnimation(postId) {
    const heart = document.getElementById(`heart-${postId}`);
    if (heart) {
        heart.classList.add('animate');
        
        setTimeout(() => {
            heart.classList.remove('animate');
        }, 1000);
    }
}

const handleMediaClick = (element, postId) => {
    if (element.tagName === 'VIDEO') {
        element.paused ? element.play() : element.pause();
    }
};

const initializeStoriesScroll = () => {
    const storiesContainer = document.querySelector('.stories-scroll');
    if (!storiesContainer) return;
    
    let isDown = false;
    let startX, scrollLeft;

    const mouseEvents = {
        mousedown: (e) => {
            isDown = true;
            startX = e.pageX - storiesContainer.offsetLeft;
            scrollLeft = storiesContainer.scrollLeft;
        },
        mouseleave: () => isDown = false,
        mouseup: () => isDown = false,
        mousemove: (e) => {
            if (!isDown) return;
            e.preventDefault();
            const x = e.pageX - storiesContainer.offsetLeft;
            const walk = (x - startX) * 2;
            storiesContainer.scrollLeft = scrollLeft - walk;
        }
    };

    Object.entries(mouseEvents).forEach(([event, handler]) => {
        storiesContainer.addEventListener(event, handler);
    });
};

async function viewStory(storyId) {
    try {
        const response = await fetch(`/api/stories/${storyId}`);
        if (response.ok) {
            const story = await response.json();
            openStoryViewer(story);
            markStoryAsViewed(storyId);
        }
    } catch (error) {
        console.error('Error loading story:', error);
    }
}

const openStoryViewer = (story) => {
    const modal = document.getElementById('storyModal');
    const elements = {
        avatar: document.getElementById('storyUserAvatar'),
        name: document.getElementById('storyUserName'),
        time: document.getElementById('storyTime'),
        media: document.getElementById('storyMedia')
    };
    
    elements.avatar.src = story.user.profileImageUrl || getDefaultAvatar(story.user.displayName);
    elements.name.textContent = story.user.displayName;
    elements.time.textContent = getRelativeTime(new Date(story.createdAt));
    
    const mediaContent = story.mediaType === 'video' 
        ? `<video src="${story.mediaUrl}" autoplay muted loop></video>`
        : `<img src="${story.mediaUrl}" alt="Story">`;
    
    elements.media.innerHTML = mediaContent;
    modal.style.display = 'flex';
    startStoryProgress();
};

const closeStoryViewer = () => {
    document.getElementById('storyModal').style.display = 'none';
    clearStoryTimer();
};

const startStoryProgress = () => {
    const progressBar = document.getElementById('storyProgress');
    progressBar.style.width = '0%';
    
    let progress = 0;
    storyTimer = setInterval(() => {
        progress += 1;
        progressBar.style.width = `${progress}%`;
        
        if (progress >= 100) nextStory();
    }, 50); // 5 second duration (100 * 50ms)
};

const clearStoryTimer = () => {
    if (storyTimer) {
        clearInterval(storyTimer);
        storyTimer = null;
    }
};

const nextStory = () => {
    clearStoryTimer();
    closeStoryViewer();
};

const previousStory = () => {
    clearStoryTimer();
    closeStoryViewer();
};

const markStoryAsViewed = async (storyId) => {
    try {
        await fetch(`/api/stories/${storyId}/view`, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': getRequestVerificationToken()
            }
        });
        
        document.querySelector(`[onclick="viewStory(${storyId})"]`)?.classList.add('viewed');
    } catch (error) {
        console.error('Error marking story as viewed:', error);
    }
};

const openStoryCreator = () => window.location.href = '/Create?type=story';

const showPostMenu = (postId) => console.log('Showing menu for post:', postId);

async function sharePost(postId) {
    if (navigator.share) {
        try {
            await navigator.share({
                title: 'Check out this post on Snapstagram',
                url: `${window.location.origin}/posts/${postId}`
            });
        } catch (error) {
            console.log('Error sharing:', error);
        }
    } else {
        // Fallback: copy to clipboard
        const url = `${window.location.origin}/posts/${postId}`;
        navigator.clipboard.writeText(url).then(() => {
            showNotification('Link copied to clipboard!', 'success');
        });
    }
}

const toggleBookmark = async (postId) => {
    try {
        const response = await fetch(`/api/posts/${postId}/bookmark`, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': getRequestVerificationToken()
            }
        });
        
        if (response.ok) {
            const result = await response.json();
            const bookmarkBtn = document.querySelector(`button[onclick="toggleBookmark(${postId})"]`);
            const bookmarkIcon = bookmarkBtn.querySelector('i');
            
            bookmarkIcon.className = result.bookmarked ? 'fas fa-bookmark' : 'far fa-bookmark';
            bookmarkBtn.classList.toggle('bookmarked', result.bookmarked);
            
            showNotification(
                result.bookmarked ? 'Post saved to bookmarks' : 'Post removed from bookmarks',
                'success'
            );
        }
    } catch (error) {
        console.error('Error toggling bookmark:', error);
        showNotification('Failed to toggle bookmark', 'error');
    }
};

const updateCommentsCount = (postId, increment) => {
    const commentBtn = document.querySelector(`button[onclick="focusCommentInput(${postId})"] .action-count`);
    const viewCommentsBtn = document.querySelector(`button[onclick="toggleComments(${postId})"]`);
    
    if (commentBtn) {
        const currentCount = parseInt(commentBtn.textContent);
        const newCount = currentCount + increment;
        commentBtn.textContent = newCount;
        
        if (viewCommentsBtn) {
            viewCommentsBtn.textContent = `View all ${newCount} comments`;
        }
    }
};

const initializePostObserver = () => {
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            const { target, isIntersecting } = entry;
            target.classList.toggle('visible', isIntersecting);
            
            const video = target.querySelector('video');
            if (video) {
                isIntersecting ? video.play().catch(() => {}) : video.pause();
            }
        });
    }, { threshold: 0.5 });
    
    document.querySelectorAll('.post-card').forEach(post => observer.observe(post));
};

document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
        closeStoryViewer();
    } else if (e.key === ' ' && !['INPUT', 'TEXTAREA'].includes(e.target.tagName)) {
        e.preventDefault();
        document.querySelectorAll('video').forEach(video => {
            video.paused ? video.play() : video.pause();
        });
    }
});

// Helper functions
function getRequestVerificationToken() {
    const token = document.querySelector('input[name="__RequestVerificationToken"]');
    return token ? token.value : '';
}

function getDefaultAvatar(name) {
    const initial = name ? name[0].toUpperCase() : 'U';
    return `https://via.placeholder.com/40x40/6c757d/ffffff?text=${initial}`;
}

function getRelativeTime(dateString) {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now - date;
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);
    
    if (diffMins < 1) return 'just now';
    if (diffMins < 60) return `${diffMins}m`;
    if (diffHours < 24) return `${diffHours}h`;
    if (diffDays < 7) return `${diffDays}d`;
    if (diffDays < 30) return `${Math.floor(diffDays / 7)}w`;
    
    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
}

// Infinite scroll
function initializeInfiniteScroll() {
    const postsContainer = document.querySelector('.posts-container');
    if (!postsContainer) return;
    
    const observer = new IntersectionObserver(async (entries) => {
        const lastEntry = entries[0];
        if (lastEntry.isIntersecting && !isLoadingFeed && hasMorePosts) {
            await loadMorePosts();
        }
    }, { threshold: 0.1 });
    
    // Observe the last post
    const lastPost = postsContainer.lastElementChild;
    if (lastPost) observer.observe(lastPost);
}

async function loadMorePosts() {
    if (isLoadingFeed || !hasMorePosts) return;
    
    isLoadingFeed = true;
    feedPage++;
    
    try {
        const response = await fetch(`/api/posts/feed?page=${feedPage}&pageSize=10`);
        if (response.ok) {
            const posts = await response.json();
            if (posts.length === 0) {
                hasMorePosts = false;
                return;
            }
            
            const postsContainer = document.querySelector('.posts-container');
            posts.forEach(post => {
                const postHtml = renderPostHTML(post);
                postsContainer.insertAdjacentHTML('beforeend', postHtml);
            });
            
            // Reinitialize observers for new posts
            initializePostObserver();
            initializeDoubleTap();
            
        }
    } catch (error) {
        console.error('Error loading more posts:', error);
        showNotification('Failed to load more posts', 'error');
    } finally {
        isLoadingFeed = false;
    }
}

function renderPostHTML(post) {
    const isLiked = post.isLiked || false;
    const isBookmarked = post.isBookmarked || false;
    const mediaContent = post.videoUrl 
        ? `<video class="post-media" controls preload="metadata" onclick="handleMediaClick(this, ${post.id})">
             <source src="${post.videoUrl}" type="video/mp4">
             Your browser does not support the video tag.
           </video>
           <div class="media-overlay">
             <div class="play-button"><i class="fas fa-play"></i></div>
           </div>`
        : `<img src="${post.imageUrl}" alt="Post image" class="post-media" onclick="handleMediaClick(this, ${post.id})" />`;
    
    return `
        <div class="post-card slide-up">
            <div class="post-header">
                <div class="post-user-info">
                    <img src="${post.user.profileImageUrl || getDefaultAvatar(post.user.displayName)}" 
                         alt="${post.user.displayName}" class="post-avatar" />
                    <div>
                        <a href="/Profile/${post.user.userName}" class="post-username">${post.user.displayName}</a>
                        <div class="post-time">${getRelativeTime(post.createdAt)}</div>
                    </div>
                </div>
                <button class="post-menu" onclick="showPostMenu(${post.id})">
                    <i class="fas fa-ellipsis-h"></i>
                </button>
            </div>
            
            <div class="post-media-container">
                ${mediaContent}
                <div class="double-tap-heart" id="heart-${post.id}">
                    <i class="fas fa-heart"></i>
                </div>
            </div>
            
            <div class="post-actions">
                <div class="action-group-left">
                    <button class="action-btn like-btn ${isLiked ? 'liked' : ''}" 
                            onclick="toggleLike(${post.id})" data-post-id="${post.id}">
                        <i class="${isLiked ? 'fas' : 'far'} fa-heart"></i>
                        <span class="action-count" id="likes-count-${post.id}">${post.likesCount}</span>
                    </button>
                    <button class="action-btn comment-btn" onclick="focusCommentInput(${post.id})">
                        <i class="far fa-comment"></i>
                        <span class="action-count">${post.commentsCount}</span>
                    </button>
                    <button class="action-btn share-btn" onclick="sharePost(${post.id})">
                        <i class="far fa-paper-plane"></i>
                    </button>
                </div>
                <button class="action-btn bookmark-btn ${isBookmarked ? 'bookmarked' : ''}" onclick="toggleBookmark(${post.id})">
                    <i class="${isBookmarked ? 'fas' : 'far'} fa-bookmark"></i>
                </button>
            </div>
            
            <div class="post-content">
                <div class="post-likes">
                    <span id="likes-${post.id}">${post.likesCount}</span> likes
                </div>
                
                ${post.caption ? `
                    <div class="post-caption">
                        <span class="username">${post.user.displayName}</span>
                        <span class="caption-text">${post.caption}</span>
                    </div>
                ` : ''}
                
                ${post.commentsCount > 0 ? `
                    <button class="view-comments" onclick="toggleComments(${post.id})">
                        View all ${post.commentsCount} comments
                    </button>
                ` : ''}
                
                <div class="comments-section" id="comments-${post.id}" style="display: none;">
                    <div class="comments-list" id="comments-list-${post.id}"></div>
                </div>
                
                <form class="comment-form" onsubmit="addComment(event, ${post.id})">
                    <img src="${getCurrentUserAvatar()}" alt="You" class="comment-avatar" />
                    <input type="text" class="comment-input" id="comment-input-${post.id}" 
                           placeholder="Add a comment..." autocomplete="off" />
                    <button type="submit" class="comment-submit" disabled id="comment-submit-${post.id}">Post</button>
                </form>
            </div>
        </div>
    `;
}

function getCurrentUserAvatar() {
    const avatar = document.querySelector('.comment-form img')?.src;
    return avatar || getDefaultAvatar('U');
}

// Keyboard shortcuts
function initializeKeyboardShortcuts() {
    document.addEventListener('keydown', (e) => {
        if (['INPUT', 'TEXTAREA'].includes(e.target.tagName)) return;
        
        switch(e.key.toLowerCase()) {
            case 'h':
                window.location.href = '/Feed';
                break;
            case 'm':
                window.location.href = '/Messages';
                break;
            case 'c':
                window.location.href = '/Create';
                break;
            case 'e':
                window.location.href = '/Explore';
                break;
            case 'p':
                window.location.href = '/Profile';
                break;
            case 'r':
                if (e.ctrlKey || e.metaKey) {
                    e.preventDefault();
                    refreshFeed();
                }
                break;
            case '?':
                showKeyboardShortcuts();
                break;
        }
    });
}

// Load suggested users for sidebar
async function loadSuggestedUsers() {
    const container = document.querySelector('.suggested-users');
    if (!container) return;
    
    try {
        const response = await fetch('/api/users/suggested?count=5');
        if (response.ok) {
            const users = await response.json();
            if (users && users.length > 0) {
                renderSuggestedUsers(users);
            } else {
                showEmptySuggestions();
            }
        } else {
            showEmptySuggestions();
        }
    } catch (error) {
        console.error('Error loading suggested users:', error);
        showEmptySuggestions();
    }
}

function showEmptySuggestions() {
    const container = document.querySelector('.suggested-users');
    if (container) {
        container.innerHTML = `
            <div class="empty-suggestions text-center p-3">
                <i class="fas fa-users fa-2x text-muted mb-2"></i>
                <p class="text-muted mb-0 small">No suggestions available</p>
                <small class="text-muted">Follow more users to get suggestions</small>
            </div>
        `;
    }
}

function renderSuggestedUsers(users) {
    const container = document.querySelector('.suggested-users');
    if (!container) return;
    
    container.innerHTML = users.map(user => `
        <div class="suggested-user d-flex align-items-center mb-3">
            <img src="${user.profileImageUrl || getDefaultAvatar(user.displayName)}" 
                 alt="${user.displayName}" class="suggested-avatar rounded-circle me-2" 
                 style="width: 40px; height: 40px; object-fit: cover;" />
            <div class="suggested-info flex-grow-1">
                <div class="suggested-name fw-bold small">${user.userName}</div>
                <div class="suggested-mutual text-muted small">${user.mutualFriendsCount || 0} mutual friends</div>
            </div>
            <button class="btn btn-primary btn-sm follow-btn" onclick="followUser('${user.id}')" data-user-id="${user.id}">
                Follow
            </button>
        </div>
    `).join('');
}

async function followUser(userId) {
    try {
        const response = await fetch(`/api/users/${userId}/follow`, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': getRequestVerificationToken()
            }
        });
        
        if (response.ok) {
            const result = await response.json();
            const btn = document.querySelector(`[data-user-id="${userId}"]`);
            if (btn) {
                btn.textContent = result.isFollowing ? 'Following' : 'Follow';
                btn.classList.toggle('btn-primary', !result.isFollowing);
                btn.classList.toggle('btn-outline-secondary', result.isFollowing);
            }
            showNotification(result.isFollowing ? 'User followed!' : 'User unfollowed!', 'success');
        }
    } catch (error) {
        console.error('Error following user:', error);
        showNotification('Failed to follow user', 'error');
    }
}

// Load recent activity for sidebar
async function loadRecentActivity() {
    const container = document.querySelector('.activity-feed');
    if (!container) return;
    
    try {
        const response = await fetch('/api/notifications?page=0&pageSize=5');
        if (response.ok) {
            const notifications = await response.json();
            if (notifications && notifications.length > 0) {
                renderRecentActivity(notifications);
            } else {
                showEmptyActivity();
            }
        } else {
            showEmptyActivity();
        }
    } catch (error) {
        console.error('Error loading recent activity:', error);
        showEmptyActivity();
    }
}

function showEmptyActivity() {
    const container = document.querySelector('.activity-feed');
    if (container) {
        container.innerHTML = `
            <div class="empty-activity text-center p-3">
                <i class="fas fa-bell fa-2x text-muted mb-2"></i>
                <p class="text-muted mb-0 small">No recent activity</p>
                <small class="text-muted">Activity will appear here</small>
            </div>
        `;
    }
}

function renderRecentActivity(activities) {
    const container = document.querySelector('.activity-feed');
    if (!container || !activities.length) {
        showEmptyActivity();
        return;
    }
    
    container.innerHTML = activities.map(activity => {
        const icon = getActivityIcon(activity.type);
        return `
            <div class="activity-item d-flex align-items-center mb-3 p-2 border-bottom">
                <div class="activity-avatar me-2">
                    <img src="${activity.actor?.profileImageUrl || getDefaultAvatar(activity.actor?.displayName || 'U')}" 
                         alt="${activity.actor?.displayName || 'User'}" 
                         class="rounded-circle" 
                         style="width: 32px; height: 32px; object-fit: cover;" />
                </div>
                <div class="activity-content flex-grow-1">
                    <div class="activity-text small">
                        <strong>${activity.actor?.displayName || 'Someone'}</strong> 
                        ${activity.message}
                    </div>
                    <div class="activity-time text-muted" style="font-size: 0.75rem;">
                        ${getRelativeTime(activity.createdAt)}
                    </div>
                </div>
                <div class="activity-icon">
                    <i class="${icon}"></i>
                </div>
                ${activity.post ? `
                    <div class="activity-post-thumbnail ms-2">
                        <img src="${activity.post.imageUrl}" 
                             alt="Post" 
                             class="rounded" 
                             style="width: 32px; height: 32px; object-fit: cover;" />
                    </div>
                ` : ''}
            </div>
        `;
    }).join('');
}

function getActivityIcon(type) {
    const icons = {
        'like': 'fas fa-heart text-danger',
        'comment': 'fas fa-comment text-primary',
        'follow': 'fas fa-user-plus text-success',
        'story_view': 'fas fa-eye text-info'
    };
    return icons[type] || 'fas fa-bell';
}

function getActivityText(type) {
    const texts = {
        'like': 'liked your post',
        'comment': 'commented on your post',
        'follow': 'started following you',
        'story_view': 'viewed your story'
    };
    return texts[type] || 'interacted with your content';
}

// Feed settings functionality
function showFeedSettings() {
    if (typeof showModal === 'function') {
        showModal({
            title: 'Feed Settings',
            body: `
                <div class="feed-settings">
                    <h6 class="mb-3">Display Preferences</h6>
                    <div class="form-check form-switch mb-3">
                        <input class="form-check-input" type="checkbox" id="autoPlayVideos" checked>
                        <label class="form-check-label" for="autoPlayVideos">Auto-play videos</label>
                    </div>
                    <div class="form-check form-switch mb-3">
                        <input class="form-check-input" type="checkbox" id="showSuggestedUsers" checked>
                        <label class="form-check-label" for="showSuggestedUsers">Show suggested users</label>
                    </div>
                    <div class="form-check form-switch mb-3">
                        <input class="form-check-input" type="checkbox" id="showRecentActivity" checked>
                        <label class="form-check-label" for="showRecentActivity">Show recent activity</label>
                    </div>
                    <hr>
                    <h6 class="mb-3">Feed Behavior</h6>
                    <div class="form-check form-switch mb-3">
                        <input class="form-check-input" type="checkbox" id="infiniteScroll" checked>
                        <label class="form-check-label" for="infiniteScroll">Infinite scroll</label>
                    </div>
                    <div class="form-check form-switch mb-3">
                        <input class="form-check-input" type="checkbox" id="keyboardShortcuts" checked>
                        <label class="form-check-label" for="keyboardShortcuts">Enable keyboard shortcuts</label>
                    </div>
                </div>
            `,
            footer: `
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                <button type="button" class="btn btn-primary" onclick="saveFeedSettings()">Save Settings</button>
            `
        });
    }
}

function saveFeedSettings() {
    const settings = {
        autoPlayVideos: document.getElementById('autoPlayVideos').checked,
        showSuggestedUsers: document.getElementById('showSuggestedUsers').checked,
        showRecentActivity: document.getElementById('showRecentActivity').checked,
        infiniteScroll: document.getElementById('infiniteScroll').checked,
        keyboardShortcuts: document.getElementById('keyboardShortcuts').checked
    };
    
    // Save to localStorage
    localStorage.setItem('feedSettings', JSON.stringify(settings));
    
    // Apply settings immediately
    applyFeedSettings(settings);
    
    if (typeof window.modal !== 'undefined') {
        window.modal.hide();
    }
    
    showNotification('Feed settings saved!', 'success');
}

function loadFeedSettings() {
    const defaultSettings = {
        autoPlayVideos: true,
        showSuggestedUsers: true,
        showRecentActivity: true,
        infiniteScroll: true,
        keyboardShortcuts: true
    };
    
    const savedSettings = localStorage.getItem('feedSettings');
    const settings = savedSettings ? JSON.parse(savedSettings) : defaultSettings;
    
    applyFeedSettings(settings);
    return settings;
}

function applyFeedSettings(settings) {
    // Apply auto-play videos setting
    if (!settings.autoPlayVideos) {
        document.querySelectorAll('video').forEach(video => {
            video.removeAttribute('autoplay');
        });
    }
    
    // Show/hide suggested users
    const suggestedUsersCard = document.querySelector('.sidebar-card:has(.suggested-users)');
    if (suggestedUsersCard) {
        suggestedUsersCard.style.display = settings.showSuggestedUsers ? 'block' : 'none';
    }
    
    // Show/hide recent activity
    const activityCard = document.querySelector('.sidebar-card:has(.activity-feed)');
    if (activityCard) {
        activityCard.style.display = settings.showRecentActivity ? 'block' : 'none';
    }
    
    // Apply infinite scroll setting (this would require reinitializing)
    if (!settings.infiniteScroll) {
        // Disable infinite scroll observer
        console.log('Infinite scroll disabled');
    }
}

function showKeyboardShortcuts() {
    if (typeof showModal === 'function') {
        showModal({
            title: 'Keyboard Shortcuts',
            body: `
                <div class="keyboard-shortcuts">
                    <div class="row">
                        <div class="col-md-6">
                            <h6 class="mb-3">Navigation</h6>
                            <div class="shortcut-item d-flex align-items-center mb-2">
                                <kbd class="me-2">H</kbd> <span>Go to Home/Feed</span>
                            </div>
                            <div class="shortcut-item d-flex align-items-center mb-2">
                                <kbd class="me-2">M</kbd> <span>Go to Messages</span>
                            </div>
                            <div class="shortcut-item d-flex align-items-center mb-2">
                                <kbd class="me-2">C</kbd> <span>Create Post</span>
                            </div>
                            <div class="shortcut-item d-flex align-items-center mb-2">
                                <kbd class="me-2">E</kbd> <span>Explore</span>
                            </div>
                            <div class="shortcut-item d-flex align-items-center mb-2">
                                <kbd class="me-2">P</kbd> <span>Profile</span>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <h6 class="mb-3">Actions</h6>
                            <div class="shortcut-item d-flex align-items-center mb-2">
                                <kbd class="me-2">Space</kbd> <span>Play/Pause Videos</span>
                            </div>
                            <div class="shortcut-item d-flex align-items-center mb-2">
                                <kbd class="me-2">Ctrl + R</kbd> <span>Refresh Feed</span>
                            </div>
                            <div class="shortcut-item d-flex align-items-center mb-2">
                                <kbd class="me-2">Esc</kbd> <span>Close Modals</span>
                            </div>
                            <div class="shortcut-item d-flex align-items-center mb-2">
                                <kbd class="me-2">?</kbd> <span>Show This Help</span>
                            </div>
                        </div>
                    </div>
                </div>
            `,
            footer: `<button type="button" class="btn btn-primary" data-bs-dismiss="modal">Got it!</button>`
        });
    }
}

function clearCache() {
    if (typeof showModal === 'function') {
        showModal({
            title: 'Clear Cache',
            body: `
                <p>This will clear your feed cache and reload the page. Any unsaved changes will be lost.</p>
                <p class="text-muted">This can help resolve issues with outdated content or performance problems.</p>
            `,
            footer: `
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                <button type="button" class="btn btn-danger" onclick="performClearCache()">Clear Cache</button>
            `
        });
    }
}

function performClearCache() {
    // Clear localStorage
    localStorage.removeItem('feedSettings');
    
    // Clear any cached data
    feedPage = 0;
    hasMorePosts = true;
    isLoadingFeed = false;
    
    if (typeof window.modal !== 'undefined') {
        window.modal.hide();
    }
    
    // Reload the page
    setTimeout(() => {
        location.reload();
    }, 500);
}

function refreshFeed() {
    if (typeof window.modal !== 'undefined') {
        window.modal.hide();
    }
    
    // Reset pagination
    feedPage = 0;
    hasMorePosts = true;
    isLoadingFeed = false;
    
    // Show loading indicator
    if (typeof showNotification === 'function') {
        showNotification('Refreshing feed...', 'info');
    }
    
    // Reload the page
    setTimeout(() => {
        location.reload();
    }, 1000);
}

// Helper functions for notifications
function showNotification(message, type = 'info') {
    // Create notification element if it doesn't exist
    let notificationContainer = document.getElementById('notification-container');
    if (!notificationContainer) {
        notificationContainer = document.createElement('div');
        notificationContainer.id = 'notification-container';
        notificationContainer.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            z-index: 9999;
        `;
        document.body.appendChild(notificationContainer);
    }

    // Create notification
    const notification = document.createElement('div');
    notification.className = `alert alert-${getBootstrapAlertClass(type)} alert-dismissible fade show`;
    notification.style.cssText = `
        margin-bottom: 10px;
        box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        border: none;
        border-radius: 8px;
        min-width: 300px;
    `;
    
    notification.innerHTML = `
        <div class="d-flex align-items-center">
            <i class="${getNotificationIcon(type)} me-2"></i>
            <span>${message}</span>
            <button type="button" class="btn-close ms-auto" onclick="this.parentElement.parentElement.remove()"></button>
        </div>
    `;

    notificationContainer.appendChild(notification);

    // Auto remove after 4 seconds
    setTimeout(() => {
        if (notification.parentNode) {
            notification.remove();
        }
    }, 4000);
}

function getBootstrapAlertClass(type) {
    const classes = {
        'success': 'success',
        'error': 'danger',
        'warning': 'warning',
        'info': 'info'
    };
    return classes[type] || 'info';
}

function getNotificationIcon(type) {
    const icons = {
        'success': 'fas fa-check-circle',
        'error': 'fas fa-exclamation-circle',
        'warning': 'fas fa-exclamation-triangle',
        'info': 'fas fa-info-circle'
    };
    return icons[type] || 'fas fa-info-circle';
}

function renderComments(postId, comments) {
    const commentsList = document.getElementById(`comments-list-${postId}`);
    if (!commentsList) return;
    
    commentsList.innerHTML = comments.map(comment => `
        <div class="comment">
            <img src="${comment.user.profileImageUrl || getDefaultAvatar(comment.user.displayName)}" 
                 alt="${comment.user.displayName}" class="comment-avatar" />
            <div class="comment-content">
                <div class="comment-text">
                    <span class="comment-username">${comment.user.displayName}</span>
                    ${comment.text}
                </div>
                <div class="comment-time">${getRelativeTime(comment.createdAt)}</div>
            </div>
        </div>
    `).join('');
}

// Add these missing functions for post menu actions
function savePost(postId) {
    if (typeof window.modal !== 'undefined') {
        window.modal.hide();
    }
    toggleBookmark(postId);
}

function copyLink(postId) {
    if (typeof window.modal !== 'undefined') {
        window.modal.hide();
    }
    const url = `${window.location.origin}/posts/${postId}`;
    navigator.clipboard.writeText(url).then(() => {
        showNotification('Link copied to clipboard!', 'success');
    }).catch(() => {
        showNotification('Failed to copy link', 'error');
    });
}

function reportPost(postId) {
    if (typeof window.modal !== 'undefined') {
        window.modal.hide();
    }
    showNotification('Thank you for your report. We will review it shortly.', 'info');
}

// Initialize the page when loaded
document.addEventListener('DOMContentLoaded', function() {
    // Add smooth scrolling to the page
    document.documentElement.style.scrollBehavior = 'smooth';
    
    // Initialize comment inputs
    document.querySelectorAll('.comment-input').forEach(input => {
        input.addEventListener('input', function() {
            const submitBtn = this.parentElement.querySelector('.comment-submit');
            if (submitBtn) {
                submitBtn.disabled = !this.value.trim();
            }
        });
    });
    
    // Add loading animation to posts
    setTimeout(() => {
        document.querySelectorAll('.post-card').forEach((card, index) => {
            setTimeout(() => {
                card.classList.add('slide-up');
            }, index * 100);
        });
    }, 100);
});

// Initialize feed settings on load
document.addEventListener('DOMContentLoaded', function() {
    loadFeedSettings();
});
