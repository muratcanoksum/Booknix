document.addEventListener("DOMContentLoaded", () => {
    let selectedButton = null;
    let selectedTime = null;

    document.querySelectorAll("button[data-slot-time]").forEach(button => {
        button.addEventListener("click", () => {
            if (selectedButton) {
                selectedButton.classList.remove("ring", "ring-offset-2", "ring-indigo-600");
            }

            if (selectedButton === button) {
                selectedButton = null;
                selectedTime = null;
                document.getElementById("selectedTime").textContent = "Henüz seçilmedi";
                return;
            }

            selectedButton = button;
            selectedTime = button.getAttribute("data-slot-time");

            button.classList.add("ring", "ring-offset-2", "ring-indigo-600");
            document.getElementById("selectedTime").textContent = selectedTime;
        });
    });

    document.getElementById("confirm-appointment")?.addEventListener("click", () => {
        if (!selectedTime) {
            alert("Lütfen bir saat seçiniz.");
            return;
        }

        // TODO: Ajax ile post işlemi yapılabilir
        console.log("Seçilen saat:", selectedTime);
    });
});

const card = document.getElementById("infoCard");

window.addEventListener("scroll", () => {
    const offset = window.scrollY;
    card.style.transform = `translateY(${offset}px)`;
});