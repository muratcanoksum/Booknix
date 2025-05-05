// Booknix Admin - User Management JavaScript

// ------------------- Kullanıcı Düzenleme -------------------

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

    showLoading();
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    axios.post('/Admin/EditUser', {
        id: userId,
        fullName,
        email,
        roleId
    }, {
        headers: { 'RequestVerificationToken': token }
    })
        .then(() => {
            closeEditUserModal();
            location.reload();
        })
        .catch(error => {
            alert(error.response?.data || 'Bir hata oluştu');
        })
        .finally(() => hideLoading());
}

// ------------------- Yeni Kullanıcı -------------------

function openNewUserModal() {
    document.getElementById('newUserForm').reset();
    document.getElementById('newUserModal').classList.remove('hidden');
}

function closeNewUserModal() {
    document.getElementById('newUserModal').classList.add('hidden');
}

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

    showLoading();
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    $.ajax({
        type: "POST",
        url: "/Admin/CreateUser",
        data: {
            FullName: fullName,
            Email: email,
            Password: password,
            RoleId: roleId
        },
        headers: {
            "RequestVerificationToken": token
        },
        success: function () {
            closeNewUserModal();
            location.reload();
        },
        error: function (xhr) {
            alert(xhr.responseText || 'Bir hata oluştu');
        },
        complete: function () {
            hideLoading();
        }
    });
}


// ------------------- Kullanıcı Silme -------------------

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
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    showLoading();

    axios.post(`/Admin/DeleteUser/${userId}`, {}, {
        headers: { 'RequestVerificationToken': token }
    })
        .then(() => {
            closeDeleteUserModal();
            location.reload();
        })
        .catch(error => {
            alert(error.response?.data || 'Bir hata oluştu');
        })
        .finally(() => hideLoading());
}

// ------------------- Oturum Yönetimi -------------------

function toggleUserSessions(userId, button) {
    const containerId = `#session-container-${userId}`;
    const rowId = `#session-row-${userId}`;
    const $row = $(rowId);
    const $container = $(containerId);

    if ($row.hasClass("hidden")) {
        $container.html(`<div class="text-sm text-gray-500">Yükleniyor...</div>`);

        $.get(`/Admin/UserSessions/${userId}`, function (html) {
            $container.html(html);
        }).fail(function () {
            $container.html(`<div class="text-sm text-red-500">Oturumlar yüklenemedi.</div>`);
        });

        $row.removeClass("hidden");
        button.innerHTML = `<i class="fas fa-chevron-up"></i>`;
    } else {
        $row.addClass("hidden");
        button.innerHTML = `<i class="fas fa-desktop"></i>`;
    }
}

function terminateSession(userId, sessionKey) {
    if (!confirm("Bu oturumu sonlandırmak istiyor musunuz?")) return;

    showLoading();

    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    $.ajax({
        type: "POST",
        url: "/Admin/Session/Terminate",
        data: {
            UserId: userId,
            SessionKey: sessionKey
        },
        headers: {
            "RequestVerificationToken": token
        },
        success: function () {
            location.reload();
        },
        error: function (xhr) {
            alert(xhr.responseText || "Oturum sonlandırılamadı.");
        },
        complete: function () {
            hideLoading();
        }
    });
}

function terminateAllSessions(userId) {
    if (!confirm("Bu kullanıcıya ait tüm oturumları sonlandırmak istiyor musunuz?")) return;

    showLoading();

    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    $.ajax({
        type: "POST",
        url: "/Admin/Session/TerminateAll",
        data: {
            UserId: userId
        },
        headers: {
            "RequestVerificationToken": token
        },
        success: function () {
            location.reload();
        },
        error: function (xhr) {
            alert(xhr.responseText || "Tüm oturumlar sonlandırılamadı.");
        },
        complete: function () {
            hideLoading();
        }
    });
}

// ------------------- Sayfa Yüklenince -------------------

document.addEventListener('DOMContentLoaded', function () {
    const editForm = document.getElementById('editUserForm');
    if (editForm) editForm.addEventListener('submit', handleEditUserSubmit);

    const newForm = document.getElementById('newUserForm');
    if (newForm) newForm.addEventListener('submit', handleNewUserSubmit);

    attachSearchFilter("user-search", "#userContainer", "#userCard");
});
