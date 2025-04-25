//Hesap şifresi değiştirme
$(document).off("submit", "#reset-password-form").on("submit", "#reset-password-form", function (e) {
    e.preventDefault();

    const $form = $(this);
    const $btn = $form.find("button[type='submit']");
    const token = $form.find('input[name="__RequestVerificationToken"]').val();

    $btn.prop("disabled", true).text("Kaydediliyor...");

    $.ajax({
        type: "POST",
        url: "/Account/ChangePassword",  // POST işlemi yapılacak URL
        data: $form.serialize(),
        headers: {
            "RequestVerificationToken": token
        },
        success: function (msg) {
            setTimeoutAlert("s", "#profile-password-alert", msg);
            $form[0].reset();
        },
        error: function (xhr) {
            const msg = xhr.responseText || "Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyiniz veya sistem yöneticisine başvurunuz..";
            setTimeoutAlert("e", "#profile-password-alert", msg);
        },
        complete: function () {
            $btn.prop("disabled", false).text("Şifreyi Güncelle");
        }
    });
});