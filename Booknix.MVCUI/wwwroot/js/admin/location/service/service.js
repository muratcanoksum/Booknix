
$(document).off("click", "#show-add-service-modal").on("click", "#show-add-service-modal", () => showModal("add-service-modal"));

$(document).off("click", "#hide-add-service-modal").on("click", "#hide-add-service-modal", () => hideModal("add-service-modal"));

$(document).off("submit", "#service-add-form").on("submit", "#service-add-form", function (e) {
    e.preventDefault();

    const hour = $("#hour").val().toString().padStart(2, '0');
    const minute = $("#minute").val().toString().padStart(2, '0');
    $("#duration-input").val(`${hour}:${minute}`);

    const $form = $(this);
    const $btn = $form.find("button[type='submit']");
    const locationId = $("#location-id").val();
    const token = $('input[name="__RequestVerificationToken"]').val();

    $btn.prop("disabled", true).html('<i class="fa fa-spinner fa-spin"></i> Kaydediliyor...');

    $.ajax({
        type: "POST",
        url: "/Admin/Location/Service/Add",
        data: $form.serialize(),
        headers: { "RequestVerificationToken": token },
        success: function (msg) {
            $form[0].reset();
            hideModal("add-service-modal");
            loadServices(locationId, msg);
        },
        error: function (xhr) {
            var msg = xhr.responseText || "Servis eklenemedi.";
            setTimeoutAlert("e", "#location-service-add-alert", msg)
        },
        complete: function () {
            $btn.prop("disabled", false).html('Kaydet');
        }
    });
});

function toggleWorkers(serviceId) {
    toggleElementById(`workers-${serviceId}`);
}

function loadServices(locationId, msg = null) {
    const $target = $("#location-operation-panel");
    const $loader = $("#location-operation-loader");

    $loader.text("Servisler yükleniyor...");
    $target.empty();

    $.get(`/Admin/Location/GetServicesByLocation/${locationId}`, function (html) {
        $target.html(html);
        $loader.text("");
        if (msg) setTimeoutAlert("s", "#location-service-alert", msg);
        setTimeout(() => {
            document.getElementById("location-operation-panel")?.scrollIntoView({ behavior: "smooth" });
        }, 100);
    }).fail(function () {
        $loader.text("Servisler yüklenemedi.");
    });
}
