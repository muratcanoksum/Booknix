$(function () {
    const initialStatus = "Pending";
    $(`.tab-btn[data-status='${initialStatus}']`).attr("data-active", "true");
    loadEmailQueue(initialStatus);

    $(".tab-btn").on("click", function () {
        const status = $(this).data("status");
        $(".tab-btn").attr("data-active", "false");
        $(this).attr("data-active", "true");
        loadEmailQueue(status);
    });
});

function loadEmailQueue(status, silent = false) {
    if (!silent) showLoading();

    $.get(`/Admin/GetEmailsByStatus?status=${status}`, function (data) {
        if (!data || data.length === 0) {
            $("#emailQueueContainer").html(`<div class="text-center text-sm text-gray-400 py-6">Bu statüde e-posta bulunamadı.</div>`);
            if (!silent) hideLoading();
            return;
        }

        let rows = data.map(e => {
            let statusColor = getStatusBadgeClass(e.status);
            let extraInfo = "";

            if (e.status === "Sent" && e.sentAt) {
                extraInfo = `<div class="text-xs text-gray-400 mt-1">Gönderildi: ${formatDate(e.sentAt)}</div>`;
            } else if (e.status === "Failed" && e.errorMessage) {
                extraInfo = `<div class="mt-2"><div class="text-xs text-red-700 bg-red-50 border border-red-200 px-2 py-1 rounded-md break-words max-w-xs">Hata: ${escapeHtml(e.errorMessage)}</div></div>`;
            }

            let actionButtons = `
                <button class="text-indigo-600 hover:text-indigo-900 text-sm p-2 rounded-md hover:bg-gray-100"
                        title="Gövdeyi Gör"
                        data-subject="${escapeHtml(e.subject)}"
                        data-body="${encodeURIComponent(e.body)}"
                        data-to="${escapeHtml(e.to)}"
                        data-sent="${e.sentAt ? formatDate(e.sentAt) : ""}"
                        onclick="showEmailBodyFromBtn(this)">
                    <i class="fas fa-envelope-open-text"></i>
                </button>`;

            if (e.status === "Pending") {
                actionButtons += `
                    <button class="text-gray-500 hover:text-gray-700 text-sm p-2 rounded-md hover:bg-gray-100 ml-1"
                            title="İptal Et" onclick="cancelEmail('${e.id}')">
                        <i class="fas fa-circle-pause"></i>
                    </button>`;
            } else if (e.status === "Failed" || e.status === "Cancelled") {
                actionButtons += `
                    <button class="text-yellow-600 hover:text-yellow-800 text-sm p-2 rounded-md hover:bg-gray-100 ml-1"
                            title="Yeniden Dene" onclick="retryEmail('${e.id}')">
                        <i class="fas fa-rotate-right"></i>
                    </button>`;
            }

            if (!silent) hideLoading();

            return `
                <tr>
                    <td class="py-4 px-4 text-sm break-words">${e.to}</td>
                    <td class="py-4 px-4 text-sm break-words">${e.subject}</td>
                    <td class="py-4 px-4 text-sm">${formatDate(e.createdAt)}</td>
                    <td class="py-4 px-4 text-sm">
                        <span class="inline-flex items-center gap-2 text-xs px-3 py-1 rounded-full border ${statusColor}">
                            ${e.status}
                            ${e.status === "Pending" ? '<i class="fas fa-spinner fa-spin text-xs"></i>' : ''}
                        </span>
                        ${extraInfo}
                    </td>
                    <td class="py-4 px-4 text-sm">${e.tryCount}</td>
                    <td class="py-4 px-4 text-sm">${formatDate(e.updatedAt)}</td>
                    <td class="py-4 px-4 text-sm">
                        <div class="flex flex-wrap gap-2 items-center justify-start min-w-[120px]">
                            ${actionButtons}
                        </div>
                    </td>
                </tr>`;
        }).join("");

        $("#emailQueueContainer").html(`
            <div class="overflow-x-auto">
                <table class="min-w-full bg-white border border-gray-200 rounded-lg overflow-hidden text-sm">
                    <thead>
                        <tr class="bg-gray-50 border-b border-gray-200 text-xs text-gray-500 uppercase">
                            <th class="py-3 px-4 text-left">Alıcı</th>
                            <th class="py-3 px-4 text-left">Konu</th>
                            <th class="py-3 px-4 text-left">Oluşturulma</th>
                            <th class="py-3 px-4 text-left">Durum</th>
                            <th class="py-3 px-4 text-left">Deneme</th>
                            <th class="py-3 px-4 text-left">Güncellenme</th>
                            <th class="py-3 px-4 text-left">İşlem</th>
                        </tr>
                    </thead>
                    <tbody class="divide-y divide-gray-200">${rows}</tbody>
                </table>
            </div>`);
    }).fail(function () {
        $("#emailQueueContainer").html(`<div class="text-center text-red-500 text-sm py-6">E-postalar yüklenemedi.</div>`);
    });
}


function cancelEmail(id) {
    if (!confirm("Bu e-postayı iptal etmek istediğinizden emin misiniz?")) return;

    $.post(`/Admin/CancelEmail`, {
        id: id,
        __RequestVerificationToken: getCsrfToken()
    }, function (res) {
        if (res.success) {
            setTimeoutAlert("s", "#emailQueueAlert", res.message || "E-posta iptal edildi", 5);
        } else {
            setTimeoutAlert("e", "#emailQueueAlert", res.message || "İşlem başarısız", 5);
        }
    }).fail(() => {
        setTimeoutAlert("e", "#emailQueueAlert", "Sunucu hatası oluştu", 5);
    });
}

function retryEmail(id) {
    $.post(`/Admin/RetryEmail`, {
        id: id,
        __RequestVerificationToken: getCsrfToken()
    }, function (res) {
        if (res.success) {
            setTimeoutAlert("s", "#emailQueueAlert", res.message || "Yeniden kuyruğa alındı", 5);
        } else {
            setTimeoutAlert("e", "#emailQueueAlert", res.message || "İşlem başarısız", 5);
        }
    }).fail(() => {
        setTimeoutAlert("e", "#emailQueueAlert", "Sunucu hatası oluştu", 5);
    });
}

function showEmailBodyFromBtn(btn) {
    const subject = $(btn).data("subject");
    const bodyHtml = decodeURIComponent($(btn).data("body"));
    const sentAt = $(btn).data("sent");
    const to = $(btn).data("to");

    $("#emailBodyModalTitle").text(subject);
    $("#emailBodyModalMeta").html(`<span class="font-medium">${to}</span> • ${sentAt ?? "Gönderilmedi"}`);

    const iframe = document.getElementById("emailBodyIframe");
    iframe.srcdoc = bodyHtml;

    $("#emailBodyModal").removeClass("hidden");
}

function closeEmailBodyModal() {
    $("#emailBodyModal").addClass("hidden");
}

function getStatusBadgeClass(status) {
    switch (status) {
        case "Pending": return "bg-yellow-100 text-yellow-800 border-yellow-300";
        case "Sent": return "bg-green-100 text-green-800 border-green-300";
        case "Failed": return "bg-red-100 text-red-800 border-red-300";
        case "Cancelled": return "bg-gray-100 text-gray-800 border-gray-300";
        default: return "bg-gray-100 text-gray-800 border-gray-300";
    }
}

function formatDate(utcStr) {
    const date = new Date(utcStr);
    return new Intl.DateTimeFormat("tr-TR", {
        year: "numeric",
        month: "2-digit",
        day: "2-digit",
        hour: "2-digit",
        minute: "2-digit",
        second: "2-digit",
        hour12: false
    }).format(date);
}


function escapeBackticks(str) {
    return str?.replace(/`/g, "\\`") ?? "";
}

function escapeHtml(str) {
    return str?.replace(/"/g, "&quot;").replace(/'/g, "&#39;") ?? "";
}

function getCsrfToken() {
    return $('input[name="__RequestVerificationToken"]').val();
}

// SignalR bağlantısı
let notificationConnection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/notifications")
    .withAutomaticReconnect()
    .build();

notificationConnection.start().then(() => {
    console.log("SignalR bağlantısı kuruldu.");
}).catch(err => {
    console.error("SignalR bağlantı hatası:", err);
});

notificationConnection.on("emailQueueUpdated", (status) => {
    const activeTab = $(".tab-btn[data-active='true']").data("status");
    if (activeTab === status) {
        loadEmailQueue(status, true); // sessiz güncelle
    }
});
