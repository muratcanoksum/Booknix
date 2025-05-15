// Randevu durumu güncelleme işlemleri
$(document)
  .off("click", ".update-appointment-status")
  .on("click", ".update-appointment-status", function (e) {
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
    const statusText =
      status === "Approved"
        ? "onaylamak"
        : status === "Completed"
        ? "tamamlamak"
        : status === "NoShow"
        ? "gelmedi olarak işaretlemek"
        : status === "Cancelled"
        ? "iptal etmek"
        : "güncellemek";

    $("#status-action-text").text(statusText);
    $("#confirm-status-modal").removeClass("hidden").addClass("flex");
  });

// Randevu durum güncellemesi onayla
$(document)
  .off("click", "#confirm-status-yes")
  .on("click", "#confirm-status-yes", function () {
    const appointmentId = $(this).data("id");
    const status = $(this).data("status");

    // Modalı kapat
    $("#confirm-status-modal").removeClass("flex").addClass("hidden");

    // CSRF token al
    const csrfToken = $('input[name="__RequestVerificationToken"]').val();

    // Güncelleme AJAX isteği
    $.ajax({
      type: "POST",
      url: "/LocationAdmin/UpdateAppointmentStatus",
      data: {
        appointmentId: appointmentId,
        status: status,
      },
      headers: {
        RequestVerificationToken: csrfToken,
      },
      success: function (response) {
        if (response && response.success) {
          // Başarılı ise sayfayı yenile veya dinamik güncelle
          // Burada sadece ilgili sekmeyi yeniden yükleyebiliriz
          const locationId = $("#location-meta").data("location-id");
          $.get(
            `/LocationAdmin/GetWorkerAppointmentsWithReviews/${locationId}`,
            function (html) {
              $("#location-operation-panel").html(html);
            }
          );

          // Başarı mesajı göster
          const successHtml = `
                <div id="success-message" class="p-4 mb-4 text-sm text-green-800 rounded-lg bg-green-100 border border-green-300 flex items-center gap-3" role="alert">
                    <i class="fas fa-check-circle text-green-600 text-lg"></i>
                    <span>Randevu durumu başarıyla güncellendi.</span>
                </div>`;

          $("#location-operation-panel").prepend(successHtml);

          // 10 saniye sonra mesajı kaybet
          setTimeout(function () {
            $("#success-message").fadeOut("slow", function () {
              $(this).remove();
            });
          }, 10000);
        } else {
          const errorMsg =
            response && response.message
              ? response.message
              : "Randevu durumu güncellenirken bir hata oluştu.";
          alert(errorMsg);
        }
      },
      error: function (xhr) {
        const response = xhr.responseJSON;
        const errorMsg =
          response && response.message
            ? response.message
            : "Randevu durumu güncellenirken bir hata oluştu.";
        alert(errorMsg);
      },
    });
  });

// İptal modalını kapat
$(document)
  .off("click", "#confirm-status-no")
  .on("click", "#confirm-status-no", function () {
    $("#confirm-status-modal").removeClass("flex").addClass("hidden");
  });

// Değerlendirme detayını göstermek için tıklama olayı
$(document)
  .off("click", ".review-box")
  .on("click", ".review-box", function (e) {
    e.preventDefault();
    e.stopPropagation();

    // Doğrudan eleman üzerinden appointmentId'yi al
    let appointmentId = $(this).data("appointment-id");

    if (!appointmentId) {
      console.error("appointmentId bulunamadı!");
      return;
    }

    // AJAX isteği ile değerlendirme bilgilerini getir
    $.get(
      "/LocationAdmin/GetAppointmentReview?appointmentId=" + appointmentId,
      function (response) {
        if (response && response.success) {
          const data = response.data;

          // Yorumu hazırla
          let comment = data.comment;
          // Yorumun kontrolünü geliştir
          if (!comment || comment.trim() === "") {
            comment = "Yorum yapılmamış";
          }

          let rating = data.rating || 0;
          let userName = data.userName || "Kullanıcı";
          let serviceName = data.serviceName || "Hizmet";
          let date = data.createdAt
            ? new Date(data.createdAt).toLocaleDateString()
            : "";

          // Modal içeriğini oluştur
          const stars = Array(rating)
            .fill('<i class="fas fa-star text-yellow-400"></i>')
            .join("");
          const modalHtml = `
                <div id="review-detail-modal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
                    <div class="bg-white rounded-lg shadow-xl p-6 w-full max-w-md">
                        <div class="flex justify-between items-center mb-4">
                            <h2 class="text-lg font-semibold text-gray-800">Değerlendirme Detayı</h2>
                            <button id="close-review-detail" class="text-gray-500 hover:text-gray-700">
                                <i class="fas fa-times"></i>
                            </button>
                        </div>
                        <div class="border-t border-b border-gray-200 py-4 my-2">
                            <div class="flex items-center mb-3">
                                <div class="mr-2 text-yellow-500">
                                    ${stars}
                                </div>
                                <span class="text-gray-700 font-medium">(${rating}/5)</span>
                            </div>
                            <div class="mb-2">
                                <span class="text-sm text-gray-600">Hizmet: <span class="font-medium text-gray-800">${serviceName}</span></span>
                            </div>
                            <div class="bg-gray-50 p-3 rounded-md mt-2 border border-gray-200 max-h-60 overflow-y-auto">
                                <p class="text-gray-700 font-medium mb-1">Yorum:</p>
                                <p class="text-gray-600 whitespace-pre-line break-words">${comment}</p>
                            </div>
                            <div class="flex justify-between items-center mt-4 text-xs text-gray-500">
                                <span>${userName}</span>
                                <span>${date}</span>
                            </div>
                        </div>
                    </div>
                </div>
            `;

          // Modalı göster
          $("body").append(modalHtml);

          // Modal kapatma olayı
          $(document).on(
            "click",
            "#close-review-detail, #review-detail-modal",
            function (e) {
              if (
                e.target === this ||
                $(e.target).closest("#close-review-detail").length
              ) {
                $("#review-detail-modal").remove();
              }
            }
          );
        } else {
          const errorMsg =
            response && response.message
              ? response.message
              : "Değerlendirme bulunamadı!";
          alert(errorMsg);
        }
      }
    ).fail(function (xhr, status, error) {
      console.error("AJAX Error:", status, error);
      const response = xhr.responseJSON;
      const errorMsg =
        response && response.message
          ? response.message
          : "Değerlendirme bilgileri alınamadı!";
      alert(errorMsg);
    });
  });
