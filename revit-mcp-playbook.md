# Revit MCP Playbook (Project-Persistent)

## Purpose

Persistent runbook for Revit 2026 MCP operations so new chats can execute quickly and consistently.

## Default Server

- Primary: `user-revit-2026-community-mcp`
- Preferred tool for edits: `send_code_to_revit`

## Session Bootstrap Checklist

1. Confirm active document (`project` vs `family`).
2. Confirm required grids/families exist before edits.
3. For bulk operations, use batched passes to avoid timeout.
4. Tag created elements in `Comments` with a run label.
5. Verify result counts and sample readback after every edit pass.

## Family Handling Rules

- Structural Framing intent aliases: `pieces`, `products`, `panel`, family-name shorthand.
- Ignore standalone placement for family names containing `DO NOT USE`.
- Nested `DO NOT USE` content created through host families is acceptable.
- When possible, prefer non-`DO NOT USE` type names.

## Proven WALL_PANEL Behavior

- Placement type: `WorkPlaneBased`
- Primary drivers: `WIDTH`, `LENGTH`, `VERT_PANEL`, `Slope_Angle`
- Common toggles: `Miter_Edge1/2`, `Chamfer_*`, `Flip_Miter`
- Width clamp: `PANEL_WIDTH` is capped by `MAX_WIDTH`

## Standard Output Pattern

- What changed (families/types/ids/counts)
- Validation (bbox/no-overlap/bounds checks)
- Any skips with reason
- Recommended next step
