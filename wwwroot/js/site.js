// search functionality
function initializeSearch() {
    const searchInput = document.querySelector('.search-input');
    const searchContainer = document.querySelector('.search-container');
    
    if (!searchInput || !searchContainer) return;
    
    let searchTimeout;
    let searchResults = null;
    
    const createSearchDropdown = () => {
        if (searchResults) return searchResults;
        
        searchResults = document.createElement('div');
        searchResults.className = 'search-results';
        searchResults.style.cssText = `
            position: absolute;
            top: 100%;
            left: 0;
            right: 0;
            background: white;
            border: 1px solid #e1e5e9;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            max-height: 400px;
            overflow-y: auto;
            z-index: 1000;
            margin-top: 4px;
            display: none;
        `;
        
        searchContainer.style.position = 'relative';
        searchContainer.appendChild(searchResults);
        return searchResults;
    };
    
    // search
    const performSearch = async (query) => {
        if (query.length < 2) {
            hideSearchResults();
            return;
        }
        
        try {
            const response = await fetch(`/api/profile/search?query=${encodeURIComponent(query)}&limit=8`);
            if (response.ok) {
                const users = await response.json();
                displaySearchResults(users);
            }
        } catch (error) {
            console.error('Search error:', error);
        }
    };
    
    const displaySearchResults = (users) => {
        const dropdown = createSearchDropdown();
        
        if (users.length === 0) {
            dropdown.innerHTML = '<div style="padding: 16px; text-align: center; color: #6c757d;">No users found</div>';
        } else {
            dropdown.innerHTML = users.map(user => `
                <a href="/ProfileView?username=${user.userName}" style="display: block; padding: 12px 16px; text-decoration: none; color: inherit; border-bottom: 1px solid #f8f9fa;">
                    <div style="display: flex; align-items: center; gap: 12px;">
                        <img src="${user.profileImageUrl || '/images/default-avatar.svg'}" 
                             style="width: 40px; height: 40px; border-radius: 50%; object-fit: cover;">
                        <div>
                            <div style="font-weight: 600; color: #1a1a1a;">${user.userName}</div>
                            <div style="font-size: 14px; color: #6c757d;">${user.displayName}</div>
                        </div>
                        ${user.isPrivate ? '<i class="fas fa-lock" style="color: #6c757d; margin-left: auto;"></i>' : ''}
                    </div>
                </a>
            `).join('');
        }
        
        dropdown.style.display = 'block';
    };
    
    const hideSearchResults = () => {
        if (searchResults) {
            searchResults.style.display = 'none';
        }
    };
    
    searchInput.addEventListener('input', (e) => {
        clearTimeout(searchTimeout);
        const query = e.target.value.trim();
        
        searchTimeout = setTimeout(() => {
            performSearch(query);
        }, 300);
    });
    
    searchInput.addEventListener('focus', (e) => {
        if (e.target.value.trim().length >= 2) {
            performSearch(e.target.value.trim());
        }
    });
    
    // hide results when clicking outside
    document.addEventListener('click', (e) => {
        if (!searchContainer.contains(e.target)) {
            hideSearchResults();
        }
    });
    
    // keyboard navigation
    searchInput.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
            hideSearchResults();
            searchInput.blur();
        }
    });
}

// profile dropdown
function initializeProfileDropdown() {
    const profileDropdown = document.querySelector('.profile-dropdown');
    if (!profileDropdown) return;
    
    const profileIcon = profileDropdown.querySelector('.profile-icon');
    const dropdownMenu = profileDropdown.querySelector('.dropdown-menu');
    
    if (!profileIcon || !dropdownMenu) return;
    
    profileIcon.addEventListener('click', (e) => {
        e.stopPropagation();
        dropdownMenu.classList.toggle('show');
    });
    
    // close dropdown when clicking outside
    document.addEventListener('click', () => {
        dropdownMenu.classList.remove('show');
    });
}

const showNotification = (message, type = 'info') => {
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.textContent = message;
    
    const bgColors = { success: '#10b981', error: '#ef4444', default: '#3b82f6' };
    
    Object.assign(notification.style, {
        position: 'fixed',
        top: '20px',
        right: '20px',
        padding: '12px 20px',
        borderRadius: '8px',
        color: 'white',
        fontWeight: '500',
        zIndex: '10000',
        backgroundColor: bgColors[type] || bgColors.default,
        transform: 'translateX(400px)',
        transition: 'transform 0.3s ease',
        maxWidth: '300px'
    });
    
    document.body.appendChild(notification);
    
    setTimeout(() => notification.style.transform = 'translateX(0)', 100);
    setTimeout(() => {
        notification.style.transform = 'translateX(400px)';
        setTimeout(() => document.body.contains(notification) && document.body.removeChild(notification), 300);
    }, 4000);
};

const getRelativeTime = (date) => {
    const now = new Date();
    const diffTime = Math.abs(now - date);
    const diffDays = Math.floor(diffTime / (1000 * 60 * 60 * 24));
    const diffHours = Math.floor(diffTime / (1000 * 60 * 60));
    const diffMinutes = Math.floor(diffTime / (1000 * 60));
    
    if (diffDays > 0) return `${diffDays}d`;
    if (diffHours > 0) return `${diffHours}h`;
    if (diffMinutes > 0) return `${diffMinutes}m`;
    return 'now';
};

const getDefaultAvatar = (name) => {
    const initial = name ? name[0].toUpperCase() : 'U';
    return `https://via.placeholder.com/40x40/6c757d/ffffff?text=${initial}`;
};

const escapeHtml = (text) => {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
};

document.addEventListener('DOMContentLoaded', function() {
    
    window.showNotification = showNotification;
    window.getRelativeTime = getRelativeTime;
    window.getDefaultAvatar = getDefaultAvatar;
    window.escapeHtml = escapeHtml;
    
    initializeSearch();
    initializeProfileDropdown();
});
