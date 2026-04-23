// Biến toàn cục để lưu trữ Map, Layer Nhiệt và Biểu đồ
let map, heatLayer, trafficChart;
let markers = [];

document.addEventListener("DOMContentLoaded", async () => {
    // 1. Gọi lần đầu tiên để vẽ khung và hiển thị dữ liệu
    await loadAndRenderDashboard(true);

    // 2. Kích hoạt vòng lặp: Cứ 5 giây cập nhật số người Online 1 lần
    fetchOnlineUsers();
    setInterval(fetchOnlineUsers, 5000);

    // 3. Kích hoạt vòng lặp: Cứ 5 giây cập nhật SỐ LIỆU TƯƠNG TÁC (ẩn ngầm, không tải lại trang)
    setInterval(() => loadAndRenderDashboard(false), 5000);
});

// ==========================================
// HÀM CHÍNH: TẢI VÀ VẼ DASHBOARD
// ==========================================
async function loadAndRenderDashboard(isInitialLoad) {
    try {
        const res = await fetch("http://127.0.0.1:5188/admin/poi", { cache: "no-store" });
        if (!res.ok) return;
        const data = await res.json();

        const processedData = data.map(p => ({
            id: p.id || p.Id,
            name: p.name || p.Name || "Không tên",
            address: p.address || p.Address || "N/A",
            lat: p.lat || p.Lat || 10.7769,
            lng: p.lng || p.Lng || 106.7009,
            views: p.views || p.Views || 0,
            qrScans: p.qrScans || p.QrScans || 0,
            audioListens: p.audioListens || p.AudioListens || 0
        }));

        // ----------------------------------------
        // A. CẬP NHẬT 4 THẺ THỐNG KÊ TRÊN CÙNG
        // ----------------------------------------
        document.getElementById("totalStalls").innerText = processedData.length;
        const totalViews = processedData.reduce((sum, item) => sum + item.views, 0);
        document.getElementById("totalViews").innerText = totalViews.toLocaleString();

        const totalQR = processedData.reduce((sum, item) => sum + item.qrScans, 0);
        const totalAudio = processedData.reduce((sum, item) => sum + item.audioListens, 0);
        
        // Đảm bảo id của thẻ tương tác là totalInteractions
        const interactionsEl = document.getElementById("totalInteractions");
        if (interactionsEl) interactionsEl.innerText = (totalQR + totalAudio).toLocaleString();

        // ----------------------------------------
        // B. CẬP NHẬT BẢNG CHI TIẾT
        // ----------------------------------------
        const tableBody = document.getElementById("statsTableBody");
        tableBody.innerHTML = ""; // Xóa bảng cũ
        processedData.forEach(p => {
            tableBody.innerHTML += `
                <tr style="border-bottom: 1px solid var(--border);">
                    <td style="padding: 12px; font-weight: 500;">${p.name}</td>
                    <td style="padding: 12px; color: #64748b;">${p.address.substring(0, 50)}...</td>
                    <td style="padding: 12px; font-weight: 600; color: #10b981;">${p.views.toLocaleString()}</td>
                    <td style="padding: 12px; font-weight: 600; color: #3b82f6;">${p.qrScans.toLocaleString()}</td>
                    <td style="padding: 12px; font-weight: 600; color: #8b5cf6;">${p.audioListens.toLocaleString()}</td>
                </tr>
            `;
        });

        // ----------------------------------------
        // C. NẾU LÀ LẦN ĐẦU TIÊN MỞ TRANG (VẼ BẢN ĐỒ VÀ CHART)
        // ----------------------------------------
        if (isInitialLoad) {
            const centerLat = processedData.length > 0 ? processedData[0].lat : 10.7769;
            const centerLng = processedData.length > 0 ? processedData[0].lng : 106.7009;

            map = L.map('heatMap').setView([centerLat, centerLng], 13);
            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(map);

            setTimeout(() => {
                map.invalidateSize();
                const maxViews = Math.max(...processedData.map(p => p.views), 1);
                const heatData = processedData.map(p => [p.lat, p.lng, p.views]);
                
                // Lưu lại biến heatLayer để sau này cập nhật
                heatLayer = L.heatLayer(heatData, { radius: 25, blur: 15, maxZoom: 15, max: maxViews }).addTo(map);
            }, 300);

            // Vẽ Marker
            processedData.forEach(p => {
                const marker = L.marker([p.lat, p.lng]).addTo(map);
                marker.bindPopup(generatePopupHTML(p));
                markers.push(marker);
            });

            // Vẽ Chart.js
            const ctx = document.getElementById('trafficChart').getContext('2d');
            trafficChart = new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: processedData.map(p => p.name.substring(0, 15) + (p.name.length > 15 ? '...' : '')),
                    datasets: [{
                        label: 'Lượt truy cập',
                        data: processedData.map(p => p.views),
                        backgroundColor: '#4f46e5', borderRadius: 6, hoverBackgroundColor: '#3b82f6'
                    }]
                },
                options: {
                    responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } },
                    onClick: (event, elements) => {
                        if (elements.length > 0) {
                            const dataIndex = elements[0].index;
                            const selectedPOI = processedData[dataIndex];
                            map.flyTo([selectedPOI.lat, selectedPOI.lng], 16, { animate: true, duration: 1.5 });
                            markers[dataIndex].openPopup();
                        }
                    }
                }
            });
        } 
        // ----------------------------------------
        // D. NẾU LÀ CÁC LẦN CẬP NHẬT TỰ ĐỘNG SAU ĐÓ (CHỈ THAY SỐ, KHÔNG VẼ LẠI)
        // ----------------------------------------
        else {
            // Cập nhật lại màu đỏ của bản đồ nhiệt
            if (heatLayer) {
                const maxViews = Math.max(...processedData.map(p => p.views), 1);
                const heatData = processedData.map(p => [p.lat, p.lng, p.views]);
                heatLayer.setOptions({ max: maxViews });
                heatLayer.setLatLngs(heatData);
            }

            // Cập nhật lại cột biểu đồ Chart.js
            if (trafficChart) {
                trafficChart.data.datasets[0].data = processedData.map(p => p.views);
                trafficChart.update(); // Lệnh này giúp biểu đồ mọc cao lên mượt mà
            }
            
            // Cập nhật lại con số trong các Popup Marker nếu đang mở
            markers.forEach((marker, index) => {
                const p = processedData[index];
                if (p) marker.setPopupContent(generatePopupHTML(p));
            });
        }
    } catch (error) {
        console.error("Lỗi cập nhật Dashboard:", error);
    }
}

// Hàm phụ trợ tạo giao diện cho Popup bản đồ
function generatePopupHTML(p) {
    return `
        <div style="text-align: center; font-family: Inter, sans-serif;">
            <h4 style="margin: 0 0 5px 0; color: #4f46e5;">${p.name}</h4>
            <p style="margin: 0; font-size: 13px; color: #64748b;">Tổng truy cập: <strong style="color: #10b981;">${p.views}</strong></p>
            <p style="margin: 0; font-size: 12px; color: #64748b;">Quét QR: <strong>${p.qrScans}</strong> | Nghe: <strong>${p.audioListens}</strong></p>
        </div>
    `;
}

// ==========================================
// HÀM LẤY SỐ NGƯỜI ONLINE
// ==========================================
async function fetchOnlineUsers() {
    try {
        const res = await fetch("http://127.0.0.1:5188/admin/tracking/online");
        if (res.ok) {
            const onlineData = await res.json();
            const el = document.getElementById("liveUsers");
            if(el) el.innerText = onlineData.onlineCount;
        }
    } catch (error) {
        console.log("Không thể cập nhật số người online");
    }
}