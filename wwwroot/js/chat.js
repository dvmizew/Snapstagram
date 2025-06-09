// Real-time chat with SignalR
class ChatManager {
    constructor() {
        this.connection = null;
        this.currentUserId = null;
        this.currentRecipientId = null;
        this.currentGroupId = null;
        this.typingTimeout = null;
        this.isTyping = false;
        this.isAuthenticated = document.querySelector('[data-user-authenticated]')?.getAttribute('data-user-authenticated') === 'true';
        this.init();
    }

    async init() {
        if (!this.isAuthenticated) return;

        // Get current user ID from meta tag or data attribute
        this.currentUserId = document.querySelector('[data-current-user-id]')?.getAttribute('data-current-user-id');
        this.currentRecipientId = document.querySelector('[data-recipient-id]')?.getAttribute('data-recipient-id');
        this.currentGroupId = document.querySelector('[data-group-id]')?.getAttribute('data-group-id');

        if (!this.currentUserId) return;

        try {
            // Initialize SignalR connection
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/chatHub")
                .withAutomaticReconnect()
                .build();

            // Set up event handlers
            this.setupSignalRHandlers();
            this.setupUIHandlers();

            // Start connection
            await this.connection.start();
            console.log("SignalR Connected for chat");

            // Join appropriate chat rooms
            if (this.currentRecipientId) {
                await this.joinConversation();
            }
            if (this.currentGroupId) {
                await this.joinGroup();
            }

        } catch (err) {
            console.error("SignalR connection error:", err);
        }
    }

    setupSignalRHandlers() {
        // Handle new individual messages
        this.connection.on("ReceiveMessage", (messageData) => {
            this.addMessageToChat(messageData);
            this.playNotificationSound();
        });

        // Handle new group messages
        this.connection.on("ReceiveGroupMessage", (messageData) => {
            this.addGroupMessageToChat(messageData);
            this.playNotificationSound();
        });

        // Handle typing indicators
        this.connection.on("TypingIndicator", (userId, isTyping) => {
            if (userId !== this.currentUserId) {
                this.showTypingIndicator(userId, isTyping);
            }
        });

        // Handle group typing indicators
        this.connection.on("GroupTypingIndicator", (userId, isTyping) => {
            if (userId !== this.currentUserId) {
                this.showGroupTypingIndicator(userId, isTyping);
            }
        });

        // Handle message read status
        this.connection.on("MessageRead", (messageId, readByUserId) => {
            this.updateMessageReadStatus(messageId, readByUserId);
        });

        // Handle connection events
        this.connection.onreconnected(() => {
            console.log("SignalR reconnected");
            if (this.currentRecipientId) {
                this.joinConversation();
            }
            if (this.currentGroupId) {
                this.joinGroup();
            }
        });
    }

    setupUIHandlers() {
        // Message form submission
        const messageForm = document.getElementById('messageForm');
        if (messageForm) {
            messageForm.addEventListener('submit', (e) => {
                e.preventDefault();
                this.sendMessage();
            });
        }

        // Typing indicator for message input
        const messageInput = document.getElementById('messageInput');
        if (messageInput) {
            messageInput.addEventListener('input', () => {
                this.handleTyping();
            });

            messageInput.addEventListener('keydown', (e) => {
                if (e.key === 'Enter' && !e.shiftKey) {
                    e.preventDefault();
                    this.sendMessage();
                }
            });
        }

        // Auto-scroll to bottom
        this.scrollToBottom();

        // Mark messages as read when page loads
        this.markVisibleMessagesAsRead();
    }

    async joinConversation() {
        if (this.currentRecipientId) {
            const conversationId = [this.currentUserId, this.currentRecipientId].sort().join('_');
            await this.connection.invoke("JoinConversation", conversationId);
        }
    }

    async joinGroup() {
        if (this.currentGroupId) {
            await this.connection.invoke("JoinChatGroup", this.currentGroupId);
        }
    }

    async sendMessage() {
        const messageInput = document.getElementById('messageInput');
        const messageContent = messageInput?.value?.trim();
        
        if (!messageContent) return;

        // Clear typing indicator
        this.stopTyping();

        // Prepare form data
        const formData = new FormData();
        formData.append('Content', messageContent);
        
        if (this.currentRecipientId) {
            formData.append('RecipientId', this.currentRecipientId);
        } else if (this.currentGroupId) {
            formData.append('GroupId', this.currentGroupId);
        }

        // Add anti-forgery token
        const token = document.querySelector('#ajaxTokenForm input[name="__RequestVerificationToken"]')?.value;
        if (token) {
            formData.append('__RequestVerificationToken', token);
        }

        try {
            const response = await fetch('/Chat/SendMessage', {
                method: 'POST',
                body: formData
            });

            if (response.ok) {
                const result = await response.json();
                if (result.success) {
                    messageInput.value = '';
                    messageInput.focus();
                } else {
                    console.error('Failed to send message:', result.message);
                }
            }
        } catch (err) {
            console.error('Error sending message:', err);
        }
    }

    addMessageToChat(messageData) {
        const messagesContainer = document.querySelector('.messages-container, .chat-messages');
        if (!messagesContainer) return;

        // Check if this is a conversation page and the message belongs to this conversation
        if (this.currentRecipientId) {
            const isInConversation = messageData.senderId === this.currentUserId || 
                                   messageData.senderId === this.currentRecipientId;
            if (!isInConversation) return;
        }

        const messageHtml = this.createMessageHTML(messageData);
        messagesContainer.insertAdjacentHTML('beforeend', messageHtml);
        this.scrollToBottom();
    }

    addGroupMessageToChat(messageData) {
        const messagesContainer = document.querySelector('.messages-container, .chat-messages');
        if (!messagesContainer) return;

        // Check if this is the correct group
        if (this.currentGroupId && messageData.groupId != this.currentGroupId) return;

        const messageHtml = this.createGroupMessageHTML(messageData);
        messagesContainer.insertAdjacentHTML('beforeend', messageHtml);
        this.scrollToBottom();
    }

    createMessageHTML(messageData) {
        const isOwn = messageData.senderId === this.currentUserId;
        const messageClass = isOwn ? 'message-own' : 'message-other';
        const timeString = new Date(messageData.sentAt).toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'});
        
        return `
            <div class="message ${messageClass}" data-message-id="${messageData.id}">
                ${!isOwn ? `
                    <div class="message-avatar">
                        <img src="${messageData.senderAvatar}" alt="${messageData.senderName}" 
                             class="rounded-circle" width="32" height="32">
                    </div>
                ` : ''}
                <div class="message-content">
                    ${!isOwn ? `<div class="message-sender">${messageData.senderName}</div>` : ''}
                    <div class="message-text">${this.escapeHtml(messageData.content)}</div>
                    <div class="message-time">${timeString}</div>
                </div>
            </div>
        `;
    }

    createGroupMessageHTML(messageData) {
        const isOwn = messageData.senderId === this.currentUserId;
        const messageClass = isOwn ? 'message-own' : 'message-other';
        const timeString = new Date(messageData.sentAt).toLocaleTimeString([], {hour: '2-digit', minute:'2-digit'});
        
        return `
            <div class="message ${messageClass}" data-message-id="${messageData.id}">
                ${!isOwn ? `
                    <div class="message-avatar">
                        <img src="${messageData.senderAvatar}" alt="${messageData.senderName}" 
                             class="rounded-circle" width="32" height="32">
                    </div>
                ` : ''}
                <div class="message-content">
                    ${!isOwn ? `<div class="message-sender">${messageData.senderName}</div>` : ''}
                    <div class="message-text">${this.escapeHtml(messageData.content)}</div>
                    <div class="message-time">${timeString}</div>
                </div>
            </div>
        `;
    }

    async handleTyping() {
        if (!this.isTyping) {
            this.isTyping = true;
            
            if (this.currentRecipientId) {
                await this.connection.invoke("SendTypingIndicator", this.currentRecipientId, true);
            } else if (this.currentGroupId) {
                await this.connection.invoke("SendGroupTypingIndicator", this.currentGroupId, true);
            }
        }

        // Clear existing timeout
        if (this.typingTimeout) {
            clearTimeout(this.typingTimeout);
        }

        // Set new timeout to stop typing
        this.typingTimeout = setTimeout(() => {
            this.stopTyping();
        }, 2000);
    }

    async stopTyping() {
        if (this.isTyping) {
            this.isTyping = false;
            
            if (this.currentRecipientId) {
                await this.connection.invoke("SendTypingIndicator", this.currentRecipientId, false);
            } else if (this.currentGroupId) {
                await this.connection.invoke("SendGroupTypingIndicator", this.currentGroupId, false);
            }
        }

        if (this.typingTimeout) {
            clearTimeout(this.typingTimeout);
            this.typingTimeout = null;
        }
    }

    showTypingIndicator(userId, isTyping) {
        const typingContainer = document.getElementById('typingIndicator');
        if (!typingContainer) return;

        if (isTyping) {
            typingContainer.innerHTML = `
                <div class="typing-indicator">
                    <span>Someone is typing</span>
                    <div class="typing-dots">
                        <span></span>
                        <span></span>
                        <span></span>
                    </div>
                </div>
            `;
            typingContainer.style.display = 'block';
        } else {
            typingContainer.style.display = 'none';
        }
    }

    showGroupTypingIndicator(userId, isTyping) {
        // Similar to individual typing but can handle multiple users
        this.showTypingIndicator(userId, isTyping);
    }

    updateMessageReadStatus(messageId, readByUserId) {
        const messageElement = document.querySelector(`[data-message-id="${messageId}"]`);
        if (messageElement) {
            messageElement.classList.add('message-read');
        }
    }

    markVisibleMessagesAsRead() {
        const messages = document.querySelectorAll('.message[data-message-id]');
        messages.forEach(async (message) => {
            const messageId = message.getAttribute('data-message-id');
            const senderId = message.querySelector('.message-content')?.getAttribute('data-sender-id');
            
            if (messageId && senderId && senderId !== this.currentUserId) {
                try {
                    const formData = new FormData();
                    formData.append('messageId', messageId);
                    
                    const token = document.querySelector('#ajaxTokenForm input[name="__RequestVerificationToken"]')?.value;
                    if (token) {
                        formData.append('__RequestVerificationToken', token);
                    }

                    await fetch('/Chat/MarkAsRead', {
                        method: 'POST',
                        body: formData
                    });
                } catch (err) {
                    console.error('Error marking message as read:', err);
                }
            }
        });
    }

    scrollToBottom() {
        const messagesContainer = document.querySelector('.messages-container, .chat-messages');
        if (messagesContainer) {
            messagesContainer.scrollTop = messagesContainer.scrollHeight;
        }
    }

    playNotificationSound() {
        // Simple notification sound using audio
        try {
            const audio = new Audio('data:audio/wav;base64,UklGRnoGAABXQVZFZm10IBAAAAABAAEAQB8AAEAfAAABAAgAZGF0YQoGAACBhYqFbF1fdJivrJBhNjVgodDbq2EcBj+a2/LDciUFLIHO8tiJNwgZaLvt559NEAxQp+PwtmMcBjiR1/LMeSsFLYDN8tiJOAkaZrzu5qNOCgxUp+Tw...'); // Base64 of a simple beep
            audio.volume = 0.3;
            audio.play().catch(() => {}); // Ignore errors if audio fails
        } catch (err) {
            // Ignore audio errors
        }
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
}

// Initialize when DOM is ready
let chatManager;
document.addEventListener('DOMContentLoaded', () => {
    chatManager = new ChatManager();
});

// Clean up when leaving page
window.addEventListener('beforeunload', () => {
    if (chatManager && chatManager.connection) {
        chatManager.stopTyping();
    }
});
