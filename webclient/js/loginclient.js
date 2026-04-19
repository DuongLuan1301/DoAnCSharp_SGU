function toggleView() {
    const login = document.getElementById("login-section");
    const register = document.getElementById("register-section");

    login.classList.toggle("hidden");
    register.classList.toggle("hidden");
}

// Xử lý sự kiện khi ấn nút Đăng nhập
async function handleLogin(event) {
    event.preventDefault(); // Ngăn chặn load lại trang

    const inputs = event.target.querySelectorAll('input');
    const email = inputs[0].value.trim();
    const password = inputs[1].value.trim();

    const btn = event.target.querySelector("button[type='submit']");
    const originalText = btn.innerText;
    btn.disabled = true;
    btn.innerText = "Đang đăng nhập...";

    try {
        const res = await fetch("http://localhost:5188/api/auth/login", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ email, password })
        });

        if (!res.ok) {
            const err = await res.json();
            throw new Error(err.message || "Đăng nhập thất bại");
        }

        const data = await res.json();

        // 🔥 LƯU LẠI ID CỦA CLIENT VÀO BỘ NHỚ
        localStorage.setItem("clientId", data.id);
        localStorage.setItem("clientName", data.name);

        alert("Đăng nhập thành công!");
        window.location.href = "index.html"; // Chuyển sang trang Dashboard
    } catch (err) {
        alert("Lỗi: " + err.message);
    } finally {
        btn.disabled = false;
        btn.innerText = originalText;
    }
}
//xử lý sự kiện khi ấn đăng ký
async function handleRegister(event) {
    event.preventDefault();

    const name = document.getElementById("name").value.trim();
    const phoneNumber = document.getElementById("phoneNumber").value.trim();
    const email = document.getElementById("email").value.trim();
    const password = document.getElementById("passwd").value.trim();

    const btn = event.target.querySelector("button[type='submit']");
    btn.disabled = true;
    btn.innerText = "Đang tạo tài khoản...";

    const res = await fetch("http://localhost:5188/api/auth/register", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            name,
            phoneNumber,
            email,
            password
        })
    });
    alert("Đăng ký thành công!");

    // quay lại login
    toggleView();

    btn.disabled = false;
    btn.innerText = "Tạo Tài Khoản";
}