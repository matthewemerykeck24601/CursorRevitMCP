# MCP Implementation Checklist

## Status
- [x] Todo 1: Gather prerequisites and lock scope decisions
- [x] Todo 2: Scaffold MCP server package
- [x] Todo 3: Implement read tools with vocabulary mapping
- [x] Todo 4: Implement guarded write flow (two-step confirm)
- [x] Todo 5: Route chat through MCP tools in all modes
- [x] Todo 6: Produce runbook and JSON verification report

## Current Scope
- Runtime: Node.js + TypeScript
- Fixture: currently loaded model
- Writes in v1: parameter edits only, always confirmation-gated
- Output style: targeted-minimal responses by default

## Notes
- Guarded write is currently scaffolded in MCP tool `set_element_parameters_guarded` with preview + token confirmation.
- Actual Revit/APS-side write execution remains intentionally gated to explicit apply workflow integration.

