document.addEventListener("DOMContentLoaded", () => {

    // =====================
    // MAP
    // =====================
    const map = L.map('map').setView([10.7769, 106.7009], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap'
    }).addTo(map);

    let marker;

    map.on('click', async (e) => {
        const { lat, lng } = e.latlng;

        document.getElementById("lat").value = lat.toFixed(6);
        document.getElementById("lng").value = lng.toFixed(6);

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