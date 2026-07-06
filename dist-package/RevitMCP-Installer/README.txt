============================================================================
 Revit 2025 MCP Bridge - Installer
============================================================================

This package connects Anthropic's Claude Desktop to Autodesk Revit 2025 so
that Claude can read from and drive your Revit models through a local bridge.

It contains two pieces:

  1. The MCP server - a small Node.js program Claude Desktop launches. It is
     shipped as pre-compiled JavaScript and runs with plain Node.js. There is
     NO TypeScript / tsx toolchain requirement on your machine.

  2. The Revit 2025 add-in (RevitPublicMCPBridge) - a gateway that loads
     inside Revit and listens on http://127.0.0.1:8767 for the MCP server.


----------------------------------------------------------------------------
 PREREQUISITES
----------------------------------------------------------------------------

  - Windows 10 or 11.
  - Autodesk Revit 2025 installed.
  - Claude Desktop installed (https://claude.ai/download).

  Node.js is installed automatically if needed. An internet connection is
  required during first install if Node.js is not already present on your
  machine.


----------------------------------------------------------------------------
 INSTALL STEPS
----------------------------------------------------------------------------

  1. Extract this entire zip to any folder (for example your Desktop). Keep
     all files together - do not run INSTALL.bat from inside the zip viewer.

  2. Right-click INSTALL.bat and choose "Run as administrator".
     (See the note under TROUBLESHOOTING about which user account runs it.)

  3. Wait for "Installation complete." Close the window.
     If Node.js was not already present, the installer downloads a portable
     copy automatically the first time - that step needs internet access and
     may take a moment.

  4. Fully restart Revit 2025 (close every open Revit window, then reopen).
     The add-in loads at startup.

  5. Fully quit and restart Claude Desktop (use File > Exit / quit from the
     system tray - simply closing the window is not enough).

  6. (Optional) Double-click VERIFY.bat to confirm every piece is in place.
     It prints PASS/FAIL for each check.

  7. In a new Claude Desktop conversation, ask Claude to "test the Revit
     bridge". With a Revit document open, it should report a healthy gateway.


----------------------------------------------------------------------------
 WHAT THE INSTALLER DOES
----------------------------------------------------------------------------

  - Resolves Node.js 18+ automatically:
        * If Node.js 18 or later is already on your PATH, it is used as-is.
        * Otherwise, if a portable copy was downloaded on a previous run, that
          is reused (%LOCALAPPDATA%\RevitMCP\nodejs\node.exe).
        * Otherwise a portable Node.js 20 LTS is downloaded silently and
          unpacked to %LOCALAPPDATA%\RevitMCP\nodejs (no admin rights needed).
    If the download fails (e.g. no internet), the installer stops and tells you
    to install Node.js manually from https://nodejs.org and re-run.

  - Copies the MCP server (compiled JS + its Node dependencies) to:
        %LOCALAPPDATA%\RevitMCP\mcp-server

  - Copies the Revit 2025 add-in DLL to:
        %APPDATA%\Autodesk\Revit\Addins\2025\RevitPublicMCPBridge-v2\
            RevitPublicMCPBridge.net8.v17.dll

  - Writes the Revit add-in manifest to:
        %APPDATA%\Autodesk\Revit\Addins\2025\RevitPublicMCPBridge.addin
    The manifest's <Assembly> line is set to the FULL absolute path of the DLL
    above, because Revit does not expand environment variables in a manifest.

  - Updates Claude Desktop's config:
        %APPDATA%\Claude\claude_desktop_config.json
    It adds (or refreshes) only the "revit-2025-bridge-mcp" entry. The
    "command" is whichever Node.js was resolved above - either "node" (a system
    install on PATH) or the absolute path to the portable node.exe. The "args"
    point at %LOCALAPPDATA%\RevitMCP\mcp-server\dist\index.js.
    Any other MCP servers you already have, and all other settings in that
    file, are preserved. The existing file is backed up to
    claude_desktop_config.json.bak first. If the existing file is not valid
    JSON, the installer leaves it untouched and prints the entry for you to
    add by hand.

  Nothing runs as a background Windows service. Claude Desktop starts the MCP
  server process itself based on the config above, so once the config is
  correct there is nothing to start manually.


----------------------------------------------------------------------------
 TROUBLESHOOTING
----------------------------------------------------------------------------

  "Node.js auto-install failed"
  -----------------------------
  The installer could not download portable Node.js (usually no internet, a
  proxy/firewall, or the download site being blocked). Either connect to the
  internet and re-run INSTALL.bat, or install Node.js 18+ yourself from
  https://nodejs.org and re-run. A manually installed Node.js on PATH is
  detected and used automatically - no portable copy is downloaded then.

  Claude Desktop did not pick up the bridge / config not updated
  --------------------------------------------------------------
  - You must FULLY quit Claude Desktop (File > Exit, or quit from the tray) and
    reopen it. Closing the window leaves it running with the old config.
  - Open %APPDATA%\Claude\claude_desktop_config.json and confirm it contains a
    "revit-2025-bridge-mcp" block. Its "command" is either "node" or the full
    path to %LOCALAPPDATA%\RevitMCP\nodejs\node.exe. If it is missing, the
    installer printed a manual entry to add - paste it under "mcpServers".
  - If the file would not parse, the installer made no changes (to avoid wiping
    your other servers). Fix the JSON, or add the printed entry by hand.
  - A backup of the previous config is at
    %APPDATA%\Claude\claude_desktop_config.json.bak

  Revit add-in not loading
  ------------------------
  - Fully restart Revit 2025. The add-in only loads at Revit startup.
  - If the DLL copy failed during install, Revit was probably running - close
    Revit completely and re-run INSTALL.bat.
  - Confirm these exist:
        %APPDATA%\Autodesk\Revit\Addins\2025\RevitPublicMCPBridge.addin
        %APPDATA%\Autodesk\Revit\Addins\2025\RevitPublicMCPBridge-v2\
            RevitPublicMCPBridge.net8.v17.dll
    Open the .addin file and confirm its <Assembly> line is the FULL path to
    that DLL (not a relative path and not containing %APPDATA%).
  - If Revit reports an add-in error at startup, check Windows Event Viewer
    (Windows Logs > Application) for entries from Revit / the .NET runtime for
    the specific load failure.

  Server not connecting (bridge shows offline)
  --------------------------------------------
  - Make sure a Revit 2025 document is open; the gateway needs an active
    document for most tools. The add-in listens on http://127.0.0.1:8767 -
    you can open that URL's /health path in a browser:
        http://127.0.0.1:8767/health
  - Path-with-spaces issue: if your Windows user profile path contains spaces,
    make sure the "args" path (and the "command" path, if a portable Node.js
    was used) in claude_desktop_config.json are each wrapped in double quotes.
    The installer writes them correctly as single JSON strings; a hand-edited
    entry must keep each whole path inside one pair of quotes.
  - Re-run VERIFY.bat. A [FAIL] line tells you exactly which piece is missing.

  "Run as administrator" and your user profile
  ---------------------------------------------
  The installer writes to per-user folders (%LOCALAPPDATA% and %APPDATA%) and
  does not strictly need administrator rights. If you DO run it as
  administrator, make sure it elevates as the SAME Windows account you use day
  to day - if Windows elevates a different admin account, the files (including
  any portable Node.js) land in that other account's profile and Revit/Claude
  (running as you) will not see them. On a normal single-user PC the standard
  UAC "Yes" prompt keeps your account, which is correct.

============================================================================
