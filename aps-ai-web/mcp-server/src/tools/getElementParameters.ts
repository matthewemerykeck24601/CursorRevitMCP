import { GetElementParametersInput } from "../schemas/toolSchemas.js";

const DEFAULT_PRIORITY = [
  "CONTROL_MARK",
  "CONTROL_NUMBER",
  "IDENTITY_DESCRIPTION",
  "CONSTRUCTION_PRODUCT",
  "DESIGN_NUMBER",
];

function matchesQuery(displayName: string, query: string): boolean {
  const a = displayName.toLowerCase();
  const b = query.toLowerCase();
  return a === b || a.includes(b) || b.includes(a);
}

export function getElementParameters(rawInput: unknown) {
  const input = GetElementParametersInput.parse(rawInput);
  const querySet = input.parameterQueries.length
    ? input.parameterQueries
    : DEFAULT_PRIORITY;

  const targetElements =
    input.dbIds.length > 0
      ? input.selectedElements.filter((e) => input.dbIds.includes(e.dbId))
      : input.selectedElements;

  const results = targetElements.map((el) => {
    const values = querySet.flatMap((query) => {
      if (query.includes("*")) {
        const prefix = query.replace("*", "").toLowerCase();
        return el.properties
          .filter((p) => p.displayName.toLowerCase().startsWith(prefix))
          .map((p) => ({
            parameter: p.displayName,
            value: p.displayValue,
            units: p.units,
          }));
      }

      const hit = el.properties.find((p) => matchesQuery(p.displayName, query));
      return hit
        ? [{ parameter: hit.displayName, value: hit.displayValue, units: hit.units }]
        : [];
    });

    return {
      dbId: el.dbId,
      name: el.name,
      parameters: values,
    };
  });

  return {
    count: results.length,
    results,
  };
}
