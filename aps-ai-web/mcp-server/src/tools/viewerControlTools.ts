import { z } from "zod";

export type ViewerControlContext = {
  onSelectElements?: (
    dbIds: Array<string | number>,
    opts: { clearFirst: boolean; zoomToSelection: boolean },
  ) => void;
};

const SelectParamsSchema = z.object({
  dbIds: z.array(z.union([z.string(), z.number()])).min(1),
  clearFirst: z.boolean().default(true),
  zoomToSelection: z.boolean().default(false),
});

export const selectElements = {
  name: "select_elements" as const,
  description:
    "Selects specific elements in the APS Viewer by their dbIds. Use this whenever the user says 'select', 'highlight', 'show', or 'find' elements. Do NOT add extra actions like isolate or zoom unless the user explicitly asks.",
  parameters: SelectParamsSchema,

  async handler(
    params: z.infer<typeof SelectParamsSchema>,
    context: ViewerControlContext,
  ) {
    const { dbIds, clearFirst = true, zoomToSelection = false } = params;

    if (context.onSelectElements) {
      context.onSelectElements(dbIds, { clearFirst, zoomToSelection });
    } else {
      console.warn(
        "onSelectElements callback not provided by host (e.g. stdio MCP). Selection is not applied in a live viewer — use returned dbIds in the web app or inject onSelectElements.",
      );
    }

    return {
      success: true,
      selectedCount: dbIds.length,
      message: `Successfully sent selection request for ${dbIds.length} element(s) to the viewer.`,
      dbIds: dbIds.slice(0, 20),
      clearFirst,
      zoomToSelection,
    };
  },
};

export async function runSelectElements(
  args: unknown,
  context: ViewerControlContext = {},
) {
  const parsed = selectElements.parameters.parse(args);
  return selectElements.handler(parsed, context);
}
