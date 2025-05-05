/**
 * Booknix Admin - User Management JavaScript
 */

// Edit user modal functions
function openEditUserModal(userId, fullName, email, roleId) {
    document.getElementById('editUserId').value = userId;
    document.getElementById('editFullName').value = fullName;
    document.getElementById('editEmail').value = email;
    document.getElementById('editRoleId').value = roleId;
    document.getElementById('editUserModal').classList.remove('hidden');
}

function closeEditUserModal() {
    document.getElementById('editUserModal').classList.add('hidden');
}

// New user modal functions
function openNewUserModal() {
    // Reset form
    document.getElementById('newUserForm').reset();
    document.getElementById('newUserModal').classList.remove('hidden');
}

function closeNewUserModal() {
    document.getElementById('newUserModal').classList.add('hidden');
}

// Delete user modal functions
function confirmDeleteUser(userId, fullName) {
    document.getElementById('deleteUserId').value = userId;
    document.getElementById('deleteUserName').textContent = fullName;
    document.getElementById('deleteUserModal').classList.remove('hidden');
}

function closeDeleteUserModal() {
    document.getElementById('deleteUserModal').classList.add('hidden');
}

function deleteUser() {
    const userId = document.getElementById('deleteUserId').value;

    // Show loading
    toggleGlobalLoading(true);

    // Add CSRF token to request
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    axios.post(`/Admin/DeleteUser/${userId}`, {}, {
        headers: {
            'RequestVerificationToken': token
        }
    })
        .then(response => {
            closeDeleteUserModal();
            location.reload();
        })
        .catch(error => {
            alert(error.response?.data || 'Bir hata oluştu');
        })
        .finally(() => {
            toggleGlobalLoading(false);
        });
}

// Initialize event listeners when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    // Form submissions
    document.getElementById('editUserForm').addEventListener('submit', handleEditUserSubmit);
    document.getElementById('newUserForm').addEventListener('submit', handleNewUserSubmit);
});

// Edit user form handler
function handleEditUserSubmit(e) {
    e.preventDefault();

    const userId = document.getElementById('editUserId').value;
    const fullName = document.getElementById('editFullName').value;
    const email = document.getElementById('editEmail').value;
    const roleId = document.getElementById('editRoleId').value;

    if (!fullName || !email) {
        alert('Ad ve e-posta alanları zorunludur.');
        return;
    }

    // Show loading
    toggleGlobalLoading(true);

    // Add CSRF token to request
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    axios.post('/Admin/EditUser', {
        id: userId,
        fullName: fullName,
        email: email,
        roleId: roleId
    }, {
        headers: {
            'RequestVerificationToken': token
        }
    })
        .then(response => {
            closeEditUserModal();
            location.reload();
        })
        .catch(error => {
            alert(error.response?.data || 'Bir hata oluştu');
        })
        .finally(() => {
            toggleGlobalLoading(false);
        });
}

// New user form handler
function handleNewUserSubmit(e) {
    e.preventDefault();

    const fullName = document.getElementById('newFullName').value;
    const email = document.getElementById('newEmail').value;
    const password = document.getElementById('newPassword').value;
    const roleId = document.getElementById('newRoleId').value;

    if (!fullName || !email || !password) {
        alert('Lütfen tüm alanları doldurun');
        return;
    }

    // Show loading
    toggleGlobalLoading(true);

    // Add CSRF token to request
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    axios.post('/Admin/CreateUser', {
        fullName: fullName,
        email: email,
        password: password,
        roleId: roleId
    }, {
        headers: {
            'RequestVerificationToken': token
        }
    })
        .then(response => {
            closeNewUserModal();
            location.reload();
        })
        .catch(error => {
            alert(error.response?.data || 'Bir hata oluştu');
        })
        .finally(() => {
            toggleGlobalLoading(false);
        });
}

attachSearchFilter("user-search", "#userContainer", "#userCard");