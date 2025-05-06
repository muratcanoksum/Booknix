// Profil fotoğrafını güncelleme işlemi    -- sorun var
$(document).off("submit", "#profile-photo-form").on("submit", "#profile-photo-form", function (e) {
    e.preventDefault();

    const $form = $(this);
    const $btn = $form.find("button[type='submit']");
    const token = $form.find('input[name="__RequestVerificationToken"]').val();

    $btn.prop("disabled", true).text("Yükleniyor...");

    var formData = new FormData($form[0]); // FormData ile dosya yükleme

    $.ajax({
        type: "POST",
        url: "/Account/ProfilePhoto",
        data: formData,
        contentType: false,
        processData: false,
        headers: {
            "RequestVerificationToken": token
        },
        success: function (msg) {
            setTimeoutAlert("s", "#profile-photo-alert", "Profil fotoğrafınız başarıyla güncellendi.");
            if (msg) {
                $("img[alt='Profil Fotoğrafı']").attr("src", msg);
            }
        },
        error: function (xhr) {
            const msg = xhr.responseText || "Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyiniz veya sistem yöneticisine başvurunuz..";
            setTimeoutAlert("e", "#profile-photo-alert", msg);
        },
        complete: function () {
            $btn.prop("disabled", false).text("Güncelle");
        }
    });
});