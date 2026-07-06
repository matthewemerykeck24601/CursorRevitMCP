import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { buildServer } from "./server.js";

// Keep the stdio server alive across unexpected async failures. A single unhandled
// rejection (e.g. a rejected fetch that escapes a handler) would otherwise terminate the
// Node process, which the MCP client reports as a hard "transport closed" disconnect. We
// log to stderr (surfaced in the client's MCP log) and stay up instead of exiting.
process.on("unhandledRejection", (reason) => {
  process.stderr.write(
    `revit-2024-diag-mcp unhandledRejection: ${
      reason instanceof Error ? reason.stack ?? reason.message : String(reason)
    }\n`,
  );
});

process.on("uncaughtException", (error) => {
  process.stderr.write(
    `revit-2024-diag-mcp uncaughtException: ${error.stack ?? error.message}\n`,
  );
});

async function main() {
  const server = buildServer();
  const transport = new StdioServerTransport();
  await server.connect(transport);
}

main().catch((error) => {
  process.stderr.write(`revit-2024-diag-mcp failed: ${String(error)}\n`);
  process.exit(1);
});
