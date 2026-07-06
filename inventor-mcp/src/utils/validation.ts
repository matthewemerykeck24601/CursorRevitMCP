import type {
  InventorParameterRecord,
  RevitFamilyParameterPayload,
  RevitFamilyParameterRecord,
} from "../types/contracts.js";

export function assertObject(value: unknown, label: string): Record<string, unknown> {
  if (typeof value !== "object" || value === null || Array.isArray(value)) {
    throw new Error(`${label} must be an object.`);
  }
  return value as Record<string, unknown>;
}

export function normalizeRevitPayload(raw: unknown): RevitFamilyParameterPayload {
  const input = assertObject(raw, "revitPayload");
  const rowsRaw = Array.isArray(input.parameters) ? input.parameters : [];
  const parameters = rowsRaw.map(normalizeRevitParameter).filter((x) => x.name.length > 0);

  return {
    source:
      input.source === "revit-public-mcp" ||
      input.source === "revit-2026-community-mcp" ||
      input.source === "revit-2027-community-mcp"
        ? input.source
        : "unknown",
    familyName: asString(input.familyName),
    typeName: asString(input.typeName),
    parameters,
  };
}

export function mapRevitToInventorParameters(
  payload: RevitFamilyParameterPayload,
): InventorParameterRecord[] {
  const mapped: InventorParameterRecord[] = [];
  const byUpperName = new Map<string, RevitFamilyParameterRecord>();

  for (const p of payload.parameters) {
    byUpperName.set(p.name.trim().toUpperCase(), p);
    pushMappedParameter(mapped, {
      name: p.name,
      units: mapUnitHint(p.unitType),
      expression: p.formula,
      value: p.value,
      isKey: p.isInstance === false,
      comment: `Imported from Revit (${payload.familyName ?? "unknown family"})`,
    });
  }

  // Canonical aliases for manufacturing parity naming in Inventor.
  upsertAlias(mapped, byUpperName, "DIM_LENGTH", "PlateLength");
  upsertAlias(mapped, byUpperName, "DIM_WIDTH", "PlateWidth");
  upsertAlias(mapped, byUpperName, "DIM_THICKNESS", "PlateThickness");
  upsertAlias(mapped, byUpperName, "HOLE_DIAMETER", "Plate_Hole_Diameter");
  upsertAlias(mapped, byUpperName, "SLOT_DIAMETER", "Plate_Slot_Diameter");
  upsertAlias(mapped, byUpperName, "SLOT_LENGTH", "Plate_Slot_Length");

  const manufactureComponent =
    getParamStringValue(byUpperName, "MANUFACTURE_COMPONENT") ?? "";
  const identityShort =
    getParamStringValue(byUpperName, "IDENTITY_DESCRIPTION_SHORT") ?? "";
  const identityLong = getParamStringValue(byUpperName, "IDENTITY_DESCRIPTION") ?? "";

  const routeCip = /\bCIP\b/i.test(manufactureComponent);
  const routeErection = /\bERECTION\b/i.test(manufactureComponent);

  const finishSignals = inferFinishSignals(payload.parameters);
  const finishGalvanized = finishSignals.galvanized;
  const finishPaintedBlack = finishSignals.paintedBlack;
  const finishUnfinished = !finishGalvanized && !finishPaintedBlack;

  const baseShort =
    identityShort.length > 0 ? identityShort : payload.familyName ?? "STEEL PLATE";
  const baseLong =
    identityLong.length > 0
      ? identityLong
      : `${payload.familyName ?? "STEEL PLATE"} (${deriveRoutingToken(routeCip, routeErection)}, ${deriveFinishToken(
          finishGalvanized,
          finishPaintedBlack,
        )})`;

  // Selector parity parameters for iLogic-heavy orchestration.
  pushMappedParameter(mapped, {
    name: "Route_CIP",
    value: routeCip,
    comment: "Derived route selector from MANUFACTURE_COMPONENT",
  });
  pushMappedParameter(mapped, {
    name: "Route_Erection",
    value: routeErection,
    comment: "Derived route selector from MANUFACTURE_COMPONENT",
  });
  pushMappedParameter(mapped, {
    name: "Finish_Galvanized",
    value: finishGalvanized,
    comment: "Derived finish selector from Revit finish signals",
  });
  pushMappedParameter(mapped, {
    name: "Finish_PaintedBlack",
    value: finishPaintedBlack,
    comment: "Derived finish selector from Revit finish signals",
  });
  pushMappedParameter(mapped, {
    name: "Finish_Unfinished",
    value: finishUnfinished,
    comment: "Derived finish fallback selector",
  });

  // Output/value parity fields that iLogic may overwrite with computed tokens.
  pushMappedParameter(mapped, {
    name: "MANUFACTURE_COMPONENT",
    value: manufactureComponent,
    comment: "Manufacturing routing output candidate",
  });
  pushMappedParameter(mapped, {
    name: "IDENTITY_DESCRIPTION_SHORT",
    value: baseShort,
    comment: "Description short output candidate",
  });
  pushMappedParameter(mapped, {
    name: "IDENTITY_DESCRIPTION",
    value: baseLong,
    comment: "Description long output candidate",
  });

  return mapped;
}

function normalizeRevitParameter(raw: unknown): RevitFamilyParameterRecord {
  const input = assertObject(raw, "parameter row");
  return {
    name: asString(input.name) ?? "",
    dataType: asString(input.dataType),
    unitType: asString(input.unitType),
    group: asString(input.group),
    formula: asString(input.formula),
    isInstance: asBoolean(input.isInstance),
    value: asValue(input.value),
  };
}

function mapUnitHint(unitType?: string): string | undefined {
  if (!unitType) {
    return undefined;
  }
  const normalized = unitType.toLowerCase();
  if (normalized.includes("length")) {
    return "mm";
  }
  if (normalized.includes("area")) {
    return "mm^2";
  }
  if (normalized.includes("volume")) {
    return "mm^3";
  }
  if (normalized.includes("angle")) {
    return "deg";
  }
  return undefined;
}

function asString(value: unknown): string | undefined {
  return typeof value === "string" && value.trim().length > 0 ? value.trim() : undefined;
}

function asBoolean(value: unknown): boolean | undefined {
  return typeof value === "boolean" ? value : undefined;
}

function asValue(value: unknown): string | number | boolean | undefined {
  if (
    typeof value === "string" ||
    typeof value === "number" ||
    typeof value === "boolean"
  ) {
    return value;
  }
  return undefined;
}

function pushMappedParameter(
  list: InventorParameterRecord[],
  next: InventorParameterRecord,
): void {
  const index = list.findIndex((x) => x.name.toLowerCase() === next.name.toLowerCase());
  if (index >= 0) {
    list[index] = {
      ...list[index],
      ...next,
    };
    return;
  }
  list.push(next);
}

function upsertAlias(
  list: InventorParameterRecord[],
  source: Map<string, RevitFamilyParameterRecord>,
  fromName: string,
  toName: string,
): void {
  const from = source.get(fromName.toUpperCase());
  if (!from) {
    return;
  }
  pushMappedParameter(list, {
    name: toName,
    units: mapUnitHint(from.unitType),
    expression: from.formula,
    value: from.value,
    isKey: from.isInstance === false,
    comment: `Alias from Revit ${from.name}`,
  });
}

function getParamStringValue(
  source: Map<string, RevitFamilyParameterRecord>,
  name: string,
): string | undefined {
  const row = source.get(name.toUpperCase());
  if (!row) {
    return undefined;
  }
  if (typeof row.value === "string") {
    return row.value.trim();
  }
  return undefined;
}

function inferFinishSignals(
  parameters: RevitFamilyParameterRecord[],
): { galvanized: boolean; paintedBlack: boolean } {
  let galvanized = false;
  let paintedBlack = false;

  for (const p of parameters) {
    const key = p.name.trim().toUpperCase();
    const valueText = typeof p.value === "string" ? p.value.toUpperCase() : "";
    const isTruthy = p.value === true || p.value === 1 || valueText === "TRUE" || valueText === "1";

    if (key.includes("GALV")) {
      galvanized = galvanized || isTruthy || valueText.includes("GALV");
    }
    if (key.includes("BLACK") || key.includes("PAINT")) {
      paintedBlack = paintedBlack || isTruthy || valueText.includes("BLACK") || valueText.includes("PAINT");
    }
    if (key.includes("FINISH") && valueText.includes("GALV")) {
      galvanized = true;
    }
    if (key.includes("FINISH") && (valueText.includes("BLACK") || valueText.includes("PAINT"))) {
      paintedBlack = true;
    }
  }

  return { galvanized, paintedBlack };
}

function deriveRoutingToken(routeCip: boolean, routeErection: boolean): string {
  if (routeCip && !routeErection) {
    return "CIP";
  }
  if (routeErection && !routeCip) {
    return "ERECTION";
  }
  if (routeCip && routeErection) {
    return "CIP+ERECTION";
  }
  return "UNSPECIFIED";
}

function deriveFinishToken(finishGalvanized: boolean, finishPaintedBlack: boolean): string {
  if (finishGalvanized && finishPaintedBlack) {
    return "GALVANIZED+BLACK";
  }
  if (finishGalvanized) {
    return "GALVANIZED";
  }
  if (finishPaintedBlack) {
    return "PAINTED_BLACK";
  }
  return "UNFINISHED";
}
