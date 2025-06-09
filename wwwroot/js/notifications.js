// Real-time notifications with SignalR
class NotificationManager {
    constructor() {
        this.connection = null;
        this.isAuthenticated = document.querySelector('[data-user-authenticated]')?.getAttribute('data-user-authenticated') === 'true';
        this.init();
    }

    async init() {
        if (!this.isAuthenticated) return;

        try {
            // Initialize SignalR connection
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/notificationHub")
                .withAutomaticReconnect()
                .build();

            // Set up event handlers
            this.setupSignalRHandlers();
            this.setupUIHandlers();

            // Start connection
            await this.connection.start();
            console.log("SignalR Connected for notifications");

            // Load initial notifications
            await this.loadNotifications();
            await this.updateNotificationCount();

        } catch (err) {
            console.error("SignalR connection error:", err);
            // Fallback to periodic refresh
            this.setupFallbackRefresh();
        }
    }

    setupSignalRHandlers() {
        // Handle new notifications
        this.connection.on("ReceiveNotification", (notification) => {
            this.addNotificationToDropdown(notification);
            this.updateNotificationCount();
            this.showNotificationToast(notification);
        });

        // Handle notification count updates
        this.connection.on("NotificationCountUpdated", (count) => {
            this.updateBadgeCount(count);
        });

        // Handle connection events
        this.connection.onreconnected(() => {
            console.log("SignalR reconnected");
            this.loadNotifications();
            this.updateNotificationCount();
        });
    }

    setupUIHandlers() {
        // Mark all as read button
        document.getElementById('markAllReadBtn')?.addEventListener('click', async () => {
            await this.markAllAsRead();
        });

        // Load notifications when dropdown is opened
        document.getElementById('notificationDropdown')?.addEventListener('show.bs.dropdown', () => {
            this.loadNotifications();
        });

        // Handle individual notification clicks
        document.addEventListener('click', (e) => {
            if (e.target.closest('.notification-item[data-notification-id]')) {
                const notificationItem = e.target.closest('.notification-item');
                const notificationId = notificationItem.getAttribute('data-notification-id');
                const isRead = notificationItem.getAttribute('data-is-read') === 'true';
                
                if (!isRead) {
                    this.markAsRead(notificationId);
                }
            }
        });
    }

    async loadNotifications() {
        try {
            const response = await fetch('/api/notifications/recent');
            if (response.ok) {
                const notifications = await response.json();
                this.renderNotifications(notifications);
            }
        } catch (err) {
            console.error("Error loading notifications:", err);
            this.showErrorInDropdown();
        }
    }

    async updateNotificationCount() {
        try {
            const response = await fetch('/api/notifications/count');
            if (response.ok) {
                const data = await response.json();
                this.updateBadgeCount(data.count);
            }
        } catch (err) {
            console.error("Error updating notification count:", err);
        }
    }

    async markAsRead(notificationId) {
        try {
            const response = await fetch(`/api/notifications/${notificationId}/read`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('#ajaxTokenForm input[name="__RequestVerificationToken"]')?.value || ''
                }
            });

            if (response.ok) {
                // Update UI immediately
                const notificationItem = document.querySelector(`[data-notification-id="${notificationId}"]`);
                if (notificationItem) {
                    notificationItem.setAttribute('data-is-read', 'true');
                    notificationItem.classList.remove('notification-unread');
                    notificationItem.classList.add('notification-read');
                }
                await this.updateNotificationCount();
            }
        } catch (err) {
            console.error("Error marking notification as read:", err);
        }
    }

    async markAllAsRead() {
        try {
            const response = await fetch('/api/notifications/read-all', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('#ajaxTokenForm input[name="__RequestVerificationToken"]')?.value || ''
                }
            });

            if (response.ok) {
                // Update UI immediately
                document.querySelectorAll('.notification-item').forEach(item => {
                    item.setAttribute('data-is-read', 'true');
                    item.classList.remove('notification-unread');
                    item.classList.add('notification-read');
                });
                this.updateBadgeCount(0);
            }
        } catch (err) {
            console.error("Error marking all notifications as read:", err);
        }
    }

    renderNotifications(notifications) {
        const container = document.getElementById('notificationList');
        if (!container) return;

        if (notifications.length === 0) {
            container.innerHTML = `
                <div class="text-center p-3 text-muted">
                    <i class="fas fa-bell-slash fa-2x mb-2"></i>
                    <div>No notifications yet</div>
                </div>
            `;
            return;
        }

        container.innerHTML = notifications.map(notification => 
            this.createNotificationHTML(notification)
        ).join('');
    }

    createNotificationHTML(notification) {
        const isRead = notification.isRead;
        const readClass = isRead ? 'notification-read' : 'notification-unread';
        const timeAgo = this.timeAgo(new Date(notification.createdAt));
        
        let icon = 'fas fa-bell';
        let actionText = '';
        
        switch (notification.type) {
            case 'FriendRequest':
                icon = 'fas fa-user-plus';
                actionText = 'sent you a friend request';
                break;
            case 'FriendRequestAccepted':
                icon = 'fas fa-user-check';
                actionText = 'accepted your friend request';
                break;
            case 'NewMessage':
                icon = 'fas fa-message';
                actionText = 'sent you a message';
                break;
            case 'PostLike':
                icon = 'fas fa-heart';
                actionText = 'liked your post';
                break;
            case 'PostComment':
                icon = 'fas fa-comment';
                actionText = 'commented on your post';
                break;
            default:
                actionText = notification.message;
        }

        return `
            <div class="dropdown-item notification-item ${readClass}" 
                 data-notification-id="${notification.id}" 
                 data-is-read="${isRead}">
                <div class="d-flex align-items-start">
                    <div class="notification-icon me-3">
                        <i class="${icon}"></i>
                    </div>
                    <div class="flex-grow-1">
                        <div class="notification-content">
                            <strong>${notification.senderName || 'Someone'}</strong> ${actionText}
                        </div>
                        <small class="text-muted">${timeAgo}</small>
                    </div>
                    ${!isRead ? '<div class="notification-dot"></div>' : ''}
                </div>
            </div>
        `;
    }

    addNotificationToDropdown(notification) {
        const container = document.getElementById('notificationList');
        if (!container) return;

        // Remove "no notifications" message if present
        const noNotifications = container.querySelector('.text-center');
        if (noNotifications) {
            container.innerHTML = '';
        }

        // Add new notification at the top
        const notificationHTML = this.createNotificationHTML(notification);
        container.insertAdjacentHTML('afterbegin', notificationHTML);

        // Keep only latest 10 notifications in dropdown
        const items = container.querySelectorAll('.notification-item');
        if (items.length > 10) {
            items[items.length - 1].remove();
        }
    }

    updateBadgeCount(count) {
        const badge = document.getElementById('notificationBadge');
        if (!badge) return;

        if (count > 0) {
            badge.textContent = count > 99 ? '99+' : count;
            badge.style.display = 'inline-block';
        } else {
            badge.style.display = 'none';
        }
    }

    showNotificationToast(notification) {
        // Create a simple toast notification
        const toast = document.createElement('div');
        toast.className = 'notification-toast';
        toast.innerHTML = `
            <div class="d-flex align-items-center">
                <i class="fas fa-bell me-2"></i>
                <div>
                    <strong>${notification.senderName || 'New notification'}</strong>
                    <div class="small">${notification.message}</div>
                </div>
            </div>
        `;
        
        document.body.appendChild(toast);
        
        // Show toast
        setTimeout(() => toast.classList.add('show'), 100);
        
        // Hide toast after 4 seconds
        setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => toast.remove(), 300);
        }, 4000);
    }

    showErrorInDropdown() {
        const container = document.getElementById('notificationList');
        if (!container) return;

        container.innerHTML = `
            <div class="text-center p-3 text-muted">
                <i class="fas fa-exclamation-triangle fa-2x mb-2"></i>
                <div>Error loading notifications</div>
                <button class="btn btn-sm btn-outline-primary mt-2" onclick="notificationManager.loadNotifications()">
                    Try again
                </button>
            </div>
        `;
    }

    setupFallbackRefresh() {
        // Fallback: refresh every 30 seconds if SignalR fails
        setInterval(async () => {
            await this.updateNotificationCount();
        }, 30000);
    }

    timeAgo(date) {
        const now = new Date();
        const diffInSeconds = Math.floor((now - date) / 1000);
        
        if (diffInSeconds < 60) return 'Just now';
        
        const diffInMinutes = Math.floor(diffInSeconds / 60);
        if (diffInMinutes < 60) return `${diffInMinutes}m ago`;
        
        const diffInHours = Math.floor(diffInMinutes / 60);
        if (diffInHours < 24) return `${diffInHours}h ago`;
        
        const diffInDays = Math.floor(diffInHours / 24);
        if (diffInDays < 7) return `${diffInDays}d ago`;
        
        return date.toLocaleDateString();
    }
}

// Initialize when DOM is ready
let notificationManager;
document.addEventListener('DOMContentLoaded', () => {
    notificationManager = new NotificationManager();
});
