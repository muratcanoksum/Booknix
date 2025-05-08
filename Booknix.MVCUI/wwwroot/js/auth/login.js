$("#login-form").on("submit", function (e) {
    e.preventDefault();

    const $btn = $(this).find("button[type='submit']");

    $("#success-message").remove();

    $btn.prop("disabled", true).text("Yükleniyor...");

    // ✅ Force serialize güncellemesi için tüm inputlara change tetikle

    $.ajax({
        type: "POST",
        url: "/Auth/Login",
        data: $(this).serialize(),
        success: function (redirectUrl) {
            window.location.href = redirectUrl;
        },
        error: function (xhr) {
            console.log(xhr)
            const msg = xhr.responseText || "Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyiniz veya sistem yöneticisine başvurunuz.";
            setTimeoutAlert("e", "#login-alert", msg, 30)

            // ✅ CSRF token yenile (örnek: formu yeniden al)
            $.get("/Auth/Login", function (html) {
                const newToken = $(html).find("input[name='__RequestVerificationToken']").val();
                $("input[name='__RequestVerificationToken']").val(newToken);
            });

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
