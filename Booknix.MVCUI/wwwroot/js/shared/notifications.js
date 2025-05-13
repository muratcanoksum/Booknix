$(function () {
    const userId = $("#notification-bell").data("userid");

    let notifConnection = new signalR.HubConnectionBuilder()
        .withUrl(`/hubs/notifications?userId=${userId}`)
        .configureLogging(signalR.LogLevel.None)
        .withAutomaticReconnect()
        .build();

    notifConnection.start().then(() => {
        console.log("Notification SignalR bağlantısı kuruldu.");
    }).catch(err => console.error("SignalR bağlantı hatası:", err));

    notifConnection.on("notificationReceived", (data) => {
        loadNotifications();
        triggerBellAnimation();
    });

    // Sayfa yüklenince bildirimi oku
    loadNotifications();

    // Zil ikonuna tıklayınca paneli aç/kapat
    $("#animatedBell").on("click", function (e) {
        e.stopPropagation();
        $("#notification-panel").toggleClass("hidden");
    });

    // Dışarı tıklayınca paneli kapat
    $(document).on("click", function (e) {
        if (!$(e.target).closest("#notification-bell").length) {
            $("#notification-panel").addClass("hidden");
        }
    });

    $("#mark-all-read").on("click", function () {
        $.post("/Notification/MarkAllAsRead", function () {
            loadNotifications();
        });
    });

    $("#delete-all-notification").on("click", function () {
        if (confirm("Tüm bildirimleri silmek istediğinize emin misiniz?")) {
            $.post("/Notification/DeleteAll", function () {
                loadNotifications();
            });
        }
    });

    function loadNotifications() {
        $.get(`/Notification/GetUserNotifications`, function (notifications) {
            const list = $("#notification-list");
            list.empty();

            if (notifications.length === 0) {
                list.html('<div class="p-4 text-gray-400 text-sm text-center">Bildirim yok.</div>');
                $("#notification-count").addClass("hidden");
                $("#notification-icon").removeClass("text-yellow-500").addClass("text-gray-600");
                $("#mark-all-read").addClass("hidden");
                $("#delete-all-notification").addClass("hidden");
                return;
            }

            $("#mark-all-read").removeClass("hidden");
            $("#delete-all-notification").removeClass("hidden");

            let unreadCount = 0;

            const sorted = notifications.sort((a, b) => {
                if (a.isRead === b.isRead) {
                    return new Date(b.createdAt) - new Date(a.createdAt);
                }
                return a.isRead ? 1 : -1;
            });

            sorted.forEach(n => {
                const isUnread = !n.isRead;
                if (isUnread) unreadCount++;

                list.append(`
                    <div class="flex justify-between items-center p-3 hover:bg-gray-50 cursor-pointer notification-item ${isUnread ? 'font-semibold' : ''}" data-id="${n.id}">
                        <div>
                            <div class="text-sm">${n.title}</div>
                            <div class="text-xs text-gray-500 mt-1">${n.message}</div>
                            <div class="text-[10px] text-gray-400 mt-1">${formatDate(n.createdAt)}</div>
                        </div>
                        <button class="delete-notification text-gray-400 hover:text-red-600 text-xs ml-2" data-id="${n.id}">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>
                `);
            });

            if (unreadCount > 0) {
                $("#notification-count").text(unreadCount).removeClass("hidden");
                $("#notification-icon").removeClass("text-gray-600").addClass("text-yellow-500");
            } else {
                $("#notification-count").addClass("hidden");
                $("#notification-icon").removeClass("text-yellow-500").addClass("text-gray-600");
            }
        });
    }

    $(document).off("click", ".notification-item").on("click", ".notification-item", function () {
        const id = $(this).data("id");
        markAsRead(id);
    });

    $(document).off("click", ".delete-notification").on("click", ".delete-notification", function (e) {
        e.stopPropagation();
        const id = $(this).data("id");
        deleteNotification(id);
    });

    function markAsRead(id) {
        $.post(`/Notification/MarkAsRead`, { id }, function () {
            loadNotifications();
        });
    }

    function deleteNotification(id) {
        $.post(`/Notification/Delete`, { notificationId: id }, function () {
            loadNotifications();
        });
    }

    function formatDate(utcStr) {
        const date = new Date(utcStr);
        return new Intl.DateTimeFormat("tr-TR", {
            dateStyle: "short",
            timeStyle: "short"
        }).format(date);
    }

    function triggerBellAnimation() {
        const icon = $("#animatedBell");
        icon.addClass("animate-bounce");

        setTimeout(() => {
            icon.removeClass("animate-bounce");
        }, 4000);
    }
});
