// site.js — tüm sayfalarda çağırılabilir ortak fonksiyonlar

const alertTimers = {};

function setTimeoutAlert(type, idSelector, message = "", timeInSeconds = 5) {
    const duration = timeInSeconds * 1000;
    const $el = $(idSelector);

    // Success ya da error için ilgili CSS sınıflarını belirle
    let alertClass = '';
    let iconClass = '';

    if (type === 's') {
        alertClass = 'mb-4 px-4 py-3 rounded border border-green-300 bg-green-50 text-green-700 text-sm shadow-sm';
        iconClass = 'fas fa-check-circle';
    } else if (type === 'e') {
        alertClass = 'mb-4 px-4 py-3 rounded border border-red-300 bg-red-50 text-red-700 text-sm shadow-sm';
        iconClass = 'fas fa-exclamation-circle';
    }

    // Mevcut zamanlayıcıyı temizle
    if (alertTimers[idSelector]) {
        clearTimeout(alertTimers[idSelector]);
        delete alertTimers[idSelector];
    }

    // Uyarıyı oluştur ve içerik ekle
    $el.removeClass("hidden").hide().fadeIn("slow")
        .removeClass().addClass(`${alertClass}`)
        .html(`<i class="${iconClass} mr-2"></i><span>${message}</span>`);

    // Zamanlayıcıyı başlat
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

