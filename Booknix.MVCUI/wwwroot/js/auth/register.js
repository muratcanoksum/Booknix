$("#register-form").on("submit", function (e) {
    e.preventDefault();

    const $btn = $(this).find("button[type='submit']");
    $btn.prop("disabled", true).text("Yükleniyor...");

    $.ajax({
        type: "POST",
        url: "/Auth/Register",
        data: $(this).serialize(),
        success: function () {
            window.location.href = "/Auth/Login";
        },
        error: function (xhr) {
            const msg = xhr.responseText || "Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyiniz veya sistem yöneticisine başvurunuz.";
            setTimeoutAlert("e", "#register-alert", msg, 10)
        },
        complete: function () {
            $btn.prop("disabled", false).text("Kayıt Ol");
        }
    });
});
