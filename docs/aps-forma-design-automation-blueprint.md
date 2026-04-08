# APS + Forma + Revit Automation Blueprint

## Goal

Build a webapp-backed automation stack that can create, edit, and query Revit models using Autodesk APIs, while preserving lessons learned from direct Revit MCP operation.

## Target Architecture

- **Webapp UI**
  - Job submission, run history, model selector, and results viewer.
- **Orchestration API**
  - Owns auth, job lifecycle, queueing, retries, and telemetry.
- **Adapter Layer**
  - Translates user intent into one of two execution paths:
    - Local Revit path (MCP-driven)
    - Cloud path (APS + Design Automation)
- **Execution Backends**
  - `Backend A`: Revit desktop + MCP for interactive/fast iteration.
  - `Backend B`: APS Design Automation for scalable cloud execution.
- **Data Plane**
  - ACC/Docs for source RVT and outputs.
  - Structured run logs for traceability and performance comparison.

## Intent Translation Contract

Define one canonical payload used by both backends:

```json
{
  "intent": "place_family_samples",
  "category": "OST_StructuralFraming",
  "filters": {
    "excludeFamilyNameContains": ["DO NOT USE"]
  },
  "placement": {
    "strategy": "within_grid_bounds_non_touching",
    "tag": "RUN_YYYYMMDD_HHMM"
  },
  "modelContext": {
    "targetModelId": "acc-item-id",
    "viewOrWorkset": "optional"
  }
}
```

## Capability Mapping (Revit vs ACC/APS)

- **Interactive geometry iteration**
  - Best first in local Revit MCP.
- **Large repeatable batch jobs**
  - Better fit for APS Design Automation.
- **Model query/report generation**
  - Viable in both; APS is better for unattended runs.
- **Human-in-the-loop edits**
  - Revit MCP is faster for immediate visual feedback.

## Progressive Delivery Plan

1. **Phase 0: Contract Stabilization**
   - Freeze intent schema + result schema.
2. **Phase 1: Local-First API**
   - Webapp calls orchestration API, orchestration calls MCP backend.
3. **Phase 2: Cloud Mirror**
   - Implement APS Design Automation backend with same schema.
4. **Phase 3: Smart Router**
   - Route jobs automatically based on size, latency target, and required interactivity.
5. **Phase 4: Forma Integration**
   - Push/pull scenario metadata and geometry deltas for early-stage design workflows.

## Operational Requirements

- Idempotent job keys to prevent duplicate model writes.
- Strong run tagging in created/edited elements.
- Result provenance:
  - backend used
  - model revision in
  - model revision out
  - element IDs touched
- Failure policy:
  - partial-failure report with replay-ready payload

## Security + Auth

- Use server-side token handling only.
- Avoid exposing APS tokens in frontend.
- Log minimal PII.
- Keep model operation logs immutable.

## Definition of Done (MVP)

- Submit one intent from webapp.
- Execute via MCP backend and APS backend.
- Receive normalized result payload from either path.
- Compare latency and output parity in one report view.
