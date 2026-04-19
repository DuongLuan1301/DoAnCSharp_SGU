document.addEventListener("DOMContentLoaded", () => {

    const BASE_URL = "http://127.0.0.1:5188";;

    // =====================
    // MAP
    // =====================
    let marker;
    const map = L.map('map').setView([10.7769, 106.7009], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap'
    }).addTo(map);

    map.on('click', async (e) => {
        const lat = e.latlng.lat.toFixed(6);
        const lng = e.latlng.lng.toFixed(6);

        // =====================
        // CHECK TRÙNG
        // =====================
        const isExist = existingLocations.some(loc =>
            loc.lat === lat && loc.lng === lng
        );

        if (isExist) {
            alert("Vị trí này đã có gian hàng!");
            return;
        }

        // =====================
        // SET VALUE
        // =====================
        document.getElementById("lat").value = lat;
        document.getElementById("lng").value = lng;

        if (marker) map.removeLayer(marker);
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
    // LOAD EXISTING POI
    // =====================
    let existingMarkers = [];
    let existingLocations = []; // lưu lat/lng

    async function loadExistingPOI() {
        const res = await fetch(`${BASE_URL}/admin/poi`);
        const data = await res.json();

        data.forEach(poi => {
            const lat = poi.lat;
            const lng = poi.lng;

            // lưu lại để check trùng
            existingLocations.push({
                lat: lat.toFixed(6),
                lng: lng.toFixed(6)
            });

            // tạo marker màu khác (ví dụ đỏ)
            const m = L.marker([lat, lng])
                .addTo(map)
                .bindPopup(`<b>${poi.name}</b>`);

            existingMarkers.push(m);
        });
    }

    // gọi khi load
    loadExistingPOI();
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
    // SUBMIT BUTTON
    // =====================
    const submitBtn = document.getElementById("submitBtn");

    submitBtn.addEventListener("click", async () => {

        // ===== GET DATA =====
        const name = document.getElementById("name").value.trim();
        const address = document.getElementById("address").value.trim();
        const lat = document.getElementById("lat").value.trim();
        const lng = document.getElementById("lng").value.trim();
        const desc = document.getElementById("desc").value.trim();

        if (!name || !address || !lat || !lng || !desc || !selectedFile) {
            alert("Vui lòng nhập đầy đủ dữ liệu");
            return;
        }

        const clientId = localStorage.getItem("clientId");
        if (!clientId) {
            alert("Chưa đăng nhập");
            return;
        }

        // =====================
        // STEP 1: UPLOAD IMAGE
        // =====================

        const formData = new FormData();
        formData.append("file", selectedFile);

        let uploadRes;
        uploadRes = await fetch(`${BASE_URL}/upload-image`, {
            method: "POST",
            body: formData
        });


        const uploadText = await uploadRes.text();
        console.log("UPLOAD RAW:", uploadText);

        if (!uploadRes.ok) {
            alert("Upload lỗi: " + uploadText);
            return;
        }

        let uploadData;
        try {
            uploadData = JSON.parse(uploadText);
        } catch {
            alert("Upload response không phải JSON");
            return;
        }
        // =====================
        // STEP 2: CREATE POI
        // =====================
        const poi = {
            clientId,
            name,
            address,
            lat: parseFloat(lat),
            lng: parseFloat(lng),
            localizations: [
                { lang: "vi", description: desc }
            ],
            image: uploadData.fileName
        };
        let res;
        res = await fetch(`${BASE_URL}/admin/poi`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(poi)
        });

        const text = await res.text();
        window.location.href = "index.html";

    });

});