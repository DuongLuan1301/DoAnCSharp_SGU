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