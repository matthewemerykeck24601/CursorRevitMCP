import { mkdtemp, rm } from "node:fs/promises";
import { tmpdir } from "node:os";
import path from "node:path";
import assert from "node:assert/strict";
import test from "node:test";
import { ExcelParameterSource } from "./ExcelParameterSource.js";

test("ExcelParameterSource can create, push, and pull via xlsx default export", async () => {
  const folder = await mkdtemp(path.join(tmpdir(), "inventor-excel-esm-"));
  try {
    const source = new ExcelParameterSource(path.join(folder, "parameters.xlsx"));
    const pushed = await source.push({
      parameterSetId: "set-1",
      parameters: [{ name: "PlateLength", expression: "12 in", value: 12 }],
    });
    assert.equal(pushed.updated, 1);

    const rows = await source.pull({ parameterSetId: "set-1" });
    assert.equal(rows.length, 1);
    assert.equal(rows[0]?.name, "PlateLength");
  } finally {
    await rm(folder, { recursive: true, force: true });
  }
});
