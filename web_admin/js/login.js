document.addEventListener("DOMContentLoaded", () => {
    const loginForm = document.getElementById("loginForm");
    const errorMsg = document.getElementById("errorMsg");

    // Nếu đã có token (đã đăng nhập), tự động chuyển thẳng vào trang Admin
    if (localStorage.getItem("adminToken")) {
        window.location.href = "index.html";
    }

    loginForm.addEventListener("submit", async (e) => {
        e.preventDefault();
        
        const btn = loginForm.querySelector(".btn-login");
        const originalText = btn.innerText;

        const username = document.getElementById("username").value.trim();
        const password = document.getElementById("password").value.trim();

        btn.innerText = "Đang kiểm tra...";
        btn.disabled = true;
        errorMsg.style.display = "none";

        try {
            // ==========================================
            // GỌI API BACKEND CỦA BẠN TẠI ĐÂY LÚC CHẤM ĐỒ ÁN
            // VD: const res = await fetch("http://127.0.0.1:5188/api/auth/login", { ... })
            // ==========================================

            // GIẢ LẬP ĐĂNG NHẬP (Tạm thời test UI)
            await new Promise(resolve => setTimeout(resolve, 800)); // Đợi 0.8s cho giống thật

            if (username === "admin" && password === "123456") {
                // Đăng nhập đúng -> Lưu token và chuyển trang
                localStorage.setItem("adminToken", "chuoi_token_cua_ban_sau_nay");
                window.location.href = "index.html";
            } else {
                throw new Error("Tên đăng nhập hoặc mật khẩu không chính xác!");
            }

        } catch (error) {
            errorMsg.innerText = error.message;
            errorMsg.style.display = "block";
            btn.innerText = originalText;
            btn.disabled = false;
        }
    });
});