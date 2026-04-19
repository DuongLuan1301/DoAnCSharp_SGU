import { getPOIs } from "./api.js";

document.addEventListener("DOMContentLoaded", async () => {
    const tableBody = document.getElementById("clientTableBody");
    const modal = document.getElementById("clientModal");
    const clientForm = document.getElementById("clientForm");

    const poiListModal = document.getElementById("poiListModal");
    const poiListBody = document.getElementById("poiListBody");
    const changeOwnerModal = document.getElementById("changeOwnerModal");
    const newOwnerSelect = document.getElementById("newOwnerSelect");

    // Nơi chứa elements cho Gán POI
    const assignPoiModal = document.getElementById("assignPoiModal");
    const assignPoiSelect = document.getElementById("assignPoiSelect");

    let clients = [];
    let poiData = [];

    // =====================================
    // 1. TẢI DỮ LIỆU TỪ MONGODB
    // =====================================
    async function loadData() {
        try {
            poiData = await getPOIs();
            const res = await fetch("http://127.0.0.1:5188/admin/users");
            clients = res.ok ? await res.json() : [];

            newOwnerSelect.innerHTML = clients.map(c =>
                `<option value="${c.id || c.Id}">${c.name || c.Name}</option>`
            ).join('');

            renderTable();
        } catch (e) { console.error("Lỗi tải dữ liệu", e); }
    }

    // =====================================
    // 2. VẼ BẢNG USER CHÍNH
    // =====================================
    function renderTable() {
        tableBody.innerHTML = "";

        clients.forEach(client => {
            const clientId = client.id || client.Id;
            const clientPois = poiData.filter(p => p.clientId === clientId);
            const ownedPoisCount = clientPois.length;

            const isLocked = client.status === "locked";
            const statusBadge = isLocked
                ? `<span style="background: #fee2e2; color: #ef4444; padding: 4px 8px; border-radius: 4px; font-size: 12px; font-weight: 600;">Đã Khóa</span>`
                : `<span style="background: #dcfce3; color: #10b981; padding: 4px 8px; border-radius: 4px; font-size: 12px; font-weight: 600;">Hoạt động</span>`;

            const row = `
                <tr style="${isLocked ? 'opacity: 0.6;' : ''}">
                    <td style="font-weight: 600;">${client.name || client.Name}</td>
                    <td>${client.email || client.Email}</td>
                    <td>${client.phone || client.Phone}</td>
                    <td style="text-align: center;">
                        <div style="display: flex; gap: 8px; justify-content: center;">
                            <button class="btn-view-poi" onclick="viewClientPOIs('${clientId}', '${client.name || client.Name}')">
                                Xem (${ownedPoisCount} gian hàng)
                            </button>
                        </div>
                    </td>
                    <td style="text-align: center;">${statusBadge}</td>
                    <td style="text-align: center;">
                        <button class="btn edit" onclick="editClient('${clientId}')" style="padding: 6px 10px; font-size: 12px;">Sửa</button>
                        <button class="btn" onclick="toggleLock('${clientId}')" style="background: ${isLocked ? "#10b981" : "#f59e0b"}; color: white; padding: 6px 10px; font-size: 12px; border: none; border-radius: 6px; cursor: pointer;">${isLocked ? "Mở khóa" : "Khóa"}</button>
                        <button class="btn delete" onclick="deleteClient('${clientId}')" style="padding: 6px 10px; font-size: 12px;">Xóa</button>
                    </td>
                </tr>
            `;
            tableBody.innerHTML += row;
        });
    }

    // =====================================
    // 3. XỬ LÝ FORM THÊM / SỬA TÀI KHOẢN 
    // =====================================
    document.getElementById("btnAddClient").addEventListener("click", () => {
        clientForm.reset();
        document.getElementById("clientId").value = "";
        document.getElementById("modalTitle").innerText = "Thêm Tài Khoản Mới";
        document.getElementById("password").required = true;
        modal.classList.add("active");
    });
    document.getElementById("btnCancel").addEventListener("click", () => modal.classList.remove("active"));
    clientForm.addEventListener("submit", async (e) => {
        e.preventDefault();
        const id = document.getElementById("clientId").value;
        const payload = {
            name: document.getElementById("fullName").value, email: document.getElementById("email").value,
            phone: document.getElementById("phone").value, password: document.getElementById("password").value
        };
        const btnSave = clientForm.querySelector("button[type='submit']");
        const originalText = btnSave.innerText;
        btnSave.innerText = "Đang lưu..."; btnSave.disabled = true;
        try {
            if (id) {
                const res = await fetch(`http://127.0.0.1:5188/admin/users/${id}`, { method: "PUT", headers: { "Content-Type": "application/json" }, body: JSON.stringify(payload) });
                if (!res.ok) throw new Error(await res.text()); alert("Cập nhật tài khoản thành công!");
            } else {
                const res = await fetch("http://127.0.0.1:5188/api/auth/register", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(payload) });
                if (!res.ok) throw new Error(await res.text()); alert("Thêm tài khoản mới thành công!");
            }
            modal.classList.remove("active"); loadData();
        } catch (error) { alert("Lỗi: " + error.message); } finally { btnSave.innerText = originalText; btnSave.disabled = false; }
    });

    // =====================================
    // 4A. CHUYỂN CHỦ TRONG DANH SÁCH (CÓ SẴN)
    // =====================================
    window.viewClientPOIs = function (clientId, clientName) {
        document.getElementById("poiListTitle").innerText = `Gian hàng của: ${clientName}`;
        poiListBody.innerHTML = "";
        const clientPois = poiData.filter(p => p.clientId === clientId);
        if (clientPois.length === 0) {
            poiListBody.innerHTML = `<tr><td colspan="5" style="text-align:center; padding: 20px;">Tài khoản này chưa có gian hàng nào.</td></tr>`;
        } else {
            clientPois.forEach(p => {
                const imgUrl = p.image ? `http://127.0.0.1:5188/images/${p.image}` : "https://via.placeholder.com/50";
                poiListBody.innerHTML += `
                    <tr>
                        <td><img src="${imgUrl}" style="width: 50px; height: 50px; object-fit: cover; border-radius: 5px;"></td>
                        <td style="font-weight: 600;">${p.name || p.Name}</td>
                        <td><span style="font-size: 12px; color: #64748b;">${p.address || p.Address}</span></td>
                        <td><span style="color: #10b981; font-weight: bold;">${p.views || 0}</span> view</td>
                        <td style="text-align: center;"><button class="btn edit" onclick="openChangeOwnerModal('${p.id}', '${p.name || p.Name}')" style="font-size: 11px; padding: 5px 8px;">Chuyển chủ</button></td>
                    </tr>
                `;
            });
        }
        poiListModal.classList.add("active");
    };

    window.openChangeOwnerModal = function (poiId, poiName) {
        document.getElementById("targetPoiId").value = poiId;
        document.getElementById("targetPoiName").innerText = `Gian hàng: ${poiName}`;
        poiListModal.classList.remove("active"); changeOwnerModal.classList.add("active");
    };

    document.getElementById("btnConfirmChangeOwner").addEventListener("click", async () => {
        const poiId = document.getElementById("targetPoiId").value; const newClientId = newOwnerSelect.value;
        if (!newClientId) return alert("Vui lòng chọn chủ mới!");
        try {
            const res = await fetch(`http://127.0.0.1:5188/admin/poi/${poiId}/assign?clientId=${newClientId}`, { method: "PUT" });
            if (res.ok) { alert("Chuyển quyền quản lý thành công!"); changeOwnerModal.classList.remove("active"); loadData(); }
            else { alert("Lỗi cập nhật trên Server!"); }
        } catch (err) { alert("Lỗi: " + err.message); }
    });
    document.getElementById("btnCancelChangeOwner").addEventListener("click", () => { changeOwnerModal.classList.remove("active"); poiListModal.classList.add("active"); });
    document.getElementById("btnClosePoiList").addEventListener("click", () => poiListModal.classList.remove("active"));

    // =====================================
    // 5. CÁC HÀM XÓA, SỬA, KHÓA ACCOUNT
    // =====================================
    window.editClient = function (id) {
        const client = clients.find(c => (c.id || c.Id) === id);
        if (!client) return;
        document.getElementById("clientId").value = id;
        document.getElementById("fullName").value = client.name || client.Name;
        document.getElementById("email").value = client.email || client.Email;
        document.getElementById("phone").value = client.phone || client.Phone;
        document.getElementById("password").required = false;
        document.getElementById("modalTitle").innerText = "Sửa Tài Khoản";
        modal.classList.add("active");
    };

    window.deleteClient = async function (id) {
        if (!confirm("Xóa tài khoản này?")) return;
            const res = await fetch(`http://127.0.0.1:5188/admin/users/${id}`, {
                method: "DELETE"
            });
            const message = await res.text();
            if (!res.ok) {
                alert(message);
                return;
            }
            alert("Xóa thành công!");
            loadData();
    };

    window.toggleLock = async function (id) {
        if (confirm("Đổi trạng thái tài khoản này?")) {
            const res = await fetch(`http://127.0.0.1:5188/admin/users/${id}/status`, { method: "PUT" });
            if (res.ok) loadData(); else alert("Lỗi hệ thống!");
        }
    };

    loadData();
});