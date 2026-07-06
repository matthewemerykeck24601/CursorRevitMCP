import {
  addAllViewsOverallDimensions,
  addDiameterDimension,
  addLinearDimension,
  addViewOverallDimensions,
  getActiveDocumentInfo,
  getDocumentSettings,
  listDrawingViews,
  runILogicRule,
  setAngleUnits,
  setDimensionPrecision,
  setLengthUnits,
  setMassUnits,
  setModelingDisplay,
  showILogicForm,
  writeILogicRule,
  gatewayHealth,
} from "../bridgeClient.js";
import type { JsonObject } from "../types/contracts.js";

export const ilogicToolDefs = [
  {
    name: "inventor_health",
    description: "Validate Inventor gateway connectivity.",
    inputSchema: {
      type: "object",
      properties: {},
      additionalProperties: false,
    },
  },
  {
    name: "inventor_get_active_document",
    description: "Get active Inventor document metadata.",
    inputSchema: {
      type: "object",
      properties: {},
      additionalProperties: false,
    },
  },
  {
    name: "inventor_run_ilogic_rule",
    description: "Run a named iLogic rule in the active Inventor document.",
    inputSchema: {
      type: "object",
      properties: {
        ruleName: { type: "string" },
        runMode: { type: "string", enum: ["activeDocument", "allReferenced"] },
      },
      required: ["ruleName"],
      additionalProperties: false,
    },
  },
  {
    name: "inventor_show_ilogic_form",
    description: "Show a named iLogic form in the active Inventor document.",
    inputSchema: {
      type: "object",
      properties: {
        formName: { type: "string" },
      },
      required: ["formName"],
      additionalProperties: false,
    },
  },
  {
    name: "inventor_write_ilogic_rule",
    description: "Create or update a named iLogic rule in active document.",
    inputSchema: {
      type: "object",
      properties: {
        ruleName: { type: "string" },
        code: { type: "string" },
        overwrite: { type: "boolean" },
      },
      required: ["ruleName", "code"],
      additionalProperties: false,
    },
  },
  {
    name: "inventor_list_drawing_views",
    description: "List drawing views on active Inventor sheet.",
    inputSchema: {
      type: "object",
      properties: {},
      additionalProperties: false,
    },
  },
  {
    name: "inventor_add_view_overall_dimensions",
    description: "Add width/height dimensions to one drawing view.",
    inputSchema: {
      type: "object",
      properties: {
        viewIndex: { type: "number" },
        viewName: { type: "string" },
        offsetInSheet: { type: "number" },
        addWidth: { type: "boolean" },
        addHeight: { type: "boolean" },
      },
      additionalProperties: false,
    },
  },
  {
    name: "inventor_add_all_views_overall_dimensions",
    description: "Add width/height dimensions to all drawing views.",
    inputSchema: {
      type: "object",
      properties: {
        offsetInSheet: { type: "number" },
        addWidth: { type: "boolean" },
        addHeight: { type: "boolean" },
      },
      additionalProperties: false,
    },
  },
  {
    name: "inventor_add_diameter_dimension",
    description: "Add a diameter dimension to a specific circle in a drawing view.",
    inputSchema: {
      type: "object",
      properties: {
        viewIndex: { type: "number" },
        viewName: { type: "string" },
        circleIndex: { type: "number" },
        textOffsetX: { type: "number" },
        textOffsetY: { type: "number" },
      },
      additionalProperties: false,
    },
  },
  {
    name: "inventor_add_linear_dimension",
    description: "Add a linear dimension between two points on a drawing sheet.",
    inputSchema: {
      type: "object",
      properties: {
        viewIndex: { type: "number" },
        viewName: { type: "string" },
        fromX: { type: "number" },
        fromY: { type: "number" },
        toX: { type: "number" },
        toY: { type: "number" },
        dimensionType: { type: "string" },
        fromCircleIndex: { type: "number" },
        toCircleIndex: { type: "number" },
        textOffsetX: { type: "number" },
        textOffsetY: { type: "number" },
      },
      additionalProperties: false,
    },
  },
  {
    name: "inventor_get_document_settings",
    description: "Get current document units, precision, and display settings.",
    inputSchema: {
      type: "object",
      properties: {},
      additionalProperties: false,
    },
  },
  {
    name: "inventor_set_length_units",
    description: "Set the length units for the active document.",
    inputSchema: {
      type: "object",
      properties: {
        units: { type: "string" },
      },
      required: ["units"],
      additionalProperties: false,
    },
  },
  {
    name: "inventor_set_angle_units",
    description: "Set the angle units for the active document.",
    inputSchema: {
      type: "object",
      properties: {
        units: { type: "string" },
      },
      required: ["units"],
      additionalProperties: false,
    },
  },
  {
    name: "inventor_set_mass_units",
    description: "Set the mass units for the active document.",
    inputSchema: {
      type: "object",
      properties: {
        units: { type: "string" },
      },
      required: ["units"],
      additionalProperties: false,
    },
  },
  {
    name: "inventor_set_dimension_precision",
    description: "Set linear and/or angular dimension precision.",
    inputSchema: {
      type: "object",
      properties: {
        linearPrecision: { type: "number" },
        angularPrecision: { type: "number" },
      },
      additionalProperties: false,
    },
  },
  {
    name: "inventor_set_modeling_display",
    description: "Set modeling display mode and visibility options.",
    inputSchema: {
      type: "object",
      properties: {
        displayMode: { type: "string" },
        showSketches: { type: "boolean" },
        showWorkFeatures: { type: "boolean" },
      },
      additionalProperties: false,
    },
  },
] as const;

export async function invokeILogicTool(name: string, args: JsonObject): Promise<JsonObject | null> {
  if (name === "inventor_health") {
    const health = await gatewayHealth();
    return {
      ok: true,
      server: "inventor-mcp",
      gateway: health,
    };
  }

  if (name === "inventor_get_active_document") {
    return getActiveDocumentInfo();
  }

  if (name === "inventor_list_drawing_views") {
    return listDrawingViews();
  }

  if (name === "inventor_run_ilogic_rule") {
    const ruleName = String(args.ruleName ?? "").trim();
    if (!ruleName) {
      throw new Error("ruleName is required.");
    }
    return runILogicRule({
      ruleName,
      runMode: args.runMode === "allReferenced" ? "allReferenced" : "activeDocument",
    });
  }

  if (name === "inventor_show_ilogic_form") {
    const formName = String(args.formName ?? "").trim();
    if (!formName) {
      throw new Error("formName is required.");
    }
    return showILogicForm({ formName });
  }

  if (name === "inventor_write_ilogic_rule") {
    const ruleName = String(args.ruleName ?? "").trim();
    const code = String(args.code ?? "");
    if (!ruleName || !code) {
      throw new Error("ruleName and code are required.");
    }
    return writeILogicRule({
      ruleName,
      code,
      overwrite: args.overwrite === true,
    });
  }

  if (name === "inventor_add_view_overall_dimensions") {
    const viewIndex = typeof args.viewIndex === "number" ? Math.trunc(args.viewIndex) : undefined;
    const viewName = typeof args.viewName === "string" ? args.viewName.trim() : undefined;
    if (!viewIndex && !viewName) {
      throw new Error("viewIndex or viewName is required.");
    }
    return addViewOverallDimensions({
      viewIndex,
      viewName,
      offsetInSheet: typeof args.offsetInSheet === "number" ? args.offsetInSheet : undefined,
      addWidth: typeof args.addWidth === "boolean" ? args.addWidth : undefined,
      addHeight: typeof args.addHeight === "boolean" ? args.addHeight : undefined,
    });
  }

  if (name === "inventor_add_all_views_overall_dimensions") {
    return addAllViewsOverallDimensions({
      offsetInSheet: typeof args.offsetInSheet === "number" ? args.offsetInSheet : undefined,
      addWidth: typeof args.addWidth === "boolean" ? args.addWidth : undefined,
      addHeight: typeof args.addHeight === "boolean" ? args.addHeight : undefined,
    });
  }

  if (name === "inventor_add_diameter_dimension") {
    const viewIndex = typeof args.viewIndex === "number" ? Math.trunc(args.viewIndex) : undefined;
    const viewName = typeof args.viewName === "string" ? args.viewName.trim() : undefined;
    if (!viewIndex && !viewName) {
      throw new Error("viewIndex or viewName is required.");
    }
    return addDiameterDimension({
      viewIndex,
      viewName,
      circleIndex: typeof args.circleIndex === "number" ? Math.trunc(args.circleIndex) : undefined,
      textOffsetX: typeof args.textOffsetX === "number" ? args.textOffsetX : undefined,
      textOffsetY: typeof args.textOffsetY === "number" ? args.textOffsetY : undefined,
    });
  }

  if (name === "inventor_add_linear_dimension") {
    const viewIndex = typeof args.viewIndex === "number" ? Math.trunc(args.viewIndex) : undefined;
    const viewName = typeof args.viewName === "string" ? args.viewName.trim() : undefined;
    const fromCircleIndex = typeof args.fromCircleIndex === "number" ? Math.trunc(args.fromCircleIndex) : undefined;
    const toCircleIndex = typeof args.toCircleIndex === "number" ? Math.trunc(args.toCircleIndex) : undefined;
    const textOffsetX = typeof args.textOffsetX === "number" ? args.textOffsetX : undefined;
    const textOffsetY = typeof args.textOffsetY === "number" ? args.textOffsetY : undefined;
    return addLinearDimension({
      viewIndex,
      viewName,
      fromX: args.fromX != null ? Number(args.fromX) : 0,
      fromY: args.fromY != null ? Number(args.fromY) : 0,
      toX: args.toX != null ? Number(args.toX) : 0,
      toY: args.toY != null ? Number(args.toY) : 0,
      dimensionType: typeof args.dimensionType === "string" ? args.dimensionType : undefined,
      fromCircleIndex,
      toCircleIndex,
      textOffsetX,
      textOffsetY,
    });
  }

  if (name === "inventor_get_document_settings") {
    return getDocumentSettings();
  }

  if (name === "inventor_set_length_units") {
    return setLengthUnits({ units: String(args.units ?? "") });
  }

  if (name === "inventor_set_angle_units") {
    return setAngleUnits({ units: String(args.units ?? "") });
  }

  if (name === "inventor_set_mass_units") {
    return setMassUnits({ units: String(args.units ?? "") });
  }

  if (name === "inventor_set_dimension_precision") {
    return setDimensionPrecision({
      linearPrecision: typeof args.linearPrecision === "number" ? Math.trunc(args.linearPrecision) : undefined,
      angularPrecision: typeof args.angularPrecision === "number" ? Math.trunc(args.angularPrecision) : undefined,
    });
  }

  if (name === "inventor_set_modeling_display") {
    return setModelingDisplay({
      displayMode: typeof args.displayMode === "string" ? args.displayMode : undefined,
      showSketches: typeof args.showSketches === "boolean" ? args.showSketches : undefined,
      showWorkFeatures: typeof args.showWorkFeatures === "boolean" ? args.showWorkFeatures : undefined,
    });
  }

  return null;
}
