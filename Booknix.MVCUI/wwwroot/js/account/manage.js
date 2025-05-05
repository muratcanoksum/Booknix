// Tab değiştirme işlevselliği
$(function() {
    // Tab tıklama olayını dinle
    $(".tab-link").on("click", function(e) {
        e.preventDefault();
        const tabId = $(this).data("tab");
        
        // Tüm tabları ve linkleri deaktif et
        $(".tab-link").removeClass("active border-blue-500 text-blue-700").addClass("border-transparent");
        $(".tab-pane").removeClass("active").addClass("hidden");
        
        // Tıklanan tabı ve linkini aktif et
        $(this).addClass("active border-blue-500 text-blue-700").removeClass("border-transparent");
        $("#" + tabId + "-tab").addClass("active").removeClass("hidden");
        
        // Eğer profil tabı seçiliyse, ilk menü linkini tıkla
        if (tabId === "profile" && !$(".menu-link.bg-blue-100").length) {
            $(".menu-link").first().trigger("click");
        }
        
        // Eğer randevular tabı seçiliyse, randevu içeriğini yükle
        if (tabId === "appointments") {
            loadAppointments();
        }
    });
    
    // Randevu içeriğini yükleme fonksiyonu
    function loadAppointments() {
        $("#appointments-content").html("<div class='text-sm text-gray-500'>Randevu bilgileri yükleniyor...</div>");
        
        $.get("/Account/Appointments", function(html) {
            $("#appointments-content").html(html);
        }).fail(function() {
            $("#appointments-content").html(`
                <div class="flex flex-col items-center justify-center text-center space-y-4 py-8">
                    <i class="fas fa-calendar-times text-4xl text-gray-400"></i>
                    <h2 class="text-xl font-bold text-gray-700">Randevu Bulunamadı</h2>
                    <p class="text-sm text-gray-500">Henüz bir randevu kaydınız bulunmamaktadır.</p>
                </div>
            `);
        });
    }
});

// Menü tıklama olayını dinle
$(function () {
    $(".menu-link").on("click", function (e) {
        e.preventDefault();
        const $link = $(this);
        const url = $link.data("url");

        // Aktif sınıfı tüm menülerden kaldır
        $(".menu-link").removeClass("bg-blue-100 text-blue-700 font-semibold");

        // Tıklanan menüye aktif sınıfı ekle
        $link.addClass("bg-blue-100 text-blue-700 font-semibold");

        // İçeriği yükle
        $("#content-panel").html("Yükleniyor...");
        $.get(url, function (html) {
            $("#content-panel").html(html);
            initCleavePhoneMask();
        }).fail(function () {
            $("#content-panel").html(`
                <div class="flex flex-col items-center justify-center text-center space-y-4">
                <img src="/images/logo.png" alt="Booknix Logo" class="w-24 h-24">
                <h2 class="text-xl font-bold text-gray-700">404 - İçerik Bulunamadı</h2>
                <p class="text-sm text-gray-500">İstediğiniz içerik yüklenemedi veya bulunamadı.</p
                </div>
`);
        });
    });

    // Sayfa ilk açıldığında ilk menü seçili olsun
    $(".menu-link").first().trigger("click");
});

// Pin input için gerekli olan sınıfları ekle
$(document).off('input', '.verify-input').on('input', '.verify-input', function () {
    const $inputs = $('.verify-input');
    const $form = $("#change-email-verify-form");
    const $form2 = $("#delete-account-verify-form");
    const $btn2 = $form2.find("button[type='submit']");
    const $btn = $form.find("button[type='submit']");
    let code = '';

    // Maksimum 1 karakter
    if (this.value.length > 1) {
        this.value = this.value.slice(0, 1);
    }

    // Sadece rakam ise sonraki inputa geç
    if (/^\d$/.test(this.value)) {
        const index = $inputs.index(this);
        if (index < $inputs.length - 1) {
            $inputs.eq(index + 1).focus();
        }
    }

    // Tüm inputlardaki değeri topla
    $inputs.each(function () {
        code += $(this).val();
    });

    // Gizli inputa yaz
    $('#VerificationCode').val(code);

    // 6 haneli değilse butonu kapat
    if (code.length === 6 && /^\d{6}$/.test(code)) {
        $btn.prop('disabled', false);
        $btn2.prop('disabled', false);
    } else {
        $btn.prop('disabled', true);
        $btn2.prop('disabled', true);
    }
});

// Backspace ile önceki inputa geç
$(document).on('keydown', '.verify-input', function (e) {
    const $inputs = $('.verify-input');
    const index = $inputs.index(this);

    // Enter tuşu
    if (e.key === "Enter") {
        if (code.length === 6 && /^\d{6}$/.test(code)) {
            $form.submit(); // ✅ Submit
            $form2.submit(); // ✅ Submit
        } else {
            e.preventDefault(); // ⛔️ Kod eksikse enter'ı engelle
        }
    }

    // Geri tuşu ile geri inputa git
    if (e.key === "Backspace" && this.value === '' && index > 0) {
        $inputs.eq(index - 1).focus();
    }
});

// Paste işlemi
$(document).off('paste', '.verify-input').on('paste', '.verify-input', function (e) {
    const pasteData = e.originalEvent.clipboardData.getData('text').trim();

    if (/^\d{6}$/.test(pasteData)) {
        const $inputs = $('.verify-input');
        for (let i = 0; i < 6; i++) {
            const $input = $inputs.eq(i);
            $input.val(pasteData[i]);

            // 🎉 Bonus: geçici yeşil arka plan animasyonu
            $input.addClass('bg-green-100 ring-2 ring-green-400');

            // 500ms sonra geri eski haline getir
            setTimeout(() => {
                $input.removeClass('bg-green-100 ring-2 ring-green-400');
            }, 500);
        }

        // Kod gizli inputa yaz
        $('#VerificationCode').val(pasteData);

        // Submit butonunu aktif et
        const $form = $("#change-email-verify-form");
        const $form2 = $("#delete-account-verify-form");
        const $btn = $form.find("button[type='submit']");
        const $btn2 = $form2.find("button[type='submit']");
        $btn.prop('disabled', false);
        $btn2.prop('disabled', false);
    }

    e.preventDefault(); // Varsayılan yapıştırmayı engelle
});