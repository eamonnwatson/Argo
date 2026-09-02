// Argo Intake page script
// Depends on common.js (window.Argo) being loaded first.

(function () {
    "use strict";
    var STORAGE_KEY = "argo-project-intake-v1";
    var SCHEMA_VERSION = 1;
    var currentStep = 1;
    var maxStep = 5;
    var saveTimer = null;
    var form = document.getElementById("request-form");

    function pad(value) {
        return String(value)
            .padStart(2, "0");
    }

    function newRequestId() {
        var d = new Date();
        return "REQ-" + d.getFullYear() + pad(d.getMonth() + 1) + pad(d.getDate()) + "-" + pad(d.getHours()) + pad(d.getMinutes()) + pad(d.getSeconds());
    }

    function setConditional(id, show) {
        var section = document.getElementById(id);
        section.hidden = !show;
        Array.from(section.querySelectorAll("input,select,textarea"))
            .forEach(function (el) {
                el.disabled = !show;
            });
    }

    function updateConditional() {
        var type = form.elements.requestType.value;
        setConditional("existing-project-wrap", form.elements.relatedProject.value === "Yes");
        setConditional("client-names-wrap", form.elements.clientImpact.value === "Yes");
        setConditional("sensitive-details-wrap", form.elements.sensitiveData.value === "Yes" || form.elements.sensitiveData.value === "Unsure");
        setConditional("reporting-section", type === "Reporting or dashboard request");
        setConditional("data-section", type === "Data integration/source request");
    }

    function getRequestData() {
        var result = {};
        Array.from(form.elements)
            .forEach(function (el) {
                if (!el.name || el.disabled || el.type === "button" || el.type === "submit" || el.type === "file") return;
                if (el.type === "checkbox") {
                    if (!result[el.name]) result[el.name] = [];
                    if (el.checked) result[el.name].push(el.value);
                } else if (el.type === "radio") {
                    if (el.checked) result[el.name] = el.value;
                } else result[el.name] = el.value;
            });
        return result;
    }

    function applyData(data) {
        form.reset();
        Array.from(form.elements)
            .forEach(function (el) {
                if (!el.name || data[el.name] === undefined) return;
                if (el.type === "checkbox") el.checked = Array.isArray(data[el.name]) && data[el.name].indexOf(el.value) >= 0;
                else if (el.type === "radio") el.checked = data[el.name] === el.value;
                else el.value = data[el.name];
            });
        if (!form.elements.requestId.value) form.elements.requestId.value = newRequestId();
        updateConditional();
        updateMeta();
    }

    function payload(status) {
        return {
            artifactType: "project-request-intake",
            schemaVersion: SCHEMA_VERSION,
            status: status || "Draft",
            requestId: form.elements.requestId.value,
            updatedAt: new Date()
                .toISOString(),
            request: getRequestData()
        };
    }

    function saveDraft(showToast) {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(payload("Draft")));
        updateMeta("Draft saved");
        if (showToast) Argo.toast("Draft saved in this browser");
    }

    function loadDraft() {
        try {
            var saved = JSON.parse(localStorage.getItem(STORAGE_KEY));
            if (saved && saved.artifactType === "project-request-intake" && saved.request) {
                applyData(saved.request);
                updateMeta("Draft restored");
                return true;
            }
        } catch (error) { }
        return false;
    }

    function updateMeta(status) {
        var id = form.elements.requestId.value || "REQ-PENDING";
        document.getElementById("request-id-pill")
            .textContent = id;
        var el = document.getElementById("save-status");
        el.textContent = status || "Editing draft";
        el.className = "pill" + (status && status.indexOf("saved") >= 0 ? " green" : "");
    }

    function scheduleSave() {
        clearTimeout(saveTimer);
        updateMeta("Unsaved changes");
        saveTimer = setTimeout(function () {
            saveDraft(false);
        }, 500);
    }

    function firstInvalidInStep(step) {
        var section = document.querySelector('.form-step[data-step="' + step + '"]');
        return Array.from(section.querySelectorAll("input,select,textarea"))
            .find(function (el) {
                return !el.disabled && !el.checkValidity();
            });
    }

    function validateStep(step) {
        var invalid = firstInvalidInStep(step);
        if (invalid) {
            invalid.reportValidity();
            invalid.focus();
            return false;
        }
        return true;
    }

    function validateAll() {
        for (var step = 1; step <= maxStep; step++) {
            var invalid = firstInvalidInStep(step);
            if (invalid) {
                goToStep(step);
                invalid.reportValidity();
                invalid.focus();
                return false;
            }
        }
        return true;
    }

    function completedStep(step) {
        return !firstInvalidInStep(step);
    }

    function goToStep(step) {
        currentStep = Math.max(1, Math.min(maxStep, step));
        document.querySelectorAll(".form-step")
            .forEach(function (el) {
                el.hidden = Number(el.dataset.step) !== currentStep;
            });
        document.querySelectorAll(".step-button")
            .forEach(function (el) {
                var number = Number(el.dataset.goStep);
                el.classList.toggle("active", number === currentStep);
                el.classList.toggle("complete", number < currentStep && completedStep(number));
            });
        document.getElementById("progress-text")
            .textContent = "Step " + currentStep + " of " + maxStep;
        document.getElementById("progress-bar")
            .style.width = (currentStep / maxStep * 100) + "%";
        document.getElementById("back-btn")
            .disabled = currentStep === 1;
        document.getElementById("next-btn")
            .hidden = currentStep === maxStep;
        document.getElementById("export-btn")
            .hidden = currentStep !== maxStep;
        document.getElementById("print-btn")
            .hidden = currentStep !== maxStep;
        if (currentStep === maxStep) updateReview();
        document.querySelector(".form-card")
            .scrollIntoView({
                behavior: "smooth",
                block: "start"
            });
    }

    function humanValue(value) {
        if (Array.isArray(value)) return value.join(", ");
        return value || "Not provided";
    }

    function reviewCard(title, items, full) {
        var rows = items.map(function (item) {
            return '<div class="review-row"><div class="review-label">' + Argo.escapeHtml(item[0]) + '</div><div class="review-value">' + Argo.escapeHtml(humanValue(item[1])) + '</div></div>';
        })
            .join("");
        return '<section class="review-card ' + (full ? 'full' : '') + '"><h3>' + Argo.escapeHtml(title) + '</h3><div class="review-list">' + rows + '</div></section>';
    }

    function updateReview() {
        var d = getRequestData();
        var html = "";
        html += reviewCard("Request and ownership", [
            ["Request ID", d.requestId],
            ["Title", d.requestTitle],
            ["Type", d.requestType],
            ["Requester", d.requesterName],
            ["Department", d.department],
            ["Business sponsor", d.businessSponsor],
            ["Business owner", d.businessOwner]
        ], false);
        html += reviewCard("Business need", [
            ["Problem", d.businessProblem],
            ["Desired outcome", d.desiredOutcome],
            ["Success measures", d.successMeasures],
            ["Affected groups", d.affectedGroups]
        ], false);
        html += reviewCard("Impact and timing", [
            ["Reach", d.impactScope],
            ["Business impact", d.businessImpact],
            ["Client impact", d.clientImpact + (d.clientNames ? " - " + d.clientNames : "")],
            ["Benefits", d.expectedBenefits],
            ["If nothing changes", d.noActionImpact],
            ["Desired date", d.desiredDate],
            ["Date reason", d.dateReason]
        ], true);
        html += reviewCard("Scope and dependencies", [
            ["In scope", d.inScope],
            ["Out of scope", d.outOfScope],
            ["Dependencies", d.dependencies],
            ["Strategic alignment", d.strategicAlignment]
        ], false);
        html += reviewCard("Systems and data", [
            ["Systems", d.systemsInvolved],
            ["Data sources", d.dataSources],
            ["Sensitive data", d.sensitiveData],
            ["Data concern", d.sensitiveDetails],
            ["Technical owners", d.technicalOwners],
            ["Supporting materials", d.supportingMaterials]
        ], false);
        if (d.requestType === "Reporting or dashboard request") html += reviewCard("Reporting details", [
            ["Reports", d.reportNames],
            ["Frequency", d.reportFrequency],
            ["Delivery", d.deliveryTime],
            ["Recipients", d.reportRecipients],
            ["Format", d.outputFormat],
            ["Samples", d.samplesAvailable],
            ["Sample references", d.sampleReferences],
            ["Manual steps", d.manualSteps]
        ], true);
        if (d.requestType === "Data integration/source request") html += reviewCard("Data integration details", [
            ["Source", d.sourceSystem],
            ["Target", d.targetSystem],
            ["Data owner", d.dataOwner],
            ["Refresh", d.refreshFrequency],
            ["Volume/history", d.dataVolume]
        ], true);
        document.getElementById("review-summary")
            .innerHTML = html;
    }

    function downloadPayload(exported) {
        var blob = new Blob([JSON.stringify(exported, null, 2)], {
            type: "application/json"
        });
        var url = URL.createObjectURL(blob);
        var link = document.createElement("a");
        link.href = url;
        link.download = "argo-request-" + exported.requestId + ".json";
        link.click();
        URL.revokeObjectURL(url);
    }
    async function submitRequest() {
        if (!validateAll()) return;
        var exported = payload("Submitted for Triage");
        exported.submittedAt = new Date()
            .toISOString();
        var button = document.getElementById("export-btn");
        button.disabled = true;
        try {
            var response = await fetch(Argo.API_BASE + "/intake-submissions", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(exported)
            });
            if (!response.ok) {
                var errorMessage = "Server responded " + response.status;
                try {
                    var body = await response.json();
                    if (Array.isArray(body) && body.length) errorMessage = body.join("; ");
                    else if (body && typeof body === "object") {
                        if (typeof body.error === "string") errorMessage = body.error;
                        else if (typeof body.message === "string") errorMessage = body.message;
                    } else if (typeof body === "string" && body) errorMessage = body;
                } catch (parseError) { }
                throw new Error(errorMessage);
            }
            localStorage.setItem(STORAGE_KEY, JSON.stringify(exported));
            updateMeta("Submitted for triage");
            document.getElementById("submitted-request-id")
                .textContent = exported.requestId;
            document.getElementById("submitted-title")
                .textContent = exported.request.requestTitle || "Untitled request";
            document.getElementById("submission-dialog")
                .showModal();
            Argo.toast("Request added to the Argo Portfolio Waiting queue");
        } catch (error) {
            console.error(error);
            Argo.toast("Could not reach the Argo server, so this request has not been submitted yet: " + error.message);
        } finally {
            button.disabled = false;
        }
    }

    function startNew() {
        if (!confirm("Start a new request and clear the current browser draft?")) return;
        localStorage.removeItem(STORAGE_KEY);
        form.reset();
        form.elements.requestId.value = newRequestId();
        updateConditional();
        goToStep(1);
        updateMeta("New draft");
        saveDraft(false);
        Argo.toast("New request started");
    }

    form.addEventListener("input", function () {
        updateConditional();
        scheduleSave();
    });
    form.addEventListener("change", function () {
        updateConditional();
        scheduleSave();
    });
    form.addEventListener("submit", function (event) {
        event.preventDefault();
    });
    document.querySelectorAll("[data-go-step]")
        .forEach(function (button) {
            button.addEventListener("click", function () {
                goToStep(Number(button.dataset.goStep));
            });
        });
    document.getElementById("back-btn")
        .addEventListener("click", function () {
            goToStep(currentStep - 1);
        });
    document.getElementById("next-btn")
        .addEventListener("click", function () {
            if (validateStep(currentStep)) goToStep(currentStep + 1);
        });
    document.getElementById("save-btn")
        .addEventListener("click", function () {
            saveDraft(true);
        });
    document.getElementById("new-btn")
        .addEventListener("click", startNew);
    document.getElementById("export-btn")
        .addEventListener("click", submitRequest);
    document.getElementById("download-copy-btn")
        .addEventListener("click", function () {
            var saved = JSON.parse(localStorage.getItem(STORAGE_KEY) || "null");
            if (saved) downloadPayload(saved);
        });
    document.querySelectorAll("[data-close]")
        .forEach(function (button) {
            button.addEventListener("click", function () {
                document.getElementById(button.dataset.close)
                    .close();
            });
        });
    document.getElementById("print-btn")
        .addEventListener("click", function () {
            updateReview();
            window.print();
        });
    document.getElementById("import-btn")
        .addEventListener("click", function () {
            document.getElementById("import-file")
                .click();
        });
    document.getElementById("import-file")
        .addEventListener("change", function (event) {
            var file = event.target.files[0];
            if (!file) return;
            var reader = new FileReader();
            reader.onload = function () {
                try {
                    var imported = JSON.parse(String(reader.result));
                    if (imported.artifactType !== "project-request-intake" || !imported.request) throw new Error("Invalid");
                    applyData(imported.request);
                    goToStep(1);
                    saveDraft(false);
                    Argo.toast("Request imported");
                } catch (error) {
                    alert("That file is not a valid Argo Intake export.");
                }
            };
            reader.readAsText(file);
            event.target.value = "";
        });

    if (!loadDraft()) {
        form.elements.requestId.value = newRequestId();
        updateConditional();
        updateMeta("New draft");
        saveDraft(false);
    } else updateConditional();
    goToStep(1);
})();