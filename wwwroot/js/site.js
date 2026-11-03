(() => {
    "use strict";

    const BASE_URL = "/api/trips";

    const form = document.getElementById("trip-form");
    const idInput = document.getElementById("trip-id");
    const submitBtn = document.getElementById("submit-btn");
    const cancelBtn = document.getElementById("cancel-btn");
    const message = document.getElementById("message");
    const tbody = document.getElementById("trips-body");
    const rowTemplate = document.getElementById("trip-row-template");

    const fields = {
        name: document.getElementById("name"),
        destination: document.getElementById("destination"),
        startDate: document.getElementById("startDate"),
        endDate: document.getElementById("endDate"),
        description: document.getElementById("description"),
        budget: document.getElementById("budget")
    };

    // ---------- helpers ----------

    function showMessage(text, isError = false) {
        message.textContent = text;
        message.classList.toggle("error", isError);
    }

    async function request(url, options = {}) {
        const response = await fetch(url, {
            headers: { "Content-Type": "application/json" },
            ...options
        });

        if (!response.ok) {
            const body = await readError(response);
            throw new Error(body || `שגיאת שרת (${response.status})`);
        }

        return response.status === 204 ? null : response.json();
    }

    async function readError(response) {
        try {
            const body = await response.json();
            return body.error || JSON.stringify(body);
        } catch {
            return null;
        }
    }

    function formatDate(value) {
        return value ? new Date(`${value}T00:00:00`).toLocaleDateString("he-IL") : "";
    }

    function formatBudget(value) {
        return value == null ? "" : Number(value).toLocaleString("he-IL");
    }

    function readForm() {
        return {
            name: fields.name.value.trim(),
            destination: fields.destination.value.trim(),
            startDate: fields.startDate.value,
            endDate: fields.endDate.value,
            description: fields.description.value.trim() || null,
            budget: fields.budget.value === "" ? null : Number(fields.budget.value)
        };
    }

    function resetForm() {
        form.reset();
        idInput.value = "";
        submitBtn.textContent = "הוספה";
        cancelBtn.hidden = true;
        showMessage("");
    }

    // ---------- rendering ----------

    function renderRow(trip) {
        const row = rowTemplate.content.firstElementChild.cloneNode(true);
        row.dataset.id = trip.id; // Guid string, never parsed as a number

        row.querySelector('[data-field="name"]').textContent = trip.name;
        row.querySelector('[data-field="destination"]').textContent = trip.destination;
        row.querySelector('[data-field="startDate"]').textContent = formatDate(trip.startDate);
        row.querySelector('[data-field="endDate"]').textContent = formatDate(trip.endDate);
        row.querySelector('[data-field="description"]').textContent = trip.description ?? "";
        row.querySelector('[data-field="budget"]').textContent = formatBudget(trip.budget);

        // Event delegation on the row — no inline onclick, so the Guid never appears in markup.
        row.addEventListener("click", (event) => {
            const button = event.target.closest("button[data-action]");
            if (!button) return;

            if (button.dataset.action === "edit") {
                startEdit(trip);
            } else if (button.dataset.action === "delete") {
                deleteTrip(trip.id);
            }
        });

        return row;
    }

    function renderTrips(trips) {
        tbody.replaceChildren();

        if (trips.length === 0) {
            const row = document.createElement("tr");
            const cell = document.createElement("td");
            cell.colSpan = 7;
            cell.className = "empty";
            cell.textContent = "אין טיולים עדיין.";
            row.appendChild(cell);
            tbody.appendChild(row);
            return;
        }

        const fragment = document.createDocumentFragment();
        trips.forEach(trip => fragment.appendChild(renderRow(trip)));
        tbody.appendChild(fragment);
    }

    // ---------- actions ----------

    async function loadTrips() {
        try {
            const trips = await request(BASE_URL);
            renderTrips(trips);
        } catch (error) {
            renderTrips([]);
            showMessage(error.message, true);
        }
    }

    function startEdit(trip) {
        idInput.value = trip.id;
        fields.name.value = trip.name;
        fields.destination.value = trip.destination;
        fields.startDate.value = trip.startDate;
        fields.endDate.value = trip.endDate;
        fields.description.value = trip.description ?? "";
        fields.budget.value = trip.budget ?? "";

        submitBtn.textContent = "שמירה";
        cancelBtn.hidden = false;
        showMessage("");
        fields.name.focus();
    }

    async function deleteTrip(id) {
        if (!confirm("למחוק את הטיול?")) return;

        try {
            await request(`${BASE_URL}/${id}`, { method: "DELETE" });
            showMessage("הטיול נמחק.");
            await loadTrips();
        } catch (error) {
            showMessage(error.message, true);
        }
    }

    form.addEventListener("submit", async (event) => {
        event.preventDefault();

        const id = idInput.value;
        const payload = readForm();

        try {
            if (id) {
                await request(`${BASE_URL}/${id}`, {
                    method: "PUT",
                    body: JSON.stringify(payload)
                });
                showMessage("הטיול עודכן.");
            } else {
                await request(BASE_URL, {
                    method: "POST",
                    body: JSON.stringify(payload)
                });
                showMessage("הטיול נוסף.");
            }

            resetForm();
            await loadTrips();
        } catch (error) {
            showMessage(error.message, true);
        }
    });

    cancelBtn.addEventListener("click", resetForm);

    // ---------- init ----------

    resetForm();
    loadTrips();
})();
