let selectedLocationId = null;

function showDeleteModal(id) {
    selectedLocationId = id;
    $("#delete-location-id").val(id);
    showModal("delete-modal");
}

function hideDeleteModal() {
    selectedLocationId = null;
    hideModal("delete-modal");
}

$(document).off("submit", "#delete-location-form").on("submit", "#delete-location-form", function (e) {
    e.preventDefault();

    const token = $(this).find('input[name="__RequestVerificationToken"]').val();

    fetch(`/Admin/Location/Delete/${selectedLocationId}`, {
        method: 'POST',
        headers: {
            'RequestVerificationToken': token
        }
    })
        .then(res => {
            if (res.ok) {
                window.location.href = "/Admin/Location";
            } else {
                return res.text().then(msg => {
                    setTimeoutAlert("#error-info", msg);
                });
            }
        })
        .catch(() => {
            setTimeoutAlert("#error-info", "Sunucuya ulaşılamadı.");
        })
        .finally(hideDeleteModal);
});
