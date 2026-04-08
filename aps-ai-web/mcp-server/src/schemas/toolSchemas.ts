import { z } from "zod";

export const PropertySchema = z.object({
  displayName: z.string(),
  displayValue: z.string(),
  units: z.string().optional(),
});

export const ElementSchema = z.object({
  dbId: z.number(),
  name: z.string().optional(),
  externalId: z.string().optional(),
  properties: z.array(PropertySchema),
});

export const ReadSelectedElementsInput = z.object({
  selectedElements: z.array(ElementSchema),
  includeAll: z.boolean().optional().default(false),
  requestedFields: z.array(z.string()).optional().default([]),
});

export const GetElementParametersInput = z.object({
  selectedElements: z.array(ElementSchema),
  dbIds: z.array(z.number()).optional().default([]),
  parameterQueries: z.array(z.string()).optional().default([]),
});

export const SearchElementsInput = z.object({
  selectedElements: z.array(ElementSchema).optional().default([]),
  query: z.string(),
});

export const ListModelViewsInput = z.object({
  views: z
    .array(
      z.object({
        guid: z.string(),
        name: z.string(),
        role: z.string(),
      }),
    )
    .optional()
    .default([]),
});

export const FitOrIsolateInput = z.object({
  action: z.enum(["fit_to_view", "isolate_selection", "clear_selection"]),
  dbIds: z.array(z.number()).optional().default([]),
});

export const ProposeParameterWriteInput = z.object({
  confirmationToken: z.string().optional(),
  apply: z.boolean().optional().default(false),
  changes: z.array(
    z.object({
      dbId: z.number(),
      parameter: z.string(),
      value: z.string(),
    }),
  ),
});

export type ElementSnapshot = z.infer<typeof ElementSchema>;
