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
$(document).off("click", "#confirm-delete-yes").on("click", "#confirm-delete-yes", function () {
    const token = $('input[name="__RequestVerificationToken"]').val();
    const locationId = $("#location-id").val();

    $.ajax({
        type: "POST",
        url: "/Admin/Location/Worker/Delete",
        data: { id: pendingDeleteId },
        headers: { "RequestVerificationToken": token },
        success: function (msg) {
            loadWorkers(locationId, msg); // Çalışanlar yeniden yüklenecek
        },
        error: function (xhr) {
            hideModal("confirm-delete-modal");
            msg = xhr.responseText || "Çalışan silinemedi.";
            setTimeoutAlert("e", "#location-worker-alert", msg);
        }
    });

    hideModal("confirm-delete-modal");
});


// Çalışan Ekleme
$(document).off("submit", "#worker-add-form").on("submit", "#worker-add-form", function (e) {
    e.preventDefault();

    const token = $('input[name="__RequestVerificationToken"]').val();
    const locationId = $("#location-id").val();
    const $form = $(this);

    $.ajax({
        type: "POST",
        url: "/Admin/Location/Worker/Add",
        data: $form.serialize(),
        headers: { "RequestVerificationToken": token },
        success: function (response) {
            hideModal("add-worker-modal");
            $form[0].reset();
            loadWorkers(locationId, response); // Burada async/await kullanmıyoruz, loadWorkers direkt çağrılabilir.
        },
        error: function (xhr) {
            hideModal("add-worker-modal");
            setTimeoutAlert("e", "#location-worker-alert", xhr.responseText || "Çalışan eklenemedi.");
        }
    });
});


// AJAX ile Çalışanları Yeniden Yükleme
function loadWorkers(locationId, msg = null) {
    const $target = $("#location-operation-panel");
    const $loader = $("#location-operation-loader");

    $loader.text("Çalışanlar yükleniyor...");
    $target.empty();

    $.get(`/Admin/Location/GetWorkersByLocation/${locationId}`, function (html) {
        $target.html(html);
        $loader.text("");
        if (msg) setTimeoutAlert("s", "#location-worker-alert", msg);
        setTimeout(() => {
            document.getElementById("location-operation-panel")?.scrollIntoView({ behavior: "smooth" });
        }, 100);
    }).fail(function () {
        $loader.text("Çalışanlar yüklenemedi.");
    });
}
