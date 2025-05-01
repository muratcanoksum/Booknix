window.pendingDeleteId = window.pendingDeleteId || null;

// Çalışan Ekleme Modali
$(document).off("click", "#show-add-worker-modal").on("click", "#show-add-worker-modal", () => showModal("add-worker-modal"));

$(document).off("click", "#hide-add-worker-modal").on("click", "#hide-add-worker-modal", () => hideModal("add-worker-modal"));

// Çalışan Düzenle Modali
$(document).off("click", "#show-edit-worker-modal").on("click", "#show-edit-worker-modal", function () {
    const $tr = $(this).closest("tr");
    const id = $(this).data("id");
    const fullName = $tr.find("td").eq(0).text().trim();
    const email = $tr.find("td").eq(1).text().trim();
    const roleText = $tr.find("td").eq(2).text().trim();

    let roleValue = "";
    if (roleText === "Yönetici") {
        roleValue = "LocationAdmin";
    } else if (roleText === "Çalışan") {
        roleValue = "LocationEmployee";
    }

    // Inputlara yaz
    $("#edit-worker-id").val(id);
    $("#edit-worker-fullname").val(fullName).data("original", fullName);
    $("#edit-worker-email").val(email).data("original", email);
    $("#edit-worker-role").val(roleValue).data("original", roleValue);

    // Eskiden email değiştirilemez mesajı vardı, şimdi kaldırıyoruz
    $("#edit-worker-alert").addClass('hidden');
    
    showModal("edit-worker-modal");
});

$(document).off("click", "#hide-edit-worker-modal").on("click", "#hide-edit-worker-modal", () => hideModal("edit-worker-modal"));

// Silme Onay Modali Açma
$(document).off("click", "#delete-worker-btn").on("click", "#delete-worker-btn", function () {
    pendingDeleteId = $(this).data("id");
    showModal("confirm-delete-modal");
});

// Silme Onay Modali Kapatma (Hayır)
$(document).off("click", "#confirm-delete-no").on("click", "#confirm-delete-no", function () {
    hideModal("confirm-delete-modal");
});

// Çalışanı Silme (Evet)
$(document).off("click", "#confirm-delete-yes").on("click", "#confirm-delete-yes", function () {
    const token = $('input[name="__RequestVerificationToken"]').val();
    const locationId = $("#location-id").val();

    $.ajax({
        type: "POST",
        url: "/LocationAdmin/Worker/Delete",
        data: { id: pendingDeleteId },
        headers: { "RequestVerificationToken": token },
        success: function (msg) {
            loadWorkers(locationId, msg); // Çalışanlar yeniden yüklenecek
        },
        error: function (xhr) {
            hideModal("confirm-delete-modal");
            msg = xhr.responseText || "Çalışan silinemedi.";
            setTimeoutAlert("e", "#location-worker-alert", msg);
        }
    });

    hideModal("confirm-delete-modal");
});


// Çalışan Ekleme
$(document).off("submit", "#worker-add-form").on("submit", "#worker-add-form", function (e) {
    e.preventDefault();

    const locationId = $("#location-id").val();
    const $form = $(this);

    $.ajax({
        type: "POST",
        url: "/LocationAdmin/Worker/Add",
        data: $form.serialize(),
        success: function (response) {
            hideModal("add-worker-modal");
            $form[0].reset();
            loadWorkers(locationId, response); // Burada async/await kullanmıyoruz, loadWorkers direkt çağrılabilir.
        },
        error: function (xhr) {
            hideModal("add-worker-modal");
            setTimeoutAlert("e", "#location-worker-alert", xhr.responseText || "Çalışan eklenemedi.");
        }
    });
});

// Çalışan Düzenleme
$(document).off("submit", "#worker-edit-form").on("submit", "#worker-edit-form", function (e) {
    e.preventDefault();

    const locationId = $("#location-id").val();
    const $form = $(this);
    const $submitButton = $form.find("button[type='submit']");

    const currentFullName = $("#edit-worker-fullname").val().trim();
    const originalFullName = $("#edit-worker-fullname").data("original");

    const currentEmail = $("#edit-worker-email").val().trim();
    const originalEmail = $("#edit-worker-email").data("original");

    const currentRole = $("#edit-worker-role").val();
    const originalRole = $("#edit-worker-role").data("original");

    if (currentFullName === originalFullName && currentRole === originalRole && currentEmail === originalEmail) {
        setTimeoutAlert("e", "#edit-worker-alert", "Değişiklik tespit edilmediği için işlem yapılmadı.");
        return;
    }

    // Kaydediliyor efekti
    $submitButton.prop("disabled", true).text("Kaydediliyor...");

    // Email değişikliği mi kontrol et
    if (currentEmail !== originalEmail) {
        const workerId = $("#edit-worker-id").val();
        const token = $('input[name="__RequestVerificationToken"]').val();
        
        // Email değişikliği için ayrı bir istek yap
        $.ajax({
            type: "POST",
            url: "/LocationAdmin/Worker/UpdateEmail",
            data: { 
                workerId: workerId,
                newEmail: currentEmail
            },
            headers: { "RequestVerificationToken": token },
            success: function(response) {
                // Email başarıyla değiştirildi, şimdi diğer bilgileri güncelle
                updateWorkerOtherInfo($form, locationId, response);
            },
            error: function(xhr) {
                $submitButton.prop("disabled", false).text("Kaydet");
                setTimeoutAlert("e", "#edit-worker-alert", xhr.responseText || "E-posta adresi güncellenemedi.");
            }
        });
    } else {
        // Sadece diğer bilgileri güncelle
        updateWorkerOtherInfo($form, locationId);
    }
});

// Yardımcı fonksiyon - Diğer bilgileri güncelleme
function updateWorkerOtherInfo($form, locationId, emailSuccessMessage = null) {
    const currentFullName = $("#edit-worker-fullname").val().trim();
    const originalFullName = $("#edit-worker-fullname").data("original");
    const currentRole = $("#edit-worker-role").val();
    const originalRole = $("#edit-worker-role").data("original");
    const $submitButton = $form.find("button[type='submit']");
    
    // Eğer isim veya rol değişmişse, güncelle
    if (currentFullName !== originalFullName || currentRole !== originalRole) {
        $.ajax({
            type: "POST",
            url: "/LocationAdmin/Worker/Edit",
            data: $form.serialize(),
            success: function (response) {
                hideModal("edit-worker-modal");
                $form[0].reset();
                loadWorkers(locationId, emailSuccessMessage || response);
            },
            error: function (xhr) {
                hideModal("edit-worker-modal");
                setTimeoutAlert("e", "#location-worker-alert", xhr.responseText || "Çalışan güncellenemedi.");
            },
            complete: function () {
                $submitButton.prop("disabled", false).text("Kaydet");
            }
        });
    } else {
        // Eğer sadece email değişmişse ve başarılıysa
        if (emailSuccessMessage) {
            hideModal("edit-worker-modal");
            $form[0].reset();
            loadWorkers(locationId, emailSuccessMessage);
        }
        $submitButton.prop("disabled", false).text("Kaydet");
    }
}

// AJAX ile Çalışanları Yeniden Yükleme
function loadWorkers(locationId, msg = null) {
    const $target = $("#location-operation-panel");
    const $loader = $("#location-operation-loader");

    $loader.text("Çalışanlar yükleniyor...");
    $target.empty();

    $.get(`/LocationAdmin/GetWorkersByLocation/${locationId}`, function (html) {
        $target.html(html);
        $loader.text("");
        if (msg) setTimeoutAlert("s", "#location-worker-alert", msg);
        setTimeout(() => {
            document.getElementById("location-operation-panel")?.scrollIntoView({ behavior: "smooth" });
        }, 100);
    }).fail(function () {
        $loader.text("Çalışanlar yüklenemedi.");
    });
}

// Utility functions
function showModal(id) {
    document.getElementById(id).classList.add('flex');
    document.getElementById(id).classList.remove('hidden');
}

function hideModal(id) {
    document.getElementById(id).classList.add('hidden');
    document.getElementById(id).classList.remove('flex');
}

function setTimeoutAlert(type, selector, message, timeout = 5000) {
    const alertClass = type === 'e' 
        ? 'bg-red-100 border-red-400 text-red-700' 
        : type === 'i'
        ? 'bg-blue-100 border-blue-400 text-blue-700'
        : 'bg-green-100 border-green-400 text-green-700';
        
    const icon = type === 'e' 
        ? '<i class="fas fa-exclamation-circle mr-2"></i>' 
        : type === 'i'
        ? '<i class="fas fa-info-circle mr-2"></i>'
        : '<i class="fas fa-check-circle mr-2"></i>';
        
    const $alert = $(selector);
    $alert.removeClass('hidden bg-red-100 border-red-400 text-red-700 bg-green-100 border-green-400 text-green-700 bg-blue-100 border-blue-400 text-blue-700')
          .addClass(`${alertClass} border rounded-lg p-4 mb-4 flex items-start`)
          .html(`${icon} ${message}`);
          
    if (timeout > 0) {
        setTimeout(() => $alert.addClass('hidden'), timeout);
    }
} 