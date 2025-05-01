// site.js — tüm sayfalarda çağırılabilir ortak fonksiyonlar

window.alertTimers = window.alertTimers || {};

function setTimeoutAlert(type, idSelector, message = "", timeInSeconds = 5) {
    const duration = timeInSeconds * 1000;
    const $el = $(idSelector);

    let alertClass = '';
    let iconClass = '';

    if (type === 's') {
        alertClass = 'flex items-center gap-3 mb-4 p-3 rounded-md border border-green-300 bg-green-50 text-green-700 text-sm shadow';
        iconClass = 'fas fa-check-circle text-green-600';
    } else if (type === 'e') {
        alertClass = 'flex items-center gap-3 mb-4 p-3 rounded-md border border-red-300 bg-red-50 text-red-700 text-sm shadow';
        iconClass = 'fas fa-exclamation-circle text-red-600';
    } else if (type === 'i') {
        alertClass = 'flex items-center gap-3 mb-4 p-3 rounded-md border border-yellow-300 bg-yellow-50 text-yellow-800 text-sm shadow';
        iconClass = 'fas fa-info-circle text-yellow-600';
    }

    if (alertTimers[idSelector]) {
        clearTimeout(alertTimers[idSelector]);
        delete alertTimers[idSelector];
    }

    $el.removeClass("hidden").hide().fadeIn("slow")
        .removeClass().addClass(alertClass)
        .html(`
            <div class="flex items-center justify-center">
                <i class="${iconClass} text-lg"></i>
            </div>
            <div class="flex-1">
                <span>${message}</span>
            </div>
        `);

    if (timeInSeconds > 0) {
        alertTimers[idSelector] = setTimeout(() => {
            $el.fadeOut("slow", function () {
                $(this).addClass("hidden").removeAttr("style").find("span").text("");
            });
            delete alertTimers[idSelector];
        }, duration);
    }
}

function showModal(id) {
    const $modal = $(`#${id}`);
    $modal.removeClass("hidden").addClass("flex");

    // Fade in + blur efekti ekle
    $modal.hide().fadeIn(300, function () {
        $modal.addClass("backdrop-blur-sm bg-opacity-40");
    });
}

function hideModal(id) {
    const $modal = $(`#${id}`);

    // Fade out + blur efekti kaldır
    $modal.fadeOut(300, function () {
        $modal.addClass("hidden").removeClass("flex backdrop-blur-sm bg-opacity-40").removeAttr("style");
    });
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

function attachSearchFilter(inputId, containerSelector, itemSelector) {
    const input = document.getElementById(inputId);
    if (!input) return;

    input.addEventListener('input', function () {
        const query = input.value.trim().toLowerCase();
        const items = document.querySelectorAll(containerSelector + ' ' + itemSelector);

        items.forEach(item => {
            const text = item.textContent.trim().toLowerCase();
            if (text.includes(query)) {
                item.classList.remove('hidden');
            } else {
                item.classList.add('hidden');
            }
        });
    });
}

function showLoading() {
    const loading = document.getElementById('global-loading-overlay');
    if (loading) {
        loading.classList.remove('hidden');
    }
}

function hideLoading() {
    const loading = document.getElementById('global-loading-overlay');
    if (loading) {
        loading.classList.add('hidden');
    }
}
