/**
 * Xử lý lưu dữ liệu gian hàng (Chuẩn cấu trúc MongoDB)
 * @param {Event} event - Sự kiện submit form
 */
function saveStoreData(event) {
    event.preventDefault();

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

    alert("Thông tin gian hàng đã được cập nhật thành công!");
}

/**
 * Giả lập thay đổi mật độ truy cập theo thời gian thực (Live Density)
 */
function initLiveDensity() {
    const bar = document.getElementById('density-bar');

    // GHI CHÚ: chỉ chạy khi tồn tại (tránh lỗi ở trang edit.html)
    if (!bar) return;

    setInterval(() => {
        const randomWidth = Math.floor(Math.random() * (90 - 45) + 45);
        bar.style.width = randomWidth + "%";
    }, 4000);
}

// Khởi tạo khi load trang
window.onload = function () {
    initLiveDensity();
};