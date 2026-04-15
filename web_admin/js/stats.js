import { getPOIs } from "./api.js";

document.addEventListener("DOMContentLoaded", async () => {
    // Gọi API lấy toàn bộ POI từ C#
    const data = await getPOIs();
    
    // 1. XỬ LÝ DỮ LIỆU TỪ MONGODB LÊN
    const processedData = data.map(p => ({
        name: p.name || p.Name || "Không tên",
        address: p.address || p.Address || "N/A",
        lat: p.lat || p.Lat || 10.7769, 
        lng: p.lng || p.Lng || 106.7009,
        // Dùng số liệu THẬT thay vì random. Dùng || 0 để phòng hờ DB chưa có trường này
        views: p.views || p.Views || 0, 
        qrScans: p.qrScans || p.QrScans || 0,
        audioListens: p.audioListens || p.AudioListens || 0
    }));

    // Cập nhật thẻ Tổng quan
    document.getElementById("totalStalls").innerText = processedData.length;
    const totalViews = processedData.reduce((sum, item) => sum + item.views, 0);
    document.getElementById("totalViews").innerText = totalViews.toLocaleString();

   // ==========================================
    // TÍNH NĂNG 1: TỔNG LƯỢT TƯƠNG TÁC (QUÉT QR + NGHE AUDIO)
    // ==========================================
    const liveUsersEl = document.getElementById("liveUsers");
    
    // Tính tổng tất cả lượt quét QR của các gian hàng
    const totalQR = processedData.reduce((sum, item) => sum + item.qrScans, 0);
    // Tính tổng tất cả lượt nghe Audio của các gian hàng
    const totalAudio = processedData.reduce((sum, item) => sum + item.audioListens, 0);
    
    // Cộng lại và hiển thị
    const totalInteractions = totalQR + totalAudio;
    liveUsersEl.innerText = totalInteractions.toLocaleString();

    // ==========================================
    // TÍNH NĂNG 2: VẼ BẢN ĐỒ NHIỆT VÀ MARKER
    // ==========================================
    const centerLat = processedData.length > 0 ? processedData[0].lat : 10.7769;
    const centerLng = processedData.length > 0 ? processedData[0].lng : 106.7009;

    const map = L.map('heatMap').setView([centerLat, centerLng], 13);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap'
    }).addTo(map);

    const maxViews = Math.max(...processedData.map(p => p.views), 1); // Đề phòng max = 0
    const heatData = processedData.map(p => [p.lat, p.lng, p.views]);

    L.heatLayer(heatData, {
        radius: 25,
        blur: 15,
        maxZoom: 15,
        max: maxViews
    }).addTo(map);

    // MẢNG LƯU TRỮ CÁC MARKER ĐỂ GỌI TỪ BIỂU ĐỒ
    const markers = [];

    processedData.forEach(p => {
        const marker = L.marker([p.lat, p.lng]).addTo(map);
        marker.bindPopup(`
            <div style="text-align: center; font-family: Inter, sans-serif;">
                <h4 style="margin: 0 0 5px 0; color: #4f46e5;">${p.name}</h4>
                <p style="margin: 0; font-size: 13px; color: #64748b;">Tổng truy cập: <strong style="color: #10b981;">${p.views}</strong></p>
                <p style="margin: 0; font-size: 12px; color: #64748b;">Quét QR: <strong>${p.qrScans}</strong> | Nghe: <strong>${p.audioListens}</strong></p>
            </div>
        `);
        // Lưu marker vào mảng
        markers.push(marker);
    });

    // ==========================================
    // TÍNH NĂNG 3: VẼ CHART CÓ SỰ KIỆN CLICK
    // ==========================================
    const ctx = document.getElementById('trafficChart').getContext('2d');
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: processedData.map(p => p.name.substring(0, 15) + (p.name.length > 15 ? '...' : '')),
            datasets: [{
                label: 'Lượt truy cập',
                data: processedData.map(p => p.views), // Dùng số view thật vẽ cột
                backgroundColor: '#4f46e5',
                borderRadius: 6,
                hoverBackgroundColor: '#3b82f6'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            onClick: (event, elements) => {
                if (elements.length > 0) {
                    const dataIndex = elements[0].index;
                    const selectedPOI = processedData[dataIndex];
                    const selectedMarker = markers[dataIndex];

                    map.flyTo([selectedPOI.lat, selectedPOI.lng], 16, {
                        animate: true,
                        duration: 1.5
                    });
                    selectedMarker.openPopup();
                }
            }
        }
    });

    // ==========================================
    // TÍNH NĂNG 4: ĐỔ DỮ LIỆU BẢNG VỚI 3 CỘT SỐ LIỆU
    // ==========================================
    const tableBody = document.getElementById("statsTableBody");
    processedData.forEach(p => {
        const row = `
            <tr style="border-bottom: 1px solid var(--border);">
                <td style="padding: 12px; font-weight: 500;">${p.name}</td>
                <td style="padding: 12px; color: #64748b;">${p.address.substring(0, 50)}...</td>
                <td style="padding: 12px; font-weight: 600; color: #10b981;">${p.views.toLocaleString()}</td>
                <td style="padding: 12px; font-weight: 600; color: #3b82f6;">${p.qrScans.toLocaleString()}</td>
                <td style="padding: 12px; font-weight: 600; color: #8b5cf6;">${p.audioListens.toLocaleString()}</td>
            </tr>
        `;
        tableBody.innerHTML += row;
    });
});