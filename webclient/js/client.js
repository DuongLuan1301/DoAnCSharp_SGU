/**
 * Chuyển đổi giữa các tab Thống kê và Chỉnh sửa
 * @param {string} tabId - ID của tab muốn hiển thị ('stats' hoặc 'edit')
 */
function switchTab(tabId) {
    // Cập nhật trạng thái active trên sidebar
    document.querySelectorAll('.sidebar-menu li').forEach(li => li.classList.remove('active'));
    document.getElementById('menu-' + tabId).classList.add('active');

    // Cập nhật tiêu đề trang
    const titleEl = document.getElementById('topbar-title');
    titleEl.innerText = tabId === 'stats' ? "Tổng Quan Thống Kê" : "Chỉnh Sửa Gian Hàng";

    // Hiển thị nội dung section tương ứng
    document.querySelectorAll('.section').forEach(sec => sec.classList.remove('active'));
    document.getElementById('section-' + tabId).classList.add('active');
}

/**
 * Xử lý lưu dữ liệu gian hàng (Chuẩn cấu trúc MongoDB)
 * @param {Event} event - Sự kiện submit form
 */
function saveStoreData(event) {
    event.preventDefault();
    
    // Đóng gói dữ liệu để gửi lên Server (hoặc in ra console để kiểm tra)
    const storeData = {
        name: document.getElementById('store-name').value,
        address: document.getElementById('store-address').value,
        image: document.getElementById('store-image').value,
        lat: parseFloat(document.getElementById('store-lat').value),
        lng: parseFloat(document.getElementById('store-lng').value),
        localizations: [
            { 
                lang: "vi", 
                description: document.getElementById('desc-vi').value 
            }
        ]
    };

    console.log("Dữ liệu chuẩn bị cập nhật MongoDB:", storeData);
    
    // Thông báo cho người dùng
    alert("Thông tin gian hàng đã được cập nhật thành công!");
}

/**
 * Giả lập thay đổi mật độ truy cập theo thời gian thực (Live Density)
 */
function initLiveDensity() {
    setInterval(() => {
        const bar = document.getElementById('density-bar');
        if (bar) {
            // Tạo một con số ngẫu nhiên từ 45% đến 90%
            const randomWidth = Math.floor(Math.random() * (90 - 45) + 45);
            bar.style.width = randomWidth + "%";
        }
    }, 4000);
}

// Khởi tạo các tính năng khi trang đã sẵn sàng
window.onload = function() {
    initLiveDensity();
};