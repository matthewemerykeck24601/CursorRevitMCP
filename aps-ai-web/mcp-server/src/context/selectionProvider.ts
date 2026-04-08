import type { ElementSnapshot } from "../schemas/toolSchemas.js";

export type SelectionContext = {
  selectedElements: ElementSnapshot[];
  selectedDbIds: number[];
  modelViews?: Array<{ guid: string; name: string; role: string }>;
};

export function fromChatContext(raw: unknown): SelectionContext {
  const obj = (raw ?? {}) as {
    selectedElements?: ElementSnapshot[];
    selectedDbIds?: number[];
    modelViews?: Array<{ guid: string; name: string; role: string }>;
  };
  return {
    selectedElements: Array.isArray(obj.selectedElements) ? obj.selectedElements : [],
    selectedDbIds: Array.isArray(obj.selectedDbIds) ? obj.selectedDbIds : [],
    modelViews: Array.isArray(obj.modelViews) ? obj.modelViews : [],
  };
}
