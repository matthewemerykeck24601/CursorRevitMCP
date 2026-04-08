import { ReadSelectedElementsInput } from "../schemas/toolSchemas.js";

function matchRequestedField(displayName: string, requested: string[]): boolean {
  if (requested.length === 0) return false;
  const key = displayName.toLowerCase();
  return requested.some((r) => {
    const rk = r.toLowerCase();
    return key === rk || key.includes(rk) || rk.includes(key);
  });
}

export function readSelectedElements(rawInput: unknown) {
  const input = ReadSelectedElementsInput.parse(rawInput);
  const elements = input.selectedElements.map((el) => {
    const filtered =
      input.includeAll || input.requestedFields.length === 0
        ? el.properties
        : el.properties.filter((p) =>
            matchRequestedField(p.displayName, input.requestedFields),
          );
    return {
      dbId: el.dbId,
      name: el.name,
      externalId: el.externalId,
      properties: filtered,
    };
  });

  return {
    count: elements.length,
    elements,
  };
}
