import { z } from "zod";

const Input = z.object({
  selectedElements: z.array(z.record(z.string(), z.unknown())).default([]),
  dbIds: z.array(z.number()).optional(),
});

/** Compact selection snapshot for Design Automation externalIds (same contract as web GET_CACHED_SELECTION). */
export function getCachedSelection(rawInput: unknown) {
  const input = Input.parse(rawInput ?? {});
  const elements = input.selectedElements.slice(0, 200).map((el) => {
    const rec = el as Record<string, unknown>;
    return {
      dbId: typeof rec.dbId === "number" ? rec.dbId : Number(rec.dbId) || 0,
      externalId: String(rec.externalId ?? "").trim(),
      name: String(rec.name ?? "").trim(),
    };
  });
  const dbIds =
    input.dbIds?.length && input.dbIds.length > 0
      ? input.dbIds.slice(0, 200)
      : elements.map((e) => e.dbId).filter((n) => n > 0);

  return {
    success: true,
    count: input.selectedElements.length,
    dbIds,
    elements,
    note: "Use externalId in parameter_patches / parameter_updates for skip_analysis DA payloads.",
  };
}
