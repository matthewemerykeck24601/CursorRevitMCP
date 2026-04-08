import { NextRequest, NextResponse } from "next/server";
import { requireSession } from "@/lib/auth-guard";
import { apsPost, resolveAecProjectId } from "@/lib/aps";

export const runtime = "nodejs";

type ProductRow = {
  id: string;
  name: string;
  externalId: string;
  controlMark: string;
  controlNumber: string;
  designNumber: string;
  category: string;
  familyName: string;
  typeName: string;
  identityDescription: string;
  constructionProduct: string;
  length: string;
  width: string;
  height: string;
};

type GraphProperty = {
  name?: string;
  value?: unknown;
};

type GraphElement = {
  id?: string;
  name?: string;
  properties?: { results?: GraphProperty[] };
};

type GraphResponse = {
  data?: {
    elementsByProject?: {
      results?: GraphElement[];
    };
  };
  errors?: Array<{ message?: string }>;
};

function getPropertyValue(properties: GraphProperty[] | undefined, name: string): string {
  const hit = (properties ?? []).find(
    (p) => String(p.name ?? "").toLowerCase() === name.toLowerCase(),
  );
  return String(hit?.value ?? "");
}

function mapProduct(element: GraphElement): ProductRow {
  const props = element.properties?.results ?? [];
  return {
    id: String(element.id ?? ""),
    name: String(element.name ?? ""),
    externalId: getPropertyValue(props, "External ID"),
    controlMark: getPropertyValue(props, "CONTROL_MARK"),
    controlNumber: getPropertyValue(props, "CONTROL_NUMBER"),
    designNumber: getPropertyValue(props, "DESIGN_NUMBER"),
    category: getPropertyValue(props, "Category"),
    familyName: getPropertyValue(props, "Family Name"),
    typeName: getPropertyValue(props, "Type Name"),
    identityDescription: getPropertyValue(props, "IDENTITY_DESCRIPTION"),
    constructionProduct: getPropertyValue(props, "CONSTRUCTION_PRODUCT"),
    length: getPropertyValue(props, "Length"),
    width: getPropertyValue(props, "Width"),
    height: getPropertyValue(props, "Height"),
  };
}

function isBeamOrDoubleTFamily(familyName: string): boolean {
  const f = familyName.toLowerCase();
  return f.includes("beam") || f.includes("double t") || f.includes("doublet");
}

function isProductByRule(product: ProductRow): boolean {
  const categoryOk = product.category.toLowerCase().includes("structural framing");
  if (!categoryOk) return false;

  const family = product.familyName || "";
  const isSpecialFamily = isBeamOrDoubleTFamily(family);
  const hasWarped = family.toLowerCase().includes("warped");
  if (isSpecialFamily && !hasWarped) return false;

  return true;
}

const FILTERED_QUERY = `
  query StructuralFramingProducts($projectId: ID!) {
    elementsByProject(
      projectId: $projectId
      filter: { query: "property.name.category=='Structural Framing' and 'property.name.Element Context'=='Instance'" }
      pagination: { limit: 200 }
    ) {
      results {
        id
        name
        properties(
          filter: { names: ["External ID", "CONTROL_MARK", "CONTROL_NUMBER", "DESIGN_NUMBER", "Category", "Family Name", "Type Name", "IDENTITY_DESCRIPTION", "CONSTRUCTION_PRODUCT", "Length", "Width", "Height"] }
        ) {
          results {
            name
            value
          }
        }
      }
    }
  }
`;

const FALLBACK_QUERY = `
  query StructuralFramingFallback($projectId: ID!) {
    elementsByProject(projectId: $projectId, pagination: { limit: 300 }) {
      results {
        id
        name
        properties(
          filter: { names: ["External ID", "CONTROL_MARK", "CONTROL_NUMBER", "DESIGN_NUMBER", "Category", "Family Name", "Type Name", "IDENTITY_DESCRIPTION", "CONSTRUCTION_PRODUCT", "Length", "Width", "Height"] }
        ) {
          results {
            name
            value
          }
        }
      }
    }
  }
`;

export async function GET(
  request: NextRequest,
  context: { params: Promise<{ projectId: string }> },
) {
  const auth = await requireSession(request);
  if (!auth.ok) return auth.response;

  const { projectId } = await context.params;
  const hubId = request.nextUrl.searchParams.get("hubId") ?? "";
  const diagnostics: string[] = [];
  const aecProjectId = await resolveAecProjectId(
    auth.session.accessToken,
    hubId,
    projectId,
  );

  let elements: GraphElement[] = [];
  try {
    const filtered = await apsPost<GraphResponse>("/aec/graphql", auth.session.accessToken, {
      query: FILTERED_QUERY,
      variables: { projectId: aecProjectId },
    });
    if (filtered.errors?.length) {
      diagnostics.push(
        `filtered query errors: ${filtered.errors.map((e) => e.message ?? "").join("; ")}`,
      );
    }
    elements = filtered.data?.elementsByProject?.results ?? [];
  } catch (error) {
    diagnostics.push(
      `filtered query failed: ${error instanceof Error ? error.message : "unknown error"}`,
    );
  }

  if (elements.length === 0) {
    try {
      const fallback = await apsPost<GraphResponse>(
        "/aec/graphql",
        auth.session.accessToken,
        {
          query: FALLBACK_QUERY,
          variables: { projectId: aecProjectId },
        },
      );
      if (fallback.errors?.length) {
        diagnostics.push(
          `fallback query errors: ${fallback.errors.map((e) => e.message ?? "").join("; ")}`,
        );
      }
      const all = fallback.data?.elementsByProject?.results ?? [];
      elements = all.filter((el) => {
        const props = el.properties?.results ?? [];
        const category = getPropertyValue(props, "Category");
        return category.toLowerCase().includes("structural framing");
      });
    } catch (error) {
      diagnostics.push(
        `fallback query failed: ${error instanceof Error ? error.message : "unknown error"}`,
      );
    }
  }

  const products = elements
    .map(mapProduct)
    .filter(isProductByRule)
    .sort((a, b) => {
      const aCp = a.constructionProduct.trim();
      const bCp = b.constructionProduct.trim();
      if (aCp && !bCp) return -1;
      if (!aCp && bCp) return 1;
      return (a.controlMark || a.name).localeCompare(b.controlMark || b.name, undefined, {
        numeric: true,
        sensitivity: "base",
      });
    })
    .filter((p) => p.id || p.externalId || p.controlMark)
    .slice(0, 300);

  const response = NextResponse.json({
    projectId,
    aecProjectId,
    count: products.length,
    products,
    productRule:
      "Category must be Structural Framing; Beam/Double T families require WARPED in family name; CONSTRUCTION_PRODUCT preferred as identifier.",
    diagnostics,
  });
  for (const cookie of auth.response.cookies.getAll()) {
    response.cookies.set(cookie);
  }
  return response;
}
