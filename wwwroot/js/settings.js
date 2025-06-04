// Settings page JavaScript
document.addEventListener('DOMContentLoaded', function() {
    // Bio character counter
    const bioTextarea = document.getElementById('Bio');
    const bioCount = document.getElementById('bioCount');
    
    if (bioTextarea && bioCount) {
        bioTextarea.addEventListener('input', function() {
            const charCount = this.value.length;
            bioCount.textContent = charCount;
            
            if (charCount > 400) {
                bioCount.style.color = '#dc3545';
            } else if (charCount > 300) {
                bioCount.style.color = '#ffc107';
            } else {
                bioCount.style.color = '';
            }
        });
    }

    // Change Email functionality
    const changeEmailBtn = document.getElementById('changeEmailBtn');
    const emailInput = document.getElementById('emailInput');
    
    if (changeEmailBtn && emailInput) {
        changeEmailBtn.addEventListener('click', function() {
            const currentEmail = emailInput.value;
            const newEmail = prompt('Enter new email address:', currentEmail);
            
            if (newEmail && newEmail !== currentEmail) {
                // Simple email validation
                if (newEmail.indexOf("@") === -1 || newEmail.indexOf(".") === -1) {
                    alert('Please enter a valid email address');
                    return;
                }
                
                // Create a form and submit
                const form = document.createElement('form');
                form.method = 'POST';
                form.action = window.location.pathname + '?handler=UpdateEmail';
                
                // Create hidden input for email
                const emailField = document.createElement('input');
                emailField.type = 'hidden';
                emailField.name = 'newEmail';
                emailField.value = newEmail;
                form.appendChild(emailField);
                
                // Create anti-forgery token
                const tokenField = document.createElement('input');
                tokenField.type = 'hidden';
                tokenField.name = '__RequestVerificationToken';
                tokenField.value = document.querySelector('input[name="__RequestVerificationToken"]').value;
                form.appendChild(tokenField);
                
                // Submit the form
                document.body.appendChild(form);
                form.submit();
            }
        });
    }
});

// Show downloading message when downloading data
function showDownloadingMessage() {
    document.getElementById('downloadingMessage').classList.remove('d-none');
    return true;
}

// Confirm deletion of account
function confirmDelete() {
    return confirm('Are you absolutely sure you want to delete your account? This action CANNOT be undone.');
}
