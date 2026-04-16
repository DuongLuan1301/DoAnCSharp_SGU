import { getPOIById, updatePOI } from "./api.js";

// === HÀM MỚI: TỰ ĐỘNG DỊCH BẰNG GOOGLE TRANSLATE ===
// Dùng phương thức POST để tránh lỗi nếu đoạn văn mô tả quá dài
async function translateText(text, targetLang) {
    if (!text) return "";
    try {
        const res = await fetch(`https://translate.googleapis.com/translate_a/single?client=gtx&sl=vi&tl=${targetLang}&dt=t`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: `q=${encodeURIComponent(text)}`
        });
        const data = await res.json();
        return data[0].map(item => item[0]).join('');
    } catch (err) {
        console.error(`Lỗi dịch sang ${targetLang}:`, err);
        return text; // Nếu lỗi mạng thì dùng tạm tiếng Việt
    }
}

document.addEventListener("DOMContentLoaded", async () => {
    const urlParams = new URLSearchParams(window.location.search);
    const poiId = urlParams.get('id');
    
    if (!poiId) return window.location.href = "index.html";

    const map = L.map('map').setView([10.7769, 106.7009], 13);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(map);

    let marker;
    let currentImage = ""; 
    let currentViews = 0, currentQrScans = 0, currentAudioListens = 0;

    // TẢI DỮ LIỆU CŨ LÊN FORM
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

    // XỬ LÝ CLICK TRÊN BẢN ĐỒ
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

    // CHỌN ẢNH MỚI
    let selectedFile = null;
    document.getElementById("fileInput").addEventListener("change", (e) => {
        selectedFile = e.target.files[0];
        if (selectedFile) {
            document.getElementById("fileName").innerText = "Sẽ đổi thành: " + selectedFile.name;
            document.getElementById("fileName").style.color = "#10b981";
        }
    });

    // NHẤN NÚT SUBMIT
    document.getElementById("poiForm").addEventListener("submit", async (e) => {
        e.preventDefault();
        const btn = document.querySelector('.submit');
        const origText = btn.innerText;
        btn.innerText = "Đang xử lý...";
        btn.disabled = true;

        try {
            let finalImg = currentImage;
            
            // Xử lý upload ảnh nếu có
            if (selectedFile) {
                const formData = new FormData();
                formData.append("file", selectedFile);
                const upRes = await fetch("http://127.0.0.1:5188/upload-image", { method: "POST", body: formData });
                if (!upRes.ok) throw new Error("Upload ảnh thất bại");
                const upData = await upRes.json();
                finalImg = upData.fileName || upData.filename || finalImg; 
            }

            const desc = document.getElementById("desc").value.trim();
            
            // === ĐÃ SỬA: GỌI API DỊCH TRƯỚC KHI ĐÓNG GÓI ===
            btn.innerText = "Đang dịch ngôn ngữ...";
            const descEn = await translateText(desc, 'en');
            const descJa = await translateText(desc, 'ja');
            const descZh = await translateText(desc, 'zh');
            btn.innerText = "Đang lưu trữ...";

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
                    { lang: "en", description: descEn },
                    { lang: "ja", description: descJa },
                    { lang: "zh", description: descZh }
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