$(document).off("submit", "#location-edit-form").on("submit", "#location-edit-form", function (e) {
    e.preventDefault();

    const $form = $(this);
    const $btn = $form.find("button[type='submit']");
    const $alert = $("#location-edit-alert");
    const token = $form.find('input[name="__RequestVerificationToken"]').val();

    $btn.prop("disabled", true).text("Güncelleniyor...");

    $.ajax({
        type: "POST",
        url: "/Admin/EditLocation",
        data: $form.serialize(),
        headers: {
            "RequestVerificationToken": token
        },
        success: function () {
            window.location.href = "/Admin/Location";
        },
        error: function (xhr) {
            setTimeoutAlert("e", "#location-edit-alert", xhr.responseText || "Hata oluştu.");
        },
        complete: function () {
            $btn.prop("disabled", false).text("Güncelle");
        }
    });
});
