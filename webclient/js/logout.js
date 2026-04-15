document.addEventListener("DOMContentLoaded", () => {
    const logoutBtn = document.querySelector(".sidebar-menu li:last-child");

    if (logoutBtn) {
        logoutBtn.addEventListener("click", () => {
            const confirmLogout = confirm("Xác nhận đăng xuất?");

            if (!confirmLogout) return;

            // Chuyển về login
            window.location.href = "loginclient.html";
        });
    }
});