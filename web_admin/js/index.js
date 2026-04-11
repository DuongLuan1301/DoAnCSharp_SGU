import { getPOIs, deletePOI } from "./api.js";

const grid = document.querySelector(".grid");

async function loadPOIs() {
    const data = await getPOIs();

    grid.innerHTML = "";

    data.forEach(p => {
        const id = p.id || p._id?.$oid;

        const vi = p.localizations?.find(l => l.lang === "vi");

        const card = `
        <div class="card">
            <img src="http://localhost:5188/images/${p.image}">

            <div class="card-body">
                <h3>${p.name}</h3>
                <p class="address">${p.address}</p>
                <p class="desc">
                    ${vi ? vi.description.substring(0, 80) + "..." : ""}
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

//DELETE
window.deletePOI_UI = async function(id) {
    if (!confirm("Delete this POI?")) return;

    await deletePOI(id);
    loadPOIs();
}

//EDIT
window.editPOI = function(id) {
    window.location.href = `edit.html?id=${id}`;
}

// load khi mở trang
loadPOIs();