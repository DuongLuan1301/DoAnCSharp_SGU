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
    console.log("Đang gọi API Đăng nhập...");
    alert("Đăng nhập thành công! Đang chuyển hướng vào Khu Phố...");
}

// Xử lý sự kiện khi ấn nút Đăng ký
function handleRegister(event) {
    event.preventDefault(); // Ngăn chặn load lại trang
    
    // Nơi này sau sẽ gọi API tới Backend ASP.NET Core
    console.log("Đang gọi API Đăng ký...");
    alert("Đăng ký thành công! Vui lòng đăng nhập lại.");
    
    // Chuyển về trang đăng nhập sau khi đăng ký thành công
    toggleView(); 
}

