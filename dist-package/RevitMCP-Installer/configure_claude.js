// configure_claude.js
// Safely merges the "revit-2025-bridge-mcp" entry into Claude Desktop's config.
// Invoked by INSTALL.bat so the JSON is parsed/written by Node, never by batch
// string tools (a naive batch rewrite would destroy other mcpServers entries and
// the user's "preferences" block).
//
// All paths resolve at runtime from the current user's environment, so the same
// package works on any machine regardless of username or extraction location.
//
// The server is launched as:  <node-command>  <install>\mcp-server\dist\index.js
// (portable: the only runtime dependency is Node.js itself — no tsx/TypeScript).
//
// The node command to write is passed by INSTALL.bat as the first argument: it
// is either the literal "node" (a system install on PATH) or the absolute path
// to the portable node.exe the installer downloaded. JSON.stringify escapes the
// backslashes in an absolute Windows path automatically (so it is written with
// double-backslashes). If no argument is given, defaults to "node".
//
// Exit codes: 0 = configured, 1 = env missing, 2 = existing config not valid JSON
//             (left untouched on purpose), 3 = unexpected write/IO failure.

const fs = require("fs");
const path = require("path");

const localAppData = process.env.LOCALAPPDATA;
const appData = process.env.APPDATA;

if (!localAppData || !appData) {
  console.error("LOCALAPPDATA / APPDATA environment variables are not set.");
  process.exit(1);
}

// Node command to launch the server with. argv[2] is the resolved node command
// from INSTALL.bat; fall back to "node" if it was not supplied.
const nodeCommand = (process.argv[2] || "node").trim() || "node";

const serverDir = path.join(localAppData, "RevitMCP", "mcp-server");
const entry = path.join(serverDir, "dist", "index.js");

const claudeDir = path.join(appData, "Claude");
const configPath = path.join(claudeDir, "claude_desktop_config.json");

try {
  // Claude Desktop may not have been launched yet; create its config dir if needed.
  fs.mkdirSync(claudeDir, { recursive: true });

  let config = {};
  if (fs.existsSync(configPath)) {
    const raw = fs.readFileSync(configPath, "utf8").trim();
    if (raw.length > 0) {
      try {
        config = JSON.parse(raw);
      } catch (e) {
        // Never overwrite a config we can't parse — that would lose the user's
        // other MCP servers. Bail; INSTALL.bat prints the manual entry to add.
        console.error(
          "Existing claude_desktop_config.json is not valid JSON; leaving it unchanged.",
        );
        console.error(String(e));
        process.exit(2);
      }
    }
    // Back up the existing config before we touch it.
    fs.copyFileSync(configPath, configPath + ".bak");
    console.log("Backed up existing config to: " + configPath + ".bak");
  }

  if (typeof config !== "object" || config === null || Array.isArray(config)) {
    config = {};
  }
  if (
    typeof config.mcpServers !== "object" ||
    config.mcpServers === null ||
    Array.isArray(config.mcpServers)
  ) {
    config.mcpServers = {};
  }

  // Set/update ONLY our entry. Every other mcpServers entry — and every other
  // top-level key (preferences, coworkUserFilesPath, etc.) — is preserved.
  config.mcpServers["revit-2025-bridge-mcp"] = {
    command: nodeCommand,
    args: [entry],
    env: {},
  };

  fs.writeFileSync(configPath, JSON.stringify(config, null, 2) + "\n", "utf8");
  console.log("Claude Desktop configured: " + configPath);
  console.log('  command: ' + nodeCommand + '  ["' + entry + '"]');
  process.exit(0);
} catch (e) {
  console.error("Failed to configure Claude Desktop: " + String(e));
  process.exit(3);
}
