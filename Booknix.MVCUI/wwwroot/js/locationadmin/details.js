$(document).off("click", ".operation-tab").on("click", ".operation-tab", function () {
    const url = $(this).data("url");

    $(".operation-tab")
        .removeClass("bg-indigo-600 text-white font-semibold")
        .addClass("bg-gray-100 text-gray-800");

    $(this)
        .removeClass("bg-gray-100 text-gray-800")
        .addClass("bg-indigo-600 text-white font-semibold");

    $("#location-operation-loader").text("Yükleniyor...");
    $("#location-operation-panel").empty();

    $.get(url, function (html) {
        $("#location-operation-panel").html(html);
        $("#location-operation-loader").text("");

        setTimeout(() => {
            document.getElementById("location-operation-panel")?.scrollIntoView({ behavior: "smooth" });
        }, 100);
    }).fail(function () {
        $("#location-operation-loader").html(`
            <div class="text-center space-y-2">
                <img src="/images/logo.png" class="w-16 h-16 mx-auto">
                <p class="text-sm text-gray-500">İçerik yüklenemedi.</p>
            </div>
        `);
    });
}); 