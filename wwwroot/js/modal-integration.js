// Profile integration functions
window.updateProfile = async function(data) {
    try {
        const response = await fetch('/api/profile/update', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify(data)
        });
        
        const result = await response.json();
        if (response.ok) {
            window.showNotification('Profile updated successfully!', 'success');
            setTimeout(() => window.location.reload(), 1000);
            return true;
        } else {
            window.showNotification(result.message || 'Failed to update profile', 'error');
            return false;
        }
    } catch (error) {
        window.showNotification('Error updating profile', 'error');
        return false;
    }
};

window.changePassword = async function(data) {
    try {
        const response = await fetch('/api/profile/change-password', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify(data)
        });
        
        const result = await response.json();
        if (response.ok) {
            window.showNotification('Password changed successfully!', 'success');
            return true;
        } else {
            window.showNotification(result.message || 'Failed to change password', 'error');
            return false;
        }
    } catch (error) {
        window.showNotification('Error changing password', 'error');
        return false;
    }
};

window.updateProfileImage = async function(data) {
    try {
        const response = await fetch('/api/profile/update-image', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify(data)
        });
        
        const result = await response.json();
        if (response.ok) {
            window.showNotification('Profile image updated successfully!', 'success');
            setTimeout(() => window.location.reload(), 1000);
            return true;
        } else {
            window.showNotification(result.message || 'Failed to update profile image', 'error');
            return false;
        }
    } catch (error) {
        window.showNotification('Error updating profile image', 'error');
        return false;
    }
};

// Enhanced profile modal functions
window.openEditProfileModal = function() {
    const fields = [
        { 
            type: 'text', 
            name: 'displayName', 
            label: 'Display Name', 
            value: document.querySelector('[data-profile-displayname]')?.textContent || document.querySelector('.profile-displayname')?.textContent || '',
            required: true 
        },
        { 
            type: 'textarea', 
            name: 'bio', 
            label: 'Bio', 
            value: document.querySelector('[data-profile-bio]')?.textContent || document.querySelector('.bio')?.textContent || '',
            attributes: { rows: 3 }
        },
        { 
            type: 'checkbox', 
            name: 'isPrivate', 
            label: 'Private Account',
            value: document.querySelector('[data-profile-private]')?.value === 'true' || document.querySelector('.badge:contains("Private")')?.length > 0
        }
    ];

    window.modal.show({
        title: 'Edit Profile',
        size: 'default'
    });

    window.modal.createForm(fields, {
        submitText: 'Save Changes',
        onSubmit: async (data) => {
            const result = await window.updateProfile({
                displayName: data.displayName,
                bio: data.bio,
                isPrivate: data.isPrivate
            });
            return result;
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
                window.showNotification('Passwords do not match', 'error');
                return false;
            }
            
            const result = await window.changePassword({
                currentPassword: data.currentPassword,
                newPassword: data.newPassword,
                confirmPassword: data.confirmPassword
            });
            return result;
        }
    });
};

window.openProfileImageModal = function() {
    const currentImageSrc = document.querySelector('.profile-image')?.src || 
                           document.querySelector('.profile-pic img')?.src ||
                           document.querySelector('img[alt*="Profile"]')?.src ||
                           '/images/default-avatar.svg';
                           
    window.modal.show({
        title: 'Update Profile Picture',
        size: 'default',
        body: `
            <div class="text-center">
                <div class="mb-3">
                    <img src="${currentImageSrc}" 
                         alt="Current Profile" 
                         class="img-thumbnail rounded-circle" 
                         style="width: 150px; height: 150px; object-fit: cover;">
                </div>
                <div class="mb-3">
                    <input type="file" class="form-control" id="profileImageInput" accept="image/*">
                    <div class="form-text">Choose a JPG, PNG, or GIF file (max 5MB)</div>
                </div>
                <div id="imagePreview" style="display: none;">
                    <img id="previewImg" class="img-thumbnail rounded-circle" style="width: 150px; height: 150px; object-fit: cover;">
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
        const previewContainer = document.getElementById('imagePreview');
        const previewImg = document.getElementById('previewImg');
        const uploadBtn = document.getElementById('uploadImageBtn');

        fileInput.addEventListener('change', function(e) {
            const file = e.target.files[0];
            if (file) {
                // Validate file type
                const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
                if (!allowedTypes.includes(file.type.toLowerCase())) {
                    window.showNotification('Please select a valid image file (JPEG, PNG, or GIF)', 'error');
                    return;
                }

                // Validate file size (5MB)
                if (file.size > 5 * 1024 * 1024) {
                    window.showNotification('Image file must be smaller than 5MB', 'error');
                    return;
                }

                const reader = new FileReader();
                reader.onload = function(e) {
                    previewImg.src = e.target.result;
                    previewContainer.style.display = 'block';
                    uploadBtn.disabled = false;
                };
                reader.readAsDataURL(file);
            }
        });

        uploadBtn.addEventListener('click', async function() {
            const file = fileInput.files[0];
            if (!file) return;

            uploadBtn.disabled = true;
            uploadBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Uploading...';

            try {
                // Create FormData for file upload
                const formData = new FormData();
                formData.append('profileImage', file);

                console.log('Uploading profile image...');

                const response = await fetch('/api/profile/upload-image', {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                    }
                });

                console.log('Upload response status:', response.status);
                const result = await response.json();
                console.log('Upload result:', result);

                if (response.ok) {
                    window.showNotification('Profile image updated successfully!', 'success');
                    window.modal.hide();
                    
                    // Update all profile images on the page immediately
                    const newImageUrl = result.imageUrl;
                    const cacheBuster = '?t=' + Date.now();
                    const newImageUrlWithCache = newImageUrl + cacheBuster;
                    
                    console.log('Updating profile images with new URL:', newImageUrlWithCache);
                    
                    // Find and update ALL images that could be profile images
                    const allImages = document.querySelectorAll('img');
                    let updatedCount = 0;
                    
                    allImages.forEach(img => {
                        // Check if this looks like a profile image
                        const isProfileImage = 
                            img.classList.contains('profile-image') ||
                            img.classList.contains('profile-pic') ||
                            img.alt?.toLowerCase().includes('profile') ||
                            img.src?.includes('profile') ||
                            img.src?.includes('avatar') ||
                            img.closest('.profile-pic-wrapper') ||
                            img.closest('.profile-header-card') ||
                            img.parentElement?.classList.contains('profile-pic-wrapper');
                        
                        if (isProfileImage) {
                            console.log('Updating profile image:', img.src, '->', newImageUrlWithCache);
                            img.src = newImageUrlWithCache;
                            updatedCount++;
                        }
                    });
                    
                    console.log(`Updated ${updatedCount} profile images`);
                    
                    // Also specifically target known selectors
                    const specificSelectors = [
                        '.profile-image',
                        '.profile-pic img',
                        '.profile-pic-wrapper img',
                        'img[alt*="Profile"]',
                        'img[alt*="profile"]',
                        '.profile-header-card img',
                        '.nav-icon img',
                        '.profile-icon img'
                    ];
                    
                    specificSelectors.forEach(selector => {
                        const elements = document.querySelectorAll(selector);
                        elements.forEach(img => {
                            console.log(`Updating via selector ${selector}:`, img.src, '->', newImageUrlWithCache);
                            img.src = newImageUrlWithCache;
                        });
                    });
                    
                    // Force a refresh after a short delay to catch any lazy-loaded images
                    setTimeout(() => {
                        document.querySelectorAll('img').forEach(img => {
                            if (img.src?.includes('profile') || img.src?.includes('avatar') || 
                                img.alt?.toLowerCase().includes('profile')) {
                                img.src = newImageUrlWithCache;
                            }
                        });
                    }, 500);
                    
                    // Trigger a custom event for other components that might need to update
                    window.dispatchEvent(new CustomEvent('profileImageUpdated', { 
                        detail: { imageUrl: newImageUrl } 
                    }));
                    
                    // If no images were updated, reload the page as fallback
                    if (updatedCount === 0) {
                        console.log('No profile images found to update, reloading page...');
                        setTimeout(() => {
                            window.location.reload();
                        }, 1000);
                    }
                    
                } else {
                    window.showNotification(result.message || 'Failed to update profile image', 'error');
                }
            } catch (error) {
                console.error('Upload error:', error);
                window.showNotification('Error uploading image', 'error');
            } finally {
                uploadBtn.disabled = false;
                uploadBtn.innerHTML = 'Update Picture';
            }
        });
    }, 100);
};

window.openFollowersModal = function() {
    const userId = document.querySelector('[data-user-id]')?.getAttribute('data-user-id') || 
                   window.profileUserId; // fallback to global variable if set

    window.modal.show({
        title: 'Followers',
        size: 'default',
        body: `
            <div id="followersList" class="user-list-container">
                <div class="text-center py-3">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                </div>
            </div>
        `,
        footer: `<button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>`
    });

    if (userId) {
        loadUserList('followers', userId, '#followersList');
    }
};

window.openFollowingModal = function() {
    const userId = document.querySelector('[data-user-id]')?.getAttribute('data-user-id') || 
                   window.profileUserId; // fallback to global variable if set

    window.modal.show({
        title: 'Following',
        size: 'default',
        body: `
            <div id="followingList" class="user-list-container">
                <div class="text-center py-3">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                </div>
            </div>
        `,
        footer: `<button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>`
    });

    if (userId) {
        loadUserList('following', userId, '#followingList');
    }
};

function loadUserList(type, userId, container) {
    fetch(`/api/profile/${type}/${userId}`)
        .then(response => response.json())
        .then(users => {
            let html = '';
            if (users.length > 0) {
                users.forEach(user => {
                    const profileImg = user.profileImageUrl || '/images/default-avatar.svg';
                    const displayName = user.displayName || user.userName;
                    const username = user.userName;
                    
                    html += `
                        <div class="d-flex align-items-center p-3 border-bottom">
                            <img src="${profileImg}" 
                                 alt="${displayName}" 
                                 class="rounded-circle me-3" 
                                 style="width: 50px; height: 50px; object-fit: cover;">
                            <div class="flex-grow-1">
                                <div class="fw-bold">${displayName}</div>
                                <div class="text-muted small">@${username}</div>
                            </div>
                            <a href="/ProfileView?username=${username}" class="btn btn-outline-primary btn-sm">
                                View Profile
                            </a>
                        </div>
                    `;
                });
            } else {
                html = `
                    <div class="text-center py-4 text-muted">
                        <i class="fas fa-users fa-2x mb-3"></i>
                        <p>No ${type} found</p>
                    </div>
                `;
            }
            document.querySelector(container).innerHTML = html;
        })
        .catch(error => {
            console.error('Error loading user list:', error);
            document.querySelector(container).innerHTML = `
                <div class="text-center py-4 text-danger">
                    <i class="fas fa-exclamation-circle fa-2x mb-3"></i>
                    <p>Failed to load ${type}</p>
                </div>
            `;
        });
}