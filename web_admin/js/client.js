import { getPOIs } from "./api.js";

document.addEventListener("DOMContentLoaded", async () => {
    const tableBody = document.getElementById("clientTableBody");
    const modal = document.getElementById("clientModal");
    const clientForm = document.getElementById("clientForm");
    const poiSelect = document.getElementById("poiSelect");
    
    // 1. TẢI DANH SÁCH POI TỪ BACKEND
    let poiData = [];
    try {
        poiData = await getPOIs();
        // Đổ POI vào thẻ Select trong Modal
        poiSelect.innerHTML = '<option value="">-- Chọn một gian hàng --</option>';
        poiData.forEach(p => {
            const id = p.id || p.Id || p._id;
            poiSelect.innerHTML += `<option value="${id}">${p.name || p.Name}</option>`;
        });
    } catch (e) {
        console.error("Lỗi tải POI", e);
    }

    // 2. GIẢ LẬP DỮ LIỆU CLIENT (Dành cho việc test UI)
    // Sau này bạn thay bằng: fetch("http://localhost:5188/api/clients")
    let clients = [
        { id: "c1", username: "bunbohue_admin", fullName: "Nguyễn Văn A", poiId: poiData[0]?.id || "p1" },
        { id: "c2", username: "banhmi_hh", fullName: "Trần Thị B", poiId: poiData[1]?.id || "p2" }
    ];

    // 3. RENDER BẢNG
    function renderTable() {
        tableBody.innerHTML = "";
        
        clients.forEach(client => {
            // Tìm POI tương ứng với client này để lấy Tên và Số lượt truy cập
            const linkedPoi = poiData.find(p => (p.id || p.Id || p._id) === client.poiId);
            const poiName = linkedPoi ? (linkedPoi.name || linkedPoi.Name) : "<span style='color:red'>Chưa gán</span>";
            // Lấy lượt view từ POI (giả lập random nếu chưa có trường views trong DB)
            const views = linkedPoi ? (linkedPoi.views || Math.floor(Math.random() * 500)) : 0;

            const row = `
                <tr>
                    <td style="font-weight: 600;">${client.username}</td>
                    <td>${client.fullName}</td>
                    <td>${poiName}</td>
                    <td style="color: var(--success); font-weight: 600;">${views.toLocaleString()}</td>
                    <td style="text-align: center;">
                        <button class="btn edit" onclick="editClient('${client.id}')" style="padding: 6px 12px; font-size: 12px;">Sửa</button>
                        <button class="btn delete" onclick="deleteClient('${client.id}')" style="padding: 6px 12px; font-size: 12px;">Xóa</button>
                    </td>
                </tr>
            `;
            tableBody.innerHTML += row;
        });
    }

    renderTable();

    // 4. XỬ LÝ MODAL (Đóng / Mở)
    document.getElementById("btnAddClient").addEventListener("click", () => {
        clientForm.reset();
        document.getElementById("clientId").value = "";
        document.getElementById("modalTitle").innerText = "Thêm Tài Khoản Mới";
        document.getElementById("password").required = true; // Bắt buộc nhập pass khi tạo mới
        modal.classList.add("active");
    });

    document.getElementById("btnCancel").addEventListener("click", () => {
        modal.classList.remove("active");
    });

    // 5. SUBMIT FORM (LƯU)
    clientForm.addEventListener("submit", (e) => {
        e.preventDefault();
        
        const id = document.getElementById("clientId").value;
        const username = document.getElementById("username").value;
        const fullName = document.getElementById("fullName").value;
        const poiId = document.getElementById("poiSelect").value;

        if (!poiId) return alert("Vui lòng gán 1 gian hàng cho tài khoản này!");

        if (id) {
            // UPDATE
            const idx = clients.findIndex(c => c.id === id);
            if (idx > -1) {
                clients[idx].username = username;
                clients[idx].fullName = fullName;
                clients[idx].poiId = poiId;
            }
            alert("Cập nhật thành công!");
        } else {
            // CREATE MỚI
            const newClient = {
                id: "c" + Date.now(), // ID tạm
                username,
                fullName,
                poiId
            };
            clients.push(newClient);
            alert("Thêm tài khoản thành công!");
        }

        modal.classList.remove("active");
        renderTable();
        // Lưu ý: Sau này gọi API POST/PUT lên ASP.NET Core ở đây
    });

    // 6. GẮN HÀM XÓA/SỬA VÀO WINDOW ĐỂ BẮT TỪ HTML TRONG ROW
    window.editClient = function(id) {
        const client = clients.find(c => c.id === id);
        if (!client) return;

        document.getElementById("clientId").value = client.id;
        document.getElementById("username").value = client.username;
        document.getElementById("fullName").value = client.fullName;
        document.getElementById("poiSelect").value = client.poiId;
        document.getElementById("password").required = false; // Đổi pass là không bắt buộc khi sửa

        document.getElementById("modalTitle").innerText = "Sửa Tài Khoản";
        modal.classList.add("active");
    };

    window.deleteClient = function(id) {
        if (confirm("Bạn có chắc chắn muốn xóa tài khoản này?")) {
            clients = clients.filter(c => c.id !== id);
            renderTable();
            // Cần gọi api delete: fetch(`.../api/clients/${id}`, { method: 'DELETE' })
        }
    };
});