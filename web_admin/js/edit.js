import { getPOIById, updatePOI } from "./api.js";

document.addEventListener("DOMContentLoaded", async () => {
    const urlParams = new URLSearchParams(window.location.search);
    const poiId = urlParams.get('id');
    
    if (!poiId) return window.location.href = "index.html";

    const map = L.map('map').setView([10.7769, 106.7009], 13);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(map);

    let marker;
    let currentImage = ""; 
    let currentViews = 0, currentQrScans = 0, currentAudioListens = 0;

    // TẢI DỮ LIỆU
    try {
        const data = await getPOIById(poiId);
        document.getElementById("name").value = data.name || data.Name || "";
        document.getElementById("address").value = data.address || data.Address || "";
        document.getElementById("lat").value = data.lat || data.Lat || "";
        document.getElementById("lng").value = data.lng || data.Lng || "";
        document.getElementById("desc").value = data.description || data.Description || "";

        currentViews = data.views || 0;
        currentQrScans = data.qrScans || 0;
        currentAudioListens = data.audioListens || 0;

        if (data.image || data.Image) {
            currentImage = (data.image || data.Image).split('?')[0].split('/').pop();
            document.getElementById("fileName").innerText = "Ảnh hiện tại: " + currentImage;
        }
        const latLng = [data.lat || data.Lat, data.lng || data.Lng];
        map.setView(latLng, 16);
        marker = L.marker(latLng).addTo(map);
    } catch (e) { alert("Lỗi tải dữ liệu POI cũ"); }

    // BẢN ĐỒ
    map.on('click', async (e) => {
        const { lat, lng } = e.latlng;
        document.getElementById("lat").value = lat.toFixed(6);
        document.getElementById("lng").value = lng.toFixed(6);
        if (marker) map.removeLayer(marker);
        marker = L.marker([lat, lng]).addTo(map);
        try {
            const res = await fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}`);
            const data = await res.json();
            if (data?.display_name) document.getElementById("address").value = data.display_name;
        } catch(err){}
    });

    // CHỌN ẢNH
    let selectedFile = null;
    document.getElementById("fileInput").addEventListener("change", (e) => {
        selectedFile = e.target.files[0];
        if (selectedFile) {
            document.getElementById("fileName").innerText = "Sẽ đổi thành: " + selectedFile.name;
            document.getElementById("fileName").style.color = "#10b981";
        }
    });

    // SUBMIT
    document.getElementById("poiForm").addEventListener("submit", async (e) => {
        e.preventDefault();
        const btn = document.querySelector('.submit');
        const origText = btn.innerText;
        btn.innerText = "Đang xử lý...";
        btn.disabled = true;

        try {
            let finalImg = currentImage;
            
            if (selectedFile) {
                const formData = new FormData();
                formData.append("file", selectedFile);
                const upRes = await fetch("http://127.0.0.1:5188/upload-image", { method: "POST", body: formData });
                if (!upRes.ok) throw new Error("Upload ảnh thất bại");
                const upData = await upRes.json();
                // Bắt cả 2 trường hợp viết hoa/viết thường
                finalImg = upData.fileName || upData.filename || finalImg; 
            }

            const desc = document.getElementById("desc").value.trim();
            // Tạm thời bỏ qua Google Dịch để test cho nhanh
            const poi = {
                name: document.getElementById("name").value.trim(), 
                address: document.getElementById("address").value.trim(), 
                lat: parseFloat(document.getElementById("lat").value), 
                lng: parseFloat(document.getElementById("lng").value), 
                image: finalImg,
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

            const saveRes = await updatePOI(poiId, poi);
            if (!saveRes.ok) throw new Error("Lỗi lưu xuống Database");

            alert("Cập nhật thành công!");
            window.location.href = "index.html";
        } catch(err) {
            alert("Lỗi: " + err.message);
            btn.innerText = origText;
            btn.disabled = false;
        }
    });
});