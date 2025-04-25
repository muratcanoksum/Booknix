// Hesabı doğrulama kodu gönderme işlemi
$(document).off("submit", "#start-delete-form").on("submit", "#start-delete-form", function (e) {
    e.preventDefault();

    const $form = $(this);
    const $btn = $form.find("button[type='submit']");
    const token = $form.find('input[name="__RequestVerificationToken"]').val();

    $btn.prop("disabled", true).text("Yükleniyor...");

    $.ajax({
        type: "POST",
        url: "/Account/Delete",  // POST işlemi yapılacak URL
        data: $form.serialize(),
        headers: {
            "RequestVerificationToken": token
        },
        success: function (msg) {
            setTimeoutAlert("#profile-delete-alert-success", msg, 900)
            $("#delete-account-info").addClass("hidden");
            $("#delete-account-verify-form").removeClass("hidden");
        },
        error: function (xhr) {
            const msg = xhr.responseText || "Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyiniz veya sistem yöneticisine başvurunuz..";
            setTimeoutAlert("#profile-delete-alert-error", msg, 8)
        },
        complete: function () {
            $btn.prop("disabled", false).text("Devam Et ve Kodu Gönder");
        }
    });
});

// Hesabı silme doğrulama
$(document).off("submit", "#delete-account-verify-form").on("submit", "#delete-account-verify-form", function (e) {
    e.preventDefault();

    const $form = $(this);
    const $btn = $form.find("button[type='submit']");
    const token = $form.find('input[name="__RequestVerificationToken"]').val();

    $btn.prop("disabled", true).text("Yükleniyor...");

    $.ajax({
        type: "POST",
        url: "/Account/DeleteVerify",  // POST işlemi yapılacak URL
        data: $form.serialize(),
        headers: {
            "RequestVerificationToken": token
        },
        success: function (msg) {
            window.location.href = "/Auth/Logout";
        },
        error: function (xhr) {
            const msg = xhr.responseText || "Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyiniz veya sistem yöneticisine başvurunuz..";
            setTimeoutAlert("#profile-delete-alert-error", msg, 8)
        },
        complete: function () {
            $btn.prop("disabled", false).text("Hesabı Kalıcı Olarak Sil");
        }
    });
});