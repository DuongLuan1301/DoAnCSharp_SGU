document.addEventListener("DOMContentLoaded", async () => {
    // 1. KIỂM TRA ĐĂNG NHẬP
    const clientId = localStorage.getItem("clientId");
    const clientName = localStorage.getItem("clientName");

    if (!clientId) {
        alert("Vui lòng đăng nhập trước!");
        window.location.href = "loginclient.html";
        return;
    }

    // Hiển thị tên chủ quán
    document.querySelectorAll(".user-profile span:nth-child(2)").forEach(el => {
        el.innerText = clientName;
    });

    // 2. TẢI DỮ LIỆU CỦA RIÊNG CLIENT NÀY
    try {
        const res = await fetch(`http://127.0.0.1:5188/client/poi/${clientId}`, { cache: "no-store" });
        if (!res.ok) throw new Error("Lỗi tải dữ liệu");
        const pois = await res.json();

        // Tính tổng lượt xem cho Thống kê
        let totalViews = 0, totalQR = 0, totalAudio = 0;
        pois.forEach(p => {
            totalViews += p.views || 0;
            totalQR += p.qrScans || 0;
            totalAudio += p.audioListens || 0;
        });

        // Đổ số liệu vào 3 thẻ đầu trang (nếu đang ở index.html)
        const statViews = document.querySelector(".stat-icon.visitor + .stat-info p");
        const statQR = document.querySelector(".stat-icon.qr + .stat-info p");
        const statAudio = document.querySelector(".stat-icon.audio + .stat-info p");
        if (statViews) statViews.innerText = totalViews.toLocaleString();
        if (statQR) statQR.innerText = totalQR.toLocaleString();
        if (statAudio) statAudio.innerText = totalAudio.toLocaleString();

        // 3. VẼ BẢNG DỮ LIỆU (Tự động nhận diện trang)
        const tbody = document.querySelector(".data-table tbody");
        if (tbody) {
            tbody.innerHTML = ""; 
            const isListPage = window.location.pathname.includes("list.html");

            if (pois.length === 0) {
                tbody.innerHTML = `<tr><td colspan="5" style="text-align:center;">Bạn chưa có gian hàng nào.</td></tr>`;
            }

            pois.forEach(p => {
                const views = p.views || 0;
                const audio = p.audioListens || 0;
                const lat = p.lat || 0;
                const lng = p.lng || 0;

                if (isListPage) {
                    // Giao diện cho trang list.html (Danh sách đầy đủ)
                    tbody.innerHTML += `
                        <tr>
                            <td><div class="store-name" style="font-weight:600; color:#333;">${p.name || p.Name}</div></td>
                            <td><span style="font-size:13px; color:#666;">${p.address || p.Address}</span></td>
                            <td><span style="font-size:12px;">${lat.toFixed(4)}, ${lng.toFixed(4)}</span></td>
                            <td><span class="status approved">Đã duyệt</span></td>
                            <td>
                                <div class="action-group">
                                    <button class="action-btn btn-edit" onclick="editPOI('${p.id}')" style="background:#f59e0b; color:white; border:none; padding:6px 12px; border-radius:4px; cursor:pointer;">Cập nhật</button>
                                    <button class="action-btn btn-delete" onclick="deletePOI('${p.id}')" style="background:#ef4444; color:white; border:none; padding:6px 12px; border-radius:4px; cursor:pointer; margin-left:5px;">Xóa</button>
                                </div>
                            </td>
                        </tr>
                    `;
                } else {
                    // Giao diện cho trang index.html (Thống kê rút gọn)
                    tbody.innerHTML += `
                        <tr>
                            <td>
                                <div class="poi-info">
                                    <div class="poi-name" style="font-weight:600;">${p.name || p.Name}</div>
                                    <div class="poi-category" style="font-size:12px; color:#64748b;">${(p.address || p.Address).substring(0, 30)}...</div>
                                </div>
                            </td>
                            <td><span class="badge badge-info">${views} lượt</span></td>
                            <td><span class="badge badge-audio">${audio} lượt</span></td>
                            <td class="text-success">↑ Hoạt động tốt</td>
                        </tr>
                    `;
                }
            });
        }
    } catch (err) {
        console.error("Lỗi:", err);
    }

    // Thanh mật độ (Live Density)
    const bar = document.getElementById('density-bar');
    if (bar) setInterval(() => { bar.style.width = Math.floor(Math.random() * (90 - 45) + 45) + "%"; }, 4000);
});

// HÀM CHUYỂN SANG TRANG SỬA
window.editPOI = function(id) {
    window.location.href = `edit.html?id=${id}`;
};

// HÀM XÓA GIAN HÀNG
window.deletePOI = async function(id) {
    if (!confirm("Bạn có chắc chắn muốn xóa gian hàng này?")) return;
    
    // Lấy ID của chủ quán đang đăng nhập
    const currentClientId = localStorage.getItem("clientId"); 
    
    try {
        // 🔥 BẢO MẬT: Truyền thêm clientId lên URL để Backend kiểm tra quyền
        const res = await fetch(`http://127.0.0.1:5188/admin/poi/${id}?clientId=${currentClientId}`, { method: "DELETE" });
        
        if (res.ok) window.location.reload();
        else alert("Lỗi: Bạn không có quyền xóa gian hàng này!");
    } catch (e) {
        alert("Lỗi kết nối mạng!");
    }
};