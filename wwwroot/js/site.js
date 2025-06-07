// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// User Search Functionality
(function() {
    let searchTimeout;
    let isSearching = false;
    
    function initializeUserSearch() {
        const searchInput = document.getElementById('userSearchInput');
        const searchDropdown = document.getElementById('searchDropdown');
        const mobileSearchInput = document.getElementById('mobileUserSearchInput');
        const mobileSearchDropdown = document.getElementById('mobileSearchDropdown');
        const mobileSearchToggle = document.getElementById('mobileSearchToggle');
        const mobileSearchSection = document.getElementById('mobileSearchSection');
        
        // Initialize desktop search
        if (searchInput && searchDropdown) {
            initializeSearchInput(searchInput, searchDropdown);
        }
        
        // Initialize mobile search
        if (mobileSearchInput && mobileSearchDropdown) {
            initializeSearchInput(mobileSearchInput, mobileSearchDropdown);
        }
        
        // Mobile search toggle
        if (mobileSearchToggle && mobileSearchSection) {
            mobileSearchToggle.addEventListener('click', function(e) {
                e.preventDefault();
                mobileSearchSection.classList.toggle('show');
                if (mobileSearchSection.classList.contains('show')) {
                    mobileSearchInput?.focus();
                }
            });
        }
        
        // Hide mobile search when clicking outside
        document.addEventListener('click', function(e) {
            if (mobileSearchSection && 
                !e.target.closest('.mobile-search-section') && 
                !e.target.closest('#mobileSearchToggle')) {
                mobileSearchSection.classList.remove('show');
            }
            
            if (!e.target.closest('.search-container')) {
                hideAllSearchDropdowns();
            }
        });
    }
    
    function initializeSearchInput(searchInput, searchDropdown) {
        // Search input event listener
        searchInput.addEventListener('input', function(e) {
            const query = e.target.value.trim();
            
            if (query.length < 2) {
                hideSearchDropdown(searchDropdown);
                return;
            }
            
            // Debounce search requests
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                performSearch(query, searchDropdown);
            }, 300);
        });
        
        // Hide dropdown on escape key
        searchInput.addEventListener('keydown', function(e) {
            if (e.key === 'Escape') {
                hideSearchDropdown(searchDropdown);
                searchInput.blur();
            }
        });
    }
    
    function performSearch(query, searchDropdown) {
        if (isSearching) return;
        isSearching = true;
        
        // Show loading state
        searchDropdown.innerHTML = `
            <div class="search-loading">
                <i class="fas fa-spinner"></i> Searching...
            </div>
        `;
        showSearchDropdown(searchDropdown);
        
        // Make API request
        fetch(`/api/search/users?query=${encodeURIComponent(query)}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Search failed');
                }
                return response.json();
            })
            .then(users => {
                displaySearchResults(users, searchDropdown);
            })
            .catch(error => {
                console.error('Search error:', error);
                searchDropdown.innerHTML = `
                    <div class="search-no-results">
                        <i class="fas fa-exclamation-triangle"></i> Search failed. Please try again.
                    </div>
                `;
            })
            .finally(() => {
                isSearching = false;
            });
    }
    
    function displaySearchResults(users, searchDropdown) {
        if (users.length === 0) {
            searchDropdown.innerHTML = `
                <div class="search-no-results">
                    <i class="fas fa-user-slash"></i> No users found
                </div>
            `;
            return;
        }
        
        const resultsHtml = users.map(user => {
            const fullName = `${user.firstName || ''} ${user.lastName || ''}`.trim();
            const displayName = fullName || user.email;
            
            const avatar = user.profilePictureUrl 
                ? `<img src="${user.profilePictureUrl}" alt="${displayName}" class="search-result-avatar">`
                : `<div class="search-result-avatar"><i class="fas fa-user"></i></div>`;
            
            const privacyIcon = user.isProfilePublic 
                ? '<i class="fas fa-globe" style="color: var(--accent-color);" title="Public profile"></i>'
                : '<i class="fas fa-lock" style="color: var(--text-secondary);" title="Private profile"></i>';
            
            return `
                <a href="/Account/Profile/${user.id}" class="search-result">
                    ${avatar}
                    <div class="search-result-info">
                        <div class="search-result-name">
                            ${escapeHtml(displayName)} ${privacyIcon}
                        </div>
                        <div class="search-result-email">${escapeHtml(user.email)}</div>
                    </div>
                </a>
            `;
        }).join('');
        
        searchDropdown.innerHTML = resultsHtml;
    }
    
    function showSearchDropdown(searchDropdown) {
        searchDropdown.classList.add('show');
    }
    
    function hideSearchDropdown(searchDropdown) {
        searchDropdown.classList.remove('show');
    }
    
    function hideAllSearchDropdowns() {
        const dropdowns = document.querySelectorAll('.search-dropdown');
        dropdowns.forEach(dropdown => {
            dropdown.classList.remove('show');
        });
    }
    
    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
    
    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeUserSearch);
    } else {
        initializeUserSearch();
    }
})();
