import { getPOIs, deletePOI } from "./api.js";

const grid = document.querySelector(".grid");

async function loadPOIs() {
    const data = await getPOIs();
    grid.innerHTML = "";

    data.forEach(p => {
        // BAO QUÁT TẤT CẢ CÁC TRƯỜNG HỢP CỦA ID TỪ BACKEND
        const id = p.id || p.Id || (p._id && p._id.$oid) || p._id;

        const vi = p.localizations?.find(l => l.lang === "vi");

        const card = `
        <div class="card">
            <img src="http://localhost:5188/images/${p.image}">
            <div class="card-body">
                <h3>${p.name}</h3>
                <p class="address">${p.address}</p>
                <p class="desc">
                    ${vi ? vi.description.substring(0, 80) + "..." : "Không có mô tả"}
                </p>
                <div class="actions">
                    <button class="btn edit" onclick="editPOI('${id}')">Edit</button>
                    <button class="btn delete" onclick="deletePOI_UI('${id}')">Delete</button>
                </div>
            </div>
        </div>
        `;
        grid.innerHTML += card;
    });
}

// DELETE (Đã cập nhật bắt lỗi)
window.deletePOI_UI = async function(id) {
    if (!id || id === 'undefined') {
        alert("Lỗi: Không lấy được ID của địa điểm này!");
        return;
    }

    if (!confirm("Bạn có chắc chắn muốn xóa POI này khỏi Database?")) return;

    try {
        const res = await deletePOI(id);
        
        if (res.ok) {
            // Chỉ tải lại danh sách NẾU backend báo xóa thành công
            loadPOIs(); 
        } else {
            // Nếu backend báo lỗi (VD: không tìm thấy)
            const errorData = await res.json();
            alert("Lỗi từ Server: " + (errorData.message || "Không thể xóa"));
        }
    } catch (err) {
        console.error(err);
        alert("Lỗi kết nối API!");
    }
}

// EDIT
window.editPOI = function(id) {
    window.location.href = `edit.html?id=${id}`;
}

// Load khi mở trang
loadPOIs();