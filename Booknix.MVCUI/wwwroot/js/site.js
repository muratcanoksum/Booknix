// site.js — tüm sayfalarda çağırılabilir ortak fonksiyonlar

const alertTimers = {};

function setTimeoutAlert(idSelector, message = "", timeInSeconds = 5) {
    const duration = timeInSeconds * 1000;
    const $el = $(idSelector);

    if (alertTimers[idSelector]) {
        clearTimeout(alertTimers[idSelector]);
        delete alertTimers[idSelector];
    }

    if (message) {
        $el.find("span").text(message);
    }

    $el.removeClass("hidden").hide().fadeIn("slow");

    alertTimers[idSelector] = setTimeout(() => {
        $el.fadeOut("slow", function () {
            $(this).addClass("hidden").removeAttr("style").find("span").text("");
        });
        delete alertTimers[idSelector];
    }, duration);
}

function showModal(id) {
    $(`#${id}`).removeClass("hidden").addClass("flex");
}

function hideModal(id) {
    $(`#${id}`).addClass("hidden").removeClass("flex");
}

function toggleElementById(id) {
    const el = document.getElementById(id);
    if (el) el.classList.toggle("hidden");
}

function toggleMobileMenu() {
    const menu = document.getElementById("mobileMenu");
    menu.classList.toggle("hidden");
}

function initCleavePhoneMask() {
    const $input = $('#phone-input');
    if ($input.length && !$input.data('cleave-applied')) {
        new Cleave('#phone-input', {
            delimiters: [' (', ') ', ' ', ' '],
            blocks: [1, 3, 3, 2, 2],
            numericOnly: true
        });
        $input.data('cleave-applied', true);
    }
};

