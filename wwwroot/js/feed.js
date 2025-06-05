let currentStoryIndex = 0;
let currentStories = [];
let storyTimer = null;

document.addEventListener('DOMContentLoaded', initializeFeed);

function initializeFeed() {
    initializeCommentInputs();
    initializeDoubleTap();
    initializeStoriesScroll();
    initializePostObserver();
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
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({ text: commentText })
        });
        
        if (response.ok) {
            const comment = await response.json();
            addCommentToUI(postId, comment);
            input.value = '';
            updateCommentsCount(postId, 1);
        } else {
            showNotification('Failed to add comment', 'error');
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
    
    try {
        const isLiked = likeBtn.classList.contains('liked');
        const newCount = parseInt(likesCount.textContent) + (isLiked ? -1 : 1);
        
        likeBtn.classList.toggle('liked');
        likeBtn.querySelector('i').className = isLiked ? 'far fa-heart' : 'fas fa-heart';
        likesCount.textContent = newCount;
        postLikesDisplay.textContent = newCount;
        
        likeBtn.style.transform = 'scale(1.2)';
        setTimeout(() => {
            likeBtn.style.transform = 'scale(1)';
        }, 150);
        
        const response = await fetch(`/api/posts/${postId}/like`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
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
            likeBtn.classList.toggle('liked');
            likeBtn.querySelector('i').className = isLiked ? 'fas fa-heart' : 'far fa-heart';
            likesCount.textContent = parseInt(likesCount.textContent) + (isLiked ? 1 : -1);
            postLikesDisplay.textContent = likesCount.textContent;
        }
    } catch (error) {
        console.error('Error toggling like:', error);
        likeBtn.classList.toggle('liked');
        likeBtn.querySelector('i').className = isLiked ? 'fas fa-heart' : 'far fa-heart';
        likesCount.textContent = parseInt(likesCount.textContent) + (isLiked ? 1 : -1);
        postLikesDisplay.textContent = likesCount.textContent;
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
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
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
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            }
        });
        
        if (response.ok) {
            const result = await response.json();
            const bookmarkBtn = document.querySelector(`button[onclick="toggleBookmark(${postId})"] i`);
            bookmarkBtn.className = result.bookmarked ? 'fas fa-bookmark' : 'far fa-bookmark';
            
            showNotification(
                result.bookmarked ? 'Post saved to bookmarks' : 'Post removed from bookmarks',
                'success'
            );
        }
    } catch (error) {
        console.error('Error toggling bookmark:', error);
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
