$(function () {
    // Profil tab tıklama olayını dinle
    $(".stab-link").on("click", function (e) {
        e.preventDefault();
        const tabId = $(this).attr("href").substring(1); // #profile-information -> profile-information
        const url = $(this).data("url");  // data-url üzerinden URL alınacak

        // Hash değerini güncelle
        window.location.hash = tabId;

        // Tab'ı aktif et
        activateTabProfile(tabId);
    });
});

// Tab aktif etme fonksiyonu
function activateTabProfile(tabId) {
    // Tüm tabları sadeleştir
    $(".stab-link").each(function () {
        $(this)
            .removeClass("text-blue-500 font-semibold bg-blue-100 border-blue-500 border-b-4")
            .addClass("text-gray-700 font-medium bg-transparent border-transparent border-b-4");
    });

    // Aktif tabı belirginleştir
    const $activeTab = $(`.stab-link[href='#${tabId}']`);
    $activeTab
        .removeClass("text-gray-700 font-medium bg-transparent border-transparent")
        .addClass("text-blue-500 font-semibold bg-blue-100 border-blue-500 border-b-4");

    // İçerik URL’sini al ve yükle
    const url = $activeTab.data("url");
    loadContentProfile(url);
}



// Profil içeriğini yükleme fonksiyonu
function loadContentProfile(url) {
    $("#profile-loader").html("<div class='text-sm text-gray-500'>Yükleniyor...</div>");

    $.get(url, function (html) {
        $("#profile-loader").html(html);
    }).fail(function () {
        $("#profile-loader").html(`
            <div class="text-sm text-gray-500">İçerik yüklenemedi.</div>
        `);
    });
}
