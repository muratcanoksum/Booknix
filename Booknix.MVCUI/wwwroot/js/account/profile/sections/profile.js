// Profil bilgilerini güncelleme
$(document).off("submit", "#profile-form").on("submit", "#profile-form", function (e) {
    e.preventDefault();

    const $form = $(this);
    const $btn = $form.find("button[type='submit']");
    const token = $form.find('input[name="__RequestVerificationToken"]').val();

    $btn.prop("disabled", true).text("Kaydediliyor...");
    $.ajax({
        type: "POST",
        url: "/Account/Profile",
        data: $form.serialize(),
        headers: {
            "RequestVerificationToken": token
        },
        success: function (msg) {
            setTimeoutAlert("s", "#profile-alert", msg);
        },
        error: function (xhr) {
            const msg = xhr.responseText || "Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyiniz veya sistem yöneticisine başvurunuz.";
            setTimeoutAlert("e", "#profile-alert", msg);

        },
        complete: function () {
            $btn.prop("disabled", false).text("Güncelle");
        }
    });
});