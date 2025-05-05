window.deleteAction = window.deleteAction || null;


$(document).off("click", "#show-add-service-modal").on("click", "#show-add-service-modal", () => {
    resetServiceForm();
    $("#service-modal-title").text("Yeni Servis Ekle");
    $("#service-submit-button").text("Kaydet");
    $("#service-add-form").attr("data-mode", "add");
    showModal("add-service-modal");
});

$(document).off("click", "#hide-add-service-modal").on("click", "#hide-add-service-modal", () => hideModal("add-service-modal"));

$(document).off("click", ".edit-service").on("click", ".edit-service", function () {
    const serviceId = $(this).data("service-id");
    $.get(`${baseUrl}/Service/Get/${serviceId}`, function (html) {
        const $container = $(html);
        const $formData = $container.find(".form-data");

        $("#service-id").val($container.find("#service-id").val());
        $("#service-name").val($formData.data("name"));
        $("#service-description").val($formData.data("description"));
        $("#service-price").val($formData.data("price"));

        const [h, m] = $formData.data("duration").split(":");
        $("#hour").val(parseInt(h));
        $("#minute").val(parseInt(m));
        $("#duration-input").val(`${h}:${m}`);

        const [gh, gm] = $formData.data("servicegap").split(":");
        $("#service-gap").val((parseInt(gh) * 60) + parseInt(gm));

        $(".worker-checkbox").prop("checked", false);
        $container.find(".selected-worker-id").each(function () {
            $(`#worker-${$(this).val()}`).prop("checked", true);
        });

        $("#service-modal-title").text("Servisi Düzenle");
        $("#service-submit-button").text("Güncelle");
        $("#service-add-form").attr("data-mode", "edit");

        showModal("add-service-modal");
    }).fail(() => {
        setTimeoutAlert("e", "#location-service-alert", "Servis bilgileri yüklenemedi.");
    });
});

function resetServiceForm() {
    $("#service-id, #service-name, #service-description, #service-price").val("");
    $("#hour, #minute").val(0);
    $("#duration-input").val("00:00");
    $(".worker-checkbox").prop("checked", false);
}

$(document).off("submit", "#service-add-form").on("submit", "#service-add-form", function (e) {
    e.preventDefault();

    const hour = $("#hour").val().toString().padStart(2, '0');
    const minute = $("#minute").val().toString().padStart(2, '0');
    $("#duration-input").val(`${hour}:${minute}`);

    const $form = $(this);
    const $btn = $form.find("button[type='submit']");
    const locationId = $("#location-id").val();
    const token = $('input[name="__RequestVerificationToken"]').val();
    const mode = $form.data("mode");

    $btn.prop("disabled", true).html('<i class="fa fa-spinner fa-spin"></i> Kaydediliyor...');

    const url = mode === "edit" ? `${baseUrl}/Service/Update` : `${baseUrl}/Service/Add`;

    $.ajax({
        type: "POST",
        url: url,
        data: $form.serialize(),
        headers: { "RequestVerificationToken": token },
        success: function (msg) {
            $form[0].reset();
            hideModal("add-service-modal");
            loadServices(locationId, msg);
        },
        error: function (xhr) {
            setTimeoutAlert("e", "#location-service-add-alert", xhr.responseText || "İşlem başarısız.");
        },
        complete: function () {
            $btn.prop("disabled", false).html(mode === "edit" ? 'Güncelle' : 'Kaydet');
        }
    });
});

function loadServices(locationId, msg = null) {
    const $target = $("#location-operation-panel");
    const $loader = $("#location-operation-loader");

    const height = $target.outerHeight();
    $target.css("min-height", height + "px");
    $loader.text("Servisler yükleniyor...");
    $target.empty();

    $.get(`${baseUrl}/GetServicesByLocation/${locationId}`, function (html) {
        $target.html(html);
        $loader.text("");
        $target.css("min-height", "");
        if (msg) setTimeoutAlert("s", "#location-service-alert", msg);
        setTimeout(() => {
            document.getElementById("location-operation-panel")?.scrollIntoView({ behavior: "smooth" });
        }, 100);
    }).fail(() => {
        $target.css("min-height", "");
        $loader.text("Servisler yüklenemedi.");
        setTimeoutAlert("e", "#location-service-alert", "Servisler yüklenemedi.");
    });
}

$(document).off("click", ".delete-service").on("click", ".delete-service", function () {
    const serviceId = $(this).data("service-id");
    const serviceName = $(this).closest("tr").find("td:first").text();

    $("#confirm-delete-modal h2").text("Servisi Sil");
    $("#confirm-delete-modal p").text(`"${serviceName}" servisini silmek istediğinize emin misiniz?`);
    showModal("confirm-delete-modal");

    deleteAction = () => deleteService(serviceId);
});

function deleteService(serviceId) {
    const locationId = $("#location-id").val();
    const token = $('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        type: "POST",
        url: `${baseUrl}/Service/Delete/${serviceId}`,
        headers: { "RequestVerificationToken": token },
        success: (msg) => loadServices(locationId, msg),
        error: (xhr) => setTimeoutAlert("e", "#location-service-alert", xhr.responseText || "Servis silinemedi.")
    });
}

$(document).off("click", ".remove-worker").on("click", ".remove-worker", function () {
    const $card = $(this).closest(".flex");
    const workerId = $(this).data("worker-id");
    const serviceId = $(this).data("service-id");
    const workerName = $card.find(".font-medium").text();

    $("#confirm-delete-modal h2").text("Çalışanı Servisten Kaldır");
    $("#confirm-delete-modal p").text(`"${workerName}" çalışanını bu servisten kaldırmak istediğinize emin misiniz?`);
    showModal("confirm-delete-modal");

    deleteAction = () => removeWorkerFromService(workerId, serviceId, $card);
});

function removeWorkerFromService(workerId, serviceId, $card) {
    const locationId = $("#location-id").val();
    const token = $('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        type: "POST",
        url: `${baseUrl}/Service/RemoveWorker`,
        data: { workerId, serviceId },
        headers: { "RequestVerificationToken": token },
        success: function (msg) {
            $(`[data-worker-id="${workerId}"][data-service-id="${serviceId}"]`).closest("div").fadeOut(function () {
                $(this).remove();
                const $list = $(`#workers-${serviceId} .space-y-2`);
                if ($list.children().length === 0) {
                    $list.html('<div class="text-gray-500 italic">Atanmış çalışan yok.</div>');
                }
                setTimeoutAlert("s", "#location-service-alert", msg);
            });
        },
        error: function (xhr) {
            setTimeoutAlert("e", "#location-service-alert", xhr.responseText || "Çalışan kaldırılamadı.");
        }
    });
}

$(document).off("click", "#confirm-delete-yes").on("click", "#confirm-delete-yes", function () {
    if (typeof deleteAction === "function") deleteAction();
    hideModal("confirm-delete-modal");
});

$(document).off("click", "#confirm-delete-no").on("click", "#confirm-delete-no", function () {
    hideModal("confirm-delete-modal");
    deleteAction = null;
});

function toggleWorkers(serviceId) {
    toggleElementById(`workers-${serviceId}`);
}
