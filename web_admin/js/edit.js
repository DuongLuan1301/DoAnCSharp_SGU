import { getPOIById, updatePOI } from "./api.js";

document.addEventListener("DOMContentLoaded", async () => {
    const urlParams = new URLSearchParams(window.location.search);
    const poiId = urlParams.get('id');
    
    if (!poiId) {
        alert("Không tìm thấy ID địa điểm!");
        return window.location.href = "index.html";
    }

    const map = L.map('map').setView([10.7769, 106.7009], 13);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(map);

    let marker;
    let currentImage = ""; 

    // TẢI DỮ LIỆU CŨ LÊN FORM
    try {
        const data = await getPOIById(poiId);
        
        document.getElementById("name").value = data.name || data.Name || "";
        document.getElementById("address").value = data.address || data.Address || "";
        document.getElementById("lat").value = data.lat || data.Lat || "";
        document.getElementById("lng").value = data.lng || data.Lng || "";
        
        // FIX: Lấy trực tiếp trường description từ API C# trả về
        document.getElementById("desc").value = data.description || data.Description || "";

        if (data.image || data.Image) {
            currentImage = (data.image || data.Image).split('?')[0].split('/').pop();
            document.getElementById("fileName").innerText = "Ảnh hiện tại: " + currentImage;
        }

        const latLng = [data.lat || data.Lat, data.lng || data.Lng];
        map.setView(latLng, 16);
        marker = L.marker(latLng).addTo(map);
    } catch (e) {
        alert("Lỗi tải dữ liệu POI cũ");
    }

    // TƯƠNG TÁC BẢN ĐỒ
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
            document.getElementById("fileName").style.color = "green";
        }
    });

    // HÀM DỊCH AN TOÀN
    async function translateText(text, tl) {
        try {
            const res = await fetch(`https://translate.googleapis.com/translate_a/single?client=gtx&sl=vi&tl=${tl}&dt=t&q=${encodeURIComponent(text)}`);
            const data = await res.json();
            return data[0].map(item => item[0]).join('');
        } catch { 
            return text; // Fallback: Lỗi dịch thì dùng lại tiếng Việt
        }
    }

    // SUBMIT UPDATE
    document.getElementById("poiForm").addEventListener("submit", async (e) => {
        e.preventDefault();
        const btn = document.querySelector('.submit');
        const origText = btn.innerText;

        const name = document.getElementById("name").value.trim();
        const address = document.getElementById("address").value.trim();
        const lat = document.getElementById("lat").value.trim();
        const lng = document.getElementById("lng").value.trim();
        const desc = document.getElementById("desc").value.trim();

        if (!name || !address || !lat || !lng || !desc) {
            return alert("Vui lòng nhập đủ thông tin!");
        }

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
                finalImg = (await upRes.json()).fileName;
            }

            btn.innerText = "Đang dịch (1-2s)...";
            const en = await translateText(desc, 'en');
            const ja = await translateText(desc, 'ja');
            const zh = await translateText(desc, 'zh-CN');

            const poi = {
                name, address, lat: parseFloat(lat), lng: parseFloat(lng), image: finalImg,
                localizations: [
                    { lang: "vi", description: desc },
                    { lang: "en", description: en },
                    { lang: "ja", description: ja },
                    { lang: "zh", description: zh }
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