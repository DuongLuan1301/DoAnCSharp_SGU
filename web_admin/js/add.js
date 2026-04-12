const API_BASE = "http://127.0.0.1:5188";

// =====================
// MAP
// =====================
const map = L.map('map').setView([10.7769, 106.7009], 13);

L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '&copy; OpenStreetMap'
}).addTo(map);

let marker;

map.on('click', async function (e) {
    const { lat, lng } = e.latlng;

    document.getElementById("lat").value = lat.toFixed(6);
    document.getElementById("lng").value = lng.toFixed(6);

    if (marker) map.removeLayer(marker);
    marker = L.marker([lat, lng]).addTo(map);

    // reverse geocoding
    const res = await fetch(
        `https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}`
    );

    const data = await res.json();

    if (data && data.display_name) {
        document.getElementById("address").value = data.display_name;
    }
});

// =====================
// IMAGE UPLOAD
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
document.querySelector(".btn.submit").addEventListener("click", async (e) => {
    e.preventDefault();

    const name = document.getElementById("name").value.trim();
    const address = document.getElementById("address").value.trim();
    const lat = document.getElementById("lat").value.trim();
    const lng = document.getElementById("lng").value.trim();
    const desc = document.getElementById("desc").value.trim();

    if (!name) return alert("Thiếu tên");
    if (!address) return alert("Thiếu địa chỉ");
    if (!lat || isNaN(lat)) return alert("Lat lỗi");
    if (!lng || isNaN(lng)) return alert("Lng lỗi");
    if (!desc) return alert("Thiếu mô tả");
    if (!selectedFile) return alert("Thiếu ảnh");

    try {
        // ===== UPLOAD =====
        const formData = new FormData();
        formData.append("file", selectedFile);

        const uploadRes = await fetch("http://127.0.0.1:5188/upload-image", {
            method: "POST",
            body: formData
        });

        // 🔥 FIX: KHÔNG parse JSON nếu fail
        if (!uploadRes.ok) {
            throw new Error("Upload lỗi");
        }

        let uploadData = {};
        try {
            uploadData = await uploadRes.json();
        } catch {
            throw new Error("Upload không trả JSON");
        }

        // ===== ADD POI =====
        const poi = {
            name,
            address,
            lat: parseFloat(lat),
            lng: parseFloat(lng),
            image: uploadData.fileName,
            localizations: [
                { lang: "vi", description: desc }
            ]
        };

        const res = await fetch("http://127.0.0.1:5188/admin/poi", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(poi)
        });

        // 🔥 FIX: KHÔNG đọc body nếu fail
        if (!res.ok) {
            throw new Error("Add POI lỗi");
        }

        // 🔥 QUAN TRỌNG: redirect NGAY LẬP TỨC
        window.location.assign("index.html");

    } catch (err) {
        console.error("ERROR:", err);
        alert(err.message);
    }
});