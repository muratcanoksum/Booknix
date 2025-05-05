let $selectedButton = null;

$(document).off("click", ".slot-button").on("click", ".slot-button", function () {
    if ($selectedButton) {
        $selectedButton.removeClass("ring ring-offset-2 ring-indigo-600");
    }

    if ($selectedButton?.is(this)) {
        $selectedButton = null;
        $("#selectedTime").text("Henüz seçilmedi");
        $("#selectedDate").text("Henüz seçilmedi");
        $("#Time").val("");
        $("#Date").val("");
        return;
    }

    $selectedButton = $(this);
    const time = $(this).data("slot-time");
    const date = $(this).data("slot-date");
    const formatted = formatDateToTurkish(date);

    $(this).addClass("ring ring-offset-2 ring-indigo-600");
    $("#selectedDate").text(formatted);
    $("#selectedTime").text(time);
    $("#Time").val(time);
    $("#Date").val(date);
});

$(document).off("submit", "#confirm-form").on("submit", "#confirm-form", function (e) {
    e.preventDefault();

    console.log($(this).serializeArray());

    // AJAX ile gönderme örneği:

    $.ajax({
        type: "POST",
        url: "/Public/ConfirmAppointment",
        data: $(this).serialize(),
        success: function (msg) {
            window.location.href = "/Account/Manage#appointments";
        },
        error: function (xhr) {
            msg = xhr.responseText || "Randevu oluşturulurken hata oluştu.";
            alert(msg);
        }
    });
});

const card = document.getElementById("infoCard");
window.addEventListener("scroll", () => {
    const offset = window.scrollY;
    card.style.transform = `translateY(${offset}px)`;
});
