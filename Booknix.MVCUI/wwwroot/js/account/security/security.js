$(function () {
    const $tbody = $("#audit-log-body");
    const $pagination = $("#audit-log-pagination");

    let currentPage = parseInt($pagination.data("current"));
    let totalPages = parseInt($pagination.data("total"));

    function loadLogs(page) {
        $.get("/Account/GetAuditLogs", { page: page }, function (data) {
            if (!data || !data.logs) return;

            $tbody.empty();
            data.logs.forEach(log => {
                const row = `
                    <tr class="hover:bg-gray-50 transition">
                        <td class="px-4 py-3">${log.action}</td>
                        <td class="px-4 py-3">${log.timestamp}</td>
                        <td class="px-4 py-3">${log.ipAddress ?? ""}</td>
                        <td class="px-4 py-3">${log.description ?? ""}</td>
                    </tr>`;
                $tbody.append(row);
            });

            currentPage = data.currentPage;
            totalPages = data.totalPages;
            renderPagination();

            document.getElementById("audit-log-table")?.scrollIntoView({
                behavior: "smooth",
                block: "center"
            });

        });
    }

    function renderPagination() {
        let html = "";

        if (currentPage > 1) {
            html += `<button class="px-2 py-1 border rounded" data-page="${currentPage - 1}">‹</button>`;
        }

        const range = getPageRange(currentPage, totalPages);

        range.forEach(p => {
            if (p === "...") {
                html += `<span class="px-2 py-1 text-gray-400">...</span>`;
            } else {
                html += `<button class="px-2 py-1 border rounded ${p === currentPage ? 'bg-gray-200 font-bold' : ''}" data-page="${p}">${p}</button>`;
            }
        });

        if (currentPage < totalPages) {
            html += `<button class="px-2 py-1 border rounded" data-page="${currentPage + 1}">›</button>`;
        }

        $pagination.html(html);
    }

    function getPageRange(current, total) {
        const delta = 2;
        const range = [];
        const rangeWithDots = [];

        let l = Math.max(2, current - delta);
        let r = Math.min(total - 1, current + delta);

        range.push(1);
        if (l > 2) range.push("...");

        for (let i = l; i <= r; i++) {
            range.push(i);
        }

        if (r < total - 1) range.push("...");
        if (total > 1) range.push(total);

        return range;
    }

    // İlk yükleme
    renderPagination();

    // Buton tıklama
    $pagination.on("click", "button[data-page]", function () {
        const targetPage = parseInt($(this).data("page"));
        if (targetPage !== currentPage) {
            loadLogs(targetPage);
        }
    });
});
