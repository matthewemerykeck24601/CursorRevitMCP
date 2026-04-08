import { FitOrIsolateInput } from "../schemas/toolSchemas.js";

export function fitOrIsolate(rawInput: unknown) {
  const input = FitOrIsolateInput.parse(rawInput);

  if (input.action === "fit_to_view") {
    return { action: "viewer.fitToView", applied: true };
  }
  if (input.action === "clear_selection") {
    return { action: "viewer.clearSelection", applied: true };
  }
  return {
    action: "viewer.isolateSelection",
    applied: true,
    dbIds: input.dbIds,
  };
}
