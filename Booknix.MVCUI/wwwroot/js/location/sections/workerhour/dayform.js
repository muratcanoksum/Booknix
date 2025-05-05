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


$(document).off("submit", "#day-form").on("submit", "#day-form", function (e) {
    e.preventDefault();

    const $form = $(this);
    const $btn = $form.find("button[type=submit]");
    const workerId = window.selectedWorkerId;

    if (!workerId) {
        alert("Lütfen bir çalışan seçiniz!");
        return;
    }

    let datesToSend = [];

    if (multiSelectMode) {
        if (!selectedDates || selectedDates.length === 0) {
            alert("Lütfen en az bir gün seçiniz!");
            return;
        }
        datesToSend = selectedDates;
    } else {
        const singleDate = $("#worker-timeform-date").val();
        if (!singleDate) {
            alert("Lütfen bir gün seçiniz!");
            return;
        }
        datesToSend = [singleDate];
    }

    $btn.prop("disabled", true).text("Kaydediliyor...");

    const formData = $form.serializeArray();

    const filteredFormData = formData.filter(x => x.name !== "SelectedDays");
    filteredFormData.push({ name: "SelectedDays", value: JSON.stringify(datesToSend) });

    $.ajax({
        type: "POST",
        url: `${baseUrl}/WorkerHour/Add`,
        data: $.param(filteredFormData),
        success: function (msg) {
            setTimeoutAlert("s", "#day-form-alert", msg, 5);
            fetchCalendarData(workerId, currentYear, currentMonth + 1); // Seçilen yıl ve ayı güncelle
            $btn.prop("disabled", false).text("Kaydet");
            // Çoklu moddaysak seçimleri sıfırla

        },
        error: function (xhr) {
            const errorMsg = xhr.responseText || "Günlük formu kaydedilemedi.";
            setTimeoutAlert("e", "#day-form-alert", errorMsg, 5);
            $btn.prop("disabled", false).text("Kaydet");
        }
    });
});



// Form ilk açıldığında da çalıştırmak için
$(document).ready(function () {
    updateDayFormUI();
}); 