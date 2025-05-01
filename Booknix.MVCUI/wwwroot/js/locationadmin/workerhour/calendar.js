// Global Ay ve Yıl Değişkenleri
let currentYear = new Date().getFullYear();
let currentMonth = new Date().getMonth(); // 0-11
let multiSelectMode = false;
let selectedDates = [];

const monthNames = ["Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran",
    "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık"];

// Takvim verisi çek
function fetchCalendarData(workerId, year, month) {
    $.get(`/LocationAdmin/GetWorkerWorkingHours/${workerId}/${year}/${month}`, function (data) {
        renderCalendar(data);
    }).fail(function () {
        console.error("Çalışma saatleri yüklenemedi.");
    });
}

// Takvim çiz
function renderCalendar(workerHours = []) {
    const $calendarGrid = $("#calendar-grid");
    $calendarGrid.empty();

    const daysInMonth = new Date(currentYear, currentMonth + 1, 0).getDate();
    const firstDayOfWeek = (new Date(currentYear, currentMonth, 1).getDay() + 6) % 7;

    const dayMap = {};
    workerHours.forEach(x => {
        dayMap[x.day] = x;
    });

    for (let i = 0; i < firstDayOfWeek; i++) {
        $calendarGrid.append(`
            <div class="calendar-day invisible aspect-square w-full max-w-[80px]"></div>
        `);
    }

    for (let day = 1; day <= daysInMonth; day++) {
        let extraAttributes = "";
        const dayText = day.toString().padStart(2, "0");
        let bgClass = "bg-gray-300";

        if (dayMap[day]) {
            if (dayMap[day].isOnLeave) {
                bgClass = "bg-orange-400";
                extraAttributes += ` data-is-on-leave="true"`;
            } else if (dayMap[day].isDayOff) {
                bgClass = "bg-red-500";
                extraAttributes += ` data-is-day-off="true"`;
            } else {
                bgClass = "bg-green-500";
                extraAttributes += ` data-start-time="${dayMap[day].startTime}" data-end-time="${dayMap[day].endTime}"`;
            }
        }

        $calendarGrid.append(`
            <div class="calendar-day ${bgClass} text-white text-center flex flex-col items-center justify-center rounded w-full aspect-square max-w-[80px] cursor-pointer select-none"
                data-day="${day}" ${extraAttributes}>
                <div class="text-base">${dayText}</div>
            </div>
        `);
    }

    $("#calendar-current").text(`${monthNames[currentMonth]} ${currentYear}`);
}

// Çoklu seçim toggle
$(document).off("click", "#multi-select-toggle").on("click", "#multi-select-toggle", function () {
    multiSelectMode = !multiSelectMode;

    $(this).text(multiSelectMode ? "Çoklu Seçim Kapat" : "Çoklu Seçim Aç");

    if (multiSelectMode) {
        // Çoklu seçim AÇILIRKEN de önceki tekli seçim temizlensin
        $(".calendar-day").removeClass("ring-2 ring-indigo-500 selected");
        updateDayFormUI();
        selectedDates = [];
        window.selectedDay = null;
        window.selectedMonth = null;
        window.selectedYear = null;
        $("#selected-day-count").text("");
        $("#day-form-section").addClass("hidden");
    }
    else {
        // Çoklu seçim KAPANIRKEN de zaten sıfırlama yapıyorduk
        selectedDates = [];
        $(".calendar-day").removeClass("ring-2 ring-indigo-500 selected");
        updateDayFormUI();
        $("#selected-day-count").text("");
        $("#day-form-section").addClass("hidden");
        window.selectedDay = null;
        window.selectedMonth = null;
        window.selectedYear = null;
    }
});

// Gün seçimi
$(document).off("click", ".calendar-day").on("click", ".calendar-day", function () {
    const clickedDay = $(this).data("day");

    const clickedDate = new Date(Date.UTC(currentYear, currentMonth, clickedDay));
    const formattedDate = clickedDate.toISOString().split('T')[0];

    if (multiSelectMode) {
        $(this).toggleClass("selected ring-2 ring-indigo-500");

        if ($(this).hasClass("selected")) {
            selectedDates.push(formattedDate);

            if ($("#day-form-section").hasClass("hidden")) {
                $("#start-time").val("");
                $("#end-time").val("");
                $("#is-on-leave").prop("checked", false);
                $("#is-day-off").prop("checked", false);
                $("#day-form-section").removeClass("hidden");
            }

        } else {
            selectedDates = selectedDates.filter(d => d !== formattedDate);

            if (selectedDates.length === 0) {
                $("#day-form-section").addClass("hidden");
            }
        }

        $("#selected-day-count").text(`${selectedDates.length} gün seçildi`);
        $("#day-form-title").text(`Çoklu Gün İşlemleri (${selectedDates.length} gün)`);

    } else {
        if (window.selectedDay === clickedDay &&
            window.selectedMonth === currentMonth + 1 &&
            window.selectedYear === currentYear) {
            clearSelection();
            $("#day-form-section").addClass("hidden");
            return;
        }

        $(".calendar-day").removeClass("ring-2 ring-indigo-500 selected");
        $(this).addClass("ring-2 ring-indigo-500");

        selectedDates = [];

        $("#selected-day-count").text("");

        window.selectedDay = clickedDay;
        window.selectedMonth = currentMonth + 1;
        window.selectedYear = currentYear;

        const $this = $(this);

        $("#start-time").val($this.data("start-time") || "");
        $("#end-time").val($this.data("end-time") || "");
        $("#is-on-leave").prop("checked", $this.data("is-on-leave") === true);
        $("#is-day-off").prop("checked", $this.data("is-day-off") === true);

        $("#day-form-title").text(`Seçili Gün İşlemleri (${clickedDay}.${currentMonth + 1}.${currentYear})`);

        $("#worker-timeform-date").val(formattedDate);

        updateDayFormUI();
        $("#day-form-section").removeClass("hidden");
    }
});

// Ay değiştir
$(document).off("click", "#calendar-prev").on("click", "#calendar-prev", function () {
    currentMonth--;
    if (currentMonth < 0) {
        currentMonth = 11;
        currentYear--;
    }
    if (window.selectedWorkerId) {
        fetchCalendarData(window.selectedWorkerId, currentYear, currentMonth + 1);
    }
});

$(document).off("click", "#calendar-next").on("click", "#calendar-next", function () {
    currentMonth++;
    if (currentMonth > 11) {
        currentMonth = 0;
        currentYear++;
    }
    if (window.selectedWorkerId) {
        fetchCalendarData(window.selectedWorkerId, currentYear, currentMonth + 1);
    }
});

// Bugüne dön
$(document).off("click", "#calendar-today").on("click", "#calendar-today", function () {
    goToday();
});

function goToday() {
    const today = new Date();
    currentYear = today.getFullYear();
    currentMonth = today.getMonth();

    if (window.selectedWorkerId) {
        fetchCalendarData(window.selectedWorkerId, currentYear, currentMonth + 1);

        setTimeout(() => {
            const todayDate = today.getDate();
            const $todayEl = $(`.calendar-day[data-day='${todayDate}']`);

            if ($todayEl.length > 0) {
                $(".calendar-day").removeClass("ring-2 ring-indigo-500 selected");
                $todayEl.addClass("ring-2 ring-indigo-500");

                window.selectedDay = todayDate;
                window.selectedMonth = currentMonth + 1;
                window.selectedYear = currentYear;

                $("#day-form-section").removeClass("hidden");
            }
        }, 300);
    }
}

function clearSelection() {
    $(".calendar-day").removeClass("ring-2 ring-indigo-500 selected");
    window.selectedDay = null;
    window.selectedMonth = null;
    window.selectedYear = null;
    $("#day-form-section").addClass("hidden");
}

function clearMultiSelection() {
    $(".calendar-day").removeClass("ring-2 ring-indigo-500 selected");
    selectedDates = [];
    $("#day-form-title").text("Seçili Gün İşlemleri");
    $("#day-form-section").addClass("hidden");
} 