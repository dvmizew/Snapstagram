// Get security token function
        function getSecurityToken() {
            const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
            if (!tokenInput) {
                showNotification('Security token not found. Please refresh the page.', 'error');
                return null;
            }
            return tokenInput.value;
        }

        // Friend request functions with improved UX
        function respondToFriendRequest(requestId, accept) {
            const button = event.target.closest('button');
            const originalContent = button.innerHTML;
            
            // Show loading state
            button.disabled = true;
            button.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
            
            const token = getSecurityToken();
            if (!token) {
                button.disabled = false;
                button.innerHTML = originalContent;
                return;
            }
            
            fetch('/Account/Friends?handler=RespondToFriendRequest', {
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
                    // Add smooth transition before reload
                    const requestItem = button.closest('.request-item');
                    if (requestItem) {
                        requestItem.style.transition = 'all 0.5s ease';
                        requestItem.style.opacity = '0';
                        requestItem.style.transform = 'translateX(-20px)';
                        setTimeout(() => location.reload(), 500);
                    } else {
                        setTimeout(() => location.reload(), 1000);
                    }
                } else {
                    showNotification(data.message, 'error');
                    button.disabled = false;
                    button.innerHTML = originalContent;
                }
            })
            .catch(error => {
                console.error('Error:', error);
                showNotification('An error occurred', 'error');
                button.disabled = false;
                button.innerHTML = originalContent;
            });
        }

        function cancelFriendRequest(requestId) {
            if (!confirm('Are you sure you want to cancel this friend request?')) {
                return;
            }
            
            const button = event.target.closest('button');
            const originalContent = button.innerHTML;
            
            // Show loading state
            button.disabled = true;
            button.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Canceling...';
            
            const token = getSecurityToken();
            if (!token) {
                button.disabled = false;
                button.innerHTML = originalContent;
                return;
            }
            
            fetch('/Account/Friends?handler=CancelFriendRequest', {
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
                    // Add smooth transition before reload
                    const requestItem = button.closest('.request-item');
                    if (requestItem) {
                        requestItem.style.transition = 'all 0.5s ease';
                        requestItem.style.opacity = '0';
                        requestItem.style.transform = 'translateX(-20px)';
                        setTimeout(() => location.reload(), 500);
                    } else {
                        setTimeout(() => location.reload(), 1000);
                    }
                } else {
                    showNotification(data.message, 'error');
                    button.disabled = false;
                    button.innerHTML = originalContent;
                }
            })
            .catch(error => {
                console.error('Error:', error);
                showNotification('An error occurred', 'error');
                button.disabled = false;
                button.innerHTML = originalContent;
            });
        }

        function removeFriend(friendId) {
            if (!confirm('Are you sure you want to remove this friend? This action cannot be undone.')) {
                return;
            }
            
            const button = event.target.closest('button');
            const originalContent = button.innerHTML;
            
            // Show loading state
            button.disabled = true;
            button.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';
            
            const token = getSecurityToken();
            if (!token) {
                button.disabled = false;
                button.innerHTML = originalContent;
                return;
            }
            
            fetch('/Account/Friends?handler=RemoveFriend', {
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
                    // Add smooth transition before reload
                    const friendItem = button.closest('.friend-item');
                    if (friendItem) {
                        friendItem.style.transition = 'all 0.5s ease';
                        friendItem.style.opacity = '0';
                        friendItem.style.transform = 'translateX(-20px)';
                        setTimeout(() => location.reload(), 500);
                    } else {
                        setTimeout(() => location.reload(), 1000);
                    }
                } else {
                    showNotification(data.message, 'error');
                    button.disabled = false;
                    button.innerHTML = originalContent;
                }
            })
            .catch(error => {
                console.error('Error:', error);
                showNotification('An error occurred', 'error');
                button.disabled = false;
                button.innerHTML = originalContent;
            });
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
            
            // Auto-remove after 5 seconds
            setTimeout(() => {
                if (toast.parentElement) {
                    toast.remove();
                }
            }, 5000);
        }

        // Add smooth transitions when tab content loads
        document.addEventListener('DOMContentLoaded', function() {
            // Initialize friend search
            initializeFriendSearch();

            const tabButtons = document.querySelectorAll('[data-bs-toggle="tab"]');
            tabButtons.forEach(button => {
                button.addEventListener('shown.bs.tab', function() {
                    const targetPane = document.querySelector(this.getAttribute('data-bs-target'));
                    if (targetPane) {
                        targetPane.style.opacity = '0';
                        targetPane.style.transform = 'translateY(10px)';
                        setTimeout(() => {
                            targetPane.style.transition = 'all 0.3s ease';
                            targetPane.style.opacity = '1';
                            targetPane.style.transform = 'translateY(0)';
                        }, 50);
                    }
                });
            });
        });

        // Friend search functionality
        function initializeFriendSearch() {
            const searchInput = document.getElementById('friendSearchInput');
            const searchResults = document.getElementById('friendSearchResults');
            let searchTimeout;

            if (!searchInput || !searchResults) return;

            searchInput.addEventListener('input', function() {
                clearTimeout(searchTimeout);
                const query = this.value.trim();

                if (query.length < 2) {
                    searchResults.innerHTML = '';
                    searchResults.style.display = 'none';
                    return;
                }

                searchTimeout = setTimeout(() => {
                    searchForFriends(query, searchResults);
                }, 300);
            });

            // Hide results when clicking outside
            document.addEventListener('click', function(e) {
                if (!searchInput.contains(e.target) && !searchResults.contains(e.target)) {
                    searchResults.style.display = 'none';
                }
            });

            searchInput.addEventListener('focus', function() {
                if (searchResults.innerHTML) {
                    searchResults.style.display = 'block';
                }
            });
        }

        function searchForFriends(query, resultsContainer) {
            fetch(`/api/search/users?query=${encodeURIComponent(query)}`)
                .then(response => response.json())
                .then(users => {
                    displayFriendSearchResults(users, resultsContainer);
                })
                .catch(error => {
                    console.error('Search error:', error);
                    resultsContainer.innerHTML = '<div class="alert alert-danger">Error searching for users</div>';
                    resultsContainer.style.display = 'block';
                });
        }

        function displayFriendSearchResults(users, container) {
            if (users.length === 0) {
                container.innerHTML = `
                    <div class="text-center py-3 text-muted">
                        <i class="fas fa-user-slash me-2"></i>
                        No users found
                    </div>
                `;
                container.style.display = 'block';
                return;
            }

            const resultsHtml = users.map(user => {
                const fullName = `${user.firstName || ''} ${user.lastName || ''}`.trim();
                const displayName = fullName || user.email;
                const avatar = user.profilePictureUrl 
                    ? `<img src="${user.profilePictureUrl}" alt="${displayName}" class="rounded-circle me-3" style="width: 40px; height: 40px; object-fit: cover;">`
                    : `<div class="rounded-circle bg-secondary d-flex align-items-center justify-content-center me-3" style="width: 40px; height: 40px;"><i class="fas fa-user text-white"></i></div>`;
                
                const privacyIcon = user.isProfilePublic 
                    ? '<i class="fas fa-globe text-success ms-2" title="Public profile"></i>'
                    : '<i class="fas fa-lock text-secondary ms-2" title="Private profile"></i>';

                return `
                    <div class="search-result-item d-flex align-items-center p-3 border-bottom bg-light-hover">
                        ${avatar}
                        <div class="flex-grow-1">
                            <div class="d-flex align-items-center">
                                <a href="/Account/Profile/${user.id}" class="text-decoration-none text-dark fw-bold me-2">
                                    ${escapeHtml(displayName)}
                                </a>
                                ${privacyIcon}
                            </div>
                            <small class="text-muted">${escapeHtml(user.email)}</small>
                        </div>
                        <div class="ms-3">
                            <button class="btn btn-primary btn-sm" onclick="sendFriendRequestFromSearch('${user.id}', this)">
                                <i class="fas fa-user-plus me-1"></i>
                                Add Friend
                            </button>
                        </div>
                    </div>
                `;
            }).join('');

            container.innerHTML = `<div class="border rounded bg-white shadow-sm">${resultsHtml}</div>`;
            container.style.display = 'block';
        }

        function sendFriendRequestFromSearch(receiverId, button) {
            const originalContent = button.innerHTML;
            button.disabled = true;
            button.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';

            const token = getSecurityToken();
            if (!token) {
                button.disabled = false;
                button.innerHTML = originalContent;
                return;
            }

            fetch('/Account/Friends?handler=SendFriendRequest', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: `__RequestVerificationToken=${encodeURIComponent(token)}&receiverId=${receiverId}`
            })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    showNotification(data.message, 'success');
                    button.innerHTML = '<i class="fas fa-check me-1"></i>Sent';
                    button.classList.remove('btn-primary');
                    button.classList.add('btn-success');
                    button.disabled = true;
                } else {
                    showNotification(data.message, 'error');
                    button.disabled = false;
                    button.innerHTML = originalContent;
                }
            })
            .catch(error => {
                console.error('Error:', error);
                showNotification('An error occurred while sending friend request', 'error');
                button.disabled = false;
                button.innerHTML = originalContent;
            });
        }

        function escapeHtml(text) {
            const div = document.createElement('div');
            div.textContent = text;
            return div.innerHTML;
        }