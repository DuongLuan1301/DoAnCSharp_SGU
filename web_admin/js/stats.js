import { getPOIs } from "./api.js";

document.addEventListener("DOMContentLoaded", async () => {
    const data = await getPOIs();
    
    // 1. Xử lý dữ liệu (Giả lập views nếu Backend chưa có)
    const processedData = data.map(p => ({
        name: p.name || p.Name || "Không tên",
        address: p.address || p.Address || "N/A",
        lat: p.lat || p.Lat || 10.7769, 
        lng: p.lng || p.Lng || 106.7009,
        views: p.views || Math.floor(Math.random() * 500) + 50 
    }));

    // Cập nhật thẻ Tổng quan
    document.getElementById("totalStalls").innerText = processedData.length;
    const totalViews = processedData.reduce((sum, item) => sum + item.views, 0);
    document.getElementById("totalViews").innerText = totalViews.toLocaleString();
// ==========================================
// TÍNH NĂNG 1: LẤY DỮ LIỆU LIVE USERS THẬT
// ==========================================
const liveUsersEl = document.getElementById("liveUsers");

async function fetchLiveUsers() {
    try {
        // Thay đường dẫn này bằng API thực tế trên Backend C# của bạn
        const res = await fetch("http://localhost:5188/api/stats/live-users");
        
        if (res.ok) {
            const data = await res.json();
            // Cập nhật số liệu từ Database lên giao diện
            liveUsersEl.innerText = data.activeCount || 0; 
        }
    } catch (error) {
        console.error("Lỗi khi lấy dữ liệu Live Users:", error);
    }
}

// Gọi ngay lần đầu tiên khi trang vừa load xong
fetchLiveUsers();

// Cứ mỗi 5 giây sẽ tự động gọi API để làm mới số lượng
setInterval(fetchLiveUsers, 5000);

// ==========================================
    // TÍNH NĂNG 2: VẼ BẢN ĐỒ NHIỆT VÀ MARKER
    // ==========================================
    const centerLat = processedData.length > 0 ? processedData[0].lat : 10.7769;
    const centerLng = processedData.length > 0 ? processedData[0].lng : 106.7009;

    const map = L.map('heatMap').setView([centerLat, centerLng], 13);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap'
    }).addTo(map);

    const maxViews = Math.max(...processedData.map(p => p.views));
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
                <p style="margin: 0; font-size: 13px; color: #64748b;">Lượt truy cập: <strong style="color: #10b981;">${p.views}</strong></p>
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
                data: processedData.map(p => p.views),
                backgroundColor: '#4f46e5',
                borderRadius: 6,
                hoverBackgroundColor: '#3b82f6' // Đổi màu nhẹ khi rê chuột vào cột
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            // THÊM SỰ KIỆN ONCLICK TẠI ĐÂY
            onClick: (event, elements) => {
                // Nếu người dùng click trúng một cột
                if (elements.length > 0) {
                    const dataIndex = elements[0].index; // Lấy vị trí của cột được click
                    const selectedPOI = processedData[dataIndex];
                    const selectedMarker = markers[dataIndex];

                    // Hiệu ứng bản đồ bay tới điểm đó (zoom level 16)
                    map.flyTo([selectedPOI.lat, selectedPOI.lng], 16, {
                        animate: true,
                        duration: 1.5 // Thời gian bay (giây)
                    });

                    // Tự động mở popup thông tin của điểm đó
                    selectedMarker.openPopup();
                }
            }
        }
    });

    // ==========================================
    // TÍNH NĂNG 4: ĐỔ DỮ LIỆU BẢNG (Giữ nguyên)
    // ==========================================
    const tableBody = document.getElementById("statsTableBody");
    processedData.forEach(p => {
        const row = `
            <tr style="border-bottom: 1px solid var(--border);">
                <td style="padding: 12px; font-weight: 500;">${p.name}</td>
                <td style="padding: 12px; color: #64748b;">${p.address.substring(0, 50)}...</td>
                <td style="padding: 12px; font-weight: 600; color: #10b981;">${p.views.toLocaleString()}</td>
            </tr>
        `;
        tableBody.innerHTML += row;
    });})