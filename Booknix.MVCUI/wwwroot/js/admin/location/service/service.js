
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
        success: function () {
            $form[0].reset();
            hideModal("add-service-modal");
            loadServices(locationId);
        },
        error: function (xhr) {
            alert(xhr.responseText || "Servis eklenemedi.");
        },
        complete: function () {
            $btn.prop("disabled", false).html('Kaydet');
        }
    });
});

function toggleWorkers(serviceId) {
    toggleElementById(`workers-${serviceId}`);
}

function loadServices(locationId) {
    const $target = $("#location-operation-panel");
    $target.html(`<div class="text-sm text-gray-500 flex items-center gap-2"><i class="fa fa-spinner fa-spin"></i> Yükleniyor...</div>`);

    $.get(`/Admin/Location/GetServicesByLocation/${locationId}`, html => {
        $target.html(html);
    }).fail(() => {
        $target.html(`<div class="text-sm text-red-600 mt-2"><i class="fa fa-circle-exclamation"></i> Servis bilgileri yüklenemedi.</div>`);
    });
}
