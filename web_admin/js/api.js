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