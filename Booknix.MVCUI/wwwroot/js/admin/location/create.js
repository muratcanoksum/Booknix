$(initCleavePhoneMask())

$(document).off("submit", "#location-create-form").on("submit", "#location-create-form", function (e) {
    e.preventDefault();

    const $form = $(this);
    const $btn = $form.find("button[type='submit']");
    const $alert = $("#location-create-form-alert");
    const token = $form.find('input[name="__RequestVerificationToken"]').val();

    $btn.prop("disabled", true).text("Kaydediliyor...");

    $.ajax({
        type: "POST",
        url: "/Admin/CreateLocation",
        data: $form.serialize(),
        headers: {
            "RequestVerificationToken": token
        },
        success: function () {
            window.location.href = "/Admin/Location";
        },
        error: function (xhr) {
            setTimeoutAlert("e", "#location-create-alert", xhr.responseText || "Hata oluştu.");
        },
        complete: function () {
            $btn.prop("disabled", false).text("Kaydet");
        }
    });
});
