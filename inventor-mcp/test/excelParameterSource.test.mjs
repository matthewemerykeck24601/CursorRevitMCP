import { mkdtemp, rm } from "node:fs/promises";
import { tmpdir } from "node:os";
import path from "node:path";
import assert from "node:assert/strict";
import test from "node:test";
import { ExcelParameterSource } from "../dist/data/ExcelParameterSource.js";

test("empty Excel pushes are rejected without deleting existing parameter rows", async () => {
  const folder = await mkdtemp(path.join(tmpdir(), "inventor-excel-source-"));
  try {
    const source = new ExcelParameterSource(path.join(folder, "parameters.xlsx"));
    await source.push({
      parameterSetId: "assembly-a",
      parameters: [
        { name: "Width", expression: "10 in" },
        { name: "Height", expression: "20 in" },
      ],
    });
    await source.push({
      parameterSetId: "assembly-b",
      parameters: [{ name: "Depth", expression: "30 in" }],
    });

    await assert.rejects(
      source.push({ parameterSetId: "assembly-a", parameters: [] }),
      /Refusing to replace an Excel parameter set/,
    );

    const assemblyA = await source.pull({ parameterSetId: "assembly-a" });
    const assemblyB = await source.pull({ parameterSetId: "assembly-b" });

    assert.deepEqual(
      assemblyA.map((row) => row.name).sort(),
      ["Height", "Width"],
    );
    assert.deepEqual(
      assemblyB.map((row) => row.name),
      ["Depth"],
    );
  } finally {
    await rm(folder, { recursive: true, force: true });
  }
});
