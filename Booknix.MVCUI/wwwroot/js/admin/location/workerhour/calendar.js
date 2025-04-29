// Global Ay ve Yıl Değişkenleri
let currentYear = new Date().getFullYear();
let currentMonth = new Date().getMonth(); // 0-11

const monthNames = ["Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran",
    "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık"];

// Takvim verisi çek
function fetchCalendarData(workerId, year, month) {
    $.get(`/Admin/Location/GetWorkerWorkingHours/${workerId}/${year}/${month}`, function (data) {
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

    // Boş kutular (ilk gün başlamadan önce)
    for (let i = 0; i < firstDayOfWeek; i++) {
        $calendarGrid.append(`
        <div class="calendar-day invisible aspect-square w-full max-w-[80px]">
            <!-- boş -->
        </div>
    `);
    }


    for (let day = 1; day <= daysInMonth; day++) {
        let extraAttributes = ""; // <<< GÜNÜN BAŞINDA HER SEFERDE SIFIRLA

        const dayText = day.toString().padStart(2, "0");
        let bgClass = "bg-gray-300";
        let timeText = "";

        if (dayMap[day]) {
            if (dayMap[day].isOnLeave) {
                bgClass = "bg-orange-400";
                extraAttributes += ` data-is-on-leave="true"`;
            } else if (dayMap[day].isDayOff) {
                bgClass = "bg-red-500";
                extraAttributes += ` data-is-day-off="true"`;
            } else {
                bgClass = "bg-green-500";
                extraAttributes += ` data-start-time="${dayMap[day].startTime}"`;
                extraAttributes += ` data-end-time="${dayMap[day].endTime}"`;
            }
        }

        $calendarGrid.append(`
        <div class="calendar-day ${bgClass} text-white text-center flex flex-col items-center justify-center rounded w-full aspect-square max-w-[80px] cursor-pointer select-none"
             data-day="${day}"
             ${extraAttributes}>
            <div class="text-base">${dayText}</div>
        </div>
    `);
    }


    $("#calendar-current").text(`${monthNames[currentMonth]} ${currentYear}`);
}

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

// Gün seçimi
$(document).off("click", ".calendar-day").on("click", ".calendar-day", function () {
    const clickedDay = $(this).data("day");

    if (window.selectedDay === clickedDay &&
        window.selectedMonth === currentMonth + 1 &&
        window.selectedYear === currentYear) {
        clearSelection();
        return;
    }

    // Değilse yeni günü seç
    $(".calendar-day").removeClass("ring-2 ring-indigo-500");
    $(this).addClass("ring-2 ring-indigo-500");

    window.selectedDay = clickedDay;
    window.selectedMonth = currentMonth + 1;
    window.selectedYear = currentYear;

    const $this = $(this);

    // 1. Önce formu temizle
    $("#start-time").val("");
    $("#end-time").val("");
    $("#is-on-leave").prop("checked", false);
    $("#is-day-off").prop("checked", false);

    // 2. Sonra yeni değerleri oku
    const startTime = $this.data("start-time") || "";
    const endTime = $this.data("end-time") || "";
    const isOnLeave = $this.data("is-on-leave") === true;
    const isDayOff = $this.data("is-day-off") === true;

    // 3. Değerleri form inputlarına bas
    $("#start-time").val(startTime);
    $("#end-time").val(endTime);
    $("#is-on-leave").prop("checked", isOnLeave);
    $("#is-day-off").prop("checked", isDayOff);

    $("#day-form-title").text(`Seçili Gün İşlemleri (${clickedDay}.${currentMonth + 1}.${currentYear})`);

    const date = new Date(Date.UTC(currentYear, currentMonth, clickedDay));
    const formattedDate = date.toISOString().split('T')[0];
    $("#worker-timeform-date").val(formattedDate);




    // 4. En son formun input aktif/pasif ayarlarını yap
    updateDayFormUI();

    $("#day-form-section").removeClass("hidden");
});

// Bugüne dön
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
                $(".calendar-day").removeClass("ring-2 ring-indigo-500");
                $todayEl.addClass("ring-2 ring-indigo-500");

                window.selectedDay = todayDate;
                window.selectedMonth = currentMonth + 1;
                window.selectedYear = currentYear;

                $("#day-form-section").removeClass("hidden");
            }
        }, 300); // fetch biter bitmez seçmesi için küçük bir delay
    }
}

function clearSelection() {
    $(".calendar-day").removeClass("ring-2 ring-indigo-500");
    window.selectedDay = null;
    window.selectedMonth = null;
    window.selectedYear = null;

    // Günlük form panelini de kapat
    $("#day-form-section").addClass("hidden");
}

// Bugün butonu
$(document).off("click", "#calendar-today").on("click", "#calendar-today", function () {
    goToday();
});
