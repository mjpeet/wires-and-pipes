---
status: accepted (empirical, not sourced from an official Elexon spec — revisit if that changes)
---

# Interconnector flow sign convention: negative generationMw = import into GB

Elexon's FUELHH dataset reports a `generation` value for each interconnector fuel type (e.g. `INTFR`, `INTNED`), but no Elexon documentation page we could find (the developer docs are JS-rendered and returned no usable text) states what a positive or negative value means directionally. During implementation of ticket #3 we determined empirically, by polling the live API and cross-checking the observed signs against the user's own independent data sampling, that **negative `generationMw` means GB is importing** (power flowing into GB) and **positive means GB is exporting**. The project's original handoff notes had assumed the opposite convention.

We adopted the negative-import/positive-export convention project-wide (consumer-side comments in `src/WiresAndPipes.Api/Elexon/InterconnectorCodes.cs`, and the map's arrow colour/direction logic in `frontend/src/app.ts`) rather than blocking the vertical slice on finding an authoritative citation. Getting this backwards would silently mislabel every import/export arrow on a project whose core value proposition is verifiable, correctly-sourced numbers — so if an authoritative Elexon source is ever found that contradicts this, every place that branches on the sign needs to be revisited together, not just the one that's "obviously" wrong.
