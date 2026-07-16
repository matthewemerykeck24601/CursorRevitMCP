import assert from "node:assert/strict";
import { createServer } from "node:http";
import test from "node:test";

test("replication sends mapped Inventor parameters to the bridge", async () => {
  let requestedPath = "";
  let requestedBody: Record<string, unknown> = {};
  const server = createServer((request, response) => {
    const chunks: Buffer[] = [];
    request.on("data", (chunk: Buffer) => chunks.push(chunk));
    request.on("end", () => {
      requestedPath = request.url ?? "";
      requestedBody = JSON.parse(Buffer.concat(chunks).toString("utf8")) as Record<
        string,
        unknown
      >;
      response.writeHead(200, { "Content-Type": "application/json" });
      response.end(
        JSON.stringify({
          success: true,
          message: "ok",
          data: { success: true, updated: 2 },
        }),
      );
    });
  });

  await new Promise<void>((resolve, reject) => {
    server.once("error", reject);
    server.listen(0, "127.0.0.1", resolve);
  });

  try {
    const address = server.address();
    assert(address && typeof address === "object");
    process.env.INVENTOR_BRIDGE_URL = `http://127.0.0.1:${address.port}`;

    const { invokeRevitInteropTool } = await import("./revitInteropTools.js");
    const result = await invokeRevitInteropTool("inventor_replicate_from_revit_payload", {
      revitPayload: {
        source: "revit-public-mcp",
        familyName: "Plate",
        parameters: [
          { name: "DIM_LENGTH", unitType: "length", value: 100 },
          { name: "MANUFACTURE_COMPONENT", value: "CIP" },
        ],
      },
      runRuleAfterSet: "UpdateGeometry",
    });

    assert.equal(requestedPath, "/api/set_parameters");
    assert.equal(requestedBody.runRuleAfterSet, "UpdateGeometry");
    const parameters = requestedBody.parameters as Array<Record<string, unknown>>;
    assert.equal(
      parameters.find((parameter) => parameter.name === "PlateLength")?.value,
      100,
    );
    assert.equal(
      parameters.find((parameter) => parameter.name === "Route_CIP")?.value,
      true,
    );
    assert.equal(result?.mappedCount, parameters.length);
  } finally {
    await new Promise<void>((resolve, reject) => {
      server.close((error) => (error ? reject(error) : resolve()));
    });
  }
});
