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
$(document).off("click", "a[data-id]").on("click", "a[data-id]:not(.cancel-appointment):not(.review-appointment)", function (e) {
    e.preventDefault();

    const appointmentId = $(this).data("id");

    if (!appointmentId) {
        alert("Randevu bilgisi bulunamadı!");
        return;
    }
    console.log("test")

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
    
    // Sayfa yüklendiğinde tamamlanmış ve değerlendirilmiş randevuların yıldızlarını kontrol et
    $(".review-appointment").each(function() {
        const appointmentId = $(this).data("id");
        const reviewBtn = $(this);
        
        // Yorum kontrolü yap
        $.get("/Account/GetReviewByAppointment?appointmentId=" + appointmentId, function(data) {
            if (data && data.rating > 0) {
                const parentContainer = reviewBtn.closest('div');
                
                // Değerlendirme butonunu kaldır
                reviewBtn.remove();
                
                // Yıldızları ekle
                const starsHtml = `
                    <div class="flex items-center text-yellow-500 text-sm stars-container cursor-pointer" 
                         data-id="${appointmentId}" 
                         data-service-id="${reviewBtn.data('service-id')}">
                        ${Array(parseInt(data.rating)).fill('<i class="fas fa-star mr-0.5"></i>').join('')}
                    </div>
                `;
                parentContainer.append(starsHtml);
            }
        });
    });
    
    // Yıldızlara tıklandığında değerlendirme modalını aç
    $(document).off("click", ".stars-container").on("click", ".stars-container", function() {
        const appointmentId = $(this).data("id");
        const serviceId = $(this).data("service-id");
        
        if (!appointmentId || !serviceId) {
            alert("Randevu veya hizmet bilgisi bulunamadı!");
            return;
        }
        
        // Modal form değerlerini temizle
        $("#review-appointment-id").val(appointmentId);
        $("#review-service-id").val(serviceId);
        $("#review-id").val(""); 
        $("#rating-value").val(0);
        $("#review-comment").val("");
        
        // Tüm yıldızları başlangıç durumuna getir
        $(".rating-star").removeClass("text-yellow-400").addClass("text-gray-300");
        
        // Mevcut yorum kontrolü
        $.get("/Account/GetReviewByAppointment?appointmentId=" + appointmentId, function(data) {
            if (data) {
                // Gelen bilgileri doldur
                $("#review-id").val(data.id);
                $("#rating-value").val(data.rating);
                $("#review-comment").val(data.comment);
                
                // Yıldızları düzenle
                $(".rating-star").each(function() {
                    const starRating = $(this).data("rating");
                    if (starRating <= data.rating) {
                        $(this).removeClass("text-gray-300").addClass("text-yellow-400");
                    } else {
                        $(this).removeClass("text-yellow-400").addClass("text-gray-300");
                    }
                });
            }
        }).always(function() {
            // Modalı her durumda göster
            $("#review-modal").removeClass("hidden").addClass("flex");
        });
    });
});

// Değerlendirme modalını aç
$(document).off("click", ".review-appointment").on("click", ".review-appointment", function(e) {
    e.preventDefault();
    
    const appointmentId = $(this).data("id");
    const serviceId = $(this).data("service-id");

    if (!appointmentId || !serviceId) {
        alert("Randevu veya hizmet bilgisi bulunamadı!");
        return;
    }

    // Modal form değerlerini temizle
    $("#review-appointment-id").val(appointmentId);
    $("#review-service-id").val(serviceId);
    $("#review-id").val(""); 
    $("#rating-value").val(0);
    $("#review-comment").val("");
    
    // Tüm yıldızları başlangıç durumuna getir
    $(".rating-star").removeClass("text-yellow-400").addClass("text-gray-300");

    // Mevcut yorum kontrolü
    $.get("/Account/GetReviewByAppointment?appointmentId=" + appointmentId, function(data) {
        if (data) {
            // Gelen bilgileri doldur
            $("#review-id").val(data.id);
            $("#rating-value").val(data.rating);
            $("#review-comment").val(data.comment);

            // Yıldızları düzenle
            $(".rating-star").each(function() {
                const starRating = $(this).data("rating");
                if (starRating <= data.rating) {
                    $(this).removeClass("text-gray-300").addClass("text-yellow-400");
                } else {
                    $(this).removeClass("text-yellow-400").addClass("text-gray-300");
                }
            });

            // Eğer zaten değerlendirme yapılmışsa butonu kaldır
            $(".review-appointment[data-id='" + appointmentId + "']").remove();
        }
    }).always(function() {
        // Modalı her durumda göster
        $("#review-modal").removeClass("hidden").addClass("flex");
    });
});

// Değerlendirme modalını kapat
$(document).off("click", "#close-review-modal").on("click", "#close-review-modal", function() {
    $("#review-modal").removeClass("flex").addClass("hidden");
});

// Yıldız değerlendirmeleri
$(document).off("click", ".rating-star").on("click", ".rating-star", function() {
    const rating = $(this).data("rating");
    $("#rating-value").val(rating);
    
    // Tüm yıldızları sıfırla
    $(".rating-star").removeClass("text-yellow-400").addClass("text-gray-300");
    
    // Seçilen yıldıza kadar olanları renklendir
    $(".rating-star").each(function() {
        if ($(this).data("rating") <= rating) {
            $(this).removeClass("text-gray-300").addClass("text-yellow-400");
        }
    });
});

// Değerlendirme gönder
$(document).off("submit", "#review-form").on("submit", "#review-form", function(e) {
    e.preventDefault();
    
    const rating = $("#rating-value").val();
    if (rating < 1) {
        alert("Lütfen bir değerlendirme puanı seçin.");
        return;
    }
    
    const csrfToken = $('input[name="__RequestVerificationToken"]').val();
    const formData = $(this).serialize();
    
    $.ajax({
        type: "POST",
        url: "/Account/CreateReview",
        data: formData,
        headers: {
            "RequestVerificationToken": csrfToken
        },
        success: function(response) {
            // Modalı kapat
            $("#review-modal").removeClass("flex").addClass("hidden");
            
            // Başarılı mesajı göster
            const alertHtml = `
                <div class="p-4 mb-4 text-sm text-green-800 rounded-lg bg-green-100 border border-green-300 flex items-center gap-3" role="alert">
                    <i class="fas fa-check-circle text-green-600 text-lg"></i>
                    <span>${response}</span>
                </div>
            `;
            
            $(".space-y-4").prepend(alertHtml);
            
            // 5 saniye sonra bildirimi kaldır
            setTimeout(function() {
                $(".bg-green-100").fadeOut("slow", function() {
                    $(this).remove();
                });
            }, 5000);
            
            // Değerlendir butonunu bulup yerine yıldızları koy
            const appointmentId = $("#review-appointment-id").val();
            const ratingValue = parseInt($("#rating-value").val());
            const serviceId = $("#review-service-id").val();
            const reviewButton = $(`.review-appointment[data-id='${appointmentId}']`);
            const parentContainer = reviewButton.closest('div');
            
            // Değerlendirme butonunu kaldır
            reviewButton.remove();
            
            // Yıldızları ekle - _AppointmentsPartial.cshtml dosyasındaki görünümle aynı
            const starsHtml = `
                <div class="flex items-center text-yellow-500 text-sm stars-container cursor-pointer" 
                     data-id="${appointmentId}" 
                     data-service-id="${serviceId}">
                    ${Array(ratingValue).fill('<i class="fas fa-star mr-0.5"></i>').join('')}
                </div>
            `;
            parentContainer.append(starsHtml);
        },
        error: function(xhr) {
            const errorMsg = xhr.responseText || "Değerlendirme gönderilirken bir hata oluştu.";
            alert(errorMsg);
        }
    });
});
