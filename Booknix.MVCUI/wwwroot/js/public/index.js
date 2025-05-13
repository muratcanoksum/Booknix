window.searchTimer = null;

$("#globalSearch").on("input", function () {
    handleSearchInput();
});

$("#globalSearch").on("keypress", function (e) {
    if (e.which === 13) { // Enter tuşu
        e.preventDefault();
        if (searchTimer) {
            clearTimeout(searchTimer);
        }
        handleSearchInput();
    }
});

function handleSearchInput() {
    const $input = $("#globalSearch");
    const query = $input.val().trim();

    clearTimeout(searchTimer);
    $("#searchResults").remove();
    $("#search-spinner").addClass("hidden");

    if (query.length < 3) return;

    $("#search-spinner").removeClass("hidden");
    $input.addClass("opacity-60 pointer-events-none");

    searchTimer = setTimeout(() => {
        performSearch();
    }, 2000);
}

function performSearch() {
    const $input = $("#globalSearch");
    const query = $input.val().trim();

    if (query.length < 3) {
        $("#search-spinner").addClass("hidden");
        $input.removeClass("opacity-60 pointer-events-none");
        return;
    }

    $.get(`/api/search?q=${encodeURIComponent(query)}`, function (data) {
        $("#searchResults").remove();
        $("#search-spinner").addClass("hidden");
        $input.removeClass("opacity-60 pointer-events-none");

        const $section = $("<section>", {
            id: "searchResults",
            class: "mb-10"
        });

        const $title = $("<h2>", {
            class: "text-xl font-semibold text-gray-800 mb-4",
            text: "Arama Sonuçları"
        });

        const $grid = $("<div>", {
            class: "grid sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6"
        });

        if (data.length === 0) {
            $grid.append(`<p class='text-sm text-gray-500 col-span-full'>Sonuç bulunamadı.</p>`);
        } else {
            data.forEach(item => {
                $grid.append(`
                    <a href="${item.url}" class="bg-white rounded-xl shadow-lg p-6 hover:shadow-2xl transition-all duration-300 transform hover:scale-105">
                        <h4 class="text-md font-semibold text-gray-800 mb-1">${item.name}</h4>
                        <p class="text-sm text-gray-500">${item.locationName || item.categoryName || ''}</p>
                    </a>
                `);
            });
        }

        $section.append($title, $grid);
        $("#categorySection").before($section);
    }).fail(() => {
        $("#search-spinner").addClass("hidden");
        $input.removeClass("opacity-60 pointer-events-none");
    });
}
