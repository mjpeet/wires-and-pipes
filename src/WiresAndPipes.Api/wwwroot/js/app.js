"use strict";
const MAP_WIDTH = 560;
const MAP_HEIGHT = 560;
const KNOWN_ANCHORS = {
    INTFR: { label: "France (IFA)", x: 400, y: 530, angleDegrees: 68 },
    INTIFA2: { label: "France (IFA2)", x: 330, y: 545, angleDegrees: 80 },
    INTNEM: { label: "Belgium (Nemo)", x: 460, y: 495, angleDegrees: 55 },
    INTELEC: { label: "Belgium (ElecLink)", x: 500, y: 450, angleDegrees: 42 },
    INTNED: { label: "Netherlands", x: 520, y: 390, angleDegrees: 25 },
    INTVKL: { label: "Denmark (Viking Link)", x: 520, y: 300, angleDegrees: 8 },
    INTNSL: { label: "Norway (North Sea Link)", x: 480, y: 200, angleDegrees: -20 },
    INTEW: { label: "Ireland (East-West)", x: 60, y: 420, angleDegrees: 200 },
    INTGRNL: { label: "Ireland (Greenlink)", x: 70, y: 470, angleDegrees: 215 },
    INTIRL: { label: "Northern Ireland (Moyle)", x: 50, y: 180, angleDegrees: 250 },
};
const FALLBACK_CENTER = { x: MAP_WIDTH / 2, y: MAP_HEIGHT / 2 };
const FALLBACK_RADIUS = 260;
/** Spreads any number of unmapped codes evenly around the map, so none can land on another. */
function fallbackAnchor(indexAmongUnmapped, totalUnmapped, code) {
    const angleDegrees = (360 / totalUnmapped) * indexAmongUnmapped - 90;
    const radians = (angleDegrees * Math.PI) / 180;
    return {
        label: code,
        x: FALLBACK_CENTER.x + FALLBACK_RADIUS * Math.cos(radians),
        y: FALLBACK_CENTER.y + FALLBACK_RADIUS * Math.sin(radians),
        angleDegrees,
    };
}
/**
 * A highly simplified GB silhouette: recognisable, not cartographically accurate. Precise
 * coastline data can replace this path later without touching the rendering logic below.
 */
const GB_OUTLINE_PATH = "M210,20 L240,70 L225,120 L245,160 L230,210 " + // east coast: Scotland down to NE England
    "L250,260 L235,310 L245,350 " + // east coast: Yorkshire to East Anglia
    "L220,400 L235,430 L210,460 " + // south-east England
    "L180,480 L190,440 L160,420 L140,450 L150,480 L130,500 " + // south-west England / Cornwall
    "L155,400 L140,370 L160,340 " + // Bristol Channel back up to Wales
    "L120,320 L135,290 L115,260 " + // Wales bulge (west coast)
    "L140,240 L160,210 L150,180 " + // north Wales into north-west England
    "L170,150 L155,110 L175,80 L165,45 Z"; // west coast of Scotland back to the start
const IMPORT_DIRECTION = { kind: "import", color: "#2b7a2b", markerId: "arrowhead-import" };
const EXPORT_DIRECTION = { kind: "export", color: "#a33a3a", markerId: "arrowhead-export" };
/**
 * Live-data observation (not an official Elexon spec citation): negative generation values for
 * an INT* fuel type mean GB is importing; positive means GB is exporting. Revisit if an
 * authoritative source says otherwise.
 */
function resolveFlowDirection(generationMw) {
    return generationMw < 0 ? IMPORT_DIRECTION : EXPORT_DIRECTION;
}
function buildArrowMarkup(anchor, code, reading) {
    const generationMw = reading?.generationMw ?? 0;
    const magnitude = Math.abs(generationMw);
    // Scale stroke width with magnitude; clamp so a huge or tiny flow stays legible.
    const strokeWidth = Math.min(16, Math.max(2, magnitude / 150));
    const length = 70;
    const flow = resolveFlowDirection(generationMw);
    // An export points outward along angleDegrees; an import reverses it so the arrowhead
    // always points in the direction the power is actually flowing.
    const travelAngle = flow.kind === "import" ? anchor.angleDegrees + 180 : anchor.angleDegrees;
    const radians = (travelAngle * Math.PI) / 180;
    const endX = anchor.x + length * Math.cos(radians);
    const endY = anchor.y + length * Math.sin(radians);
    const label = anchor.label || code;
    const valueText = reading ? `${generationMw} MW (${flow.kind})` : "no reading yet";
    return `
    <g class="interconnector" data-code="${code}">
      <line x1="${anchor.x}" y1="${anchor.y}" x2="${endX}" y2="${endY}"
            stroke="${flow.color}" stroke-width="${strokeWidth}"
            marker-end="url(#${flow.markerId})" />
      <circle cx="${anchor.x}" cy="${anchor.y}" r="4" fill="${flow.color}" />
      <text x="${anchor.x}" y="${anchor.y - 10}" font-size="11" text-anchor="middle">${label}</text>
      <title>${label}: ${valueText}</title>
    </g>
  `;
}
function renderMap(readings) {
    const byCode = new Map(readings.map((r) => [r.interconnectorCode, r]));
    const allCodes = [...new Set([...Object.keys(KNOWN_ANCHORS), ...byCode.keys()])];
    const unmappedCodes = allCodes.filter((code) => !KNOWN_ANCHORS[code]);
    const arrows = allCodes
        .map((code) => {
        const anchor = KNOWN_ANCHORS[code] ?? fallbackAnchor(unmappedCodes.indexOf(code), unmappedCodes.length, code);
        return buildArrowMarkup(anchor, code, byCode.get(code));
    })
        .join("\n");
    return `
    <svg viewBox="0 0 ${MAP_WIDTH} ${MAP_HEIGHT}" xmlns="http://www.w3.org/2000/svg"
         role="img" aria-label="Electricity flows into and out of Great Britain">
      <defs>
        <marker id="${IMPORT_DIRECTION.markerId}" markerWidth="8" markerHeight="8" refX="4" refY="4" orient="auto">
          <path d="M0,0 L8,4 L0,8 Z" fill="${IMPORT_DIRECTION.color}" />
        </marker>
        <marker id="${EXPORT_DIRECTION.markerId}" markerWidth="8" markerHeight="8" refX="4" refY="4" orient="auto">
          <path d="M0,0 L8,4 L0,8 Z" fill="${EXPORT_DIRECTION.color}" />
        </marker>
      </defs>
      <path d="${GB_OUTLINE_PATH}" fill="#d8d8d8" stroke="#888" stroke-width="2" />
      <text x="${MAP_WIDTH / 2}" y="${MAP_HEIGHT / 2}" font-size="14" text-anchor="middle" fill="#555">
        Great Britain
      </text>
      ${arrows}
    </svg>
  `;
}
async function renderElectricityMap() {
    const container = document.getElementById("map");
    if (!container) {
        return;
    }
    try {
        const response = await fetch("/api/interconnectors/latest");
        if (!response.ok) {
            container.textContent = "No readings available yet.";
            return;
        }
        const readings = await response.json();
        container.innerHTML = renderMap(readings);
    }
    catch {
        container.textContent = "Failed to load electricity flow data.";
    }
}
void renderElectricityMap();
