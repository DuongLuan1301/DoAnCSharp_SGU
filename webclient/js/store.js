let map, marker;

function initMap() {
    // Lấy tọa độ từ ô input 
    const latInput = document.getElementById('lat');
    const lngInput = document.getElementById('lng');
    
    let latInit = parseFloat(latInput.value) || 10.7551245;
    let lngInit = parseFloat(lngInput.value) || 106.696809;

    // Khởi tạo map
    map = L.map('map-picker').setView([latInit, lngInit], 16);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors'
    }).addTo(map);

    // Tạo ghim có thể kéo thả
    marker = L.marker([latInit, lngInit], { draggable: true }).addTo(map);

    // Khi kéo ghim xong -> cập nhật input
    marker.on('dragend', function() {
        const pos = marker.getLatLng();
        updateInputs(pos.lat, pos.lng);
    });

    // Khi click vào map -> dời ghim và cập nhật input
    map.on('click', function(e) {
        marker.setLatLng(e.latlng);
        updateInputs(e.latlng.lat, e.latlng.lng);
    });
    
    // Khi gõ tay vào input -> Cập nhật ngược lại bản đồ
    function updateMapFromInput() {
        const newLat = parseFloat(latInput.value);
        const newLng = parseFloat(lngInput.value);
        if (!isNaN(newLat) && !isNaN(newLng)) {
            const newPos = new L.LatLng(newLat, newLng);
            marker.setLatLng(newPos);
            map.panTo(newPos); 
        }
    }
    
    latInput.addEventListener('input', updateMapFromInput);
    lngInput.addEventListener('input', updateMapFromInput);
}

function updateInputs(lat, lng) {
    document.getElementById('lat').value = lat.toFixed(7);
    document.getElementById('lng').value = lng.toFixed(7);
}

function handleUpdate(e) {
    e.preventDefault();
    
    // Đã lấy đầy đủ tất cả các trường
    const data = {
        name: document.getElementById('name').value,
        image: document.getElementById('image').value,
        address: document.getElementById('address').value,
        lat: parseFloat(document.getElementById('lat').value),
        lng: parseFloat(document.getElementById('lng').value),
        localizations: [
            { lang: "vi", description: document.getElementById('desc-vi').value }
        ]
    };
    
    console.log("Dữ liệu gửi lên server (MongoDB):", data);
    alert("Cập nhật thông tin và tọa độ thành công!");
}

window.onload = initMap;