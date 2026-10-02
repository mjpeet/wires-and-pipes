"use strict";
async function renderLatestReading() {
    const output = document.getElementById("latest-reading");
    if (!output) {
        return;
    }
    try {
        const response = await fetch("/api/interconnectors/INTFR/latest");
        if (!response.ok) {
            output.textContent = "No reading available yet.";
            return;
        }
        const reading = await response.json();
        output.textContent =
            `${reading.interconnectorCode}: ${reading.generationMw} MW ` +
                `(settlement period ${reading.settlementPeriod} on ${reading.settlementDate})`;
    }
    catch {
        output.textContent = "Failed to load the latest reading.";
    }
}
void renderLatestReading();
