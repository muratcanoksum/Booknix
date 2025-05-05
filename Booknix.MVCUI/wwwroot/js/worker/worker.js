// Randevu durumu güncelleme işlemleri
$(document).off("click", ".update-appointment-status").on("click", ".update-appointment-status", function (e) {
    e.preventDefault();

    const appointmentId = $(this).data("id");
    const status = $(this).data("status");

    if (!appointmentId || !status) {
        alert("Randevu bilgisi bulunamadı!");
        return;
    }

    // Konfirmasyon modalını göster
    $("#confirm-status-yes").data("id", appointmentId);
    $("#confirm-status-yes").data("status", status);
    
    // Status değeri için doğru Türkçe metni göster
    const statusText = status === "Approved" ? "Onaylamak" : 
                      status === "Completed" ? "Tamamlamak" : 
                      status === "NoShow" ? "Gelmedi olarak işaretlemek" : 
                      status === "Cancelled" ? "İptal etmek" : "Güncellemek";
    
    $("#status-action-text").text(statusText);
    $("#confirm-status-modal").removeClass("hidden").addClass("flex");
});

// Randevu durum güncellemesi onayla
$(document).off("click", "#confirm-status-yes").on("click", "#confirm-status-yes", function () {
    const appointmentId = $(this).data("id");
    const status = $(this).data("status");
    
    // Modalı kapat
    $("#confirm-status-modal").removeClass("flex").addClass("hidden");
    
    // CSRF token al
    const csrfToken = $('input[name="__RequestVerificationToken"]').val();
    
    // Güncelleme AJAX isteği
    $.ajax({
        type: "POST",
        url: "/Worker/UpdateAppointmentStatus",
        data: {
            appointmentId: appointmentId,
            status: status
        },
        headers: {
            "RequestVerificationToken": csrfToken
        },
        success: function (response) {
            // Randevu listesini yeniden yükle
            location.reload();
        },
        error: function (xhr) {
            const errorMsg = xhr.responseText || "Randevu durumu güncellenirken bir hata oluştu.";
            alert(errorMsg);
        }
    });
});

// İptal modalını kapat
$(document).off("click", "#confirm-status-no").on("click", "#confirm-status-no", function() {
    $("#confirm-status-modal").removeClass("flex").addClass("hidden");
});

// Başarı mesajlarını 10 saniye sonra kaybet
$(function () {
    setTimeout(function () {
        $("#success-message").fadeOut("slow", function () {
            $(this).remove();
        });
    }, 10000); // 10 saniye
}); 