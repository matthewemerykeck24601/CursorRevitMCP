import { Server } from "@modelcontextprotocol/sdk/server/index.js";
import { z } from "zod";
import {
  CallToolRequestSchema,
  ListToolsRequestSchema,
} from "@modelcontextprotocol/sdk/types.js";
import {
  addFamilyParameter,
  addFamilyType,
  addSharedParamsToFamilyLibrary,
  bindSharedParameters,
  createBlend,
  createDimension,
  createExtrusion,
  createPointBasedElement,
  createReferencePlane,
  createRevolve,
  createSweep,
  createSweptBlend,
  createWall,
  createLevel,
  createGrid,
  createFloor,
  createRoom,
  createStructuralColumn,
  createBeam,
  placePointBasedElement,
  createOpeningByBoundary,
  moveElements,
  rotateElements,
  mirrorElements,
  copyElementsToLevel,
  setElementParameters,
  deleteElements,
  createFloorPlanView,
  createReflectedCeilingPlan,
  createSectionView,
  create3dView,
  createSheet,
  placeViewOnSheet,
  setViewCropRegion,
  getSheets,
  getSheetContents,
  openSheet,
  getViewContents,
  compareSheetToTemplate,
  deleteFamilyType,
  getFamilyDocumentInfo,
  loadFamilyIntoProject,
  lockConstraint,
  renameFamilyType,
  saveFamily,
  saveFamilyAs,
  setDimensionLabel,
  setFamilyParameterValue,
  setFormula,
  setGeometrySolidVoid,
  extractFamilyDescVariants,
  extractFamilyLibraryParameters,
  ensureSharedParameters,
  gatewayHealth,
  getActiveDocumentContext,
  getElementGeometry,
  getInplaceElements,
  reconstructProfileForExtrusion,
  getLevels,
  getGrids,
  getElementTypes,
  queryElements,
  getElementById,
  exportParameterBindings,
  importInstances,
  createDatums,
  getDocumentMetadata,
  getElementMetadata,
  getFamilyParameters,
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
    { name: "revit-2024-community-bridge-mcp", version: "0.1.0" },
    { capabilities: { tools: {} } },
  );

  server.setRequestHandler(ListToolsRequestSchema, async () => ({
    tools: [
      {
        name: "say_hello",
        description: "Validate the Revit 2024 local gateway connection.",
        inputSchema: {
          type: "object",
          properties: {},
          additionalProperties: false,
        },
      },
      {
        name: "get_document_metadata",
        description:
          "Project/document provenance: worksharing central path, file save user/time, project info, current Revit user.",
        inputSchema: {
          type: "object",
          properties: {},
          additionalProperties: false,
        },
      },
      {
        name: "get_element_metadata",
        description:
          "Element provenance for selection or explicit ids: worksharing owner/creator/last-changed-by, checkout status, central sync state, version guid, optional parameters.",
        inputSchema: {
          type: "object",
          properties: {
            elementIds: { type: "array", items: { type: "number" } },
            useSelection: { type: "boolean" },
            includeParameters: { type: "boolean" },
            parameterNames: { type: "array", items: { type: "string" } },
          },
          additionalProperties: false,
        },
      },
      {
        name: "get_family_parameters",
        description:
          "Read family-level type/instance parameters from the active Family Editor document (FamilyManager). Not for nested placed instances in a project. Optional parameterNames filter.",
        inputSchema: {
          type: "object",
          properties: {
            parameterNames: { type: "array", items: { type: "string" } },
          },
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
          "Create one or more point-based family instances in Revit 2024.",
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
        name: "add_shared_params_to_family_library",
        description:
          "Add shared parameters from a shared parameter file/group into family files under a library folder.",
        inputSchema: {
          type: "object",
          properties: {
            sourceRoot: { type: "string" },
            query: { type: "string" },
            maxFiles: { type: "number" },
            sharedParameterFilePath: { type: "string" },
            groupName: { type: "string" },
            parameterNames: { type: "array", items: { type: "string" } },
            parameterGroup: { type: "string" },
            isInstance: { type: "boolean" },
          },
          required: ["parameterNames"],
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
      {
        name: "extract_family_library_parameters",
        description:
          "Extract per-type parameter values from family files in a library folder.",
        inputSchema: {
          type: "object",
          properties: {
            sourceRoot: { type: "string" },
            query: { type: "string" },
            maxFiles: { type: "number" },
            parameterNames: { type: "array", items: { type: "string" } },
          },
          required: ["parameterNames"],
          additionalProperties: false,
        },
      },
      {
        name: "extract_family_desc_variants",
        description:
          "Expand possible IDENTITY_DESCRIPTION outcomes from boolean formulas and return matching description/component variants.",
        inputSchema: {
          type: "object",
          properties: {
            sourceRoot: { type: "string" },
            query: { type: "string" },
            maxFiles: { type: "number" },
            descriptionParameterName: { type: "string" },
            descriptionShortParameterName: { type: "string" },
            manufactureComponentParameterName: { type: "string" },
            maxBooleanDrivers: { type: "number" },
          },
          additionalProperties: false,
        },
      },
      // ─── Family Editor: Sketch & Solid Geometry ───────────────────────────────
      {
        name: "create_extrusion",
        description:
          "Family Editor: create an extruded solid or void from a closed 2D profile loop in the active family document. All coordinates and lengths are millimetres.",
        inputSchema: {
          type: "object",
          properties: {
            profile: {
              type: "array",
              items: {
                type: "object",
                properties: {
                  x: { type: "number" },
                  y: { type: "number" },
                },
                required: ["x", "y"],
                additionalProperties: false,
              },
            },
            depth: { type: "number" },
            isSolid: { type: "boolean" },
            workPlaneName: { type: "string" },
          },
          required: ["profile", "depth", "isSolid"],
          additionalProperties: false,
        },
      },
      {
        name: "create_blend",
        description:
          "Family Editor: create a solid or void blend between a bottom and a top 2D profile in the active family document. Lengths in mm.",
        inputSchema: {
          type: "object",
          properties: {
            bottomProfile: {
              type: "array",
              items: {
                type: "object",
                properties: { x: { type: "number" }, y: { type: "number" } },
                required: ["x", "y"],
                additionalProperties: false,
              },
            },
            topProfile: {
              type: "array",
              items: {
                type: "object",
                properties: { x: { type: "number" }, y: { type: "number" } },
                required: ["x", "y"],
                additionalProperties: false,
              },
            },
            depth: { type: "number" },
            isSolid: { type: "boolean" },
            workPlaneName: { type: "string" },
          },
          required: ["bottomProfile", "topProfile", "depth", "isSolid"],
          additionalProperties: false,
        },
      },
      {
        name: "create_revolve",
        description:
          "Family Editor: create a solid or void revolve of a 2D profile around a named axis reference plane. Angles in degrees.",
        inputSchema: {
          type: "object",
          properties: {
            profile: {
              type: "array",
              items: {
                type: "object",
                properties: { x: { type: "number" }, y: { type: "number" } },
                required: ["x", "y"],
                additionalProperties: false,
              },
            },
            axisReferenceName: { type: "string" },
            startAngleDeg: { type: "number" },
            endAngleDeg: { type: "number" },
            isSolid: { type: "boolean" },
            workPlaneName: { type: "string" },
          },
          required: ["profile", "axisReferenceName", "startAngleDeg", "endAngleDeg", "isSolid"],
          additionalProperties: false,
        },
      },
      {
        name: "create_sweep",
        description:
          "Family Editor: create a solid or void sweep of a 2D profile along a (planar) 3D path. Lengths in mm.",
        inputSchema: {
          type: "object",
          properties: {
            path: {
              type: "array",
              items: {
                type: "object",
                properties: {
                  x: { type: "number" },
                  y: { type: "number" },
                  z: { type: "number" },
                },
                required: ["x", "y", "z"],
                additionalProperties: false,
              },
            },
            profile: {
              type: "array",
              items: {
                type: "object",
                properties: { x: { type: "number" }, y: { type: "number" } },
                required: ["x", "y"],
                additionalProperties: false,
              },
            },
            isSolid: { type: "boolean" },
            workPlaneName: { type: "string" },
          },
          required: ["path", "profile", "isSolid"],
          additionalProperties: false,
        },
      },
      {
        name: "create_swept_blend",
        description:
          "Family Editor: create a swept blend between a start and an end 2D profile along a path. Note: uses the first path segment only (the Revit API takes a single-curve path). Lengths in mm.",
        inputSchema: {
          type: "object",
          properties: {
            path: {
              type: "array",
              items: {
                type: "object",
                properties: {
                  x: { type: "number" },
                  y: { type: "number" },
                  z: { type: "number" },
                },
                required: ["x", "y", "z"],
                additionalProperties: false,
              },
            },
            startProfile: {
              type: "array",
              items: {
                type: "object",
                properties: { x: { type: "number" }, y: { type: "number" } },
                required: ["x", "y"],
                additionalProperties: false,
              },
            },
            endProfile: {
              type: "array",
              items: {
                type: "object",
                properties: { x: { type: "number" }, y: { type: "number" } },
                required: ["x", "y"],
                additionalProperties: false,
              },
            },
            isSolid: { type: "boolean" },
            workPlaneName: { type: "string" },
          },
          required: ["path", "startProfile", "endProfile", "isSolid"],
          additionalProperties: false,
        },
      },
      {
        name: "set_geometry_solid_void",
        description:
          "Family Editor: toggle an existing form (extrusion/blend/revolve/sweep/swept blend) between solid and void. Note: the Revit API exposes IsSolid as read-only; this attempts the 'Solid/Void' parameter and errors clearly if the form cannot be toggled in place.",
        inputSchema: {
          type: "object",
          properties: {
            elementId: { type: "number" },
            isSolid: { type: "boolean" },
          },
          required: ["elementId", "isSolid"],
          additionalProperties: false,
        },
      },
      // ─── Family Editor: Reference Planes, Dimensions & Constraints ────────────
      {
        name: "create_reference_plane",
        description:
          "Family Editor: create a named reference plane in the active family document. Points are millimetres.",
        inputSchema: {
          type: "object",
          properties: {
            name: { type: "string" },
            bubbleEnd: {
              type: "object",
              properties: {
                x: { type: "number" },
                y: { type: "number" },
                z: { type: "number" },
              },
              required: ["x", "y", "z"],
              additionalProperties: false,
            },
            freeEnd: {
              type: "object",
              properties: {
                x: { type: "number" },
                y: { type: "number" },
                z: { type: "number" },
              },
              required: ["x", "y", "z"],
              additionalProperties: false,
            },
            cutVector: {
              type: "object",
              properties: {
                x: { type: "number" },
                y: { type: "number" },
                z: { type: "number" },
              },
              required: ["x", "y", "z"],
              additionalProperties: false,
            },
            isReference: {
              type: "string",
              enum: [
                "NotAReference",
                "Left",
                "Right",
                "Front",
                "Back",
                "Bottom",
                "Top",
                "Center",
                "WeakReference",
                "StrongReference",
              ],
            },
          },
          required: ["name", "bubbleEnd", "freeEnd"],
          additionalProperties: false,
        },
      },
      {
        name: "create_dimension",
        description:
          "Family Editor: create a linear dimension between exactly two references (currently reference planes) in the active family. Optionally label it with an existing family parameter. Points in mm.",
        inputSchema: {
          type: "object",
          properties: {
            referenceIds: {
              type: "array",
              items: { type: "number" },
              minItems: 2,
              maxItems: 2,
            },
            line: {
              type: "object",
              properties: {
                start: {
                  type: "object",
                  properties: {
                    x: { type: "number" },
                    y: { type: "number" },
                    z: { type: "number" },
                  },
                  required: ["x", "y", "z"],
                  additionalProperties: false,
                },
                end: {
                  type: "object",
                  properties: {
                    x: { type: "number" },
                    y: { type: "number" },
                    z: { type: "number" },
                  },
                  required: ["x", "y", "z"],
                  additionalProperties: false,
                },
              },
              required: ["start", "end"],
              additionalProperties: false,
            },
            labelParameterName: { type: "string" },
          },
          required: ["referenceIds", "line"],
          additionalProperties: false,
        },
      },
      {
        name: "set_dimension_label",
        description:
          "Family Editor: associate an existing dimension with a family parameter (label it).",
        inputSchema: {
          type: "object",
          properties: {
            dimensionId: { type: "number" },
            parameterName: { type: "string" },
          },
          required: ["dimensionId", "parameterName"],
          additionalProperties: false,
        },
      },
      {
        name: "lock_constraint",
        description:
          "Family Editor: lock or unlock a dimension/alignment constraint (sets Dimension.IsLocked).",
        inputSchema: {
          type: "object",
          properties: {
            constraintId: { type: "number" },
            locked: { type: "boolean" },
          },
          required: ["constraintId", "locked"],
          additionalProperties: false,
        },
      },
      // ─── Family Editor: Parameters & Types ────────────────────────────────────
      {
        name: "add_family_parameter",
        description:
          "Family Editor: add a new instance or type parameter to the active family. parameterType: Length|Angle|Number|Text|Boolean|Integer|Material|YesNo|MultilineText. parameterGroup: a BuiltInParameterGroup token (PG_GEOMETRY, PG_CONSTRAINTS, PG_IDENTITY_DATA, PG_OTHER, …).",
        inputSchema: {
          type: "object",
          properties: {
            name: { type: "string" },
            parameterType: {
              type: "string",
              enum: ["Length", "Angle", "Number", "Text", "Boolean", "Integer", "Material", "YesNo", "MultilineText"],
            },
            parameterGroup: { type: "string" },
            isInstance: { type: "boolean" },
          },
          required: ["name", "parameterType", "parameterGroup", "isInstance"],
          additionalProperties: false,
        },
      },
      {
        name: "set_family_parameter_value",
        description:
          "Family Editor: set the value of an existing family parameter on a type (defaults to the current type). Length values in mm, angles in degrees; string for Text, boolean for YesNo.",
        inputSchema: {
          type: "object",
          properties: {
            parameterName: { type: "string" },
            value: { type: ["string", "number", "boolean"] },
            typeName: { type: "string" },
          },
          required: ["parameterName", "value"],
          additionalProperties: false,
        },
      },
      {
        name: "add_family_type",
        description:
          "Family Editor: create a new family type, optionally cloning values from an existing type, then applying parameter overrides.",
        inputSchema: {
          type: "object",
          properties: {
            typeName: { type: "string" },
            cloneFromType: { type: "string" },
            parameters: {
              type: "object",
              additionalProperties: { type: ["string", "number", "boolean"] },
            },
          },
          required: ["typeName"],
          additionalProperties: false,
        },
      },
      {
        name: "rename_family_type",
        description: "Family Editor: rename a family type.",
        inputSchema: {
          type: "object",
          properties: {
            oldName: { type: "string" },
            newName: { type: "string" },
          },
          required: ["oldName", "newName"],
          additionalProperties: false,
        },
      },
      {
        name: "delete_family_type",
        description: "Family Editor: delete a family type. Fails if it is the only type.",
        inputSchema: {
          type: "object",
          properties: {
            typeName: { type: "string" },
          },
          required: ["typeName"],
          additionalProperties: false,
        },
      },
      {
        name: "set_formula",
        description:
          "Family Editor: set a parameter formula string (e.g. \"Width / 2\"). Clear error if the parameter is not found.",
        inputSchema: {
          type: "object",
          properties: {
            parameterName: { type: "string" },
            formula: { type: "string" },
          },
          required: ["parameterName", "formula"],
          additionalProperties: false,
        },
      },
      // ─── Family Editor: Document Management ───────────────────────────────────
      {
        name: "save_family",
        description:
          "Family Editor: save the active family document. Errors if it has no save path (use save_family_as first).",
        inputSchema: {
          type: "object",
          properties: {},
          additionalProperties: false,
        },
      },
      {
        name: "save_family_as",
        description: "Family Editor: save the active family to a new .rfa path (overwrite allowed).",
        inputSchema: {
          type: "object",
          properties: {
            filePath: { type: "string" },
          },
          required: ["filePath"],
          additionalProperties: false,
        },
      },
      {
        name: "load_family_into_project",
        description:
          "Family Editor: load (and reload) the active family into the first open project document.",
        inputSchema: {
          type: "object",
          properties: {
            overwriteParameterValues: { type: "boolean" },
          },
          additionalProperties: false,
        },
      },
      {
        name: "get_family_document_info",
        description:
          "Family Editor: return metadata about the active family — name, category, conceptual/face-based flags, host/placement type, type count, file path.",
        inputSchema: {
          type: "object",
          properties: {},
          additionalProperties: false,
        },
      },
      // ─── Project: Model Element Creation ──────────────────────────────────────
      {
        name: "create_wall",
        description:
          "Project: create a straight wall between two 3D points on a base level. All coordinates and lengths are millimetres. wallTypeId omitted = project default wall type; height omitted = level-to-level (top constraint = next level above).",
        inputSchema: {
          type: "object",
          properties: {
            startX: { type: "number" },
            startY: { type: "number" },
            startZ: { type: "number" },
            endX: { type: "number" },
            endY: { type: "number" },
            endZ: { type: "number" },
            levelId: { type: "number" },
            wallTypeId: { type: "number" },
            height: { type: "number" },
            structural: { type: "boolean" },
            flipped: { type: "boolean" },
          },
          required: ["startX", "startY", "startZ", "endX", "endY", "endZ", "levelId"],
          additionalProperties: false,
        },
      },
      {
        name: "create_level",
        description:
          "Project: create a level at a given elevation. elevationMm is millimetres. name omitted = Revit auto-names the level; a duplicate name will error (the level is still created).",
        inputSchema: {
          type: "object",
          properties: {
            elevationMm: { type: "number" },
            name: { type: "string" },
          },
          required: ["elevationMm"],
          additionalProperties: false,
        },
      },
      {
        name: "create_grid",
        description:
          "Project: create a straight grid line between two points in plan. startX/startY/endX/endY are millimetres (Z is not used for grids). name omitted = Revit auto-names the grid (next in sequence); a duplicate name will error (the grid is still created).",
        inputSchema: {
          type: "object",
          properties: {
            startX: { type: "number" },
            startY: { type: "number" },
            endX: { type: "number" },
            endY: { type: "number" },
            name: { type: "string" },
          },
          required: ["startX", "startY", "endX", "endY"],
          additionalProperties: false,
        },
      },
      {
        name: "create_floor",
        description:
          "Project: create a floor from a closed XY boundary loop on a level. Coordinates in millimetres. floorTypeId omitted = project default floor type.",
        inputSchema: {
          type: "object",
          properties: {
            profile: {
              type: "array",
              items: {
                type: "object",
                properties: { x: { type: "number" }, y: { type: "number" } },
                required: ["x", "y"],
                additionalProperties: false,
              },
            },
            levelId: { type: "number" },
            floorTypeId: { type: "number" },
            structural: { type: "boolean" },
          },
          required: ["profile", "levelId"],
          additionalProperties: false,
        },
      },
      {
        name: "create_room",
        description:
          "Project: create a room at an XY placement point on a level (the point must lie inside a region bounded by room-bounding walls or separation lines). Coordinates in millimetres.",
        inputSchema: {
          type: "object",
          properties: {
            levelId: { type: "number" },
            locationX: { type: "number" },
            locationY: { type: "number" },
            name: { type: "string" },
            number: { type: "string" },
          },
          required: ["levelId", "locationX", "locationY"],
          additionalProperties: false,
        },
      },
      {
        name: "create_structural_column",
        description:
          "Project: place a structural column (FamilySymbol id) at a 3D point on a level. Coordinates and heightMm in millimetres; heightMm omitted = family default top constraint. Use get_element_types (e.g. OST_StructuralColumns) to find familyTypeId.",
        inputSchema: {
          type: "object",
          properties: {
            familyTypeId: { type: "number" },
            locationX: { type: "number" },
            locationY: { type: "number" },
            locationZ: { type: "number" },
            levelId: { type: "number" },
            heightMm: { type: "number" },
          },
          required: ["familyTypeId", "locationX", "locationY", "locationZ", "levelId"],
          additionalProperties: false,
        },
      },
      {
        name: "create_beam",
        description:
          "Project: place a structural framing beam (FamilySymbol id) between two 3D points on a level. Coordinates in millimetres. Use get_element_types (e.g. OST_StructuralFraming) to find familyTypeId.",
        inputSchema: {
          type: "object",
          properties: {
            familyTypeId: { type: "number" },
            startX: { type: "number" },
            startY: { type: "number" },
            startZ: { type: "number" },
            endX: { type: "number" },
            endY: { type: "number" },
            endZ: { type: "number" },
            levelId: { type: "number" },
          },
          required: [
            "familyTypeId",
            "startX",
            "startY",
            "startZ",
            "endX",
            "endY",
            "endZ",
            "levelId",
          ],
          additionalProperties: false,
        },
      },
      {
        name: "place_point_based_element",
        description:
          "Project: place any point-based family instance (furniture, equipment, fixtures, etc.) by FamilySymbol id at a 3D point. Coordinates in millimetres; rotationDeg rotates around Z. levelId omitted = nearest level by elevation. (Routed to /api/project/create_point_based_element; named distinctly from the legacy batch create_point_based_element tool.)",
        inputSchema: {
          type: "object",
          properties: {
            familyTypeId: { type: "number" },
            locationX: { type: "number" },
            locationY: { type: "number" },
            locationZ: { type: "number" },
            levelId: { type: "number" },
            rotationDeg: { type: "number" },
          },
          required: ["familyTypeId", "locationX", "locationY", "locationZ"],
          additionalProperties: false,
        },
      },
      {
        name: "create_opening_by_boundary",
        description:
          "Project: cut an opening in a roof, floor, or ceiling host from a closed boundary loop. Coordinates in millimetres. projectOntHost (default true) projects the opening perpendicular onto the host face.",
        inputSchema: {
          type: "object",
          properties: {
            hostId: { type: "number" },
            profile: {
              type: "array",
              items: {
                type: "object",
                properties: {
                  x: { type: "number" },
                  y: { type: "number" },
                  z: { type: "number" },
                },
                required: ["x", "y"],
                additionalProperties: false,
              },
            },
            projectOntHost: { type: "boolean" },
          },
          required: ["hostId", "profile"],
          additionalProperties: false,
        },
      },
      // ─── Project: Modify & Edit (Group 7) ─────────────────────────────────────
      {
        name: "move_elements",
        description:
          "Project: translate one or more elements by a delta vector. Deltas in millimetres.",
        inputSchema: {
          type: "object",
          properties: {
            elementIds: { type: "array", items: { type: "number" } },
            deltaX: { type: "number" },
            deltaY: { type: "number" },
            deltaZ: { type: "number" },
          },
          required: ["elementIds", "deltaX", "deltaY", "deltaZ"],
          additionalProperties: false,
        },
      },
      {
        name: "rotate_elements",
        description:
          "Project: rotate one or more elements about an axis through an origin point. Origin in millimetres; angleDeg in degrees; axis is X, Y, or Z (default Z).",
        inputSchema: {
          type: "object",
          properties: {
            elementIds: { type: "array", items: { type: "number" } },
            originX: { type: "number" },
            originY: { type: "number" },
            originZ: { type: "number" },
            angleDeg: { type: "number" },
            axis: { type: "string", enum: ["X", "Y", "Z"] },
          },
          required: ["elementIds", "originX", "originY", "originZ", "angleDeg"],
          additionalProperties: false,
        },
      },
      {
        name: "mirror_elements",
        description:
          "Project: mirror one or more elements across a plane defined by an origin point and an axis normal (X, Y, or Z). Origin in millimetres. createCopy (default true) keeps the originals and adds mirrored copies; false mirrors in place.",
        inputSchema: {
          type: "object",
          properties: {
            elementIds: { type: "array", items: { type: "number" } },
            planeOriginX: { type: "number" },
            planeOriginY: { type: "number" },
            planeOriginZ: { type: "number" },
            planeNormal: { type: "string", enum: ["X", "Y", "Z"] },
            createCopy: { type: "boolean" },
          },
          required: ["elementIds", "planeOriginX", "planeOriginY", "planeOriginZ", "planeNormal"],
          additionalProperties: false,
        },
      },
      {
        name: "copy_elements_to_level",
        description:
          "Project: copy one or more elements to a target level, translated in Z by the elevation difference (aligned using the first element's level as reference). Returns the new element ids.",
        inputSchema: {
          type: "object",
          properties: {
            elementIds: { type: "array", items: { type: "number" } },
            targetLevelId: { type: "number" },
          },
          required: ["elementIds", "targetLevelId"],
          additionalProperties: false,
        },
      },
      {
        name: "set_element_parameters",
        description:
          "Project: set instance parameters on one element by name. Length values are millimetres, angles degrees, booleans for Yes/No. Read-only/unknown parameters are reported in skipped.",
        inputSchema: {
          type: "object",
          properties: {
            elementId: { type: "number" },
            parameters: {
              type: "object",
              additionalProperties: { type: ["string", "number", "boolean"] },
            },
          },
          required: ["elementId", "parameters"],
          additionalProperties: false,
        },
      },
      {
        name: "delete_elements",
        description:
          "Project: delete one or more elements by id. Returns all ids Revit removed, including cascade-deleted dependents.",
        inputSchema: {
          type: "object",
          properties: {
            elementIds: { type: "array", items: { type: "number" } },
          },
          required: ["elementIds"],
          additionalProperties: false,
        },
      },
      // ─── Project: Views, Sheets & Sheet Audit (Group 8) ───────────────────────
      {
        name: "create_floor_plan_view",
        description:
          "Project: create a floor plan view for a level (uses the first Floor Plan view family type). Optional name (uniquified on clash).",
        inputSchema: {
          type: "object",
          properties: {
            levelId: { type: "number" },
            name: { type: "string" },
          },
          required: ["levelId"],
          additionalProperties: false,
        },
      },
      {
        name: "create_reflected_ceiling_plan",
        description:
          "Project: create a reflected ceiling plan view for a level (uses the first Ceiling Plan view family type). Optional name.",
        inputSchema: {
          type: "object",
          properties: {
            levelId: { type: "number" },
            name: { type: "string" },
          },
          required: ["levelId"],
          additionalProperties: false,
        },
      },
      {
        name: "create_section_view",
        description:
          "Project: create a section view through a 3D bounding box (looking horizontally along +Y; view right = X, up = Z). boundingBoxMin/Max are [x,y,z] in millimetres. Optional name.",
        inputSchema: {
          type: "object",
          properties: {
            boundingBoxMin: { type: "array", items: { type: "number" }, minItems: 3, maxItems: 3 },
            boundingBoxMax: { type: "array", items: { type: "number" }, minItems: 3, maxItems: 3 },
            name: { type: "string" },
          },
          required: ["boundingBoxMin", "boundingBoxMax"],
          additionalProperties: false,
        },
      },
      {
        name: "create_3d_view",
        description:
          "Project: create a 3D view. isOrthographic=true makes an isometric view; false (default) makes a perspective view. Optional name.",
        inputSchema: {
          type: "object",
          properties: {
            name: { type: "string" },
            isOrthographic: { type: "boolean" },
          },
          additionalProperties: false,
        },
      },
      {
        name: "create_sheet",
        description:
          "Project: create a sheet with a sheet number and name. titleBlockTypeId optional (omit for no title block). Use get_element_types (OST_TitleBlocks) to find a title block type id.",
        inputSchema: {
          type: "object",
          properties: {
            sheetNumber: { type: "string" },
            sheetName: { type: "string" },
            titleBlockTypeId: { type: "number" },
          },
          required: ["sheetNumber", "sheetName"],
          additionalProperties: false,
        },
      },
      {
        name: "place_view_on_sheet",
        description:
          "Project: place a view on a sheet at a center point (millimetres in sheet space). A view can only be placed on one sheet.",
        inputSchema: {
          type: "object",
          properties: {
            sheetId: { type: "number" },
            viewId: { type: "number" },
            locationX: { type: "number" },
            locationY: { type: "number" },
          },
          required: ["sheetId", "viewId", "locationX", "locationY"],
          additionalProperties: false,
        },
      },
      {
        name: "set_view_crop_region",
        description:
          "Project: set a view's crop box extents (millimetres, in the view plane) and activate cropping.",
        inputSchema: {
          type: "object",
          properties: {
            viewId: { type: "number" },
            minX: { type: "number" },
            minY: { type: "number" },
            maxX: { type: "number" },
            maxY: { type: "number" },
          },
          required: ["viewId", "minX", "minY", "maxX", "maxY"],
          additionalProperties: false,
        },
      },
      {
        name: "get_sheets",
        description:
          "Project: list all sheets with id, sheet number, name, title block name, and the ids of viewports placed on each.",
        inputSchema: {
          type: "object",
          properties: {},
          additionalProperties: false,
        },
      },
      {
        name: "get_sheet_contents",
        description:
          "Project: full inventory of one sheet — title block parameter values, viewports (view id/name/type, center mm, crop box mm), text notes, tags, and filled regions.",
        inputSchema: {
          type: "object",
          properties: {
            sheetId: { type: "number" },
          },
          required: ["sheetId"],
          additionalProperties: false,
        },
      },
      {
        name: "open_sheet",
        description:
          "Project: set the active Revit view to a sheet, by sheetId or sheetNumber (one required).",
        inputSchema: {
          type: "object",
          properties: {
            sheetNumber: { type: "string" },
            sheetId: { type: "number" },
          },
          additionalProperties: false,
        },
      },
      {
        name: "get_view_contents",
        description:
          "Project: inventory of one view — model elements grouped by category, dimensions (labeled/unlabeled), tags and tagged element ids, untagged model elements, and text notes.",
        inputSchema: {
          type: "object",
          properties: {
            viewId: { type: "number" },
            includeElements: { type: "boolean" },
            includeAnnotations: { type: "boolean" },
          },
          required: ["viewId"],
          additionalProperties: false,
        },
      },
      {
        name: "compare_sheet_to_template",
        description:
          "Project: audit a sheet against a JSON template (required title block fields, required view types/count, minimum dimensions, require-all-tagged, required text notes). Returns pass/fail and a list of issues. templateJson is a serialized SheetTemplate object.",
        inputSchema: {
          type: "object",
          properties: {
            sheetId: { type: "number" },
            templateJson: { type: "string" },
          },
          required: ["sheetId", "templateJson"],
          additionalProperties: false,
        },
      },
      // ─── Project: Geometry Read & In-Place Extraction (Group 5) ───────────────
      {
        name: "get_active_document_context",
        description:
          "Diagnostic: report the active Revit document context — 'project', 'family_editor', or 'edit_in_place' — with the document title and (for family editor) the family category. Note: Revit's public API cannot reliably detect an in-place edit session, so in-place edits report as 'project' (see returned note).",
        inputSchema: {
          type: "object",
          properties: {},
          additionalProperties: false,
        },
      },
      {
        name: "get_element_geometry",
        description:
          "Extract curve-loop sketch profiles and solid face/edge geometry from a selected or specified element. Works in project and Edit-In-Place contexts. Lines and arcs are returned as clean analytic curves (so reconstruction is crisp, not polyline-approximated); splines fall back to sampled points. Output is structured to feed create_extrusion / create_sweep. All coordinates, lengths, radii in mm; volumes in mm³.",
        inputSchema: {
          type: "object",
          properties: {
            elementId: { type: "number" },
            includeSketches: { type: "boolean" },
            includeSolids: { type: "boolean" },
            useSelection: { type: "boolean" },
          },
          additionalProperties: false,
        },
      },
      {
        name: "get_inplace_elements",
        description:
          "Inventory all in-place model family instances in the active project, optionally filtered by an OST category token (e.g. OST_GenericModel). Returns id, name, category, family name, and bounding-box center (mm). Use before extracting geometry to see what in-place geometry exists.",
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
        name: "reconstruct_profile_for_extrusion",
        description:
          "Reconstruct clean, ordered, closed 2D loops from an extruded element's cap face — ready to feed create_extrusion. Picks the cap face (planar, axis-aligned, largest area), chains ALL its edge loops (outer boundary + interior voids) by endpoint matching, projects to 2D by dropping the extrusion axis, normalizes all loops to a shared origin, and forces consistent counter-clockwise winding (fixes mirrored output). Returns loops[] (each with isOuter + ordered points), extrusion depth (mm), and axis. Provide elementId or set useSelection=true. All lengths in mm.",
        inputSchema: {
          type: "object",
          properties: {
            elementId: { type: "number" },
            epsilonMm: { type: "number" },
            normalizeToOrigin: { type: "boolean" },
            useSelection: { type: "boolean" },
          },
          additionalProperties: false,
        },
      },
      // ─── Project: Query Tools (Group 11, read-only) ───────────────────────────
      {
        name: "get_levels",
        description:
          "Project: list all levels with id, name, elevation (mm), and isGroundFloor (the level whose elevation is closest to 0). Requires an active project document.",
        inputSchema: {
          type: "object",
          properties: {},
          additionalProperties: false,
        },
      },
      {
        name: "get_grids",
        description:
          "Project: list all grids with id, name, and the two endpoints of the grid line (mm). Requires an active project document.",
        inputSchema: {
          type: "object",
          properties: {},
          additionalProperties: false,
        },
      },
      {
        name: "get_element_types",
        description:
          "Project: list loadable family types (FamilySymbols), optionally filtered by an OST category token (e.g. OST_Walls). Returns id, name, familyName, category, isActive, plus totalCount. Requires an active project document.",
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
        name: "query_elements",
        description:
          "Project: general element query by category (OST token), level id, and/or bounding box (mm; provide all six bbox values to filter). Returns id, name, category, familyName, and bounding-box center (mm), plus totalCount. Requires an active project document.",
        inputSchema: {
          type: "object",
          properties: {
            category: { type: "string" },
            levelId: { type: "number" },
            bboxMinX: { type: "number" },
            bboxMinY: { type: "number" },
            bboxMinZ: { type: "number" },
            bboxMaxX: { type: "number" },
            bboxMaxY: { type: "number" },
            bboxMaxZ: { type: "number" },
            limit: { type: "number" },
          },
          additionalProperties: false,
        },
      },
      {
        name: "get_element_by_id",
        description:
          "Project: full metadata and (optionally) all readable parameters for one element by id — name, category, familyName, typeName, location center (mm), and parameters as key/value pairs. Requires an active project document.",
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
        name: "create_datums",
        description:
          "Project (v20.1): batch-create levels and grids from a levels_grids.json file. The gateway reads the file from disk and creates every level (by elevationMm) and grid (by start/end mm) in a single transaction, skipping any whose name already exists (skipExisting, default true). Returns created/skipped counts + per-item errors. Establishes the rebuild reference frame / column lines. Requires an active project document.",
        inputSchema: {
          type: "object",
          properties: {
            levelsGridsPath: { type: "string" },
            skipExisting: { type: "boolean" },
            createLevels: { type: "boolean" },
            createGrids: { type: "boolean" },
          },
          required: ["levelsGridsPath"],
          additionalProperties: false,
        },
      },
      {
        name: "export_parameter_bindings",
        description:
          "Project: export all project parameter bindings and the full shared-parameter inventory to a JSON file on disk (written by the gateway; only a summary crosses MCP). Output contains bindings[] {name, isInstance, group, dataType, boundCategories[], guid, isShared} plus allSharedParameterElements[] {name, guid}. Used to GUID-verify the 2024 template's shared parameters against the extracted snapshot. Requires an active project document.",
        inputSchema: {
          type: "object",
          properties: {
            outputPath: { type: "string" },
          },
          required: ["outputPath"],
          additionalProperties: false,
        },
      },
      {
        name: "import_instances",
        description:
          "Project (v20, Revit 2024 rebuild): rebuild placed instances from an export_instances snapshot. The gateway reads instancesPath and sibling datasets (levels_grids.json, family_types.json, shared_project_parameters.json) directly from disk and writes a detailed per-element report plus an external remap file — NO element data crosses the MCP boundary; only a summary returns. Type resolution: exact (familyName+typeName) → typemap JSON (load family file + clone type with family_types params + overrides) → unresolved. Placement routes by placementType/host: hostPass 1 = level-hosted point/curve records; hostPass 2 = instance-hosted (reveals/voids) resolved through the remap file with a workPlaneName cross-check. Shared-parameter writes key on GUID (inventory), non-shared by name; raw is authoritative for Length (mm) and Angle (deg). mode 'dryrun' resolves everything and writes the report with NO transaction; 'execute' places in chunked, progressively-committed transactions. Long-running: use resumeFromIndex + the returned nextIndex/done to resume; optional maxElements bounds a call (a small maxElements, ≤64, also enables per-element orientation verification for pilot runs).",
        inputSchema: {
          type: "object",
          properties: {
            instancesPath: { type: "string" },
            typeMapPath: { type: "string" },
            levelsGridsPath: { type: "string" },
            familyTypesPath: { type: "string" },
            sharedParamsPath: { type: "string" },
            remapOutPath: { type: "string" },
            reportPath: { type: "string" },
            mode: { type: "string", enum: ["dryrun", "execute", "reapply"] },
            categories: { type: "array", items: { type: "string" } },
            hostPass: { type: "number", enum: [1, 2] },
            resumeFromIndex: { type: "number" },
            maxElements: { type: "number" },
          },
          required: ["instancesPath", "remapOutPath", "mode", "hostPass"],
          additionalProperties: false,
        },
      },
    ],
  }));

  server.setRequestHandler(CallToolRequestSchema, async (request) => {
    const name = request.params.name;
    try {
    const args = ArgsSchema.parse(request.params.arguments ?? {});

    if (name === "say_hello") {
      const health = await gatewayHealth();
      return result({
        ok: true,
        server: "revit-2024-community-bridge-mcp",
        gateway: health,
      });
    }

    if (name === "get_document_metadata") {
      const metadata = await getDocumentMetadata(args);
      return result({
        outcome: "Retrieved active document metadata.",
        document: metadata,
      });
    }

    if (name === "get_element_metadata") {
      const useSelection = Boolean(args.useSelection);
      const elementIds = Array.isArray(args.elementIds)
        ? args.elementIds.map((v) => Number(v)).filter((v) => Number.isFinite(v) && v > 0)
        : [];
      if (!useSelection && elementIds.length === 0) {
        throw new Error("Set useSelection=true or provide elementIds.");
      }
      const elements = await getElementMetadata({
        ...args,
        useSelection,
        elementIds: elementIds.length > 0 ? elementIds : undefined,
      });
      return result({
        outcome: `Retrieved metadata for ${elements.length} element(s).`,
        elements,
      });
    }

    if (name === "get_family_parameters") {
      const family = await getFamilyParameters(args);
      const params = Array.isArray((family as { parameters?: unknown }).parameters)
        ? (family as { parameters: unknown[] }).parameters
        : [];
      return result({
        outcome: `Retrieved ${params.length} family parameter definition(s) from active family document.`,
        family,
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

    if (name === "add_shared_params_to_family_library") {
      const updated = await addSharedParamsToFamilyLibrary(args);
      return result({
        outcome: "Shared parameters added to family library files.",
        ...updated,
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

    if (name === "extract_family_library_parameters") {
      const extracted = await extractFamilyLibraryParameters(args);
      return result({
        outcome: "Family library parameter extraction completed.",
        ...extracted,
      });
    }

    if (name === "extract_family_desc_variants") {
      const variants = await extractFamilyDescVariants(args);
      return result({
        outcome: "Family description variants extraction completed.",
        ...variants,
      });
    }

    // ─── Family Editor: Sketch & Solid Geometry ───────────────────────────────
    if (name === "create_extrusion") {
      const created = await createExtrusion(args);
      return result({
        outcome: `Created ${args.isSolid ? "solid" : "void"} extrusion in active family document.`,
        ...created,
      });
    }

    if (name === "create_blend") {
      const created = await createBlend(args);
      return result({
        outcome: `Created ${args.isSolid ? "solid" : "void"} blend in active family document.`,
        ...created,
      });
    }

    if (name === "create_revolve") {
      const created = await createRevolve(args);
      return result({
        outcome: `Created ${args.isSolid ? "solid" : "void"} revolve in active family document.`,
        ...created,
      });
    }

    if (name === "create_sweep") {
      const created = await createSweep(args);
      return result({
        outcome: `Created ${args.isSolid ? "solid" : "void"} sweep in active family document.`,
        ...created,
      });
    }

    if (name === "create_swept_blend") {
      const created = await createSweptBlend(args);
      return result({
        outcome: `Created ${args.isSolid ? "solid" : "void"} swept blend in active family document.`,
        ...created,
      });
    }

    if (name === "set_geometry_solid_void") {
      const updated = await setGeometrySolidVoid(args);
      return result({
        outcome: `Set element ${String(args.elementId ?? "")} to ${args.isSolid ? "solid" : "void"}.`,
        ...updated,
      });
    }

    // ─── Family Editor: Reference Planes, Dimensions & Constraints ────────────
    if (name === "create_dimension") {
      const created = await createDimension(args);
      return result({
        outcome: "Created dimension in active family document.",
        ...created,
      });
    }

    if (name === "set_dimension_label") {
      const updated = await setDimensionLabel(args);
      return result({
        outcome: `Labeled dimension ${String(args.dimensionId ?? "")} with '${String(args.parameterName ?? "")}'.`,
        ...updated,
      });
    }

    if (name === "lock_constraint") {
      const updated = await lockConstraint(args);
      return result({
        outcome: `${args.locked ? "Locked" : "Unlocked"} constraint ${String(args.constraintId ?? "")}.`,
        ...updated,
      });
    }

    // ─── Family Editor: Parameters & Types ────────────────────────────────────
    if (name === "add_family_parameter") {
      const created = await addFamilyParameter(args);
      return result({
        outcome: `Added ${args.isInstance ? "instance" : "type"} parameter '${String(args.name ?? "")}'.`,
        ...created,
      });
    }

    if (name === "set_family_parameter_value") {
      const updated = await setFamilyParameterValue(args);
      return result({
        outcome: `Set parameter '${String(args.parameterName ?? "")}'.`,
        ...updated,
      });
    }

    if (name === "add_family_type") {
      const created = await addFamilyType(args);
      return result({
        outcome: `Added family type '${String(args.typeName ?? "")}'.`,
        ...created,
      });
    }

    if (name === "rename_family_type") {
      const updated = await renameFamilyType(args);
      return result({
        outcome: `Renamed family type '${String(args.oldName ?? "")}' to '${String(args.newName ?? "")}'.`,
        ...updated,
      });
    }

    if (name === "delete_family_type") {
      const updated = await deleteFamilyType(args);
      return result({
        outcome: `Deleted family type '${String(args.typeName ?? "")}'.`,
        ...updated,
      });
    }

    if (name === "set_formula") {
      const updated = await setFormula(args);
      return result({
        outcome: `Set formula on '${String(args.parameterName ?? "")}'.`,
        ...updated,
      });
    }

    // ─── Family Editor: Document Management ───────────────────────────────────
    if (name === "save_family") {
      const saved = await saveFamily(args);
      return result({
        outcome: "Saved family document.",
        ...saved,
      });
    }

    if (name === "save_family_as") {
      const saved = await saveFamilyAs(args);
      return result({
        outcome: "Saved family document to new path.",
        ...saved,
      });
    }

    if (name === "load_family_into_project") {
      const loaded = await loadFamilyIntoProject(args);
      return result({
        outcome: "Loaded family into project.",
        ...loaded,
      });
    }

    if (name === "get_family_document_info") {
      const info = await getFamilyDocumentInfo(args);
      return result({
        outcome: "Retrieved family document info.",
        ...info,
      });
    }

    // ─── Family Editor: Reference Planes, Dimensions & Constraints ────────────
    if (name === "create_reference_plane") {
      const created = await createReferencePlane(args);
      return result({
        outcome: `Created reference plane '${String(args.name ?? "")}'.`,
        ...created,
      });
    }

    // ─── Project: Model Element Creation (Group 6) ────────────────────────────
    if (name === "create_wall") {
      const created = await createWall(args);
      return result({
        outcome: "Created wall in active project.",
        ...created,
      });
    }

    if (name === "create_level") {
      const created = await createLevel(args);
      return result({
        outcome: "Created level in active project.",
        ...created,
      });
    }

    if (name === "create_grid") {
      const created = await createGrid(args);
      return result({
        outcome: "Created grid in active project.",
        ...created,
      });
    }

    if (name === "create_floor") {
      const created = await createFloor(args);
      return result({
        outcome: "Created floor in active project.",
        ...created,
      });
    }

    if (name === "create_room") {
      const created = await createRoom(args);
      return result({
        outcome: "Created room in active project.",
        ...created,
      });
    }

    if (name === "create_structural_column") {
      const created = await createStructuralColumn(args);
      return result({
        outcome: "Created structural column in active project.",
        ...created,
      });
    }

    if (name === "create_beam") {
      const created = await createBeam(args);
      return result({
        outcome: "Created beam in active project.",
        ...created,
      });
    }

    if (name === "place_point_based_element") {
      const created = await placePointBasedElement(args);
      return result({
        outcome: "Placed point-based family instance in active project.",
        ...created,
      });
    }

    if (name === "create_opening_by_boundary") {
      const created = await createOpeningByBoundary(args);
      return result({
        outcome: "Created opening in host element.",
        ...created,
      });
    }

    // ─── Project: Modify & Edit (Group 7) ─────────────────────────────────────
    if (name === "move_elements") {
      const moved = await moveElements(args);
      return result({
        outcome: `Moved ${Number((moved as { movedCount?: unknown }).movedCount ?? 0)} element(s).`,
        ...moved,
      });
    }

    if (name === "rotate_elements") {
      const rotated = await rotateElements(args);
      return result({
        outcome: `Rotated ${Number((rotated as { rotatedCount?: unknown }).rotatedCount ?? 0)} element(s).`,
        ...rotated,
      });
    }

    if (name === "mirror_elements") {
      const mirrored = await mirrorElements(args);
      return result({
        outcome: `Mirrored ${Number((mirrored as { mirroredCount?: unknown }).mirroredCount ?? 0)} element(s).`,
        ...mirrored,
      });
    }

    if (name === "copy_elements_to_level") {
      const copied = await copyElementsToLevel(args);
      return result({
        outcome: `Copied ${Number((copied as { copiedCount?: unknown }).copiedCount ?? 0)} element(s) to target level.`,
        ...copied,
      });
    }

    if (name === "set_element_parameters") {
      const updated = await setElementParameters(args);
      return result({
        outcome: `Set ${Number((updated as { setCount?: unknown }).setCount ?? 0)} parameter(s).`,
        ...updated,
      });
    }

    if (name === "delete_elements") {
      const deleted = await deleteElements(args);
      return result({
        outcome: `Deleted ${Number((deleted as { deletedCount?: unknown }).deletedCount ?? 0)} element(s).`,
        ...deleted,
      });
    }

    // ─── Project: Views, Sheets & Sheet Audit (Group 8) ───────────────────────
    if (name === "create_floor_plan_view") {
      const created = await createFloorPlanView(args);
      return result({ outcome: "Created floor plan view.", ...created });
    }

    if (name === "create_reflected_ceiling_plan") {
      const created = await createReflectedCeilingPlan(args);
      return result({ outcome: "Created reflected ceiling plan view.", ...created });
    }

    if (name === "create_section_view") {
      const created = await createSectionView(args);
      return result({ outcome: "Created section view.", ...created });
    }

    if (name === "create_3d_view") {
      const created = await create3dView(args);
      return result({ outcome: "Created 3D view.", ...created });
    }

    if (name === "create_sheet") {
      const created = await createSheet(args);
      return result({ outcome: "Created sheet.", ...created });
    }

    if (name === "place_view_on_sheet") {
      const placed = await placeViewOnSheet(args);
      return result({ outcome: "Placed view on sheet.", ...placed });
    }

    if (name === "set_view_crop_region") {
      const updated = await setViewCropRegion(args);
      return result({ outcome: "Set view crop region.", ...updated });
    }

    if (name === "get_sheets") {
      const data = await getSheets(args);
      const total = Number((data as { totalCount?: unknown }).totalCount ?? 0);
      return result({ outcome: `Retrieved ${total} sheet(s).`, ...data });
    }

    if (name === "get_sheet_contents") {
      const data = await getSheetContents(args);
      return result({
        outcome: `Retrieved contents for sheet ${String((data as { sheetNumber?: unknown }).sheetNumber ?? "")}.`,
        ...data,
      });
    }

    if (name === "open_sheet") {
      const data = await openSheet(args);
      return result({
        outcome: `Opened sheet ${String((data as { sheetNumber?: unknown }).sheetNumber ?? "")}.`,
        ...data,
      });
    }

    if (name === "get_view_contents") {
      const data = await getViewContents(args);
      return result({
        outcome: `Retrieved contents for view ${String((data as { viewName?: unknown }).viewName ?? "")}.`,
        ...data,
      });
    }

    if (name === "compare_sheet_to_template") {
      const data = await compareSheetToTemplate(args);
      const passed = (data as { passed?: unknown }).passed === true;
      const issueCount = Number((data as { issueCount?: unknown }).issueCount ?? 0);
      return result({
        outcome: passed
          ? "Sheet passed template audit (0 errors)."
          : `Sheet failed template audit (${issueCount} issue(s)).`,
        ...data,
      });
    }

    // ─── Project: Geometry Read & In-Place Extraction (Group 5) ───────────────
    if (name === "get_active_document_context") {
      const context = await getActiveDocumentContext(args);
      return result({
        outcome: "Retrieved active document context.",
        ...context,
      });
    }

    if (name === "get_element_geometry") {
      const geometry = await getElementGeometry(args);
      const sketchCount = Array.isArray((geometry as { sketches?: unknown }).sketches)
        ? (geometry as { sketches: unknown[] }).sketches.length
        : 0;
      const solidCount = Array.isArray((geometry as { solids?: unknown }).solids)
        ? (geometry as { solids: unknown[] }).solids.length
        : 0;
      return result({
        outcome: `Extracted ${sketchCount} sketch profile(s) and ${solidCount} solid(s).`,
        ...geometry,
      });
    }

    if (name === "get_inplace_elements") {
      const inplace = await getInplaceElements(args);
      const total = Number((inplace as { totalCount?: unknown }).totalCount ?? 0);
      return result({
        outcome: `Found ${total} in-place model element(s).`,
        ...inplace,
      });
    }

    if (name === "reconstruct_profile_for_extrusion") {
      const reconstructed = await reconstructProfileForExtrusion(args);
      const loops = Array.isArray((reconstructed as { loops?: unknown }).loops)
        ? (reconstructed as { loops: unknown[] }).loops
        : [];
      const closed = (reconstructed as { closed?: unknown }).closed === true;
      return result({
        outcome: closed
          ? `Reconstructed ${loops.length} closed loop(s).`
          : `Reconstructed ${loops.length} loop(s) (a loop did not close).`,
        ...reconstructed,
      });
    }

    // ─── Project: Query Tools (Group 11, read-only) ───────────────────────────
    if (name === "get_levels") {
      const data = await getLevels(args);
      const count = Array.isArray((data as { levels?: unknown }).levels)
        ? (data as { levels: unknown[] }).levels.length
        : 0;
      return result({
        outcome: `Retrieved ${count} level(s).`,
        ...data,
      });
    }

    if (name === "get_grids") {
      const data = await getGrids(args);
      const count = Array.isArray((data as { grids?: unknown }).grids)
        ? (data as { grids: unknown[] }).grids.length
        : 0;
      return result({
        outcome: `Retrieved ${count} grid(s).`,
        ...data,
      });
    }

    if (name === "get_element_types") {
      const data = await getElementTypes(args);
      const total = Number((data as { totalCount?: unknown }).totalCount ?? 0);
      return result({
        outcome: `Retrieved ${total} element type(s).`,
        ...data,
      });
    }

    if (name === "query_elements") {
      const data = await queryElements(args);
      const total = Number((data as { totalCount?: unknown }).totalCount ?? 0);
      return result({
        outcome: `Query matched ${total} element(s).`,
        ...data,
      });
    }

    if (name === "get_element_by_id") {
      const data = await getElementById(args);
      return result({
        outcome: `Retrieved element ${String(args.elementId ?? "")}.`,
        ...data,
      });
    }

    if (name === "create_datums") {
      const data = await createDatums(args);
      return result({
        outcome:
          `create_datums: levels +${Number((data as { levelsCreated?: unknown }).levelsCreated ?? 0)} ` +
          `(skipped ${Number((data as { levelsSkipped?: unknown }).levelsSkipped ?? 0)}), ` +
          `grids +${Number((data as { gridsCreated?: unknown }).gridsCreated ?? 0)} ` +
          `(skipped ${Number((data as { gridsSkipped?: unknown }).gridsSkipped ?? 0)}).`,
        ...data,
      });
    }

    if (name === "export_parameter_bindings") {
      const data = await exportParameterBindings(args);
      return result({
        outcome: `Exported parameter bindings to ${String((data as { outputPath?: unknown }).outputPath ?? args.outputPath ?? "")}.`,
        ...data,
      });
    }

    if (name === "import_instances") {
      const data = await importInstances(args);
      const mode = String((data as { mode?: unknown }).mode ?? args.mode ?? "");
      const attempted = Number((data as { attempted?: unknown }).attempted ?? 0);
      const placed = Number((data as { placed?: unknown }).placed ?? 0);
      const skipped = Number((data as { skipped?: unknown }).skipped ?? 0);
      const failed = Number((data as { failed?: unknown }).failed ?? 0);
      const done = (data as { done?: unknown }).done === true;
      const verb = mode === "execute" ? "placed" : "resolved (would place)";
      return result({
        outcome:
          `import_instances pass ${String(args.hostPass ?? "")} [${mode}]: ` +
          `attempted ${attempted}, ${verb} ${placed}, skipped ${skipped}, failed ${failed}. ` +
          `${done ? "Done." : `Not done — resume from index ${String((data as { nextIndex?: unknown }).nextIndex ?? "")}.`}`,
        ...data,
      });
    }

    throw new Error(`Unknown tool: ${name}`);
    } catch (error) {
      // Surface the underlying error text (gateway/C# exception message, arg
      // validation error, etc.) as tool content so the client shows it instead
      // of a generic "Tool execution failed". isError flags the failed call.
      return {
        content: [
          {
            type: "text",
            text: error instanceof Error ? error.message : String(error),
          },
        ],
        isError: true,
      };
    }
  });

  return server;
}
