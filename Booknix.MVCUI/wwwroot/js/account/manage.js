$(function () {
    // Sayfa ilk yüklendiğinde hash varsa o tabı aktif et
    const hash = window.location.hash;
    // Eğer hash bir kelimeden oluşuyorsa (örn: #profile)
    if (hash && !hash.includes("-")) {
        const tabId = hash.substring(1); // #profile -> profile
        activateTab(tabId);
    }

    // Eğer hash 'profile-xxxx' gibi bir yapıya sahipse
    if (hash && hash.includes("-")) {
        const baseTabId = hash.split('-')[0].substring(1); // profile-xxxx -> profile
        const fullTabId = hash.substring(1); // profile-password

        // İlk olarak base tab'ı aktif et
        activateTab(baseTabId, fullTabId);
    }

    // Ana tab tıklama olayını dinle
    $(".tab-link").on("click", function (e) {
        e.preventDefault();
        const tabId = $(this).attr("href").split('-')[0].substring(1); // #profile -> profile

        // Hash değerini güncelle
        window.location.hash = tabId;

        // Tab'ı aktif et
        activateTab(tabId);
    });
});

// Tab aktif etme fonksiyonu
function activateTab(tabId, fullTabId = null) {
    // Tüm tabları deaktif et
    $(".tab-link").removeClass("active border-blue-500 text-blue-700").addClass("border-transparent");
    $(".tab-pane").removeClass("active").addClass("hidden");

    // Aktif tabı ve linkini belirle
    $("#" + tabId + "-tab").addClass("active").removeClass("hidden");
    $(".tab-link[href='#" + tabId + "']").addClass("active border-blue-500 text-blue-700").removeClass("border-transparent");

    // İçeriği yükle
    loadContent(tabId, fullTabId);
}

// İçeriği yükleme fonksiyonu
function loadContent(tabId, fullTabId) {
    if (tabId === "appointments") {
        loadAppointments();
    } else if (tabId === "profile") {
        loadProfile(fullTabId);
    } else if (tabId === "security") {
        loadSecurity();
    }
}

// Profil içeriğini yükle
function loadProfile(fullTabId) {
    $("#account-loader").html("<div class='text-sm text-gray-500'>Profil içeriği yükleniyor...</div>");

    $.get("/Account/ProfileView", function (html) {
        $("#account-loader").html(html);
    }).fail(function () {
        $("#account-loader").html(`
            <div class="text-sm text-gray-500">Profil bilgileri yüklenemedi.</div>
        `);
    }).done(function () {
        if (fullTabId) {
            activateTabProfile(fullTabId); // Profil yüklendikten sonra içeriği yükle
        }
    });
}

// Randevuları yükle
function loadAppointments() {
    $("#account-loader").html("<div class='text-sm text-gray-500'>Randevu bilgileri yükleniyor...</div>");

    $.get("/Account/Appointments", function (html) {
        $("#account-loader").html(html);
    }).fail(function () {
        $("#account-loader").html(`
            <div class="flex flex-col items-center justify-center text-center space-y-4 py-8">
                <i class="fas fa-calendar-times text-4xl text-gray-400"></i>
                <h2 class="text-xl font-bold text-gray-700">Randevu Bulunamadı</h2>
                <p class="text-sm text-gray-500">Henüz bir randevu kaydınız bulunmamaktadır.</p>
            </div>
        `);
    });
}

function loadSecurity() {
    $("#account-loader").html("<div class='text-sm text-gray-500'>Güvenlik ayarları yükleniyor...</div>");
    $.get("/Account/Security", function (html) {
        $("#account-loader").html(html);
    }).fail(function () {
        $("#account-loader").html(`
            <div class="text-sm text-gray-500">Güvenlik ayarları yüklenemedi.</div>
        `);
    });
}