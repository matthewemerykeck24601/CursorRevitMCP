import { Server } from "@modelcontextprotocol/sdk/server/index.js";
import { z } from "zod";
import {
  CallToolRequestSchema,
  ListToolsRequestSchema,
} from "@modelcontextprotocol/sdk/types.js";
import {
  bindSharedParameters,
  createPointBasedElement,
  ensureSharedParameters,
  gatewayHealth,
  getAvailableFamilyTypes,
  getCurrentViewElements,
  loadFamilyFromLibrary,
  openFamilyFromLibrary,
  openFamilyEditorByElementId,
  openSelectedFamilyEditor,
  searchFamilyLibrary,
  upgradeFamilyLibraryVersion,
} from "./bridgeClient.js";

const ArgsSchema = z.record(z.string(), z.unknown());

function result(payload: unknown) {
  return {
    content: [{ type: "text", text: JSON.stringify(payload) }],
  };
}

export function buildServer() {
  const server = new Server(
    { name: "revit-2027-community-mcp", version: "0.1.0" },
    { capabilities: { tools: {} } },
  );

  server.setRequestHandler(ListToolsRequestSchema, async () => ({
    tools: [
      {
        name: "say_hello",
        description: "Validate the Revit 2027 local gateway connection.",
        inputSchema: {
          type: "object",
          properties: {},
          additionalProperties: false,
        },
      },
      {
        name: "get_current_view_elements",
        description:
          "Get elements from active Revit view; supports model category filters like OST_Walls.",
        inputSchema: {
          type: "object",
          properties: {
            modelCategoryList: { type: "array", items: { type: "string" } },
            includeHidden: { type: "boolean" },
            limit: { type: "number" },
          },
          additionalProperties: false,
        },
      },
      {
        name: "get_available_family_types",
        description:
          "List available family types with optional category/family-name filtering.",
        inputSchema: {
          type: "object",
          properties: {
            categoryList: { type: "array", items: { type: "string" } },
            familyNameFilter: { type: "string" },
            limit: { type: "number" },
          },
          additionalProperties: false,
        },
      },
      {
        name: "create_point_based_element",
        description:
          "Create one or more point-based family instances in Revit 2027.",
        inputSchema: {
          type: "object",
          properties: {
            data: {
              type: "array",
              items: {
                type: "object",
                properties: {
                  name: { type: "string" },
                  typeId: { type: "number" },
                  locationPoint: {
                    type: "object",
                    properties: {
                      x: { type: "number" },
                      y: { type: "number" },
                      z: { type: "number" },
                    },
                    required: ["x", "y", "z"],
                    additionalProperties: false,
                  },
                  width: { type: "number" },
                  depth: { type: "number" },
                  height: { type: "number" },
                  baseLevel: { type: "number" },
                  baseOffset: { type: "number" },
                  rotation: { type: "number" },
                  hostWallId: { type: "number" },
                  facingFlipped: { type: "boolean" },
                },
                required: [
                  "name",
                  "typeId",
                  "locationPoint",
                  "width",
                  "height",
                  "baseLevel",
                  "baseOffset",
                ],
                additionalProperties: false,
              },
            },
          },
          required: ["data"],
          additionalProperties: false,
        },
      },
      {
        name: "open_selected_family_editor",
        description:
          "Open the selected family instance in Revit Family Editor (active selection required).",
        inputSchema: {
          type: "object",
          properties: {},
          additionalProperties: false,
        },
      },
      {
        name: "open_family_editor_by_element_id",
        description:
          "Open a family editor document by source element id in the active Revit model.",
        inputSchema: {
          type: "object",
          properties: {
            elementId: { type: "number" },
          },
          required: ["elementId"],
          additionalProperties: false,
        },
      },
      {
        name: "ensure_shared_parameters",
        description:
          "Create or verify shared parameter definitions in a shared parameter file.",
        inputSchema: {
          type: "object",
          properties: {
            sharedParameterFilePath: { type: "string" },
            groupName: { type: "string" },
            parameters: {
              type: "array",
              items: {
                type: "object",
                properties: {
                  name: { type: "string" },
                  dataType: {
                    type: "string",
                    enum: ["text", "integer", "number", "length", "area", "volume", "yesno"],
                  },
                  description: { type: "string" },
                  visible: { type: "boolean" },
                  userModifiable: { type: "boolean" },
                },
                required: ["name"],
                additionalProperties: false,
              },
            },
          },
          required: ["parameters"],
          additionalProperties: false,
        },
      },
      {
        name: "bind_shared_parameters",
        description:
          "Bind existing shared parameters to project categories as instance or type parameters.",
        inputSchema: {
          type: "object",
          properties: {
            sharedParameterFilePath: { type: "string" },
            groupName: { type: "string" },
            parameterNames: { type: "array", items: { type: "string" } },
            categoryList: { type: "array", items: { type: "string" } },
            bindingType: { type: "string", enum: ["instance", "type"] },
            parameterGroup: { type: "string" },
          },
          required: ["parameterNames", "categoryList"],
          additionalProperties: false,
        },
      },
      {
        name: "search_family_library",
        description:
          "Search one or more Revit family library folders for .rfa files matching a query.",
        inputSchema: {
          type: "object",
          properties: {
            libraryRoots: { type: "array", items: { type: "string" } },
            query: { type: "string" },
            maxResults: { type: "number" },
          },
          required: ["query"],
          additionalProperties: false,
        },
      },
      {
        name: "open_family_from_library",
        description:
          "Open a family document from library path or by query search in configured roots.",
        inputSchema: {
          type: "object",
          properties: {
            familyPath: { type: "string" },
            libraryRoots: { type: "array", items: { type: "string" } },
            query: { type: "string" },
          },
          additionalProperties: false,
        },
      },
      {
        name: "load_family_from_library",
        description:
          "Load a family into the active Revit project from explicit path or library query.",
        inputSchema: {
          type: "object",
          properties: {
            familyPath: { type: "string" },
            libraryRoots: { type: "array", items: { type: "string" } },
            query: { type: "string" },
            overwriteParameterValues: { type: "boolean" },
          },
          additionalProperties: false,
        },
      },
      {
        name: "upgrade_family_library_version",
        description:
          "Upgrade source family files to the running Revit year and save to a target folder.",
        inputSchema: {
          type: "object",
          properties: {
            sourceRoot: { type: "string" },
            targetRoot: { type: "string" },
            query: { type: "string" },
            sourceYear: { type: "number" },
            targetYear: { type: "number" },
            maxFiles: { type: "number" },
            overwriteExisting: { type: "boolean" },
          },
          additionalProperties: false,
        },
      },
    ],
  }));

  server.setRequestHandler(CallToolRequestSchema, async (request) => {
    const name = request.params.name;
    const args = ArgsSchema.parse(request.params.arguments ?? {});

    if (name === "say_hello") {
      const health = await gatewayHealth();
      return result({
        ok: true,
        server: "revit-2027-community-mcp",
        gateway: health,
      });
    }

    if (name === "get_current_view_elements") {
      const rows = await getCurrentViewElements(args);
      return result({
        outcome: `Retrieved ${rows.length} element(s) from active view.`,
        elements: rows,
      });
    }

    if (name === "get_available_family_types") {
      const rows = await getAvailableFamilyTypes(args);
      return result({
        outcome: `Retrieved ${rows.length} family type(s).`,
        familyTypes: rows,
      });
    }

    if (name === "create_point_based_element") {
      const created = await createPointBasedElement(args);
      return result({
        outcome: `Created ${created.length} element(s).`,
        created,
      });
    }

    if (name === "open_selected_family_editor") {
      const opened = await openSelectedFamilyEditor(args);
      return result({
        outcome: "Opened selected family in Family Editor.",
        opened,
      });
    }

    if (name === "open_family_editor_by_element_id") {
      const elementId = Number(args.elementId);
      if (!Number.isFinite(elementId) || elementId <= 0) {
        throw new Error("elementId must be a positive number.");
      }
      const opened = await openFamilyEditorByElementId({ elementId });
      return result({
        outcome: `Opened family editor for element ${elementId}.`,
        opened,
      });
    }

    if (name === "ensure_shared_parameters") {
      const created = await ensureSharedParameters(args);
      return result({
        outcome: "Shared parameter definitions ensured.",
        ...created,
      });
    }

    if (name === "bind_shared_parameters") {
      const bound = await bindSharedParameters(args);
      return result({
        outcome: "Shared parameters bound to categories.",
        ...bound,
      });
    }

    if (name === "search_family_library") {
      const search = await searchFamilyLibrary(args);
      return result({
        outcome: "Family library search complete.",
        ...search,
      });
    }

    if (name === "open_family_from_library") {
      const opened = await openFamilyFromLibrary(args);
      return result({
        outcome: "Opened family document from library.",
        ...opened,
      });
    }

    if (name === "load_family_from_library") {
      const loaded = await loadFamilyFromLibrary(args);
      return result({
        outcome: "Loaded family into active project.",
        ...loaded,
      });
    }

    if (name === "upgrade_family_library_version") {
      const upgraded = await upgradeFamilyLibraryVersion(args);
      return result({
        outcome: "Family library upgrade completed.",
        ...upgraded,
      });
    }

    throw new Error(`Unknown tool: ${name}`);
  });

  return server;
}
