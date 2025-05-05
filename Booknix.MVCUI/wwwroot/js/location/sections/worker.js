window.pendingDeleteId = window.pendingDeleteId || null;

const $ctx = $("#location-meta");
const baseUrl = $ctx.data("base-url");
const locationId = $ctx.data("location-id");

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
    $("#edit-worker-email").val(email);
    $("#edit-worker-role").val(roleValue).data("original", roleValue);

    var msg = "E-posta adresi buradan değiştirilemez. Kullanıcı kendi hesabı üzerinden güncelleme yapabilir veya sistem yetkilisine başvurmalıdır."

    setTimeoutAlert("i", "#edit-worker-info", msg, 0)
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
        url: `${baseUrl}/Worker/Delete`,
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
        url: `${baseUrl}/Worker/Add`,
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

    const currentRole = $("#edit-worker-role").val();
    const originalRole = $("#edit-worker-role").data("original");

    if (currentFullName === originalFullName && currentRole === originalRole) {
        setTimeoutAlert("e", "#edit-worker-alert", "Değişiklik tespit edilmediği için işlem yapılmadı.");
        return;
    }

    // Kaydediliyor efekti
    $submitButton.prop("disabled", true).text("Kaydediliyor...");

    $.ajax({
        type: "POST",
        url: `${baseUrl}/Worker/Edit`,
        data: $form.serialize(),
        success: function (response) {
            hideModal("edit-worker-modal");
            $form[0].reset();
            loadWorkers(locationId, response);
        },
        error: function (xhr) {
            hideModal("edit-worker-modal");
            setTimeoutAlert("e", "#location-worker-alert", xhr.responseText || "Çalışan güncellenemedi.");
        },
        complete: function () {
            // Butonu eski haline getir
            $submitButton.prop("disabled", false).text("Kaydet");
        }
    });
});



// AJAX ile Çalışanları Yeniden Yükleme
function loadWorkers(locationId, msg = null) {
    const $target = $("#location-operation-panel");
    const $loader = $("#location-operation-loader");

    $loader.text("Çalışanlar yükleniyor...");
    $target.empty();

    $.get(`${baseUrl}/GetWorkersByLocation/${locationId}`, function (html) {
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
