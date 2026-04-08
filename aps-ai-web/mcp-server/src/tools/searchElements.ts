import { SearchElementsInput } from "../schemas/toolSchemas.js";

export function searchElements(rawInput: unknown) {
  const input = SearchElementsInput.parse(rawInput);
  const q = input.query.toLowerCase().trim();

  const hits = input.selectedElements.filter((el) => {
    const nameHit = (el.name ?? "").toLowerCase().includes(q);
    const propHit = el.properties.some(
      (p) =>
        p.displayName.toLowerCase().includes(q) ||
        p.displayValue.toLowerCase().includes(q),
    );
    return nameHit || propHit;
  });

  return {
    query: input.query,
    count: hits.length,
    dbIds: hits.map((h) => h.dbId),
    hits: hits.map((h) => ({ dbId: h.dbId, name: h.name })),
  };
}
