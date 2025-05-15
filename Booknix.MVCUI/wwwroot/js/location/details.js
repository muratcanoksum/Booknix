$(function () {
    window.$ctx = $("#location-meta")
    window.baseUrl = $ctx.data("base-url")
    window.locationId = $ctx.data("location-id")

})


$(document).off("click", ".operation-tab").on("click", ".operation-tab", function () {
    const url = $(this).data("url");
    const tag = $(this).data("tag");

    $(".operation-tab")
        .removeClass("bg-indigo-600 text-white font-semibold")
        .addClass("bg-gray-100 text-gray-800");

    $(this)
        .removeClass("bg-gray-100 text-gray-800")
        .addClass("bg-indigo-600 text-white font-semibold");

    // Panel yüksekliğini koru
    const $panel = $("#location-operation-panel");
    const currentHeight = $panel.outerHeight();
    $panel.css("min-height", currentHeight + "px");

    $("#location-operation-loader").text("Yükleniyor...");
    $panel.empty();

    if (tag) {
        window.location.hash = tag;
    }

    $.get(url, function (html) {
        $panel.html(html);
        $("#location-operation-loader").text("");
        $panel.css("min-height", ""); // Yükleme bitince yükseklik sınırını kaldır

        setTimeout(() => {
            const offsetTop = $panel.offset()?.top;
            if (offsetTop !== undefined) {
                $("html, body").animate({ scrollTop: offsetTop - 80 }, 300);
            }
        }, 50);
    }).fail(function (xhr) {
        $panel.css("min-height", "");
        $("#location-operation-loader").html(xhr.responseText);
    });

});


$(function () {
    const hash = window.location.hash?.substring(1); // #services → services
    if (hash) {
        $(`.operation-tab[data-tag="${hash}"]`).trigger("click");
    }
});


// Tab tıklama yakalama
$(document).on("click", ".operation-tab", function () {
    var url = $(this).data("url");
    if (!url) return;

    $("#location-operation-loader").removeClass("hidden");
    $("#location-operation-panel").html("");

    $.get(url, function (response) {
        $("#location-operation-loader").addClass("hidden");
        $("#location-operation-panel").html(response);
    });
});
