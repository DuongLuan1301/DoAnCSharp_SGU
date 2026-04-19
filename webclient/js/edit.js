document.addEventListener("DOMContentLoaded", () => {

    const BASE_URL = "http://127.0.0.1:5188";
    const poiId = new URLSearchParams(window.location.search).get("id");

    let map;
    let marker;
    let selectedFile = null;


    // =====================
    // INIT MAP (SAFE 1 LẦN)
    // =====================
    function initMap(lat = 10.7769, lng = 106.7009) {

        if (map) return;

        map = L.map('map').setView([lat, lng], 20); // zoom gần hơn khi có POI

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; OpenStreetMap'
        }).addTo(map);

        map.on('click', (e) => {

            const lat = e.latlng.lat.toFixed(6);
            const lng = e.latlng.lng.toFixed(6);

            document.getElementById("lat").value = lat;
            document.getElementById("lng").value = lng;

            if (marker) map.removeLayer(marker);

            marker = L.marker([lat, lng]).addTo(map);
        });
    }

    // =====================
    // LOAD DETAIL (FIX BACKEND API)
    // =====================
    async function loadDetail() {

        try {
            if (!poiId) {
                alert("Missing ID");
                return;
            }

            const res = await fetch(`${BASE_URL}/api/poi/${poiId}`);
            const poi = await res.json();

            console.log("POI:", poi);

            // ===== FORM FILL =====
            document.getElementById("name").value = poi.name || "";
            document.getElementById("address").value = poi.address || "";
            document.getElementById("lat").value = poi.lat || "";
            document.getElementById("lng").value = poi.lng || "";
            document.getElementById("desc").value = poi.description || "";

            // ===== MAP UPDATE =====
            initMap();

            const lat = parseFloat(poi.lat);
            const lng = parseFloat(poi.lng);

            map.setView([lat, lng], 16);

            if (marker) map.removeLayer(marker);

            marker = L.marker([lat, lng]).addTo(map);

        } catch (err) {
            console.error("LOAD DETAIL ERROR:", err);
        }
    }

    // =====================
    // IMAGE HANDLING
    // =====================
    const fileInput = document.getElementById("fileInput");
    const fileName = document.getElementById("fileName");

    fileInput.addEventListener("change", (e) => {
        selectedFile = e.target.files[0];
        fileName.innerText = selectedFile?.name || "";
    });

    // =====================
    // UPDATE POI
    // =====================
    document.getElementById("submitBtn").addEventListener("click", async () => {

        const body = {
            name: document.getElementById("name").value.trim(),
            address: document.getElementById("address").value.trim(),
            lat: parseFloat(document.getElementById("lat").value),
            lng: parseFloat(document.getElementById("lng").value),
            localizations: [
                {
                    lang: "vi",
                    description: document.getElementById("desc").value.trim()
                }
            ]
        };

        // upload image nếu có
        if (selectedFile) {

            const fd = new FormData();
            fd.append("file", selectedFile);

            const upload = await fetch(`${BASE_URL}/upload-image`, {
                method: "POST",
                body: fd
            });

            const data = await upload.json();
            body.image = data.fileName;
        }

        const res = await fetch(`${BASE_URL}/api/poi/${poiId}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(body)
        });

        if (!res.ok) {
            alert("Cập nhật thất bại!");
            return;
        }

        alert("Cập nhật thành công!");
        window.location.href = "list.html";
    });

    (async function init() {

        let lat = 10.7769;
        let lng = 106.7009;

        if (poiId) {
            try {
                const res = await fetch(`${BASE_URL}/api/poi/${poiId}`);
                const poi = await res.json();

                lat = parseFloat(poi.lat);
                lng = parseFloat(poi.lng);

            } catch (err) {
                console.error(err);
            }
        }

        initMap(lat, lng);

        if (poiId) {
            await loadDetail();
        }

    })();

});