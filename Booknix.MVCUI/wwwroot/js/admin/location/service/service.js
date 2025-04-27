$(document).off("click", "#show-add-service-modal").on("click", "#show-add-service-modal", () => {
    resetServiceForm();
    $("#service-modal-title").text("Yeni Servis Ekle");
    $("#service-submit-button").text("Kaydet");
    $("#service-add-form").attr("data-mode", "add");
    showModal("add-service-modal");
});

$(document).off("click", "#hide-add-service-modal").on("click", "#hide-add-service-modal", () => hideModal("add-service-modal"));

// Edit Service Button Click
$(document).off("click", ".edit-service").on("click", ".edit-service", function() {
    const serviceId = $(this).data("service-id");
    loadServiceForEdit(serviceId);
});

function loadServiceForEdit(serviceId) {
    $.get(`/Admin/Location/Service/Get/${serviceId}`, function(html) {
        // Partial view HTML yanıtını işle
        const $container = $(html);
        const $formData = $container.find(".form-data");
        
        // Gizli alanlardan veri çek
        const id = $container.find("#service-id").val();
        const locationId = $container.find("#service-location-id").val();
        
        // Form verilerini doldur
        $("#service-id").val(id);
        $("#service-name").val($formData.attr("data-name"));
        $("#service-description").val($formData.attr("data-description"));
        $("#service-price").val($formData.attr("data-price"));
        
        // Duration'ı parse et
        const durationString = $formData.attr("data-duration");
        const duration = parseTimeSpan(durationString);
        $("#hour").val(duration.hours);
        $("#minute").val(duration.minutes);
        
        // Update duration input for form submission
        const hour = String(duration.hours).padStart(2, '0');
        const minute = String(duration.minutes).padStart(2, '0');
        $("#duration-input").val(`${hour}:${minute}`);
        
        // Reset all worker checkboxes
        $(".worker-checkbox").prop("checked", false);
        
        // Check the appropriate worker checkboxes
        $container.find(".selected-worker-id").each(function() {
            const workerId = $(this).val();
            $(`#worker-${workerId}`).prop("checked", true);
        });
        
        // Update modal title and button
        $("#service-modal-title").text("Servisi Düzenle");
        $("#service-submit-button").text("Güncelle");
        $("#service-add-form").attr("data-mode", "edit");
        
        // Show the modal
        showModal("add-service-modal");
    }).fail(function(xhr) {
        setTimeoutAlert("e", "#location-service-alert", "Servis bilgileri yüklenemedi.");
    });
}

function parseTimeSpan(timeSpanString) {
    // Expected format: "hh:mm:ss"
    const parts = timeSpanString.split(':');
    return {
        hours: parseInt(parts[0], 10),
        minutes: parseInt(parts[1], 10)
    };
}

function resetServiceForm() {
    $("#service-id").val("");
    $("#service-name").val("");
    $("#service-description").val("");
    $("#service-price").val("");
    $("#hour").val(0);
    $("#minute").val(0);
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
    const mode = $form.attr("data-mode") || "add";

    $btn.prop("disabled", true).html('<i class="fa fa-spinner fa-spin"></i> Kaydediliyor...');

    // Determine URL based on mode
    const url = mode === "edit" ? "/Admin/Location/Service/Update" : "/Admin/Location/Service/Add";

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
            var msg = xhr.responseText || (mode === "edit" ? "Servis güncellenemedi." : "Servis eklenemedi.");
            setTimeoutAlert("e", "#location-service-add-alert", msg)
        },
        complete: function () {
            $btn.prop("disabled", false).html(mode === "edit" ? 'Güncelle' : 'Kaydet');
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

// Delete Service Button Click
$(document).off("click", ".delete-service").on("click", ".delete-service", function() {
    const serviceId = $(this).data("service-id");
    const serviceName = $(this).closest("tr").find("td:first").text();
    
    if (confirm(`"${serviceName}" servisini silmek istediğinize emin misiniz? Bu işlem geri alınamaz.`)) {
        deleteService(serviceId);
    }
});

function deleteService(serviceId) {
    const locationId = $("#location-id").val();
    const token = $('input[name="__RequestVerificationToken"]').val();
    
    $.ajax({
        type: "POST",
        url: `/Admin/Location/Service/Delete/${serviceId}`,
        headers: { "RequestVerificationToken": token },
        success: function (msg) {
            loadServices(locationId, msg);
        },
        error: function (xhr) {
            const msg = xhr.responseText || "Servis silinemedi.";
            setTimeoutAlert("e", "#location-service-alert", msg);
        }
    });
}

// Remove Worker Button Click
$(document).off("click", ".remove-worker").on("click", ".remove-worker", function() {
    const workerId = $(this).data("worker-id");
    const serviceId = $(this).data("service-id");
    const workerName = $(this).closest("div").find("span").text();
    
    if (confirm(`"${workerName}" çalışanını bu servisten kaldırmak istediğinize emin misiniz?`)) {
        removeWorkerFromService(workerId, serviceId);
    }
});

function removeWorkerFromService(workerId, serviceId) {
    const locationId = $("#location-id").val();
    const token = $('input[name="__RequestVerificationToken"]').val();
    
    $.ajax({
        type: "POST",
        url: "/Admin/Location/Service/RemoveWorker",
        data: { 
            workerId: workerId, 
            serviceId: serviceId 
        },
        headers: { "RequestVerificationToken": token },
        success: function (msg) {
            // Başarılı olduğunda sadece ilgili çalışanı kaldır
            $(`[data-worker-id="${workerId}"][data-service-id="${serviceId}"]`).closest("div").fadeOut(function() {
                $(this).remove();
                
                // Eğer son çalışan kaldırıldıysa "Atanmış çalışan yok" mesajını göster
                const $workerList = $(`#workers-${serviceId} .space-y-2`);
                if ($workerList.children().length === 0) {
                    $workerList.html('<div class="text-gray-500 italic">Atanmış çalışan yok.</div>');
                }
                
                setTimeoutAlert("s", "#location-service-alert", msg);
            });
        },
        error: function (xhr) {
            const msg = xhr.responseText || "Çalışan servisten kaldırılamadı.";
            setTimeoutAlert("e", "#location-service-alert", msg);
        }
    });
}
