# Revit vs ACC/APS Benchmark Plan

## Question

Is a given automation task faster and more reliable when executed:

- in local Revit (MCP path), or
- in ACC/APS Design Automation (cloud path)?

## Benchmark Scope

Use the same intent payload for both backends and compare:

1. Query tasks (read-only)
2. Placement tasks (create)
3. Edit tasks (modify parameters/geometry)
4. Cleanup tasks (delete by tag)

## Metrics

- End-to-end latency (request to final result)
- Execution time (backend-only)
- Success rate
- Retry count
- Determinism (same payload -> same count/shape)
- Output parity (element count and key params)
- Human time-to-approve (for interactive workflows)

## Test Matrix

- Model sizes: small / medium / large
- Task sizes: single element / tens / hundreds
- Concurrency: 1 / 3 / 5 simultaneous jobs
- Network conditions: normal and constrained

## Logging Schema

```json
{
  "runId": "uuid",
  "backend": "mcp-local | aps-da",
  "intent": "string",
  "modelId": "string",
  "startedAt": "iso",
  "endedAt": "iso",
  "durationMs": 0,
  "success": true,
  "createdCount": 0,
  "updatedCount": 0,
  "deletedCount": 0,
  "warnings": [],
  "error": null
}
```

## Decision Rules

- Prefer **MCP local** when:
  - low-latency interactive iteration matters
  - visual confirmation loops are frequent
- Prefer **APS DA** when:
  - repeatable unattended batches are needed
  - job volume and scalability dominate
- Hybrid route:
  - design/iterate in local path
  - finalize/scale in cloud path

## First 2-Week Execution Plan

1. Implement normalized intent contract and logger.
2. Run 20 paired jobs (same payload on each backend).
3. Publish comparison dashboard (latency + parity + failures).
4. Set routing thresholds and lock v1 policy.
