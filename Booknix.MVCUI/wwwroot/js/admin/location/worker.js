window.pendingDeleteId = window.pendingDeleteId || null;

// Çalışan Ekleme Modali
$(document).off("click", "#show-add-worker-modal").on("click", "#show-add-worker-modal", () => showModal("add-worker-modal"));

$(document).off("click", "#hide-add-worker-modal").on("click", "#hide-add-worker-modal", () => hideModal("add-worker-modal"));

// Silme Onay Modali Açma
$(document).off("click", "#delete-worker-btn").on("click", "#delete-worker-btn", function () {
    pendingDeleteId = $(this).data("id");
    showModal("confirm-delete-modal");
});

// Silme Onay Modali Kapatma (Hayır)
$(document).off("click", "#confirm-delete-no").on("click", "#confirm-delete-no", function () {
    hideModal("confirm-delete-modal");
});

// Çalışanı Silme (Evet)
$(document).off("click", "#confirm-delete-yes").on("click", "#confirm-delete-yes", async function () {
    const token = $('input[name="__RequestVerificationToken"]').val();
    const locationId = $("#location-id").val();

    try {
        await $.ajax({
            type: "POST",
            url: "/Admin/Location/Worker/Delete",
            data: { id: pendingDeleteId },
            headers: { "RequestVerificationToken": token }
        });

        await loadWorkers(locationId);
        setTimeoutAlert("#worker-alert-success", "Çalışan başarıyla silindi.");
    } catch {
        setTimeoutAlert("#worker-alert-error", "Çalışan silinemedi.");
    }

    hideModal("confirm-delete-modal");
});

// Çalışan Ekleme
$(document).off("submit", "#worker-add-form").on("submit", "#worker-add-form", async function (e) {
    e.preventDefault();

    const token = $('input[name="__RequestVerificationToken"]').val();
    const locationId = $("#location-id").val();
    const $form = $(this);

    try {
        const response = await $.ajax({
            type: "POST",
            url: "/Admin/Location/Worker/Add",
            data: $form.serialize(),
            headers: { "RequestVerificationToken": token }
        });

        hideModal("add-worker-modal");
        $form[0].reset();
        await loadWorkers(locationId);
        setTimeoutAlert("#worker-alert-success", response);
    } catch (xhr) {
        hideModal("add-worker-modal");
        setTimeoutAlert("#worker-alert-error", xhr.responseText || "Çalışan eklenemedi.");
    }
});

// AJAX ile Çalışanları Yeniden Yükleme
async function loadWorkers(locationId) {
    const $target = $("#location-operation-panel");
    const $loader = $("#location-operation-loader");

    $loader.text("Çalışanlar yükleniyor...");
    $target.empty();

    try {
        const html = await $.get(`/Admin/Location/GetWorkersByLocation/${locationId}`);
        $target.html(html);
        $loader.text("");
    } catch {
        $loader.text("Çalışanlar yüklenemedi.");
    }
}
