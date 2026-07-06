import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { buildServer } from "./server.js";

// Keep the stdio server alive across unexpected async failures. Without these guards a
// single unhandled rejection (e.g. a transport/stdout write error, or a rejected fetch
// that escapes a handler) terminates the Node process — which the MCP client sees as
// "server transport closed unexpectedly … process exiting early" and a hard disconnect.
// We log to stderr (surfaced in the client's MCP log) and stay up instead of exiting.
process.on("unhandledRejection", (reason) => {
  process.stderr.write(
    `revit-2024-community-bridge-mcp unhandledRejection: ${
      reason instanceof Error ? reason.stack ?? reason.message : String(reason)
    }\n`,
  );
});

process.on("uncaughtException", (error) => {
  process.stderr.write(
    `revit-2024-community-bridge-mcp uncaughtException: ${error.stack ?? error.message}\n`,
  );
});

async function main() {
  const server = buildServer();
  const transport = new StdioServerTransport();
  await server.connect(transport);
}

main().catch((error) => {
  process.stderr.write(`revit-2024-community-bridge-mcp failed: ${String(error)}\n`);
  process.exit(1);
});
