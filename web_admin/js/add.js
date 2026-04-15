document.addEventListener("DOMContentLoaded", () => {

    // =====================
    // MAP
    // =====================
    const map = L.map('map').setView([10.7769, 106.7009], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap'
    }).addTo(map);

    let marker;

    // =====================
    // TÍNH NĂNG MỚI: HIỂN THỊ CÁC POI ĐÃ CÓ LÊN BẢN ĐỒ
    // =====================
    async function loadExistingPOIs() {
        try {
            const res = await fetch("http://127.0.0.1:5188/admin/poi");
            if (res.ok) {
                const data = await res.json();
                data.forEach(p => {
                    const lat = p.lat || p.Lat;
                    const lng = p.lng || p.Lng;
                    const name = p.name || p.Name || "Gian hàng";
                    
                    if (lat && lng) {
                        // Tạo marker mờ (opacity: 0.5) cho các điểm đã tồn tại
                        const existingMarker = L.marker([lat, lng], { opacity: 0.5 }).addTo(map);
                        existingMarker.bindPopup(`
                            <div style="text-align:center;">
                                <b style="color: #4f46e5;">${name}</b><br>
                                <span style="font-size: 11px; color: gray;">(Đã tồn tại)</span>
                            </div>
                        `);
                    }
                });
            }
        } catch (error) {
            console.error("Lỗi khi tải danh sách POI cũ:", error);
        }
    }
    
    // Gọi hàm tải POI cũ ngay khi load xong map
    loadExistingPOIs();

    // =====================
    // CLICK ĐỂ CHỌN VỊ TRÍ MỚI
    // =====================
    map.on('click', async (e) => {
        const { lat, lng } = e.latlng;

        document.getElementById("lat").value = lat.toFixed(6);
        document.getElementById("lng").value = lng.toFixed(6);

        if (marker) map.removeLayer(marker);
        
        // Marker mới sẽ đậm và rõ nét (opacity mặc định = 1)
        marker = L.marker([lat, lng]).addTo(map);

        const res = await fetch(
            `https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}`
        );

        const data = await res.json();

        if (data?.display_name) {
            document.getElementById("address").value = data.display_name;
        }
    });

    // =====================
    // IMAGE
    // =====================
    let selectedFile = null;

    const fileInput = document.getElementById("fileInput");
    const fileName = document.getElementById("fileName");

    fileInput.addEventListener("change", (e) => {
        const file = e.target.files[0];
        if (!file) return;

        selectedFile = file;
        fileName.innerText = file.name;
    });

    // =====================
    // SUBMIT
    // =====================
    const form = document.getElementById("poiForm");

    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        console.log("SUBMIT START");

        const name = document.getElementById("name").value.trim();
        const address = document.getElementById("address").value.trim();
        const lat = document.getElementById("lat").value.trim();
        const lng = document.getElementById("lng").value.trim();
        const desc = document.getElementById("desc").value.trim();

        if (!name || !address || !lat || !lng || !desc || !selectedFile) {
            alert("Vui lòng nhập đầy đủ dữ liệu");
            return;
        }

        // UPLOAD IMAGE
        const formData = new FormData();
        formData.append("file", selectedFile);

        const uploadRes = await fetch("http://127.0.0.1:5188/upload-image", {
            method: "POST",
            body: formData
        });

        const uploadData = await uploadRes.json();

        // CREATE POI
        const poi = {
            name,
            address,
            lat: parseFloat(lat),
            lng: parseFloat(lng),
            localizations: [
                { lang: "vi", description: desc },
                { lang: "en", description: desc },
                { lang: "ja", description: desc },
                { lang: "zh", description: desc }
            ],
            image: uploadData.fileName
        };

        // SEND POI
        const res = await fetch("http://127.0.0.1:5188/admin/poi", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(poi)
        });

        if (!res.ok) {
            const err = await res.text();
            throw new Error(err);
        }

        alert("Added successfully!");
        setTimeout(() => {
            window.location.href = "index.html";
        }, 200);

    });
});