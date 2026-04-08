import { Server } from "@modelcontextprotocol/sdk/server/index.js";
import { z } from "zod";
import {
  CallToolRequestSchema,
  ListToolsRequestSchema,
} from "@modelcontextprotocol/sdk/types.js";
import { readSelectedElements } from "./tools/readSelectedElements.js";
import { getElementParameters } from "./tools/getElementParameters.js";
import { searchElements } from "./tools/searchElements.js";
import { listModelViews } from "./tools/listModelViews.js";
import { fitOrIsolate } from "./tools/fitOrIsolate.js";
import { proposeParameterWrite } from "./tools/proposeParameterWrite.js";
import { aecQuery } from "./tools/aecQuery.js";
import {
  analyzeProductsAndMark,
  assignControlMarks,
  getProductSamenessReport,
} from "./tools/precastMarkTools.js";

const ToolArgsSchema = z.record(z.string(), z.unknown());

function textResult(payload: unknown) {
  return {
    content: [{ type: "text", text: JSON.stringify(payload) }],
  };
}

export function buildServer() {
  const server = new Server(
    { name: "aps-ai-web-mcp-server", version: "0.1.0" },
    { capabilities: { tools: {} } },
  );

  server.setRequestHandler(ListToolsRequestSchema, async () => ({
    tools: [
      {
        name: "get_selected_elements",
        description:
          "Read selected elements and optionally filter returned properties.",
        inputSchema: {
          type: "object",
          properties: {
            selectedElements: { type: "array" },
            includeAll: { type: "boolean" },
            requestedFields: { type: "array", items: { type: "string" } },
          },
          required: ["selectedElements"],
        },
      },
      {
        name: "get_element_parameters",
        description:
          "Return parameter values for selected elements using query aliases.",
        inputSchema: {
          type: "object",
          properties: {
            selectedElements: { type: "array" },
            dbIds: { type: "array", items: { type: "number" } },
            parameterQueries: { type: "array", items: { type: "string" } },
          },
          required: ["selectedElements"],
        },
      },
      {
        name: "search_elements",
        description:
          "Search selected elements by name/property text and return matching dbIds.",
        inputSchema: {
          type: "object",
          properties: {
            selectedElements: { type: "array" },
            query: { type: "string" },
          },
          required: ["query"],
        },
      },
      {
        name: "list_model_views",
        description: "List already-available model views from caller context.",
        inputSchema: {
          type: "object",
          properties: {
            views: { type: "array" },
          },
          required: [],
        },
      },
      {
        name: "viewer_action",
        description: "Return fit/isolate/clear actions for caller to execute.",
        inputSchema: {
          type: "object",
          properties: {
            action: { type: "string" },
            dbIds: { type: "array", items: { type: "number" } },
          },
          required: ["action"],
        },
      },
      {
        name: "aec_query",
        description:
          "Query AEC Data Model API and return flattened rows for table display.",
        inputSchema: {
          type: "object",
          properties: {
            accessToken: { type: "string" },
            hubId: { type: "string" },
            projectId: { type: "string" },
            itemId: { type: "string" },
          },
          required: ["accessToken", "hubId", "projectId"],
        },
      },
      {
        name: "set_element_parameters_guarded",
        description:
          "Two-step guarded write for parameter edits (preview then explicit confirmation token).",
        inputSchema: {
          type: "object",
          properties: {
            apply: { type: "boolean" },
            confirmationToken: { type: "string" },
            changes: {
              type: "array",
              items: {
                type: "object",
                properties: {
                  dbId: { type: "number" },
                  parameter: { type: "string" },
                  value: { type: "string" },
                },
                required: ["dbId", "parameter", "value"],
              },
            },
          },
          required: ["changes"],
        },
      },
      {
        name: "analyze_products_and_mark",
        description:
          "Analyzes selected Structural Framing elements (or all WPA/WPB/COLUMN), runs mark verification using geometric bounds + intersecting elements, groups identical pieces, and assigns CONTROL_MARK starting at 100.",
        inputSchema: {
          type: "object",
          properties: {
            product_prefix: {
              type: "string",
              enum: ["WPA", "WPB", "CLA", "ALL"],
            },
            dry_run: { type: "boolean", default: true },
          },
          required: ["product_prefix"],
        },
      },
      {
        name: "get_product_sameness_report",
        description:
          "Returns sameness analysis for given elements using tolerances and intersecting geometry (mirrors MCPToolHelper logic).",
        inputSchema: {
          type: "object",
          properties: {
            element_ids: { type: "array", items: { type: "string" } },
          },
          required: ["element_ids"],
        },
      },
      {
        name: "assign_control_marks",
        description:
          "Applies CONTROL_MARK to verified identical pieces (guarded write).",
        inputSchema: {
          type: "object",
          properties: {
            mark_groups: { type: "array", items: { type: "object" } },
            start_number: { type: "integer", default: 100 },
          },
          required: ["mark_groups"],
        },
      },
    ],
  }));

  server.setRequestHandler(CallToolRequestSchema, async (request) => {
    const name = request.params.name;
    const args = ToolArgsSchema.parse(request.params.arguments ?? {});

    if (name === "get_selected_elements") {
      return textResult(readSelectedElements(args));
    }
    if (name === "get_element_parameters") {
      return textResult(getElementParameters(args));
    }
    if (name === "search_elements") {
      return textResult(searchElements(args));
    }
    if (name === "list_model_views") {
      return textResult(listModelViews(args));
    }
    if (name === "viewer_action") {
      return textResult(fitOrIsolate(args));
    }
    if (name === "aec_query") {
      return textResult(await aecQuery(args));
    }
    if (name === "set_element_parameters_guarded") {
      return textResult(proposeParameterWrite(args));
    }
    if (name === "analyze_products_and_mark") {
      return textResult(analyzeProductsAndMark(args));
    }
    if (name === "get_product_sameness_report") {
      return textResult(getProductSamenessReport(args));
    }
    if (name === "assign_control_marks") {
      return textResult(assignControlMarks(args));
    }

    throw new Error(`Unknown tool: ${name}`);
  });

  return server;
}
