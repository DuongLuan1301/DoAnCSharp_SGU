document.addEventListener("DOMContentLoaded", async () => {
    // 1. Lấy ID gian hàng từ thanh URL
    const urlParams = new URLSearchParams(window.location.search);
    const poiId = urlParams.get('id');
    
    if (!poiId) {
        alert("Không tìm thấy mã gian hàng!");
        window.location.href = "list.html"; 
        return;
    }

    const clientId = localStorage.getItem("clientId");
    if (!clientId) return window.location.href = "loginclient.html";

    // 2. Khởi tạo bản đồ
    let map = L.map('map').setView([10.7769, 106.7009], 15);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(map);
    let marker = L.marker([10.7769, 106.7009], { draggable: true }).addTo(map);

    let currentViews = 0, currentQrScans = 0, currentAudioListens = 0;

    // 3. TẢI DỮ LIỆU CŨ TỪ DATABASE
    try {
        const res = await fetch(`http://127.0.0.1:5188/api/poi/${poiId}`);
        if (!res.ok) throw new Error("Không thể tải dữ liệu");
       const data = await res.json();

        // 🔥 BẢO MẬT: Kiểm tra xem POI này có phải của người đang đăng nhập không
        if (data.clientId && data.clientId !== clientId) {
            alert("⛔ CẢNH BÁO: Bạn không có quyền chỉnh sửa gian hàng của người khác!");
            window.location.href = "index.html"; // Đá văng ra ngoài
            return;
        }

        document.getElementById('store-name').value = data.name || data.Name || "";
        document.getElementById('store-address').value = data.address || data.Address || "";
        document.getElementById('store-lat').value = data.lat || data.Lat || "";
        document.getElementById('store-lng').value = data.lng || data.Lng || "";
        document.getElementById('desc-vi').value = data.description || data.Description || "";
        
        const imgInput = document.getElementById('store-image');
        if(imgInput) imgInput.value = data.image || data.Image || "";

        currentViews = data.views || 0;
        currentQrScans = data.qrScans || 0;
        currentAudioListens = data.audioListens || 0;

        const latInit = data.lat || data.Lat || 10.7769;
        const lngInit = data.lng || data.Lng || 106.7009;
        map.setView([latInit, lngInit], 16);
        marker.setLatLng([latInit, lngInit]);

    } catch (error) {
        alert("Lỗi tải thông tin gian hàng: " + error.message);
    }

    // Tương tác kéo thả Bản đồ
    marker.on('dragend', function() {
        const pos = marker.getLatLng();
        document.getElementById('store-lat').value = pos.lat.toFixed(6);
        document.getElementById('store-lng').value = pos.lng.toFixed(6);
    });

    map.on('click', function(e) {
        marker.setLatLng(e.latlng);
        document.getElementById('store-lat').value = e.latlng.lat.toFixed(6);
        document.getElementById('store-lng').value = e.latlng.lng.toFixed(6);
    });

    // 4. SUBMIT DỮ LIỆU CẬP NHẬT
    const form = document.getElementById("stall-form");
    if(form) {
        form.onsubmit = null; // Bỏ onsubmit html
        form.addEventListener("submit", async (e) => {
            e.preventDefault();
            
            const btnSave = document.querySelector(".btn-submit");
            if(btnSave) { btnSave.innerText = "Đang lưu..."; btnSave.disabled = true; }

            const desc = document.getElementById('desc-vi').value.trim();
            const finalImg = document.getElementById('store-image') ? document.getElementById('store-image').value.trim() : "default.jpg";
            
            const updatedPoi = {
                clientId: clientId, // 🔥 BẮT BUỘC ĐỂ KHÔNG MẤT QUYỀN SỞ HỮU
                name: document.getElementById('store-name').value.trim(),
                address: document.getElementById('store-address').value.trim(),
                image: finalImg,
                lat: parseFloat(document.getElementById('store-lat').value),
                lng: parseFloat(document.getElementById('store-lng').value),
                views: currentViews,
                qrScans: currentQrScans,
                audioListens: currentAudioListens,
                localizations: [
                    { lang: "vi", description: desc },
                    { lang: "en", description: desc },
                    { lang: "ja", description: desc },
                    { lang: "zh", description: desc }
                ]
            };

            try {
                const res = await fetch(`http://127.0.0.1:5188/api/poi/${poiId}`, {
                    method: "PUT",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(updatedPoi)
                });

                if (!res.ok) throw new Error("Lỗi cập nhật Database");
                
                alert("Cập nhật thông tin gian hàng thành công!");
                window.location.href = "list.html";
            } catch (err) {
                alert("Lỗi: " + err.message);
                if(btnSave) { btnSave.innerText = "Cập Nhật"; btnSave.disabled = false; }
            }
        });
    }
});