import { ListModelViewsInput } from "../schemas/toolSchemas.js";

export function listModelViews(rawInput: unknown) {
  const input = ListModelViewsInput.parse(rawInput);
  return {
    count: input.views.length,
    views: input.views,
  };
}
