type Property = {
  displayName: string;
  displayValue: string;
  units?: string;
};

type ElementSnapshot = {
  dbId: number;
  name?: string;
  externalId?: string;
  properties: Property[];
};

function matchField(displayName: string, query: string): boolean {
  const a = displayName.toLowerCase();
  const b = query.toLowerCase();
  return a === b || a.includes(b) || b.includes(a);
}

export function mcpGetElementParameters(input: {
  selectedElements: ElementSnapshot[];
  dbIds?: number[];
  parameterQueries?: string[];
}) {
  const target =
    input.dbIds && input.dbIds.length > 0
      ? input.selectedElements.filter((e) => input.dbIds?.includes(e.dbId))
      : input.selectedElements;

  const queries =
    input.parameterQueries && input.parameterQueries.length > 0
      ? input.parameterQueries
      : ["CONTROL_MARK", "CONTROL_NUMBER", "DIM_*", "IDENTITY_DESCRIPTION"];

  return target.map((el) => {
    const parameters = queries.flatMap((query) => {
      if (query.includes("*")) {
        const prefix = query.replace("*", "").toLowerCase();
        return el.properties
          .filter((p) => p.displayName.toLowerCase().startsWith(prefix))
          .map((p) => ({ parameter: p.displayName, value: p.displayValue, units: p.units }));
      }
      const hit = el.properties.find((p) => matchField(p.displayName, query));
      return hit
        ? [{ parameter: hit.displayName, value: hit.displayValue, units: hit.units }]
        : [];
    });
    return { dbId: el.dbId, name: el.name, parameters };
  });
}
