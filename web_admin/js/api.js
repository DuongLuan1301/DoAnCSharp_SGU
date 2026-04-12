const BASE_URL = "http://localhost:5188/admin/poi";

// GET ALL POIs
export async function getPOIs() {
    const res = await fetch(BASE_URL);
    return await res.json();
}

// DELETE POI
export async function deletePOI(id) {
    return await fetch(`${BASE_URL}/${id}`, {
        method: "DELETE"
    });
}
// GET POI BY ID (Lấy dữ liệu 1 POI để điền vào form sửa)
export async function getPOIById(id) {
    // Dùng cổng 5188. Gọi endpoint /api/poi/{id} đã có sẵn ở Backend
    const res = await fetch(`http://127.0.0.1:5188/api/poi/${id}`);
    return await res.json();
}

// UPDATE POI (Gửi dữ liệu đã sửa lên server)
export async function updatePOI(id, poiData) {
    return await fetch(`http://127.0.0.1:5188/api/poi/${id}`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(poiData)
    });}