import { Server } from "@modelcontextprotocol/sdk/server/index.js";
import { z } from "zod";
import {
  CallToolRequestSchema,
  ListToolsRequestSchema,
} from "@modelcontextprotocol/sdk/types.js";
import {
  gatewayHealth,
  getDocumentMetadata,
  getActiveDocumentContext,
  getElementById,
  getElementMetadata,
  getElementTypes,
  getLevels,
  getGrids,
  queryElements,
  getCurrentViewElements,
  getSchedules,
  getScheduleData,
  getWarnings,
  getMissingLinks,
  getJournalTail,
  getCorruptElements,
} from "./bridgeClient.js";

const ArgsSchema = z.record(z.string(), z.unknown());

function result(payload: unknown) {
  return {
    content: [{ type: "text", text: JSON.stringify(payload) }],
  };
}

export function buildServer() {
  const server = new Server(
    { name: "revit-2024-diag-mcp", version: "0.1.0" },
    { capabilities: { tools: {} } },
  );

  server.setRequestHandler(ListToolsRequestSchema, async () => ({
    tools: [
      // ─── Core read tools ───
      {
        name: "say_hello",
        description: "Liveness check: validate the Revit 2024 diagnostic gateway connection.",
        inputSchema: { type: "object", properties: {}, additionalProperties: false },
      },
      {
        name: "get_document_metadata",
        description:
          "Active document provenance: model name/path, Revit version, worksharing status, central path, link count, project information.",
        inputSchema: { type: "object", properties: {}, additionalProperties: false },
      },
      {
        name: "get_active_document_context",
        description:
          "Report whether the active document is a 'project' or 'family', with title, isFamilyDocument, and (for families) the family category.",
        inputSchema: { type: "object", properties: {}, additionalProperties: false },
      },
      {
        name: "get_element_by_id",
        description:
          "Fetch a single element by integer id: category, familyName, typeName, location (mm), and (optionally) all readable parameters.",
        inputSchema: {
          type: "object",
          properties: {
            elementId: { type: "number" },
            includeParameters: { type: "boolean" },
          },
          required: ["elementId"],
          additionalProperties: false,
        },
      },
      {
        name: "get_element_metadata",
        description:
          "Fetch one or more elements by id list or active selection: category, familyName, typeName, location (mm), and (optionally) all readable parameters.",
        inputSchema: {
          type: "object",
          properties: {
            elementIds: { type: "array", items: { type: "number" } },
            useSelection: { type: "boolean" },
            includeParameters: { type: "boolean" },
          },
          additionalProperties: false,
        },
      },
      {
        name: "get_element_types",
        description:
          "Enumerate loaded family types (FamilySymbols), optionally filtered by an OST category token (e.g. OST_Walls). Returns id, name, familyName, category, isActive, plus totalCount.",
        inputSchema: {
          type: "object",
          properties: {
            category: { type: "string" },
            limit: { type: "number" },
          },
          additionalProperties: false,
        },
      },
      {
        name: "get_levels",
        description:
          "List all levels with id, name, elevation (mm), and isGroundFloor (level closest to elevation 0). Requires an active project document.",
        inputSchema: { type: "object", properties: {}, additionalProperties: false },
      },
      {
        name: "get_grids",
        description:
          "List all grids with id, name, and the two endpoints (mm) of the grid line. Requires an active project document.",
        inputSchema: { type: "object", properties: {}, additionalProperties: false },
      },
      {
        name: "query_elements",
        description:
          "Query model elements by optional filters (all AND'd): category (OST token), familyName (exact match), and parameterFilters (each { name, value, operator } where operator is equals|contains|startsWith|notEquals, matched case-insensitively against any readable instance OR type parameter). Returns for each element: elementId, category, familyName, typeName, level, and a parameters object of every readable instance parameter (string-coerced), plus totalCount (elements matching all filters). limit defaults to 100 when unspecified and has no maximum (pass a large value or totalCount to retrieve all matches). Primary tool for the piece-mark split workflow — filter on CONTROL_MARK, MANUFACTURE_COMPONENT, Assembly Name, etc.",
        inputSchema: {
          type: "object",
          properties: {
            category: { type: "string" },
            familyName: { type: "string" },
            levelId: { type: "number" },
            parameterFilters: {
              type: "array",
              items: {
                type: "object",
                properties: {
                  name: { type: "string" },
                  value: { type: "string" },
                  operator: {
                    type: "string",
                    enum: ["equals", "contains", "startsWith", "notEquals"],
                  },
                },
                required: ["name", "value"],
                additionalProperties: false,
              },
            },
            limit: { type: "number" },
          },
          additionalProperties: false,
        },
      },
      {
        name: "get_current_view_elements",
        description:
          "Get elements visible in the active Revit view; supports OST model category filters (e.g. OST_Walls).",
        inputSchema: {
          type: "object",
          properties: {
            modelCategoryList: { type: "array", items: { type: "string" } },
            limit: { type: "number" },
          },
          additionalProperties: false,
        },
      },
      {
        name: "get_schedules",
        description:
          "Index of all non-template schedules (ViewSchedule). For each: scheduleId, name, categoryName, rowCount (data rows only, no header), plus totalCount. No row data — use get_schedule_data to read a specific schedule.",
        inputSchema: { type: "object", properties: {}, additionalProperties: false },
      },
      {
        name: "get_schedule_data",
        description:
          "Read a schedule's table, identified by either scheduleName (first non-template schedule whose name matches, case-insensitive — no get_schedules call needed) or scheduleId. Returns headers (column header strings), rows (each an ordered header->cell-value map, string-coerced), rowCount, columnCount, and truncated/totalRows. Caps at 1000 data rows by default; pass maxRows (up to 5000) to read more. Useful for validating a piece-mark split against an existing schedule.",
        inputSchema: {
          type: "object",
          properties: {
            scheduleId: { type: "number" },
            scheduleName: { type: "string" },
            maxRows: { type: "number" },
          },
          additionalProperties: false,
        },
      },
      // ─── Diagnostic tools ───
      {
        name: "get_warnings",
        description:
          "Return Revit's native model warnings (doc.GetWarnings()). For each: warningText, severity, failingElementIds, resolutionDescription. Capped at 200 with truncated/totalCount flags.",
        inputSchema: { type: "object", properties: {}, additionalProperties: false },
      },
      {
        name: "get_missing_links",
        description:
          "Enumerate all RevitLinkInstance elements. For each: linkName, linkPath, isLoaded, linkedFileStatus, elementId. Use to find broken/unloaded links.",
        inputSchema: { type: "object", properties: {}, additionalProperties: false },
      },
      {
        name: "get_journal_tail",
        description:
          "Read the tail of the most recently modified Revit 2024 journal file. Returns the last N lines (default 50, max 500), the journal path, and its last-write timestamp.",
        inputSchema: {
          type: "object",
          properties: {
            lines: { type: "number" },
          },
          additionalProperties: false,
        },
      },
      {
        name: "get_corrupt_elements",
        description:
          "Best-effort integrity scan: iterate up to 1000 model elements and record any that throw on basic access (Name/Category). Returns accessibleCount and a failed list (id + error).",
        inputSchema: { type: "object", properties: {}, additionalProperties: false },
      },
    ],
  }));

  server.setRequestHandler(CallToolRequestSchema, async (request) => {
    const name = request.params.name;
    try {
      const args = ArgsSchema.parse(request.params.arguments ?? {});

      if (name === "say_hello") {
        const health = await gatewayHealth();
        return result({ ok: true, server: "revit-2024-diag-mcp", gateway: health });
      }

      if (name === "get_document_metadata") {
        const document = await getDocumentMetadata(args);
        return result({ outcome: "Retrieved active document metadata.", document });
      }

      if (name === "get_active_document_context") {
        const context = await getActiveDocumentContext(args);
        return result({ outcome: "Retrieved active document context.", ...context });
      }

      if (name === "get_element_by_id") {
        const elementId = Number(args.elementId);
        if (!Number.isFinite(elementId) || elementId <= 0) {
          throw new Error("elementId must be a positive number.");
        }
        const data = await getElementById(args);
        return result({ outcome: `Retrieved element ${elementId}.`, ...data });
      }

      if (name === "get_element_metadata") {
        const useSelection = Boolean(args.useSelection);
        const elementIds = Array.isArray(args.elementIds)
          ? args.elementIds.map((v) => Number(v)).filter((v) => Number.isFinite(v) && v > 0)
          : [];
        if (!useSelection && elementIds.length === 0) {
          throw new Error("Set useSelection=true or provide elementIds.");
        }
        const data = await getElementMetadata({
          ...args,
          useSelection,
          elementIds: elementIds.length > 0 ? elementIds : undefined,
        });
        const elements = Array.isArray((data as { elements?: unknown }).elements)
          ? (data as { elements: unknown[] }).elements
          : [];
        return result({ outcome: `Retrieved metadata for ${elements.length} element(s).`, ...data });
      }

      if (name === "get_element_types") {
        const data = await getElementTypes(args);
        const total = Number((data as { totalCount?: unknown }).totalCount ?? 0);
        return result({ outcome: `Retrieved ${total} element type(s).`, ...data });
      }

      if (name === "get_levels") {
        const data = await getLevels(args);
        const count = Array.isArray((data as { levels?: unknown }).levels)
          ? (data as { levels: unknown[] }).levels.length
          : 0;
        return result({ outcome: `Retrieved ${count} level(s).`, ...data });
      }

      if (name === "get_grids") {
        const data = await getGrids(args);
        const count = Array.isArray((data as { grids?: unknown }).grids)
          ? (data as { grids: unknown[] }).grids.length
          : 0;
        return result({ outcome: `Retrieved ${count} grid(s).`, ...data });
      }

      if (name === "query_elements") {
        const data = await queryElements(args);
        const total = Number((data as { totalCount?: unknown }).totalCount ?? 0);
        return result({ outcome: `Query matched ${total} element(s).`, ...data });
      }

      if (name === "get_current_view_elements") {
        const rows = await getCurrentViewElements(args);
        return result({ outcome: `Retrieved ${rows.length} element(s) from active view.`, elements: rows });
      }

      if (name === "get_schedules") {
        const data = await getSchedules(args);
        const total = Number((data as { totalCount?: unknown }).totalCount ?? 0);
        return result({ outcome: `Found ${total} schedule(s).`, ...data });
      }

      if (name === "get_schedule_data") {
        const scheduleName =
          typeof args.scheduleName === "string" ? args.scheduleName.trim() : "";
        const scheduleId = Number(args.scheduleId);
        const hasId = Number.isFinite(scheduleId) && scheduleId > 0;
        if (!scheduleName && !hasId) {
          throw new Error("Provide a positive scheduleId or a scheduleName.");
        }
        const data = await getScheduleData(args);
        const rowCount = Number((data as { rowCount?: unknown }).rowCount ?? 0);
        const ref = scheduleName ? `"${scheduleName}"` : `${scheduleId}`;
        return result({ outcome: `Read ${rowCount} row(s) from schedule ${ref}.`, ...data });
      }

      if (name === "get_warnings") {
        const data = await getWarnings(args);
        const total = Number((data as { totalCount?: unknown }).totalCount ?? 0);
        return result({ outcome: `Model has ${total} warning(s).`, ...data });
      }

      if (name === "get_missing_links") {
        const data = await getMissingLinks(args);
        const total = Number((data as { totalCount?: unknown }).totalCount ?? 0);
        return result({ outcome: `Found ${total} Revit link instance(s).`, ...data });
      }

      if (name === "get_journal_tail") {
        const data = await getJournalTail(args);
        const count = Number((data as { lineCount?: unknown }).lineCount ?? 0);
        return result({ outcome: `Read ${count} journal line(s).`, ...data });
      }

      if (name === "get_corrupt_elements") {
        const data = await getCorruptElements(args);
        const failed = Array.isArray((data as { failed?: unknown }).failed)
          ? (data as { failed: unknown[] }).failed.length
          : 0;
        return result({ outcome: `Scan complete: ${failed} element(s) failed basic access.`, ...data });
      }

      throw new Error(`Unknown tool: ${name}`);
    } catch (error) {
      return {
        content: [
          { type: "text", text: error instanceof Error ? error.message : String(error) },
        ],
        isError: true,
      };
    }
  });

  return server;
}
