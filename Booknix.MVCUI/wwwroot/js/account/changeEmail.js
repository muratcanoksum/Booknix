// E-posta güncelleme işlemi
$(document).off("submit", "#change-email-form").on("submit", "#change-email-form", function (e) {
    e.preventDefault();

    const $form = $(this);
    const $verifyForm = $("#change-email-verify-form");
    const $btn = $form.find("button[type='submit']");
    const token = $form.find('input[name="__RequestVerificationToken"]').val();

    $btn.prop("disabled", true).text("Yükleniyor...");

    $.ajax({
        type: "POST",
        url: "/Account/ChangeEmail",  // POST işlemi yapılacak URL
        data: $form.serialize(),
        headers: {
            "RequestVerificationToken": token
        },
        success: function (msg) {
            setTimeoutAlert("s", "#profile-email-alert", msg, 900);
            $form.addClass("hidden");
            $verifyForm.removeClass("hidden");
        },
        error: function (xhr) {
            const msg = xhr.responseText || "Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyiniz veya sistem yöneticisine başvurunuz..";
            setTimeoutAlert("e", "#profile-email-alert", msg);
        },
        complete: function () {
            $btn.prop("disabled", false).text("Email Güncelle");
        }
    });
});

// E-posta doğrulama kodunu verify
$(document).off("submit", "#change-email-verify-form").on("submit", "#change-email-verify-form", function (e) {
    e.preventDefault();

    const $form = $(this);
    const $changeEmailForm = $("#change-email-form");
    const $btn = $form.find("button[type='submit']");
    const $alert = $("#change-email-verify-alert");
    const $alertmain = $("#change-email-alert");
    const token = $form.find('input[name="__RequestVerificationToken"]').val();

    $btn.prop("disabled", true).text("Yükleniyor...");

    $.ajax({
        type: "POST",
        url: "/Account/ChangeEmailVerify",  // POST işlemi yapılacak URL
        data: $form.serialize(),
        headers: {
            "RequestVerificationToken": token
        },
        success: function (msg) {
            // ✅ Aktif menü linkinin data-url'ini al
            const $activeLink = $(".menu-link.bg-blue-100");
            const url = $activeLink.data("url");

            if (url) {
                // AJAX ile yeniden yükle
                $("#content-panel").html("Yenileniyor...");

                $.get(url, function (html) {
                    $("#content-panel").html(html);
                    setTimeoutAlert("s", "#profile-email-alert", msg, 10);
                });
            }

        },
        error: function (xhr) {
            const msg = xhr.responseText || "Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyiniz veya sistem yöneticisine başvurunuz..";

            setTimeoutAlert("e", "#profile-email-alert", msg);

        },
        complete: function () {
            $btn.prop("disabled", false).text("Email Güncelle");
        }
    });
});