
$("#approveForm").on("submit", function (e) {
    e.preventDefault();

    const $btn = $(this).find("button[type='submit']");

    $btn.prop("disabled", true).text("Onaylanıyor...");

    $.ajax({
        type: "POST",
        url: '/Auth/ApproveIpAjax',
        data: $(this).serialize(),
        success: function (msg) {
            $("#approveForm").fadeOut(200, () => {
                $("#resultMessage").html(`
                            <div class="text-green-600 font-semibold flex flex-col items-center gap-2">
                                <i class="fa-solid fa-circle-check text-4xl"></i>
                                <span>${msg}</span>
                            </div>
                        `);
            });
        },
        error: function (xhr) {
            $("#resultMessage").html(`
                        <div class="text-red-500 font-semibold flex items-center justify-center gap-2">
                            <i class="fa-solid fa-circle-xmark text-lg"></i>
                            <span>${xhr.responseText}</span>
                        </div>
                    `);
        },
        complete: function () {
            $btn.prop("disabled", false).text("IP Onayla");
        }
    });
});