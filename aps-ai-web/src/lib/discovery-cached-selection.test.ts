import test from "node:test";
import assert from "node:assert/strict";
import {
  buildDiscoveryCachedSelection,
  subsetDiscoveryByDbIds,
} from "@/lib/discovery-cached-selection";
import type { AecdmElementListRow } from "@/lib/aecdm-get-elements";

const provenance = {
  modelUrn: "urn:test",
  hubId: "hub",
  projectId: "project",
  itemId: "item",
  publishedVersionId: "version",
};

function row(dbId: number, externalId?: string): AecdmElementListRow {
  return {
    dbId,
    externalId,
    family: "Family",
    type: "Type",
  };
}

test("buildDiscoveryCachedSelection preserves externalId index alignment", () => {
  const selection = buildDiscoveryCachedSelection(
    [row(101, "uid-101"), row(102), row(103, "uid-103")],
    provenance,
    "test selection",
  );

  assert.deepEqual(selection.dbIds, [101, 102, 103]);
  assert.deepEqual(selection.externalIds, ["uid-101", "", "uid-103"]);
});

test("subsetDiscoveryByDbIds does not shift externalIds onto missing rows", () => {
  const selection = buildDiscoveryCachedSelection(
    [row(101, "uid-101"), row(102), row(103, "uid-103")],
    provenance,
    "test selection",
  );
  const restoredSelection = { ...selection, element_preview: [] };

  const subset = subsetDiscoveryByDbIds(restoredSelection, [102, 103]);

  assert.deepEqual(subset.dbIds, [102, 103]);
  assert.deepEqual(subset.externalIds, ["", "uid-103"]);
  assert.deepEqual(
    subset.element_preview.map((element) => ({
      dbId: element.dbId,
      externalId: element.externalId,
    })),
    [
      { dbId: 102, externalId: "" },
      { dbId: 103, externalId: "uid-103" },
    ],
  );
});
