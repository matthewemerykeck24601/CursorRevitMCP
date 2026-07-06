# APS MCP Server — Reconnaissance Report

**Target:** `D:\CursorRevitMCP`
**MCP server (Cursor display name):** `aps-ai-web-mcp`
**Server name string in code:** `aps-ai-web-mcp-server` (package + `new Server({ name: ... })`)
**Report date:** 2026-06-24
**Author:** automated recon (Claude Code)

> ⚠️ **Security finding up front:** `D:\CursorRevitMCP\aps-ai-web\.env.local` contains **live, unredacted secrets** (APS client ID/secret, OpenAI key, xAI key, Firebase shared secret, DA activity IDs). All secret values are **redacted** in this report. See §6 and §8.

---

## Step 1 — Directory Inventory

`D:\CursorRevitMCP` is **not a single MCP project** — it is a large multi-tool monorepo (Revit/Inventor MCP bridges, Design Automation appbundles, iOS app, a Monty AI server, ACC admin CSV exports, and several `.rvt` test models). The APS MCP server that Cursor exposes as **`aps-ai-web-mcp`** lives in **one subfolder**: `aps-ai-web\mcp-server`.

### Top-level layout (folders + notable root files)

| Item | Type | Notes |
|---|---|---|
| `.cursor\` | dir | **`mcp.json`** (project MCP registry — defines `aps-ai-web-mcp`), `rules\precast-revit-ontology.mdc` |
| `aps-ai-web\` | dir | **The APS app.** Contains the Next.js web app (`src\`) **and** the MCP server (`mcp-server\`). `.env.local`, `.env.example` here. |
| `aps-auto-delete-walls\` | dir | Separate DA experiment |
| `data\` | dir | Inventor params, misc data |
| `design-automation\` | dir | DA appbundle sources |
| `dist\`, `dist-package\` | dir | Build artifacts / packaged installer |
| `docs\`, `install\`, `schemas\` | dir | Docs, installer, schemas |
| `inventor-mcp\`, `InventorMCPBridge\` | dir | Inventor MCP (separate server) |
| `mcps\`, `MCPToolHelper\` | dir | Shared MCP helpers / precast sameness logic |
| `monty-ai-server\` | dir | Monty AI server (separate) |
| `revit-2024/2025/2026/2027-*-mcp\`, `RevitDiag2024Bridge\`, `RevitPublicMCPBridge\` | dir | Per-version Revit MCP bridges (separate servers) |
| `ios\` | dir | Monty iOS native app (APS PKCE OAuth) |
| `*.rvt` (5 files, ~327 MB) | file | Test Revit models (`2026test*.rvt`, `cursroRevitTest*.rvt`) |
| `admin_*.csv` (15 files) | file | ACC Data Connector exports (projects, users, companies, roles) — large (≈100 MB total) |
| `README.md`, `*.json`, `*.md`, `*.pdf`, `*.docx` | file | Playbooks, handoffs, scope docs |

**Repo-wide file mix** (excl. `node_modules`/`.git`): 3,665 `.sst` + 3,663 `.meta` (≈1 GB — a local object/cache store, mostly under `aps-ai-web\.design-files`), 823 `.cs` (Revit/DA C# addins & bridges), 611 `.js` + 136 `.ts` + 480 `.map` (TS/JS sources & builds), 412 `.json`, 73 `.xaml` (WPF), 68 `.dll`, 20 `.csv`, 5 `.rvt`.

### Entry point(s)

| Concern | Path |
|---|---|
| **MCP server entry (the one Cursor runs)** | `aps-ai-web\mcp-server\src\index.ts` |
| MCP server package | `aps-ai-web\mcp-server\package.json` → name `aps-ai-web-mcp-server`, `"type": "module"` |
| MCP server config | `aps-ai-web\mcp-server\tsconfig.json` |
| Companion web app | `aps-ai-web\` (Next.js; `src\app\api\...` routes, `src\lib\aps*.ts`) |
| Project MCP registry | `D:\CursorRevitMCP\.cursor\mcp.json` |

### `mcp-server` source tree

```
aps-ai-web/mcp-server/
├─ package.json            name: aps-ai-web-mcp-server  (dev: tsx src/index.ts)
├─ tsconfig.json
├─ config/
│  ├─ parameter-priority.json
│  ├─ Shared_Params_2015_v01.txt   (bundled shared-parameter catalog, 86 KB)
│  └─ vocabulary.precast.json
└─ src/
   ├─ index.ts             ← ENTRY: stdio transport + buildServer()
   ├─ server.ts            ← tool registry (ListTools / CallTool), 25 tools
   ├─ aps/
   │  ├─ client.ts         APS GET helper (developer.api.autodesk.com)
   │  └─ tokens.ts         ApsSession type + isExpired()
   ├─ schemas/toolSchemas.ts
   ├─ context/selectionProvider.ts
   ├─ lib/
   │  ├─ aec-elements-for-marks.ts   AEC GraphQL → mark grouping
   │  ├─ aecdmMarkGrouping.ts        AECDM REST rows → mark grouping
   │  ├─ apsForAecMarks.ts           AEC GraphQL project-id resolution
   │  ├─ daWorkitemsMcp.ts           DA v3 client (2-legged code:all)
   │  ├─ da-parameter-patch.ts       parameter patch/update builders
   │  ├─ parametersApiClient.ts      Autodesk Parameters API v1
   │  └─ sharedParametersCatalog.ts  bundled .txt loader
   └─ tools/                          (25 tool handlers — see Step 2)
```

---

## Step 2 — MCP Server Identification

- **SDK:** `@modelcontextprotocol/sdk` (TypeScript). Server constructed in `src/server.ts`:
  `new Server({ name: "aps-ai-web-mcp-server", version: "0.1.0" }, { capabilities: { tools: {} } })`.
- **Transport:** **stdio** (`StdioServerTransport`) — `src/index.ts` lines 1–7.
- **Resources / Prompts:** **None.** Only `tools` capability is declared; no `resources` or `prompts` handlers are registered.
- **Handlers:** `ListToolsRequestSchema` (static tool list) and `CallToolRequestSchema` (dispatch by name). Every tool returns `{ content: [{ type: "text", text: JSON.stringify(payload) }] }`.

### How it is registered / started

`D:\CursorRevitMCP\.cursor\mcp.json` → entry **`aps-ai-web-mcp`**:

```json
"aps-ai-web-mcp": {
  "command": "cmd",
  "args": ["/c", "npx", "-y", "tsx",
           "d:\\CursorRevitMCP\\aps-ai-web\\mcp-server\\src\\index.ts"]
}
```

- Run via **`tsx`** (TypeScript executed directly, no build step) under `cmd /c npx -y`.
- **No `env` block** in the Cursor entry → the server inherits the parent process environment. APS/DA env vars (`APS_CLIENT_ID`, `DA_*`, etc.) are **not** injected by Cursor here; `.env.local` is **not** auto-loaded by the MCP process (no dotenv import in `mcp-server`). DA-dependent tools therefore rely on those vars being present in the ambient environment, **or** on the caller passing `access_token` directly.
- The **user-level** config `C:\Users\mstri\.cursor\mcp.json` only registers `revit-2026-community-mcp` — `aps-ai-web-mcp` is **project-scoped** only.

### Complete tool catalog (25 tools)

| # | Tool name | Description (verbatim/condensed) | Required inputs |
|---|---|---|---|
| 1 | `get_selected_elements` | Read selected elements; optionally filter returned properties. | `selectedElements` |
| 2 | `get_element_parameters` | Return parameter values for selected elements using query aliases. | `selectedElements` |
| 3 | `get_cached_selection` | Return dbIds/externalIds/names for current selection (build cached_selection for DA). | `selectedElements` |
| 4 | `search_elements` | Search selected elements by name/property text → matching dbIds. | `query` |
| 5 | `list_model_views` | List already-available model views from caller context. | — |
| 6 | `viewer_action` | Return fit/isolate/clear actions for the caller to execute. | `action` |
| 7 | `select_elements` | Select elements in the APS Viewer by dbIds (host callback). | `dbIds` |
| 8 | `aec_query` | Query **AEC Data Model** (GraphQL, REST fallback) → flattened rows. | `accessToken`, `hubId`, `projectId` |
| 9 | `set_element_parameters_guarded` | Two-step guarded parameter write (preview → confirmation token). | `changes` |
| 10 | `shared_parameters_lookup` | Resolve shared params from bundled `.txt`; fallback to **Parameters API**. | `query` |
| 11 | `shared_parameters_catalog_stats` | Report catalog size/source path; `reload` to clear cache. | — |
| 12 | `analyze_products_and_mark` | Analyze Structural Framing (WPA/WPB/COLUMN), group identical pieces, assign `CONTROL_MARK`≥100. | `product_prefix` |
| 13 | `get_product_sameness_report` | Sameness analysis using tolerances + intersecting geometry. | `element_ids` |
| 14 | `assign_control_marks` | Apply `CONTROL_MARK` to verified identical pieces (guarded write). | `mark_groups` |
| 15 | `admin_add_users_to_projects` | Add emails to multiple **ACC** projects by project number. | `project_numbers`, `emails` |
| 16 | `create_revit_cloud_workshared_model` | Create a workshared Revit **cloud model** in ACC via DA `SaveAsCloudModel`. | — (template + hub context) |
| 17 | `analyze_pdf_for_form_template` | Analyze PDF → normalized **ACC Forms** template fields (proxies web app). | — |
| 18 | `create_form_template_from_pdf` | Create native account-level **ACC Forms (Forma)** template from analysis. | `analysis_id` |
| 19 | `get_elements_by_category` | Query **AEC Data Model REST** for design elements (Structural Framing/Metromont filters). | — (needs token+project+urn) |
| 20 | `inspect_published_selection` | Alias of #19 (matches web chat planner naming). | — |
| 21 | `get_element_properties` | Full params/metadata for dbIds — **STUB** (`TODO: real Model Derivative / AEC property fetch`). | `dbIds` |
| 22 | `analyze_published_model_aecdm_cache` | Experimental: AECDM REST pull → cache a stub analysis. | — |
| 23 | `analyze_published_model_and_cache` | Primary analyzer: AECDM REST first, AEC GraphQL fallback; cache for DA. | — (token+hub+project or urn) |
| 24 | `trigger_design_automation_mark_update` | **APS Design Automation** mark/parameter write (guarded by `confirm:true`). | — (cache_id or edits) |
| 25 | `get_cached_mark_analysis` | Return latest cached mark analysis for preview. | — |

> Most tools accept both snake_case and camelCase aliases (`access_token`/`accessToken`, `project_id`/`projectId`, `model_urn`/`urn`, `hub_id`/`hubId`).

---

## Step 3 — APS / ACC Integration Surface

**Base URL (everywhere):** `https://developer.api.autodesk.com`
**Regional DA bases:** `/da/us-east/v3` (default), `/da/eu/v3`, `/da/apac/v3` (via `DA_REGION`).

### APIs called by the **MCP server**

| APS API | Path(s) | File | Purpose |
|---|---|---|---|
| **Authentication v2** | `POST /authentication/v2/token` | `lib/daWorkitemsMcp.ts` | **2-legged** `client_credentials`, scope `code:all` → DA token |
| **AEC Data Model — REST** | `GET /aecdm/v1/projects/{projectId}/designs/{designId}/elements` | `tools/apsQueryTools.ts` | Query design elements by category/family/type |
| **AEC Data Model — GraphQL** | `POST /aec/graphql` | `tools/aecQuery.ts`, `lib/apsForAecMarks.ts`, `lib/aec-elements-for-marks.ts` | `elementsByProject`, project-id resolution, mark analysis |
| **AEC Data Model — legacy REST** | `GET /aecdatamodel/v1/hubs/{hubId}/projects/{projectId}[/items/{itemId}]` | `tools/aecQuery.ts` | Fallback if GraphQL fails |
| **Data Management v1** | `GET /data/v1/projects/{projectId}/folders/{folderId}/contents` | `tools/revitCloudModelTools.ts` | List folder children/items |
| | `GET /data/v1/projects/{projectId}/items/{itemId}/tip` | same | Resolve tip version |
| | `GET /data/v1/projects/{projectId}/versions/{versionId}` | same | Get storage (OSS) URN |
| **Project v1 (Data Mgmt)** | `GET /project/v1/hubs/{hubId}/projects` | `tools/revitCloudModelTools.ts`, `tools/adminProjectTools.ts` | List hub projects (paginated) |
| | `GET /project/v1/hubs/{hubId}/projects/{projectId}/topFolders` | `tools/revitCloudModelTools.ts` | Resolve target folder by path |
| **OSS v2** | `GET /oss/v2/buckets/{bucketKey}/objects/{objectKey}/signeds3download` | `tools/revitCloudModelTools.ts` | Signed S3 URL for template `.rvt` input |
| **Design Automation v3** | `POST /da/{region}/v3/workitems` | `lib/daWorkitemsMcp.ts`, `tools/revitCloudModelTools.ts` | Submit Revit workitem (mark update / create model) |
| | `GET /da/{region}/v3/activities/{activityId}` | `tools/revitCloudModelTools.ts` | Validate activity parameter profile |
| **ACC Admin v1** | `POST /construction/admin/v1/projects/{projectId}/users` | `tools/adminProjectTools.ts` | Add user to project (header `Region: US/EMEA`) |
| **Parameters API v1** | `POST /parameters/v1/accounts/{acct}/groups/{grp}/collections/{col}/parameters:search` | `lib/parametersApiClient.ts` | Shared-parameter search fallback |
| **ACC Forms v1** (indirect) | `POST {APS_AI_WEB_BASE_URL}/api/admin/forms/{analyze,create-template}` | `tools/formTemplateTools.ts` | MCP **proxies the web app**, which calls Forms API |

### Authentication model

- **3-legged (user OAuth):** The MCP tools themselves **do not run the OAuth login**. They expect a **3-legged user `access_token` passed in as a tool argument** (`access_token`/`accessToken`) — see `getApsToken()` in `apsQueryTools.ts`, `getToken()` in `adminProjectTools.ts`/`revitCloudModelTools.ts`. The Revit cloud-model DA payload explicitly carries `adsk3LeggedToken: accessToken`.
- **2-legged (client credentials):** Used **only** for Design Automation token minting in `daWorkitemsMcp.ts` (`grant_type=client_credentials`, scope `code:all`), reading `APS_CLIENT_ID` / `APS_CLIENT_SECRET` from env.
- **Where 3-legged login actually happens:** the **web app** (`aps-ai-web/src/lib/aps.ts`): `POST /authentication/v2/authorize` + `POST /authentication/v2/token` (`grant_type=authorization_code`, refresh via `refresh_token`), tokens stored in **HTTP-only cookies** (`aps_access_token`, `aps_refresh_token` 30-day, `aps_expires_at`). iOS native uses **PKCE** (`monty://autodesk-oauth`, `S256`) via `/api/auth/native-exchange`.

### App credentials / secret storage

| Item | Location | Notes |
|---|---|---|
| Client ID / secret | `aps-ai-web\.env.local` (live), `.env.example` (placeholder) | **Redacted** — see §6 |
| DA activity IDs, appbundle IDs | `.env.local` (`DA_ACTIVITY_ID*`, `DA_APPBUNDLE_ID_*`) | Live values present |
| Parameters API IDs | env `APS_PARAMETERS_ACCOUNT_ID/GROUP_ID/COLLECTION_ID` | Or passed per-call |
| Shared-params file | `mcp-server\config\Shared_Params_2015_v01.txt` or `SHARED_PARAMETERS_FILE_PATH` | Bundled catalog |
| Issues container | env `APS_ISSUES_CONTAINER_ID`, `APS_ISSUES_DEFAULT_TYPE_ID` | Used by web app only |

### ACC/Forma-specific surface

- **ACC Forms (Forma):** `analyze_pdf_for_form_template` + `create_form_template_from_pdf` (MCP) → web app `/api/admin/forms/*` → Forms API base `https://developer.api.autodesk.com/construction/forms/v1`, path `/accounts/{accountId}/form-templates`. PDF layout parsing via **Google Document AI** (optional).
- **ACC Admin:** `admin_add_users_to_projects` (project-user provisioning).
- **ACC Issues / RFIs / Submittals:** **No MCP tool.** Issues exist **only in the web app** (`src/lib/aps-issues.ts`: `GET`/`POST /issues/v2/containers/{containerId}/issues`). RFIs and Submittals: **none found anywhere.**
- **Data Connector** (`/data-connector/v1/...`): web-app only, read-mostly (default `ADMIN_DATA_CONNECTOR_READ_ONLY_LATEST_EXTRACTION=true`), used to seed admin role/project caches.

---

## Step 4 — Revit / BIM Data Access

### What model data is accessed

- **Element data via AEC Data Model** (`get_elements_by_category` / `inspect_published_selection`): pulls `id, category, family, type, properties` and maps Metromont fields (`External ID`, `CONTROL_MARK`, `DIM_LENGTH/HEIGHT/THICKNESS`, `Length/Width/Height`). Filters Structural Framing + WPA/WPB/CLA/COLUMN.
- **Mark/geometry analysis** (`analyze_products_and_mark`, `get_product_sameness_report`, `analyze_published_model_and_cache`): geometric bounds + intersecting elements → sameness groups → `CONTROL_MARK` starting at 100. Two engines: AECDM REST rows (`aecdmMarkGrouping.ts`) and AEC GraphQL (`aec-elements-for-marks.ts`).
- **Parameter writes** back to Revit go through **Design Automation** (`trigger_design_automation_mark_update`) — `modify_parameters`, `apply_marks`, `run_mark_analysis`, `apply_marks_and_modify`, `clear_cache`. Supports `parameter_patches`, `parameter_updates`, `cached_selection + updates`, and `sharedParameterGuidMap`.
- **`get_element_properties` is a STUB** — returns `{ CONTROL_MARK: null, note: "TODO: real Model Derivative / AEC property fetch" }`. **No live per-element property fetch from Model Derivative in the MCP server.**
- **Sheets / Views:** `list_model_views` only echoes views already supplied by the caller — no Model Derivative metadata/manifest call. Geometry extraction: none directly (delegated to DA/viewer).

### URN handling

- `resolveAecdmDesignId()` (`apsQueryTools.ts`): accepts `urn:` strings or base64; extracts `dm.lineage:` or `fs.file:vf.` design id; base64url-normalizes.
- `apsForAecMarks.ts`: builds DM project-id candidates (`urn:adsk.wipprod:dm.project:b.{id}`), resolves AEC project id via GraphQL `alternativeIdentifiers.dataManagementAPIProjectId`.
- `revitCloudModelTools.ts`: parses OSS object URNs (`urn:adsk.objects:os.object:`), folder URNs (`urn:adsk.wipprod:fs.folder:co.*`), item lineage (`urn:adsk.wipprod:dm.lineage:`), strips `b.` account prefixes.

### Design Automation / Design Collaboration

- **Design Automation API for Revit: present and central.** `daWorkitemsMcp.ts` submits workitems; activity routing by Revit version (`DA_ACTIVITY_ID_2024/2025/2026/2027`, `NET8`/`NET10` aliases). `create_revit_cloud_workshared_model` uses a `create_model` activity (`DA_ACTIVITY_ID_CREATE_MODEL`) with `revitmodel`/`toolinputs` data-URL args + bootstrap `inputFile` from a template `.rvt`.
- **DA gating:** workitems only POST when `DA_ENABLED=true`; otherwise returns a `da_stub`. `confirm:true` is required for mark updates.
- **ACC Design Collaboration / model coordination:** **not referenced** anywhere.

---

## Step 5 — Current Capability Map

Status legend: **working** = end-to-end implemented; **partial** = implemented but gated/depends on external setup or has stubbed pieces; **stub** = placeholder.

| Tool / Capability | APS API Used | Auth Type | Status |
|---|---|---|---|
| `get_elements_by_category` / `inspect_published_selection` | AEC Data Model REST `/aecdm/v1` | 3L (arg) | working |
| `aec_query` | AEC Data Model GraphQL `/aec/graphql` (+ legacy REST) | 3L (arg) | working |
| `analyze_published_model_and_cache` | AECDM REST + AEC GraphQL | 3L (arg) | working |
| `analyze_published_model_aecdm_cache` | AECDM REST | 3L (arg) | partial (caches stub analysis) |
| `get_element_properties` | (Model Derivative / AEC) | 3L (arg) | **stub** (TODO) |
| `analyze_products_and_mark` / `get_product_sameness_report` / `assign_control_marks` | local geometry logic (no direct APS call) | n/a / 3L | partial (host-data dependent) |
| `trigger_design_automation_mark_update` | Design Automation v3 `/da/.../workitems` | 2L (`code:all`) | partial (needs `DA_ENABLED`+activity) |
| `create_revit_cloud_workshared_model` | Data Mgmt `/data/v1`,`/project/v1`, OSS `/oss/v2`, DA v3 | 3L (arg) + 2L (DA) | partial (needs templates + `DA_ACTIVITY_ID_CREATE_MODEL`) |
| `get_cached_mark_analysis` | in-memory cache | n/a | working |
| `admin_add_users_to_projects` | ACC Admin `/construction/admin/v1`, Project `/project/v1` | 3L (arg) | working |
| `shared_parameters_lookup` | bundled `.txt` + Parameters API `/parameters/v1` | 3L (arg, fallback) | working |
| `shared_parameters_catalog_stats` | local file | n/a | working |
| `analyze_pdf_for_form_template` / `create_form_template_from_pdf` | ACC Forms `/construction/forms/v1` (via web app proxy) | 3L (arg) | partial (requires running web app + DocAI) |
| `select_elements` / `viewer_action` / `list_model_views` / `get_selected_elements` / `get_element_parameters` / `get_cached_selection` / `search_elements` | none (viewer/host context) | n/a | working (host-dependent) |
| `set_element_parameters_guarded` | none (returns preview/confirmation) | n/a | partial (preview only; write via DA) |

**Web app (companion, not MCP tools) — additional reach:** `/authentication/v2`, Data Management folders/items/versions (full tree walk + model discovery), **Model Derivative** `/modelderivative/v2/.../manifest|metadata`, **ACC Issues** `/issues/v2` (list+create), **OSS** bucket CRUD, **Data Connector** `/data-connector/v1` (read extractions).

---

## Step 6 — Config and Environment

### `aps-ai-web\.env.example` (template — safe)

```
APS_CLIENT_ID=your_aps_client_id
APS_CLIENT_SECRET=your_aps_client_secret
APS_CALLBACK_URL=http://localhost:3000/auth/callback
APS_NATIVE_REDIRECT_URI=monty://autodesk-oauth
APP_BASE_URL=http://localhost:3000
APS_SCOPE=data:read data:write data:create data:search user:read offline_access viewables:read account:read
AI_PROVIDER=xai ; AI_GATEWAY_MODE=direct|firebase_functions
APS_ISSUES_CONTAINER_ID= ; APS_ISSUES_DEFAULT_TYPE_ID= ; APS_DESIGN_FILES_BUCKET=
ADMIN_DATA_CONNECTOR_READ_ONLY_LATEST_EXTRACTION=true
ADMIN_ALLOW_DATA_CONNECTOR_REQUEST_CREATION=false
APS_FORMS_API_BASE_URL=https://developer.api.autodesk.com/construction/forms/v1
APS_FORMS_TEMPLATE_CREATE_PATH=/accounts/{accountId}/form-templates
GOOGLE_DOCAI_ENABLED=false ; GOOGLE_DOCAI_LOCATION=us
DA_ENABLED=false ; DA_REGION=us-east ; DA_ACTIVITY_ID=
# (+ optional DA_ACTIVITY_ID_2024..2027 / NET8 / NET10, poll tuning)
```

### `aps-ai-web\.env.local` (LIVE — values redacted)

```
APS_CLIENT_ID=<REDACTED>
APS_CLIENT_SECRET=<REDACTED>
APS_CALLBACK_URL=http://localhost:3000/auth/callback
APP_BASE_URL=http://localhost:3000
APS_SCOPE=data:read data:write data:create data:search bucket:create bucket:read
          bucket:update bucket:delete user:read offline_access viewables:read
          account:read account:write code:all
AI_PROVIDER=xai
OPENAI_API_KEY=<REDACTED>
XAI_API_KEY=<REDACTED>
XAI_MODEL=grok-4.20-multi-agent-0309
DA_ENABLED=true
DA_REGION=us-east
DA_ACTIVITY_ID=<clientId>.revitmarkworkitem+prod
DA_ACTIVITY_ID_CREATE_MODEL=<clientId>.revitmarkworkitem2025+create
DA_APPBUNDLE_ID_2025=revitmarkworkitem2025
DA_APPBUNDLE_ID_2026=metromont_revitmarkworkitem_2026
DA_APPBUNDLE_ID_2027=metromont_revitmarkworkitem_2027
DA_ACTIVITY_ID_2025/2026/2027 = <clientId>.revitmarkworkitem20XX+prod
DA_ACTIVITY_ID_NET10=<clientId>.revitmarkworkitem2027+prod
AI_GATEWAY_MODE=firebase_functions
AI_GATEWAY_FUNCTION_URL=https://us-central1-monty-33c09.cloudfunctions.net/aiGateway
AI_GATEWAY_SHARED_SECRET=<REDACTED>
GOOGLE_DOCAI_ENABLED=true
GOOGLE_DOCAI_KEY_FILE=C:\Users\mstri\AppData\Local\MontyAI\secrets\google_document_ai_key.json
GOOGLE_DOCAI_PROJECT_ID=castcam-deed5
GOOGLE_DOCAI_PROCESSOR_ID=<id>
```

Notes:
- `.env.local` requests a **broad scope** (adds `bucket:*`, `account:write`, `code:all`) vs the example's read-mostly scope.
- The **MCP server process does not load `.env.local`** itself (no dotenv in `mcp-server`). DA tools need these vars in the ambient environment to function from Cursor.

### Cursor MCP config entry for `aps-ai-web-mcp`

- **Project config** `D:\CursorRevitMCP\.cursor\mcp.json` — the registration shown in Step 2 (no `env` block).
- **User config** `C:\Users\mstri\.cursor\mcp.json` — only `revit-2026-community-mcp`; **`aps-ai-web-mcp` is not present at user scope.**
- `globalStorage` search returned no MCP json referencing `aps-ai-web-mcp` (Cursor reads the project `.cursor/mcp.json`).
- A `.cursor\mcp.json.example`, `claude_desktop_config.example.json`, and `mcp-prerequisites.json` also exist at root as templates.

---

## Step 7 — Gap Analysis for Monty

| Monty goal | Covered today? | Where it stands |
|---|---|---|
| **1. Read ACC project document libraries** (corpus ingestion) | **Partial — web app only** | The **web app** can walk the full ACC folder tree (`/data/v1/.../folders/.../contents`, `topFolders`), discover items/versions, and produce **signed S3 download URLs** (OSS `signeds3download`). **No MCP tool exposes generic folder/document browsing or file download** — the MCP server only lists folders internally while resolving a target folder for cloud-model creation. **Gap:** add MCP tools `list_project_folders` / `list_folder_items` / `download_document` to make this usable for Monty ingestion. |
| **2. Read Revit Cloud Model element data** | **Partial** | `get_elements_by_category`, `inspect_published_selection`, `aec_query`, `analyze_published_model_and_cache` already return element + property rows via AEC Data Model (REST + GraphQL), including URN→design-id resolution. **But** `get_element_properties` (the detailed per-element fetch) is a **stub**, and there is **no Model Derivative metadata/properties path** in the MCP layer (the web app has manifest/metadata routes). **Gap:** finish `get_element_properties` (AEC batch or Model Derivative `/metadata/{guid}/properties`). |
| **3. Create ACC issues programmatically** | **Partial / Missing at MCP layer** | Issue create+list **exists in the web app** (`aps-issues.ts`, `POST /issues/v2/containers/{containerId}/issues`, needs `APS_ISSUES_CONTAINER_ID`). **No MCP tool** calls it. **Gap:** add `create_acc_issue` / `list_acc_issues` MCP tools (thin wrappers over the existing web-app logic or direct `/issues/v2`), plus container-id resolution from project id. |
| **4. Read BIM360/ACC model coordination data** | **Missing** | No references to ACC **Model Coordination**, clash, or **Design Collaboration** APIs anywhere (MCP or web app). Only Design **Automation** is present. **Gap:** net-new integration (`/bim360/modelset`/coordination clash endpoints) if required. |

### Other observations relevant to Monty

- **Auth bridge needed:** MCP tools expect a 3-legged `access_token` passed per call. Monty would need to obtain/relay a user token (the web app's cookie-based session, or the iOS PKCE flow) into MCP tool arguments. There is **no token-acquisition tool** in the MCP server itself.
- **Forms pipeline depends on the running Next.js web app** (MCP proxies `APS_AI_WEB_BASE_URL/api/admin/forms/*`) and optionally Google Document AI — not standalone.
- **In-memory caches** (`analysisCache`, `queryCache`) reset on each stdio process start; not durable across Cursor restarts.
- **Security:** rotate the secrets currently committed in `aps-ai-web\.env.local` (APS client secret, OpenAI, xAI, Firebase shared secret) and ensure `.env.local` is git-ignored; confirm it is not present in `dist-package`/installer artifacts.

---

## Appendix — Key file references

- Entry / transport: `aps-ai-web\mcp-server\src\index.ts`
- Tool registry: `aps-ai-web\mcp-server\src\server.ts`
- AEC DM REST: `...\src\tools\apsQueryTools.ts`
- AEC DM GraphQL: `...\src\tools\aecQuery.ts`, `...\src\lib\apsForAecMarks.ts`, `...\src\lib\aec-elements-for-marks.ts`
- Design Automation: `...\src\lib\daWorkitemsMcp.ts`, `...\src\tools\precastDesignAutomationTools.ts`
- Cloud model create (Data Mgmt + OSS + DA): `...\src\tools\revitCloudModelTools.ts`
- ACC Admin: `...\src\tools\adminProjectTools.ts`
- ACC Forms: `...\src\tools\formTemplateTools.ts`
- Parameters API: `...\src\lib\parametersApiClient.ts`, `...\src\tools\sharedParametersTools.ts`
- Web-app OAuth + Issues + Data Mgmt: `aps-ai-web\src\lib\aps.ts`, `...\src\lib\aps-issues.ts`, `...\src\app\api\aps\...`
- Config registry: `D:\CursorRevitMCP\.cursor\mcp.json`; env: `aps-ai-web\.env.example`, `aps-ai-web\.env.local`
