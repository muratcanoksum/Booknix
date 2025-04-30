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

    let roleValue = "LocationEmployee"; // LocationAdmin sadece çalışan atayabilir

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
        url: `/LocationAdmin/Worker/Delete/${pendingDeleteId}`,
        headers: { "RequestVerificationToken": token },
        success: function (msg) {
            loadWorkers(locationId, msg);
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
    const $submitButton = $form.find("button[type='submit']");
    const token = $('input[name="__RequestVerificationToken"]').val();

    // Kaydediliyor efekti
    $submitButton.prop("disabled", true).text("Kaydediliyor...");

    $.ajax({
        type: "POST",
        url: "/LocationAdmin/Worker/Add",
        data: $form.serialize(),
        headers: {
            "RequestVerificationToken": token
        },
        success: function (response) {
            hideModal("add-worker-modal");
            $form[0].reset();
            
            // Önce çalışanları yükle, sonra mesajı göster
            $.get(`/LocationAdmin/Workers`, function (html) {
                $("#location-operation-panel").html(html);
                setTimeoutAlert("s", "#location-worker-alert", response);
            });
        },
        error: function (xhr) {
            let errorMessage = "Çalışan eklenirken bir hata oluştu.";
            
            if (xhr.responseText) {
                if (xhr.responseText.includes("başka bir lokasyonda çalışıyor")) {
                    errorMessage = "Bu kullanıcı başka bir lokasyonda çalışıyor. Aynı kullanıcı birden fazla lokasyonda çalışamaz.";
                } else {
                    errorMessage = xhr.responseText;
                }
            }
            
            setTimeoutAlert("e", "#location-worker-alert", errorMessage);
        },
        complete: function () {
            // Butonu eski haline getir
            $submitButton.prop("disabled", false).text("Kaydet");
            hideModal("add-worker-modal");
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

    if (currentFullName === originalFullName) {
        setTimeoutAlert("e", "#edit-worker-alert", "Değişiklik tespit edilmediği için işlem yapılmadı.");
        return;
    }

    // Kaydediliyor efekti
    $submitButton.prop("disabled", true).text("Kaydediliyor...");

    $.ajax({
        type: "POST",
        url: "/LocationAdmin/Worker/Edit",
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

    $.get(`/LocationAdmin/Workers`, function (html) {
        $target.html(html);
        $loader.text("");
        if (msg) {
            setTimeoutAlert("s", "#location-worker-alert", msg);
        }
    }).fail(function () {
        $loader.text("Çalışanlar yüklenemedi.");
        setTimeoutAlert("e", "#location-worker-alert", "Çalışanlar yüklenirken bir hata oluştu.");
    });
}

// Çalışan Arama İşlevi
function filterWorkers() {
    const searchText = $("#worker-search").val().toLowerCase();
    $(".worker-item").each(function() {
        const $row = $(this);
        const searchData = $row.data("search");
        if (searchData.includes(searchText)) {
            $row.show();
        } else {
            $row.hide();
        }
    });
}

// Event Listeners
$(document).ready(function () {
    // Çalışanları Göster butonuna tıklandığında
    $(document).on("click", ".operation-tab[data-url*='Workers']", function (e) {
        e.preventDefault();
        loadWorkers();
    });

    // Çalışan arama
    $(document).on("keyup", "#worker-search", filterWorkers);
}); 