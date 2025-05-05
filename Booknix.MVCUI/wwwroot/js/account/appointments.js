// Randevu iptal etme işlemi
$(document).off("click", ".cancel-appointment").on("click", ".cancel-appointment", function (e) {
    e.preventDefault();

    const appointmentId = $(this).data("id");

    if (!appointmentId) {
        alert("Randevu bilgisi bulunamadı!");
        return;
    }

    // Konfirmasyon modalını göster
    $("#confirm-cancel-yes").data("id", appointmentId);
    $("#confirm-cancel-modal").removeClass("hidden").addClass("flex");
});

// Randevu iptal modalından iptal işlemini başlat
$(document).off("click", "#confirm-cancel-yes").on("click", "#confirm-cancel-yes", function () {
    const appointmentId = $(this).data("id");
    
    // Modalı kapat
    $("#confirm-cancel-modal").removeClass("flex").addClass("hidden");
    
    // CSRF token al (sayfada form varsa)
    const csrfToken = $('input[name="__RequestVerificationToken"]').val();
    
    // İptal AJAX isteği
    $.ajax({
        type: "POST",
        url: "/Account/CancelAppointment/" + appointmentId,
        headers: {
            "RequestVerificationToken": csrfToken
        },
        success: function (response) {
            // Randevu listesini yeniden yükle
            $.get("/Account/Appointments", function (html) {
                $("#appointments-content").html(html);
                
                // Başarılı bildirim ekle
                const alertHtml = `
                    <div class="bg-green-100 border-l-4 border-green-500 text-green-700 p-4 mb-4" role="alert">
                        <div class="flex">
                            <div class="flex-shrink-0">
                                <i class="fas fa-check-circle"></i>
                            </div>
                            <div class="ml-3">
                                <p class="text-sm">${response}</p>
                            </div>
                        </div>
                    </div>
                `;
                $("#appointments-content").prepend(alertHtml);
                
                // 5 saniye sonra bildirimi kaldır
                setTimeout(function() {
                    $("#appointments-content .bg-green-100").fadeOut("slow", function() {
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
});

// İptal modalını kapat
$(document).off("click", "#confirm-cancel-no").on("click", "#confirm-cancel-no", function() {
    $("#confirm-cancel-modal").removeClass("flex").addClass("hidden");
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

// Detay modalından iptal et butonuna tıklama
$(document).off("click", ".cancel-appointment-modal").on("click", ".cancel-appointment-modal", function() {
    const appointmentId = $(this).data("id");
    
    // Detay modalini gizle
    $("#modal-container").addClass("hidden");
    
    // Konfirmasyon modalını göster
    $("#confirm-cancel-yes").data("id", appointmentId);
    $("#confirm-cancel-modal").removeClass("hidden").addClass("flex");
});

// Detay modalını kapat
$(document).off("click", ".close-modal").on("click", ".close-modal", function() {
    $("#modal-container").addClass("hidden");
});

// Başarı mesajlarını 10 saniye sonra kaybet
$(function () {
    setTimeout(function () {
        $("#succes-appointment-msg").fadeOut("slow", function () {
            $(this).remove();
        });
    }, 10000); // 10 saniye
});
