// Çalışan seçimi
$(document).off("click", ".worker-item").on("click", ".worker-item", function () {
    $(".worker-item").removeClass("border-indigo-600 bg-indigo-100");
    $(this).addClass("border-indigo-600 bg-indigo-100");

    const selectedWorkerId = $(this).data("id");
    const selectedWorkerName = $(this).data("name");

    window.selectedWorkerId = selectedWorkerId;
    window.selectedWorkerName = selectedWorkerName;

    clearSelection()

    if (selectedWorkerId) {
        $("#calendar-section").removeClass("hidden"); // Takvim panelini göster
        fetchCalendarData(selectedWorkerId, currentYear, currentMonth + 1);
        $("#worker-timeform-id").val(selectedWorkerId);

        setTimeout(() => {
            const calendarSection = document.getElementById("calendar-section");
            if (calendarSection) {
                calendarSection.scrollIntoView({ behavior: "smooth", block: "start" });

                // 🎯 Kayan elemana kısa bir parlayan gölge efekti ver
                calendarSection.classList.add("ring-4", "ring-indigo-300");

                setTimeout(() => {
                    calendarSection.classList.remove("ring-4", "ring-indigo-300");
                }, 800); // 0.8 saniye sonra geri kaldır
            }
        }, 300);

    }
});

attachSearchFilter('worker-search', '#worker-list', '.worker-item'); 