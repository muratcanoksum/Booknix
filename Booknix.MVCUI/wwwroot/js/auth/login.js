$("#login-form").on("submit", function (e) {
    e.preventDefault();

    const $btn = $(this).find("button[type='submit']");
    const $alert = $("#login-alert");

    $("#success-message").remove();

    $btn.prop("disabled", true).text("Yükleniyor...");

    $.ajax({
        type: "POST",
        url: "/Auth/Login",
        data: $(this).serialize(),
        success: function (redirectUrl) {
            window.location.href = redirectUrl;
        },
        error: function (xhr) {
            const msg = xhr.responseText || "Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyiniz veya sistem yöneticisine başvurunuz.";
            setTimeoutAlert("e", "#login-alert", msg, 30)
        },
        complete: function () {
            $btn.prop("disabled", false).text("Giriş Yap");
        }
    });
});

$(function () {
    // URL'deki hash kısmını alıyoruz
    var hash = window.location.hash;

    if (hash) {
        // Eğer hash varsa, returnUrl'yi gizli inputtan alıyoruz
        var returnUrl = $("#returnUrl").val();

        // returnUrl'nin sonuna hash'i ekliyoruz
        $("#returnUrl").val(returnUrl + hash);
    }
});

// ✅ 10 saniye sonra TempData Success mesajını gizle
setTimeout(() => {
    const $success = $("#success-message");
    if ($success.length) {
        $success.fadeOut("slow", function () {
            $(this).remove();
        });
    }
}, 10000);
