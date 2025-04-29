// Global fonksiyon: Formu duruma göre güncelle
function updateDayFormUI() {
    const isOnLeave = $("#is-on-leave").is(":checked");
    const isDayOff = $("#is-day-off").is(":checked");

    if (isOnLeave || isDayOff) {
        $("#start-time").prop("disabled", true).val("");
        $("#end-time").prop("disabled", true).val("");
    } else {
        $("#start-time").prop("disabled", false);
        $("#end-time").prop("disabled", false);
    }
}

// Checkbox değişince birbirini kontrol et
$(document).off("change", "#is-on-leave, #is-day-off").on("change", "#is-on-leave, #is-day-off", function () {
    const changedId = $(this).attr("id");

    if (changedId === "is-on-leave" && $(this).is(":checked")) {
        $("#is-day-off").prop("checked", false).trigger("change");
    }
    if (changedId === "is-day-off" && $(this).is(":checked")) {
        $("#is-on-leave").prop("checked", false).trigger("change");
    }

    updateDayFormUI();
});


// Form submit işlemi (şu anlık sadece console.log yapıyor)
$(document).off("submit", "#day-form").on("submit", "#day-form", function (e) {
    e.preventDefault();
    const $form = $(this);
    const $btn = $form.find("button[type=submit]");
    const $workerId = window.selectedWorkerId;
    const $month = window.selectedMonth;
    const $year = window.selectedYear;

    if (!$workerId || !window.selectedDay || !$month || !$year) {
        alert("Lütfen sayfayı yenileyin veya sistem yöneticinize başvurun!");
        return;
    }

    $btn.prop("disabled", true).text("Kaydediliyor...");

    $.ajax({
        type: "POST",
        url: "/Admin/Location/WorkerHour/Add",
        data: $form.serialize(),
        success: function (msg) {
            setTimeoutAlert("s", "#day-form-alert", msg, 5);
            fetchCalendarData($workerId, $year, $month);
            $btn.prop("disabled", false).text("Kaydet");
        },
        error: function (xhr) {
            var msg = xhr.responseText || "Günlük formu kaydedilemedi.";
            setTimeoutAlert("e", "#day-form-alert", msg, 5);
            $btn.prop("disabled", false).text("Kaydet");
        }
    });
});


// Form ilk açıldığında da çalıştırmak için
$(document).ready(function () {
    updateDayFormUI();
});
