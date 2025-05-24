document.addEventListener('DOMContentLoaded', function() {
    initializeCreatePage();
});

function initializeCreatePage() {
    initializeFileUpload();
    initializeCaptionCounter();
    initializeHashtags();
    initializeDragDrop();
    initializeTabs();
    initializeStoryCreation();
}

function initializeTabs() {
    const tabBtns = document.querySelectorAll('.tab-btn');
    tabBtns.forEach(btn => {
        btn.addEventListener('click', function() {
            const tabName = this.onclick.toString().match(/showTab\('(.+?)'\)/)[1];
            showTab(tabName);
        });
    });
}

function showTab(tabName) {
    document.querySelectorAll('.tab-btn').forEach(btn => btn.classList.remove('active'));
    document.querySelectorAll('.tab-content').forEach(content => content.classList.remove('active'));
    
    document.querySelector(`button[onclick="showTab('${tabName}')"]`).classList.add('active');
    document.getElementById(`${tabName}Tab`).classList.add('active');
}

function initializeFileUpload() {
    const mediaFile = document.getElementById('mediaFile');
    const storyFile = document.getElementById('storyFile');
    
    if (mediaFile) {
        mediaFile.addEventListener('change', handleMediaFileSelect);
    }
    
    if (storyFile) {
        storyFile.addEventListener('change', handleStoryFileSelect);
    }
}

function handleMediaFileSelect(event) {
    const file = event.target.files[0];
    if (file) {
        previewMedia(file);
        togglePublishButton();
    }
}

function handleStoryFileSelect(event) {
    const file = event.target.files[0];
    if (file) {
        previewStoryMedia(file);
        toggleStoryButton();
    }
}

function previewMedia(file) {
    const uploadPlaceholder = document.getElementById('uploadPlaceholder');
    const mediaPreview = document.getElementById('mediaPreview');
    const previewMedia = document.getElementById('previewMedia');
    
    uploadPlaceholder.style.display = 'none';
    mediaPreview.style.display = 'block';
    
    if (file.type.startsWith('video/')) {
        const video = document.createElement('video');
        video.src = URL.createObjectURL(file);
        video.controls = true;
        video.className = 'preview-video';
        previewMedia.innerHTML = '';
        previewMedia.appendChild(video);
    } else {
        const img = document.createElement('img');
        img.src = URL.createObjectURL(file);
        img.className = 'preview-image';
        previewMedia.innerHTML = '';
        previewMedia.appendChild(img);
    }
}

function previewStoryMedia(file) {
    const storyPlaceholder = document.getElementById('storyPlaceholder');
    const storyPreviewContainer = document.getElementById('storyPreviewContainer');
    const storyMediaPreview = document.getElementById('storyMediaPreview');
    
    storyPlaceholder.style.display = 'none';
    storyPreviewContainer.style.display = 'block';
    
    if (file.type.startsWith('video/')) {
        const video = document.createElement('video');
        video.src = URL.createObjectURL(file);
        video.controls = true;
        video.className = 'story-preview-video';
        storyMediaPreview.innerHTML = '';
        storyMediaPreview.appendChild(video);
    } else {
        const img = document.createElement('img');
        img.src = URL.createObjectURL(file);
        img.className = 'story-preview-image';
        storyMediaPreview.innerHTML = '';
        storyMediaPreview.appendChild(img);
    }
}

function removeMedia() {
    const uploadPlaceholder = document.getElementById('uploadPlaceholder');
    const mediaPreview = document.getElementById('mediaPreview');
    const mediaFile = document.getElementById('mediaFile');
    
    uploadPlaceholder.style.display = 'block';
    mediaPreview.style.display = 'none';
    mediaFile.value = '';
    
    togglePublishButton();
}

function removeStoryMedia() {
    const storyPlaceholder = document.getElementById('storyPlaceholder');
    const storyPreviewContainer = document.getElementById('storyPreviewContainer');
    const storyFile = document.getElementById('storyFile');
    
    storyPlaceholder.style.display = 'block';
    storyPreviewContainer.style.display = 'none';
    storyFile.value = '';
    
    toggleStoryButton();
}

function initializeDragDrop() {
    const uploadArea = document.getElementById('uploadArea');
    const storyUploadArea = document.getElementById('storyUploadArea');
    
    if (uploadArea) {
        setupDragDrop(uploadArea, handleMediaFileSelect);
    }
    
    if (storyUploadArea) {
        setupDragDrop(storyUploadArea, handleStoryFileSelect);
    }
}

function setupDragDrop(element, handler) {
    element.addEventListener('dragover', function(e) {
        e.preventDefault();
        element.classList.add('drag-over');
    });
    
    element.addEventListener('dragleave', function(e) {
        e.preventDefault();
        element.classList.remove('drag-over');
    });
    
    element.addEventListener('drop', function(e) {
        e.preventDefault();
        element.classList.remove('drag-over');
        
        const files = e.dataTransfer.files;
        if (files.length > 0) {
            const file = files[0];
            if (file.type.startsWith('image/') || file.type.startsWith('video/')) {
                handler({ target: { files: [file] } });
            } else {
                showNotification('Please upload an image or video file', 'error');
            }
        }
    });
}

function initializeCaptionCounter() {
    const captionInput = document.getElementById('Caption');
    const captionCount = document.getElementById('captionCount');
    
    if (captionInput && captionCount) {
        captionInput.addEventListener('input', function() {
            const count = this.value.length;
            captionCount.textContent = count;
            
            if (count > 2200) {
                captionCount.style.color = '#e74c3c';
            } else if (count > 2000) {
                captionCount.style.color = '#f39c12';
            } else {
                captionCount.style.color = '#6c757d';
            }
            
            togglePublishButton();
        });
    }
    
    const storyText = document.getElementById('storyText');
    const storyTextCount = document.getElementById('storyTextCount');
    
    if (storyText && storyTextCount) {
        storyText.addEventListener('input', function() {
            const count = this.value.length;
            storyTextCount.textContent = count;
            
            if (count > 100) {
                storyTextCount.style.color = '#e74c3c';
            } else if (count > 80) {
                storyTextCount.style.color = '#f39c12';
            } else {
                storyTextCount.style.color = '#6c757d';
            }
        });
    }
}

function initializeHashtags() {
    const captionInput = document.getElementById('Caption');
    
    if (captionInput) {
        captionInput.addEventListener('input', function() {
            highlightHashtags(this);
        });
    }
}

function addHashtag(hashtag) {
    const captionInput = document.getElementById('Caption');
    if (captionInput) {
        const currentText = captionInput.value;
        const cursorPos = captionInput.selectionStart;
        
        // Add hashtag at cursor position
        const newText = currentText.slice(0, cursorPos) + hashtag + ' ' + currentText.slice(cursorPos);
        captionInput.value = newText;
        
        const newCursorPos = cursorPos + hashtag.length + 1;
        captionInput.setSelectionRange(newCursorPos, newCursorPos);
        
        const event = new Event('input', { bubbles: true });
        captionInput.dispatchEvent(event);
        
        captionInput.focus();
    }
}

function highlightHashtags(input) {
    const hashtags = input.value.match(/#[\w]+/g);
    if (hashtags) {
        console.log('Hashtags found:', hashtags);
    }
}

function togglePublishButton() {
    const publishBtn = document.getElementById('publishBtn');
    const mediaFile = document.getElementById('mediaFile');
    
    if (publishBtn && mediaFile) {
        const hasFile = mediaFile.files.length > 0;
        publishBtn.disabled = !hasFile;
        
        if (hasFile) {
            publishBtn.classList.remove('btn-secondary');
            publishBtn.classList.add('btn-primary');
        } else {
            publishBtn.classList.remove('btn-primary');
            publishBtn.classList.add('btn-secondary');
        }
    }
}

function toggleStoryButton() {
    const shareStoryBtn = document.getElementById('shareStoryBtn');
    const storyFile = document.getElementById('storyFile');
    
    if (shareStoryBtn && storyFile) {
        const hasFile = storyFile.files.length > 0;
        shareStoryBtn.disabled = !hasFile;
        
        if (hasFile) {
            shareStoryBtn.classList.remove('btn-secondary');
            shareStoryBtn.classList.add('btn-primary');
        } else {
            shareStoryBtn.classList.remove('btn-primary');
            shareStoryBtn.classList.add('btn-secondary');
        }
    }
}

function initializeStoryCreation() {
    // Initialize story-specific features
    const storyUploadArea = document.getElementById('storyUploadArea');
    
    if (storyUploadArea) {
        storyUploadArea.addEventListener('click', function(e) {
            if (e.target.closest('.story-placeholder')) {
                document.getElementById('storyFile').click();
            }
        });
    }
}

document.addEventListener('submit', function(e) {
    const form = e.target;
    
    if (form.classList.contains('create-form')) {
        handlePostSubmission(e);
    } else if (form.classList.contains('story-form')) {
        handleStorySubmission(e);
    }
});

function handlePostSubmission(e) {
    const form = e.target;
    const submitBtn = form.querySelector('button[type="submit"]');
    
    // Show loading state
    const originalText = submitBtn.innerHTML;
    submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Sharing...';
    submitBtn.disabled = true;
    
    // Validate file
    const mediaFile = document.getElementById('mediaFile');
    if (!mediaFile.files.length) {
        e.preventDefault();
        showNotification('Please select an image or video to share', 'error');
        submitBtn.innerHTML = originalText;
        submitBtn.disabled = false;
        return;
    }
    
    // Validate file size (10MB limit)
    const file = mediaFile.files[0];
    const maxSize = 10 * 1024 * 1024; // 10MB
    
    if (file.size > maxSize) {
        e.preventDefault();
        showNotification('File size must be less than 10MB', 'error');
        submitBtn.innerHTML = originalText;
        submitBtn.disabled = false;
        return;
    }
    
    // If validation passes, form will submit normally
}

function handleStorySubmission(e) {
    const form = e.target;
    const submitBtn = form.querySelector('button[type="submit"]');
    
    // Show loading state
    const originalText = submitBtn.innerHTML;
    submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Sharing...';
    submitBtn.disabled = true;
    
    // Validate file
    const storyFile = document.getElementById('storyFile');
    if (!storyFile.files.length) {
        e.preventDefault();
        showNotification('Please select an image or video for your story', 'error');
        submitBtn.innerHTML = originalText;
        submitBtn.disabled = false;
        return;
    }
    
    // Validate file size (5MB limit for stories)
    const file = storyFile.files[0];
    const maxSize = 5 * 1024 * 1024; // 5MB
    
    if (file.size > maxSize) {
        e.preventDefault();
        showNotification('Story file size must be less than 5MB', 'error');
        submitBtn.innerHTML = originalText;
        submitBtn.disabled = false;
        return;
    }
    
    // If validation passes, form will submit normally
}

function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

function showNotification(message, type = 'info') {
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.textContent = message;
    
    // Style the notification
    Object.assign(notification.style, {
        position: 'fixed',
        top: '20px',
        right: '20px',
        padding: '12px 20px',
        borderRadius: '8px',
        color: 'white',
        fontWeight: '500',
        zIndex: '10000',
        transform: 'translateX(400px)',
        transition: 'transform 0.3s ease',
        maxWidth: '300px'
    });
    
    // Set background color based on type
    switch (type) {
        case 'success':
            notification.style.backgroundColor = '#10b981';
            break;
        case 'error':
            notification.style.backgroundColor = '#ef4444';
            break;
        default:
            notification.style.backgroundColor = '#3b82f6';
    }
    
    document.body.appendChild(notification);
    
    // Animate in
    setTimeout(() => {
        notification.style.transform = 'translateX(0)';
    }, 100);
    
    // Remove after 4 seconds
    setTimeout(() => {
        notification.style.transform = 'translateX(400px)';
        setTimeout(() => {
            if (document.body.contains(notification)) {
                document.body.removeChild(notification);
            }
        }, 300);
    }, 4000);
}

document.addEventListener('keydown', function(e) {
    // Ctrl/Cmd + Enter to submit
    if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') {
        const activeTab = document.querySelector('.tab-content.active');
        if (activeTab) {
            const submitBtn = activeTab.querySelector('button[type="submit"]');
            if (submitBtn && !submitBtn.disabled) {
                submitBtn.click();
            }
        }
    }
    
    // Escape to clear current upload
    if (e.key === 'Escape') {
        const activeTab = document.querySelector('.tab-content.active');
        if (activeTab && activeTab.id === 'postTab') {
            const mediaPreview = document.getElementById('mediaPreview');
            if (mediaPreview && mediaPreview.style.display !== 'none') {
                removeMedia();
            }
        } else if (activeTab && activeTab.id === 'storyTab') {
            const storyPreview = document.getElementById('storyPreviewContainer');
            if (storyPreview && storyPreview.style.display !== 'none') {
                removeStoryMedia();
            }
        }
    }
});

let autoSaveTimer;
function autoSave() {
    clearTimeout(autoSaveTimer);
    autoSaveTimer = setTimeout(() => {
        const captionInput = document.getElementById('Caption');
        if (captionInput && captionInput.value.trim()) {
            // Save draft to localStorage
            const draft = {
                caption: captionInput.value,
                timestamp: Date.now()
            };
            localStorage.setItem('snapstagram_draft', JSON.stringify(draft));
        }
    }, 2000);
}

function loadDraft() {
    const draft = localStorage.getItem('snapstagram_draft');
    if (draft) {
        try {
            const parsedDraft = JSON.parse(draft);
            // Only load if draft is less than 24 hours old
            if (Date.now() - parsedDraft.timestamp < 24 * 60 * 60 * 1000) {
                const captionInput = document.getElementById('Caption');
                if (captionInput && !captionInput.value) {
                    captionInput.value = parsedDraft.caption;
                    // Trigger input event to update counter
                    captionInput.dispatchEvent(new Event('input', { bubbles: true }));
                }
            } else {
                localStorage.removeItem('snapstagram_draft');
            }
        } catch (e) {
            localStorage.removeItem('snapstagram_draft');
        }
    }
}

document.addEventListener('DOMContentLoaded', function() {
    loadDraft();
    
    const captionInput = document.getElementById('Caption');
    if (captionInput) {
        captionInput.addEventListener('input', autoSave);
    }
});
