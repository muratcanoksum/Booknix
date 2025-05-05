// Randevu iptal etme işlemi
$(document).off("click", ".cancel-appointment").on("click", ".cancel-appointment", function (e) {
    e.preventDefault();

    const appointmentId = $(this).data("id");

    if (!appointmentId) {
        alert("Randevu bilgisi bulunamadı!");
        return;
    }

    if (confirm("Bu randevuyu iptal etmek istediğinizden emin misiniz?")) {
        $.ajax({
            type: "POST",
            url: "/Account/CancelAppointment/" + appointmentId,
            data: {
                id: appointmentId
            },
            success: function (response) {
                // Randevu listesini yeniden yükle
                $.get("/Account/Appointments", function (html) {
                    $("#appointments-content").html(html);

                    // Başarılı bildirim ekle
                    const alertHtml = `
                        <div class="bg-green-100 border-l-4 border-green-500 text-green-700 p-4 mb-4" role="alert">
                            <p>${response}</p>
                        </div>
                    `;
                    $("#appointments-content").prepend(alertHtml);

                    // 5 saniye sonra bildirimi kaldır
                    setTimeout(function () {
                        $("#appointments-content .bg-green-100").fadeOut("slow", function () {
                            $(this).remove();
                        });
                    }, 5000);
                });
            },
            error: function (xhr) {
                const errorMsg = xhr.responseText || "Randevu iptal işlemi başarısız oldu.";
                alert(errorMsg);
            }
        });
    }
});

// Detay görüntüleme
$(document).off("click", "a[data-id]").on("click", "a[data-id]:not(.cancel-appointment)", function (e) {
    e.preventDefault();

    const appointmentId = $(this).data("id");

    if (!appointmentId) {
        alert("Randevu bilgisi bulunamadı!");
        return;
    }

    // Detay modal'ı aç
    $.get("/Account/AppointmentDetail/" + appointmentId, function (html) {
        // Modal içeriğini doldur ve göster
        $("#modal-container").html(html).removeClass("hidden");
    }).fail(function () {
        alert("Randevu detayları yüklenemedi!");
    });
});

// Başarı mesajlarını 10 saniye sonra kaybet
$(function () {
    setTimeout(function () {
        $("#succes-appointment-msg").fadeOut("slow", function () {
            $(this).remove();
        });
    }, 10000); // 10 saniye
});
