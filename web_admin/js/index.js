import { getPOIs, deletePOI } from "./api.js";

const grid = document.querySelector(".grid");
const searchInput = document.querySelector(".search input"); // Lấy ô input tìm kiếm

// Biến toàn cục để lưu trữ toàn bộ dữ liệu tải về từ Backend
let allPOIs = []; 

// 1. HÀM TẢI DỮ LIỆU TỪ API
async function loadPOIs() {
    try {
        allPOIs = await getPOIs(); // Lưu dữ liệu vào biến toàn cục
        renderPOIs(allPOIs);       // Gọi hàm vẽ giao diện với toàn bộ dữ liệu
    } catch (error) {
        console.error("Lỗi khi tải danh sách POI:", error);
        grid.innerHTML = "<p style='color: red;'>Lỗi kết nối đến máy chủ!</p>";
    }
}

// 2. HÀM VẼ GIAO DIỆN (RENDER)
function renderPOIs(dataList) {
    grid.innerHTML = ""; // Xóa dữ liệu cũ trên màn hình

    // Nếu không có dữ liệu nào khớp với tìm kiếm
    if (dataList.length === 0) {
        grid.innerHTML = `
            <div style="grid-column: 1 / -1; text-align: center; padding: 40px; color: var(--text-muted);">
                <span style="font-size: 40px; display: block; margin-bottom: 10px;">🔍</span>
                Không tìm thấy gian hàng nào phù hợp.
            </div>
        `;
        return;
    }

    // Vẽ từng thẻ POI
    dataList.forEach(p => {
        const id = p.id || p.Id || (p._id && p._id.$oid) || p._id;
        const vi = p.localizations?.find(l => l.lang === "vi");
        const name = p.name || p.Name || "Không tên";
        const address = p.address || p.Address || "Không có địa chỉ";
        
        // Sửa đường dẫn ảnh nếu cần khớp với Backend C# của bạn
        const imageUrl = p.image ? `http://localhost:5188/images/${p.image}` : "https://via.placeholder.com/300x180?text=No+Image";

        const card = `
        <div class="card">
            <img src="${imageUrl}" alt="${name}">
            <div class="card-body">
                <h3>${name}</h3>
                <p class="address">${address}</p>
                <p class="desc">
                    ${vi ? vi.description.substring(0, 80) + "..." : "Không có mô tả chi tiết."}
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

// ==========================================
// 3. TÍNH NĂNG TÌM KIẾM (REAL-TIME)
// ==========================================
if (searchInput) {
    searchInput.addEventListener("input", (e) => {
        // Lấy từ khóa người dùng nhập, chuyển về chữ thường và xóa khoảng trắng thừa
        const searchTerm = e.target.value.toLowerCase().trim();

        // Lọc danh sách allPOIs
        const filteredPOIs = allPOIs.filter(p => {
            const name = (p.name || p.Name || "").toLowerCase();
            const address = (p.address || p.Address || "").toLowerCase();
            
            // Trả về true nếu tên hoặc địa chỉ có chứa từ khóa
            return name.includes(searchTerm) || address.includes(searchTerm);
        });

        // Vẽ lại giao diện với danh sách đã lọc
        renderPOIs(filteredPOIs);
    });
}

// ==========================================
// 4. HÀM XÓA VÀ SỬA (Đã gắn vào Window để gọi từ HTML)
// ==========================================
window.deletePOI_UI = async function(id) {
    if (!id || id === 'undefined') {
        alert("Lỗi: Không lấy được ID của địa điểm này!");
        return;
    }

    if (!confirm("Bạn có chắc chắn muốn xóa POI này khỏi Database?")) return;

    try {
        const res = await deletePOI(id);
        if (res.ok) {
            loadPOIs(); // Tải lại danh sách từ server sau khi xóa thành công
        } else {
            const errorData = await res.json();
            alert("Lỗi từ Server: " + (errorData.message || "Không thể xóa"));
        }
    } catch (err) {
        console.error(err);
        alert("Lỗi kết nối API!");
    }
}

window.editPOI = function(id) {
    window.location.href = `edit.html?id=${id}`;
}

// Khởi chạy khi mở trang
loadPOIs();