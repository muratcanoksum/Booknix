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

// Gün seçildiğinde çalışanları tetikleyecek fonksiyon zaten calendar.js içinde çağrılıyor
// Seçim yapılınca otomatik formu temizleyip yeni verileri basıyoruz

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

    if (!window.selectedWorkerId || !window.selectedDay || !window.selectedMonth || !window.selectedYear) {
        alert("Önce bir gün seçmelisiniz!");
        return;
    }

    const data = {
        WorkerId: window.selectedWorkerId,
        Day: window.selectedDay,
        Month: window.selectedMonth,
        Year: window.selectedYear,
        StartTime: $("#start-time").val(),
        EndTime: $("#end-time").val(),
        IsOnLeave: $("#is-on-leave").is(":checked"),
        IsDayOff: $("#is-day-off").is(":checked")
    };

    console.log("Günlük Form Verisi:", data);

    // İleride buraya ajax ile veri gönderimi eklenecek
});

// Form ilk açıldığında da çalıştırmak için
$(document).ready(function () {
    updateDayFormUI();
});
