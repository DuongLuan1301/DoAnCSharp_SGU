// Hàm chuyển đổi giữa form đăng nhập và đăng ký
function toggleView() {
    const loginSection = document.getElementById('login-section');
    const registerSection = document.getElementById('register-section');
    const title = document.getElementById('form-title');
    const subtitle = document.getElementById('form-subtitle');

    // Nếu đang ở form đăng nhập -> chuyển sang đăng ký
    if (!loginSection.classList.contains('hidden')) {
        loginSection.classList.add('hidden');
        registerSection.classList.remove('hidden');
        title.innerText = 'Đăng ký';
        subtitle.innerText = 'Tạo tài khoản để khám phá món ngon!';
    }
    // Nếu đang ở form đăng ký -> chuyển về đăng nhập
    else {
        registerSection.classList.add('hidden');
        loginSection.classList.remove('hidden');
        title.innerText = 'Đăng nhập';
        subtitle.innerText = 'Chào mừng trở lại khu phố ẩm thực!';
    }
}

// Xử lý sự kiện khi ấn nút Đăng nhập
function handleLogin(event) {
    event.preventDefault(); // Ngăn chặn load lại trang

    // Nơi này sau sẽ gọi API tới Backend ASP.NET Core

}

// Xử lý đăng ký
async function handleRegister(event) {
    event.preventDefault();

    const name = document.getElementById("name").value.trim();
    const phone = document.getElementById("phoneNumber").value.trim();
    const email = document.getElementById("email").value.trim();
    const password = document.getElementById("passwd").value.trim();

    // =========================
    // (7) UX: DISABLE BUTTON + LOADING
    // =========================
    const btn = event.target.querySelector("button[type='submit']");
    const originalText = btn.innerText;

    btn.disabled = true;
    btn.innerText = "Đang xử lý...";


    try {
        const res = await fetch("http://localhost:5188/api/auth/register", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                name,
                phone,
                email,
                password
            })
        });

        if (!res.ok) {
            const err = await res.json();
            throw new Error(err.message);
        }

        alert("Đăng ký thành công!");

        // =========================
        // (8) RESET FORM SAU KHI ĐĂNG KÝ
        // =========================
        document.getElementById("register-section").reset();

        // quay về login
        toggleView();

    } catch (err) {
        console.error(err);
        alert("Lỗi: " + err.message);

    } finally {
        // restore button
        btn.disabled = false;
        btn.innerText = originalText;
    }
}
