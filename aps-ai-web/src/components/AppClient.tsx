"use client";

import {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
  type KeyboardEvent,
  type ReactNode,
} from "react";
import {
  ViewerPanel,
  type SelectedElementSnapshot,
  type ViewerAction,
  type ViewerPanelHandle,
} from "@/components/ViewerPanel";
import { DesignViewSplit3D } from "@/components/DesignViewSplit3D";
import {
  BIM_TABLE_HEADERS,
  bimRowsToCsv,
  buildBimTableRows,
  type BimTableRow,
} from "@/lib/bimSelectionTable";
import {
  extractFromForgeObjectName,
  isUsableRevitElementId,
} from "@/lib/forge-revit-object-name";
import type { DiscoveryCachedSelection } from "@/lib/discovery-cached-selection";
import { aiBrowserDebug } from "@/lib/ai-browser-debug";

type Hub = { id: string; name: string; type: string };
type Project = { id: string; name: string; type: string };
type Model = {
  itemId: string;
  versionId: string;
  viewerUrn: string;
  name: string;
  sourceFolder: string;
  extensionType: string;
};
type DaExecutionContext = {
  resolvedActivityId?: string;
  resolvedActivitySource?: string;
  revitVersionMajor?: number;
  cloudModel?: {
    region: string;
    projectGuid: string;
    modelGuid: string;
    revitVersionMajor?: number;
  };
};
type ProjectBrowserNode = {
  id: string;
  name: string;
  kind: "folder" | "file";
  parentId: string | null;
  itemId?: string;
  extensionType?: string;
  versionId?: string;
  viewerUrn?: string;
  children: ProjectBrowserNode[];
  childrenLoaded?: boolean;
};

type SessionResponse = {
  authenticated: boolean;
  expiresAt: number;
  scope: string;
};
type WorkspaceTab = "folders" | "viewer" | "model-data" | "element-data" | "issues";
type WorkspaceMode = "model" | "product-analysis" | "admin" | "form-builder";
type ModelDataRow = { key: string; value: string; source: string };
type FormTemplateType = "custom" | "inspection" | "quality" | "safety" | "daily_report";
type FormBuilderFieldType =
  | "text"
  | "multiline_text"
  | "number"
  | "checkbox"
  | "radio"
  | "dropdown"
  | "date"
  | "signature"
  | "section_heading"
  | "table";
type FormBuilderField = {
  id: string;
  sourceName: string;
  label: string;
  type: FormBuilderFieldType;
  required: boolean;
  options: string[];
  confidence: number;
  reviewState: "auto" | "needs_review" | "approved" | "rejected";
  notes: string[];
  readOnly?: boolean;
  calculated?: boolean;
  formula?: string;
  groupKey?: string;
  columnKey?: string;
  rowIndex?: number;
};
type FormaTableColumn = {
  key: string;
  label: string;
  type: FormBuilderFieldType;
  calculated: boolean;
  formula?: string;
};
type FormaElement =
  | {
      kind: "field";
      fieldId: string;
      label: string;
      type: FormBuilderFieldType;
      required: boolean;
      options: string[];
    }
  | {
      kind: "section";
      sectionId: string;
      title: string;
      entryMode: "single" | "multiple";
      fieldIds: string[];
      rationale: string;
    }
  | {
      kind: "table";
      tableId: string;
      title: string;
      columns: FormaTableColumn[];
      fieldIds: string[];
      hasCalculatedColumns: boolean;
      rationale: string;
    };
type FormaSchema = {
  elements: FormaElement[];
  sectionCount: number;
  tableCount: number;
  calculatedColumnCount: number;
  notes: string[];
};
type FormBuilderAnalysis = {
  analysisId: string;
  accountId: string;
  templateName: string;
  templateType: FormTemplateType;
  extractedTextSnippet: string;
  extractedFieldCount: number;
  confidenceScore: number;
  status: "ready" | "needs_review";
  fields: FormBuilderField[];
  formaSchema?: FormaSchema;
  reviewNotes: string[];
  createdAt: string;
};
type ChatBimSelectionContext = {
  count: number;
  withControlMark: number;
  withControlNumber: number;
  sample: Array<{
    elementId: string;
    category: string;
    family: string;
    type: string;
    controlMark: string;
    controlNumber: string;
  }>;
};
type SemanticDataRow = {
  entity: string;
  parameter: string;
  value: string;
  source: string;
};
type IssueRecord = {
  id: string;
  title: string;
  status?: string;
  description?: string;
  dueDate?: string;
};
type DesignFileListItem = {
  objectKey: string;
  name: string;
  baseName?: string;
  version?: number;
  isLatest?: boolean;
  source?: "oss" | "local-fallback";
};
type DesignFileData = {
  fileName: string;
  createdAt: string;
  pieceLinks: Array<{
    pieceId: string;
    dbId: number;
    externalId: string;
  }>;
  productInput?: ProductInputState;
  analysisResults?: AnalysisResults;
  selectionContext: {
    hubId: string;
    hubName: string;
    projectId: string;
    projectNumber: string;
    projectName: string;
    modelItemId: string;
    modelVersionId: string;
    modelName: string;
    modelUrn: string;
  };
  products: Array<Record<string, unknown>>;
};
type ProductInputState = {
  governingCode: string;
  riskCategory: string;
  exposureCategory: string;
  windSpeedMph: string;
  seismicDesignCategory: string;
  concreteStrengthPsi: string;
  reinforcementGrade: string;
  fireResistanceHours: string;
  spanFt: string;
  uniformLoadKipFt: string;
};
type AnalysisPoint = {
  x: number;
  deflection: number;
  shear: number;
  stress: number;
  warp: number;
};
type AnalysisResults = {
  span: number;
  uniformLoad: number;
  points: AnalysisPoint[];
  generatedAt: string;
};

type AiProvider = "openai" | "xai" | "cursor";
type AssistantMode = "agent";

const AI_MODEL_OPTIONS: Record<AiProvider, string[]> = {
  xai: [
    "grok-4.20-multi-agent-0309",
    "grok-4.20-reasoning",
    "grok-4-0709",
    "grok-3",
  ],
  openai: ["gpt-4.1-mini", "gpt-4.1", "gpt-4o-mini", "gpt-4o"],
  cursor: ["cursor-beta-default"],
};

export function AppClient() {
  const CHAT_INPUT_MIN_HEIGHT_PX = 40;
  const CHAT_INPUT_MAX_HEIGHT_PX = 220;

  const [auth, setAuth] = useState<SessionResponse | null>(null);
  const [loadingAuth, setLoadingAuth] = useState(true);
  const [loadingHubs, setLoadingHubs] = useState(false);
  const [loadingProjects, setLoadingProjects] = useState(false);
  const [loadingModels, setLoadingModels] = useState(false);
  const [hubs, setHubs] = useState<Hub[]>([]);
  const [projects, setProjects] = useState<Project[]>([]);
  const [models, setModels] = useState<Model[]>([]);
  const [selectedHub, setSelectedHub] = useState<string>("");
  const [selectedProject, setSelectedProject] = useState<string>("");
  const [selectedModel, setSelectedModel] = useState<string>("");
  const [chatInput, setChatInput] = useState("");
  const [chatLog, setChatLog] = useState<string[]>([]);
  const [chatPending, setChatPending] = useState(false);
  const modelChatInputRef = useRef<HTMLTextAreaElement | null>(null);
  const adminChatInputRef = useRef<HTMLTextAreaElement | null>(null);
  const formBuilderUploadInputRef = useRef<HTMLInputElement | null>(null);
  const productLayoutRef = useRef<HTMLDivElement | null>(null);
  const chatAbortControllerRef = useRef<AbortController | null>(null);
  const activeChatRequestIdRef = useRef(0);
  const [bimExportNotice, setBimExportNotice] = useState("");
  const viewerFeedbackQueueRef = useRef<string[]>([]);
  const viewerFeedbackTimerRef = useRef<ReturnType<typeof setTimeout> | null>(
    null,
  );
  const debouncedSetChatLog = useCallback((message: string) => {
    viewerFeedbackQueueRef.current.push(message);
    if (viewerFeedbackTimerRef.current) {
      clearTimeout(viewerFeedbackTimerRef.current);
    }
    viewerFeedbackTimerRef.current = setTimeout(() => {
      viewerFeedbackTimerRef.current = null;
      const batch = viewerFeedbackQueueRef.current.splice(0);
      if (batch.length === 0) return;
      setChatLog((prev) => [...prev, ...batch.map((m) => `AI: ${m}`)]);
    }, 300);
  }, []);

  useEffect(() => {
    return () => {
      if (viewerFeedbackTimerRef.current) {
        clearTimeout(viewerFeedbackTimerRef.current);
      }
      chatAbortControllerRef.current?.abort();
    };
  }, []);

  const [viewerActions, setViewerActions] = useState<ViewerAction[]>([]);
  const viewerPanelRef = useRef<ViewerPanelHandle>(null);
  const handleSelectElements = useCallback(
    (
      dbIds: (string | number)[],
      options: { clearFirst: boolean; zoomToSelection: boolean } = {
        clearFirst: true,
        zoomToSelection: false,
      },
    ) => {
      viewerPanelRef.current?.selectElements(dbIds, {
        clearFirst: options.clearFirst,
        zoomToSelection: options.zoomToSelection,
      });
    },
    [],
  );

  useEffect(() => {
    if (process.env.NODE_ENV !== "development") return;
    const w = window as unknown as {
      __apsSelectElements?: typeof handleSelectElements;
    };
    w.__apsSelectElements = handleSelectElements;
    return () => {
      delete w.__apsSelectElements;
    };
  }, [handleSelectElements]);

  const [error, setError] = useState<string>("");
  const [hubsError, setHubsError] = useState<string>("");
  const [workspaceExpanded, setWorkspaceExpanded] = useState(false);
  const [productTopPaneHeight, setProductTopPaneHeight] = useState(520);
  const [productAnalysisPaneHeight, setProductAnalysisPaneHeight] = useState(320);
  const [productDataPaneHeight] = useState(280);
  const [activeResizeHandle, setActiveResizeHandle] = useState<
    "top" | "analysis" | null
  >(null);
  const resizeStartYRef = useRef(0);
  const resizeStartHeightRef = useRef(0);
  const [workspaceMode, setWorkspaceMode] = useState<WorkspaceMode>("model");
  const [formBuilderSourceType, setFormBuilderSourceType] = useState<
    "upload" | "acc-version"
  >("upload");
  const [formBuilderTemplateName, setFormBuilderTemplateName] = useState("");
  const [formBuilderTemplateType, setFormBuilderTemplateType] =
    useState<FormTemplateType>("custom");
  const [formBuilderUploadToken, setFormBuilderUploadToken] = useState("");
  const [formBuilderProjectId, setFormBuilderProjectId] = useState("");
  const [formBuilderVersionId, setFormBuilderVersionId] = useState("");
  const [formBuilderSelectedFileName, setFormBuilderSelectedFileName] = useState("");
  const [formBuilderAnalysis, setFormBuilderAnalysis] =
    useState<FormBuilderAnalysis | null>(null);
  const [formBuilderPublishing, setFormBuilderPublishing] = useState(false);
  const [formBuilderAnalyzing, setFormBuilderAnalyzing] = useState(false);
  const [formBuilderUploadPending, setFormBuilderUploadPending] = useState(false);
  const [formBuilderMessage, setFormBuilderMessage] = useState("");
  const [formBuilderError, setFormBuilderError] = useState("");
  const [assistantMode] = useState<AssistantMode>("agent");
  const [aiProvider, setAiProvider] = useState<AiProvider>("xai");
  const [aiModel, setAiModel] = useState<string>(AI_MODEL_OPTIONS.xai[0]);
  const [selectedDbIds, setSelectedDbIds] = useState<number[]>([]);
  const [selectedElements, setSelectedElements] = useState<SelectedElementSnapshot[]>([]);
  /** Server-built discovery session for DA (resend each chat turn). */
  const [discoveryCachedSelection, setDiscoveryCachedSelection] =
    useState<DiscoveryCachedSelection | null>(null);
  /** Last DA workitem id from chat API (resend for status / server auto-poll). */
  const [lastDaJob, setLastDaJob] = useState<{
    workitem_id: string;
    submitted_at?: string;
    cache_id?: string;
    operation?: string;
  } | null>(null);
  const [workspaceTab, setWorkspaceTab] = useState<WorkspaceTab>("viewer");
  const [modelDataRows, setModelDataRows] = useState<ModelDataRow[]>([]);
  const [elementDataRows, setElementDataRows] = useState<ModelDataRow[]>([]);
  /** AEC element API rows per dbId — BIM table merge (Revit Element ID, shared params). */
  const [aecRowsByDbId, setAecRowsByDbId] = useState<
    Record<number, ModelDataRow[]>
  >({});
  const [loadingModelData, setLoadingModelData] = useState(false);
  const [loadingElementData, setLoadingElementData] = useState(false);
  const [issues, setIssues] = useState<IssueRecord[]>([]);
  const [loadingIssues, setLoadingIssues] = useState(false);
  const [issuesError, setIssuesError] = useState("");
  const [newIssueTitle, setNewIssueTitle] = useState("");
  const [newIssueDescription, setNewIssueDescription] = useState("");
  const [creatingIssue, setCreatingIssue] = useState(false);
  const [designFiles, setDesignFiles] = useState<DesignFileListItem[]>([]);
  const [loadingDesignFiles, setLoadingDesignFiles] = useState(false);
  const [designFilesError, setDesignFilesError] = useState("");
  const [selectedDesignFileBase, setSelectedDesignFileBase] = useState("");
  const [selectedDesignFileKey, setSelectedDesignFileKey] = useState("");
  const [selectedDesignFileData, setSelectedDesignFileData] =
    useState<DesignFileData | null>(null);
  const [designFilesStorage, setDesignFilesStorage] = useState<
    "oss" | "local-fallback" | ""
  >("");
  const [designFilesBucket, setDesignFilesBucket] = useState("");
  const [designFilesPrefix, setDesignFilesPrefix] = useState("");
  const [designFilesDiagnostics, setDesignFilesDiagnostics] = useState<string[]>([]);
  const [savingDesignFile, setSavingDesignFile] = useState(false);
  const [projectBrowserRoots, setProjectBrowserRoots] = useState<ProjectBrowserNode[]>([]);
  const [loadingProjectBrowser, setLoadingProjectBrowser] = useState(false);
  const [projectBrowserError, setProjectBrowserError] = useState("");
  const [projectBrowserWarning, setProjectBrowserWarning] = useState("");
  const [loadingProjectFolderIds, setLoadingProjectFolderIds] = useState<Set<string>>(
    () => new Set(),
  );
  const [expandedProjectFolders, setExpandedProjectFolders] = useState<Set<string>>(
    () => new Set(),
  );
  const [selectedProjectBrowserNodeId, setSelectedProjectBrowserNodeId] = useState("");
  const [selectedProjectBrowserViewerUrn, setSelectedProjectBrowserViewerUrn] = useState<
    string | null
  >(null);
  const [showDesignFileModal, setShowDesignFileModal] = useState(false);
  const [designFileNameInput, setDesignFileNameInput] = useState("");
  const [designFileModalError, setDesignFileModalError] = useState("");
  const [analysisInputs, setAnalysisInputs] = useState<ProductInputState>({
    governingCode: "IBC 2021 / ASCE 7-22 / ACI 318 / PCI",
    riskCategory: "II",
    exposureCategory: "B",
    windSpeedMph: "",
    seismicDesignCategory: "",
    concreteStrengthPsi: "",
    reinforcementGrade: "",
    fireResistanceHours: "",
    spanFt: "",
    uniformLoadKipFt: "",
  });
  const [analysisResults, setAnalysisResults] = useState<AnalysisResults | null>(null);
  const [runningAnalysis, setRunningAnalysis] = useState(false);
  const [selectedModelDaContext, setSelectedModelDaContext] =
    useState<DaExecutionContext | null>(null);
  const [loadingSelectedModelDaContext, setLoadingSelectedModelDaContext] =
    useState(false);

  const selectedModelData = useMemo(
    () => models.find((m) => m.itemId === selectedModel) ?? null,
    [models, selectedModel],
  );
  const selectedProjectBrowserNode = useMemo(() => {
    if (!selectedProjectBrowserNodeId) return null;
    const stack = [...projectBrowserRoots];
    while (stack.length > 0) {
      const node = stack.pop()!;
      if (node.id === selectedProjectBrowserNodeId) return node;
      if (node.children.length > 0) stack.push(...node.children);
    }
    return null;
  }, [projectBrowserRoots, selectedProjectBrowserNodeId]);
  const updateProjectBrowserNodeById = useCallback(
    (
      nodes: ProjectBrowserNode[],
      targetId: string,
      updater: (node: ProjectBrowserNode) => ProjectBrowserNode,
    ): ProjectBrowserNode[] =>
      nodes.map((node) => {
        if (node.id === targetId) return updater(node);
        if (node.children.length === 0) return node;
        return {
          ...node,
          children: updateProjectBrowserNodeById(node.children, targetId, updater),
        };
      }),
    [],
  );
  const toggleProjectFolder = useCallback((folderId: string) => {
    setExpandedProjectFolders((prev) => {
      const next = new Set(prev);
      if (next.has(folderId)) next.delete(folderId);
      else next.add(folderId);
      return next;
    });
  }, []);
  const loadProjectFolderChildren = useCallback(
    async (hubId: string, projectId: string, folderId: string) => {
      setLoadingProjectFolderIds((prev) => {
        const next = new Set(prev);
        next.add(folderId);
        return next;
      });
      try {
        const response = await fetch(
          `/api/aps/projects/${encodeURIComponent(projectId)}/folders?hubId=${encodeURIComponent(
            hubId,
          )}&folderId=${encodeURIComponent(folderId)}`,
        );
        const json = (await response.json()) as {
          nodes?: ProjectBrowserNode[];
          error?: string;
          details?: string;
          warning?: string;
        };
        if (!response.ok) {
          throw new Error(json.error ?? json.details ?? "Failed to load folder contents");
        }
        const children = Array.isArray(json.nodes) ? json.nodes : [];
        setProjectBrowserRoots((prev) =>
          updateProjectBrowserNodeById(prev, folderId, (node) => ({
            ...node,
            children,
            childrenLoaded: true,
          })),
        );
        if (json.warning) setProjectBrowserWarning(json.warning);
      } catch (err) {
        setProjectBrowserError(
          err instanceof Error ? err.message : "Failed to load folder contents",
        );
      } finally {
        setLoadingProjectFolderIds((prev) => {
          const next = new Set(prev);
          next.delete(folderId);
          return next;
        });
      }
    },
    [updateProjectBrowserNodeById],
  );
  const onSelectProjectBrowserNode = useCallback((node: ProjectBrowserNode) => {
    setSelectedProjectBrowserNodeId(node.id);
    if (node.kind !== "file") return;
    if (node.viewerUrn) {
      setSelectedProjectBrowserViewerUrn(node.viewerUrn);
    } else {
      setSelectedProjectBrowserViewerUrn(null);
    }
  }, []);
  const renderProjectBrowserNodes = useCallback(
    (nodes: ProjectBrowserNode[], depth = 0): ReactNode =>
      nodes.map((node) => {
        const isFolder = node.kind === "folder";
        const expanded = isFolder ? expandedProjectFolders.has(node.id) : false;
        const selected = selectedProjectBrowserNodeId === node.id;
        const folderLoading = isFolder ? loadingProjectFolderIds.has(node.id) : false;
        const indentClass =
          depth <= 0
            ? "pl-2"
            : depth === 1
              ? "pl-5"
              : depth === 2
                ? "pl-8"
                : depth === 3
                  ? "pl-11"
                  : depth === 4
                    ? "pl-14"
                    : depth === 5
                      ? "pl-[68px]"
                      : "pl-[80px]";
        return (
          <div key={node.id}>
            <button
              type="button"
              onClick={() => {
                if (isFolder) {
                  const willExpand = !expanded;
                  toggleProjectFolder(node.id);
                  if (
                    willExpand &&
                    !node.childrenLoaded &&
                    selectedHub &&
                    selectedProject &&
                    !loadingProjectFolderIds.has(node.id)
                  ) {
                    void loadProjectFolderChildren(selectedHub, selectedProject, node.id);
                  }
                  return;
                }
                onSelectProjectBrowserNode(node);
              }}
              className={`mb-1 flex w-full items-center gap-2 rounded border px-2 py-1 text-left text-xs ${
                selected
                  ? "border-black bg-black text-white"
                  : "border-black/10 bg-white hover:bg-gray-100"
              } ${indentClass}`}
            >
              <span className="inline-flex w-4 justify-center text-[11px]">
                {isFolder ? (expanded ? "▾" : "▸") : "•"}
              </span>
              <span className="truncate">{node.name}</span>
              {folderLoading ? (
                <span className="ml-auto text-[10px] text-gray-500">loading...</span>
              ) : null}
            </button>
            {isFolder && expanded && node.children.length > 0
              ? renderProjectBrowserNodes(node.children, depth + 1)
              : null}
          </div>
        );
      }),
    [
      expandedProjectFolders,
      loadProjectFolderChildren,
      loadingProjectFolderIds,
      onSelectProjectBrowserNode,
      selectedHub,
      selectedProject,
      selectedProjectBrowserNodeId,
      toggleProjectFolder,
    ],
  );
  const aiModelOptions = AI_MODEL_OPTIONS[aiProvider];
  const selectedProjectBrowserFileExtension = useMemo(() => {
    if (selectedProjectBrowserNode?.kind !== "file") return "";
    const segments = selectedProjectBrowserNode.name.split(".");
    if (segments.length < 2) return "";
    const ext = segments[segments.length - 1].trim().toLowerCase();
    if (!ext || ext.length > 10) return "";
    return `.${ext}`;
  }, [selectedProjectBrowserNode]);
  useEffect(() => {
    if (!selectedProject) return;
    setFormBuilderProjectId((prev) => prev || selectedProject);
  }, [selectedProject]);
  useEffect(() => {
    if (selectedProjectBrowserNode?.kind !== "file") return;
    if (selectedProjectBrowserNode.versionId) {
      setFormBuilderVersionId(selectedProjectBrowserNode.versionId);
    }
    setFormBuilderSelectedFileName(selectedProjectBrowserNode.name);
  }, [selectedProjectBrowserNode]);
  const elementIdentityRows = useMemo(() => {
    const identityKeyOrder = [
      "internalElementID",
      "elementCategory",
      "elementName",
      "elementType",
    ];
    const identityByKey = new Map(
      elementDataRows
        .filter((row) => identityKeyOrder.includes(row.key))
        .map((row) => [row.key, row]),
    );
    return identityKeyOrder
      .map((key) => identityByKey.get(key))
      .filter((row): row is ModelDataRow => Boolean(row));
  }, [elementDataRows]);
  const elementParameterRows = useMemo(
    () =>
      elementDataRows
        .filter((row) => row.key.startsWith("parameter."))
        .slice()
        .sort((a, b) => a.key.localeCompare(b.key, undefined, { sensitivity: "base" })),
    [elementDataRows],
  );
  const elementDiagnosticRows = useMemo(
    () =>
      elementDataRows.filter(
        (row) =>
          !["internalElementID", "elementCategory", "elementName", "elementType"].includes(
            row.key,
          ) && !row.key.startsWith("parameter."),
      ),
    [elementDataRows],
  );
  const selectedHubData = useMemo(
    () => hubs.find((h) => h.id === selectedHub) ?? null,
    [hubs, selectedHub],
  );
  const selectedProjectData = useMemo(
    () => projects.find((p) => p.id === selectedProject) ?? null,
    [projects, selectedProject],
  );
  const selectedProjectNumber = useMemo(() => {
    const name = selectedProjectData?.name ?? "";
    const token = name.match(/^\s*([A-Za-z0-9._-]+)/)?.[1] ?? "";
    return token || selectedProject || "UnknownProject";
  }, [selectedProjectData, selectedProject]);

  useEffect(() => {
    setDiscoveryCachedSelection(null);
    setLastDaJob(null);
  }, [selectedHub, selectedProject, selectedModel]);

  useEffect(() => {
    setProjectBrowserRoots([]);
    setProjectBrowserError("");
    setProjectBrowserWarning("");
    setExpandedProjectFolders(new Set());
    setLoadingProjectFolderIds(new Set());
    setSelectedProjectBrowserNodeId("");
    setSelectedProjectBrowserViewerUrn(null);
  }, [selectedHub, selectedProject]);

  const selectedPieceRefs = useMemo(() => {
    return selectedElements.map((el) => {
      const controlMark =
        el.properties.find(
          (p) => p.displayName.toLowerCase() === "control_mark",
        )?.displayValue ?? "";
      return {
        dbId: el.dbId,
        externalId: el.externalId ?? "",
        pieceId: controlMark || `dbId-${el.dbId}`,
      };
    });
  }, [selectedElements]);
  const bimTableRows = useMemo(
    () =>
      buildBimTableRows(selectedElements, {
        aecRowsByDbId,
      }),
    [selectedElements, aecRowsByDbId],
  );
  const chatBimSelectionContext = useMemo<ChatBimSelectionContext | undefined>(() => {
    if (bimTableRows.length === 0) return undefined;
    const norm = (s: string) => s.trim();
    const withControlMark = bimTableRows.reduce(
      (n, row) => n + (norm(row.controlMark) ? 1 : 0),
      0,
    );
    const withControlNumber = bimTableRows.reduce(
      (n, row) => n + (norm(row.controlNumber) ? 1 : 0),
      0,
    );
    const sampleRows: BimTableRow[] = bimTableRows.slice(0, 24);
    return {
      count: bimTableRows.length,
      withControlMark,
      withControlNumber,
      sample: sampleRows.map((row) => ({
        elementId: row.elementId,
        category: row.category,
        family: row.family,
        type: row.type,
        controlMark: row.controlMark,
        controlNumber: row.controlNumber,
      })),
    };
  }, [bimTableRows]);

  useEffect(() => {
    setBimExportNotice("");
  }, [selectedElements]);

  const exportBimSelectionCsv = useCallback(async () => {
    const csv = bimRowsToCsv(bimTableRows);
    const blob = new Blob([csv], { type: "text/csv;charset=utf-8" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = "bim-selection-export.csv";
    a.click();
    URL.revokeObjectURL(url);
    try {
      const res = await fetch("/api/export/bim-selection-csv", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ csv }),
      });
      const json = (await res.json()) as {
        ok?: boolean;
        error?: string;
        filePath?: string;
        fileName?: string;
      };
      if (res.ok && json.ok) {
        setBimExportNotice(
          `Also saved to app root: ${json.fileName ?? "bim-selection-export.csv"}`,
        );
      } else {
        setBimExportNotice(
          json.error
            ? `Browser download OK; server save failed: ${json.error}`
            : "Browser download OK; server save failed.",
        );
      }
    } catch {
      setBimExportNotice(
        "Browser download OK; server save unavailable (offline or read-only).",
      );
    }
  }, [bimTableRows]);

  const semanticModelRows = useMemo<SemanticDataRow[]>(() => {
    return modelDataRows.slice(0, 250).map((row) => {
      const key = row.key || "";
      const idx = key.lastIndexOf(".");
      const parameter = idx >= 0 ? key.slice(idx + 1) : key;
      const entity = idx >= 0 ? key.slice(0, idx) : "root";
      return {
        entity,
        parameter,
        value: row.value,
        source: row.source,
      };
    });
  }, [modelDataRows]);
  const primaryDesignProduct = useMemo(() => {
    const products = selectedDesignFileData?.products ?? [];
    return products.length > 0 ? products[0] : null;
  }, [selectedDesignFileData]);
  const groupedDesignFiles = useMemo(() => {
    const map = new Map<string, DesignFileListItem[]>();
    for (const file of designFiles) {
      const base = file.baseName ?? file.name;
      const list = map.get(base) ?? [];
      list.push(file);
      map.set(base, list);
    }
    return Array.from(map.entries())
      .map(([baseName, versions]) => ({
        baseName,
        versions: versions.sort((a, b) => (b.version ?? 1) - (a.version ?? 1)),
      }))
      .sort((a, b) =>
        a.baseName.localeCompare(b.baseName, undefined, {
          numeric: true,
          sensitivity: "base",
        }),
      );
  }, [designFiles]);
  const selectedDesignFileVersions = useMemo(() => {
    return groupedDesignFiles.find((g) => g.baseName === selectedDesignFileBase)?.versions ?? [];
  }, [groupedDesignFiles, selectedDesignFileBase]);
  const formBuilderProjectOptions = useMemo(() => {
    return projects
      .map((project) => {
        const token = project.name.match(/^\s*([A-Za-z0-9._-]+)/)?.[1] ?? "";
        return {
          id: project.id,
          label: token ? `${token} - ${project.name}` : project.name,
        };
      })
      .sort((a, b) =>
        a.label.localeCompare(b.label, undefined, { sensitivity: "base", numeric: true }),
      );
  }, [projects]);
  const formBuilderFileOptions = useMemo(() => {
    type Row = { versionId: string; label: string };
    const out: Row[] = [];
    const seen = new Set<string>();
    const walk = (nodes: ProjectBrowserNode[], prefix: string) => {
      for (const node of nodes) {
        const nextPrefix = prefix ? `${prefix}/${node.name}` : node.name;
        if (node.kind === "file" && node.versionId) {
          const versionId = node.versionId.trim();
          if (!versionId || seen.has(versionId)) continue;
          seen.add(versionId);
          out.push({
            versionId,
            label: nextPrefix,
          });
        }
        if (node.children.length > 0) {
          walk(node.children, nextPrefix);
        }
      }
    };
    walk(projectBrowserRoots, "");
    return out.sort((a, b) =>
      a.label.localeCompare(b.label, undefined, { sensitivity: "base", numeric: true }),
    );
  }, [projectBrowserRoots]);

  const loadSession = useCallback(async () => {
    setLoadingAuth(true);
    try {
      const response = await fetch("/api/auth/session", { cache: "no-store" });
      if (!response.ok) {
        setAuth(null);
        return;
      }
      const json = (await response.json()) as SessionResponse;
      setAuth(json);
    } catch {
      setAuth(null);
    } finally {
      setLoadingAuth(false);
    }
  }, []);

  const loadHubs = useCallback(async () => {
    setLoadingHubs(true);
    setHubsError("");
    try {
      const response = await fetch("/api/aps/hubs");
      if (!response.ok) throw new Error("Failed to load hubs");
      const json = (await response.json()) as { hubs: Hub[] };
      setHubs(json.hubs);
    } catch (err) {
      const msg = err instanceof Error ? err.message : "Failed to load hubs";
      setHubsError(msg);
      setError(msg);
      setHubs([]);
    } finally {
      setLoadingHubs(false);
    }
  }, []);

  const loadProjects = useCallback(async (hubId: string) => {
    setLoadingProjects(true);
    const response = await fetch(`/api/aps/hubs/${encodeURIComponent(hubId)}/projects`);
    if (!response.ok) throw new Error("Failed to load projects");
    const json = (await response.json()) as { projects: Project[] };
    setProjects(json.projects);
    setLoadingProjects(false);
  }, []);

  const loadModels = useCallback(async (hubId: string, projectId: string) => {
    setLoadingModels(true);
    const response = await fetch(
      `/api/aps/projects/${encodeURIComponent(projectId)}/models?hubId=${encodeURIComponent(hubId)}`,
    );
    if (!response.ok) throw new Error("Failed to load models");
    const json = (await response.json()) as { models: Model[] };
    setModels(json.models);
    setLoadingModels(false);
  }, []);

  const loadProjectBrowser = useCallback(async (hubId: string, projectId: string) => {
    setLoadingProjectBrowser(true);
    setProjectBrowserError("");
    setProjectBrowserWarning("");
    try {
      const response = await fetch(
        `/api/aps/projects/${encodeURIComponent(projectId)}/folders?hubId=${encodeURIComponent(hubId)}`,
      );
      const json = (await response.json()) as {
        nodes?: ProjectBrowserNode[];
        error?: string;
        details?: string;
        warning?: string;
      };
      if (!response.ok) {
        throw new Error(json.error ?? json.details ?? "Failed to load project browser");
      }
      const roots = Array.isArray(json.nodes) ? json.nodes : [];
      setProjectBrowserRoots(roots);
      setProjectBrowserWarning(json.warning ?? "");
      setExpandedProjectFolders(new Set());
    } catch (err) {
      setProjectBrowserError(
        err instanceof Error ? err.message : "Failed to load project browser",
      );
      setProjectBrowserRoots([]);
    } finally {
      setLoadingProjectBrowser(false);
    }
  }, []);
  const loadModelData = useCallback(async () => {
    if (!selectedHub || !selectedProject || !selectedModelData) {
      setModelDataRows([]);
      return;
    }
    setLoadingModelData(true);
    try {
      const response = await fetch(
        `/api/aps/projects/${encodeURIComponent(selectedProject)}/aec-query?hubId=${encodeURIComponent(selectedHub)}&itemId=${encodeURIComponent(selectedModelData.itemId)}`,
      );
      if (!response.ok) throw new Error("Failed to load model data");
      const json = (await response.json()) as { rows?: ModelDataRow[] };
      setModelDataRows(json.rows ?? []);
    } catch (err) {
      setModelDataRows([
        {
          key: "error",
          value: err instanceof Error ? err.message : "Failed to load model data",
          source: "app",
        },
      ]);
    } finally {
      setLoadingModelData(false);
    }
  }, [selectedHub, selectedProject, selectedModelData]);

  const loadElementData = useCallback(async () => {
    if (!selectedHub || !selectedProject || !selectedModelData) {
      setElementDataRows([]);
      setAecRowsByDbId({});
      return;
    }
    if (selectedElements.length === 0) {
      setElementDataRows([]);
      setAecRowsByDbId({});
      return;
    }
    setLoadingElementData(true);
    const projectPath = encodeURIComponent(selectedProject);
    const hubQ = encodeURIComponent(selectedHub);
    const itemQ = encodeURIComponent(selectedModelData.itemId);
    const versionQ = encodeURIComponent(selectedModelData.versionId);
    const buildViewerFallbackRows = (el: SelectedElementSnapshot): ModelDataRow[] => {
      const props = el.properties ?? [];
      const norm = (name: string) => name.trim().toLowerCase().replace(/[\s_-]+/g, "");
      const getProp = (...candidates: string[]) => {
        for (const prop of props) {
          const key = norm(prop.displayName ?? "");
          if (!key) continue;
          for (const candidate of candidates) {
            if (key === norm(candidate)) return String(prop.displayValue ?? "").trim();
          }
        }
        return "";
      };

      const rows: ModelDataRow[] = [];
      const internalElementId = getProp("Element Id", "Element ID", "Revit Element Id");
      const parsedFromName = extractFromForgeObjectName(el.name).elementId ?? "";
      const elementCategory = getProp("Category", "Category Name");
      const elementName = getProp("Family Name", "Family") || el.name || "";
      const elementType = getProp("Type Name", "Type");

      rows.push({
        key: "internalElementID",
        value: isUsableRevitElementId(internalElementId)
          ? internalElementId
          : isUsableRevitElementId(parsedFromName)
            ? parsedFromName
            : String(el.dbId),
        source: "viewer-fallback",
      });
      rows.push({
        key: "elementCategory",
        value: elementCategory || "(unknown)",
        source: "viewer-fallback",
      });
      rows.push({
        key: "elementName",
        value: elementName || "(unknown)",
        source: "viewer-fallback",
      });
      rows.push({
        key: "elementType",
        value: elementType || "(unknown)",
        source: "viewer-fallback",
      });

      for (const prop of props) {
        const name = String(prop.displayName ?? "").trim();
        if (!name) continue;
        const value = String(prop.displayValue ?? "").trim();
        const units = String(prop.units ?? "").trim();
        rows.push({
          key: `parameter.${name}`,
          value: units && value ? `${value} ${units}` : value || "(empty)",
          source: "viewer-fallback",
        });
      }
      return rows;
    };
    const fetchOne = async (el: SelectedElementSnapshot) => {
      const url = `/api/aps/projects/${projectPath}/aec-element?hubId=${hubQ}&itemId=${itemQ}&externalId=${encodeURIComponent(el.externalId ?? "")}&dbId=${encodeURIComponent(String(el.dbId))}&elementName=${encodeURIComponent(el.name ?? "")}`;
      const urlWithVersion = `${url}&versionId=${versionQ}`;
      const response = await fetch(urlWithVersion);
      if (!response.ok) return { dbId: el.dbId, rows: buildViewerFallbackRows(el) };
      const json = (await response.json()) as { rows?: ModelDataRow[] };
      const rows = json.rows ?? [];
      const hasIdentity = rows.some((r) =>
        ["internalElementID", "elementCategory", "elementName", "elementType"].includes(
          r.key,
        ),
      );
      const hasParams = rows.some((r) => r.key.startsWith("parameter."));
      if (!hasIdentity || !hasParams) {
        const fallback = buildViewerFallbackRows(el);
        const fallbackByKey = new Map(fallback.map((r) => [r.key, r]));
        const merged = rows.map((row) => {
          if (row.key !== "internalElementID") return row;
          if (isUsableRevitElementId(row.value)) return row;
          const fallbackRow = fallbackByKey.get("internalElementID");
          return fallbackRow ?? row;
        });
        for (const [key, row] of fallbackByKey.entries()) {
          if (!merged.some((existing) => existing.key === key)) {
            merged.push(row);
          }
        }
        return { dbId: el.dbId, rows: merged };
      }
      return { dbId: el.dbId, rows };
    };
    try {
      const slice = selectedElements.slice(0, 80);
      const byDb: Record<number, ModelDataRow[]> = {};
      const chunkSize = 6;
      for (let i = 0; i < slice.length; i += chunkSize) {
        const chunk = slice.slice(i, i + chunkSize);
        const results = await Promise.all(chunk.map((el) => fetchOne(el)));
        for (const { dbId, rows } of results) {
          byDb[dbId] = rows;
        }
      }
      setAecRowsByDbId(byDb);
      const first = selectedElements[0];
      setElementDataRows(byDb[first.dbId] ?? []);
    } catch (err) {
      setElementDataRows([
        {
          key: "error",
          value:
            err instanceof Error ? err.message : "Failed to load element data",
          source: "app",
        },
      ]);
      setAecRowsByDbId({});
    } finally {
      setLoadingElementData(false);
    }
  }, [selectedHub, selectedProject, selectedModelData, selectedElements]);

  const loadIssues = useCallback(async () => {
    if (!selectedProject) {
      setIssues([]);
      return;
    }
    setLoadingIssues(true);
    setIssuesError("");
    try {
      const response = await fetch(
        `/api/aps/projects/${encodeURIComponent(selectedProject)}/issues`,
      );
      const json = (await response.json()) as {
        issues?: IssueRecord[];
        error?: string;
        details?: string;
      };
      if (!response.ok) {
        throw new Error(json.error ?? json.details ?? "Failed to load issues");
      }
      setIssues(json.issues ?? []);
    } catch (err) {
      setIssues([]);
      setIssuesError(err instanceof Error ? err.message : "Failed to load issues");
    } finally {
      setLoadingIssues(false);
    }
  }, [selectedProject]);

  const loadDesignFiles = useCallback(async () => {
    if (!selectedProject) {
      setDesignFiles([]);
        setSelectedDesignFileBase("");
      setSelectedDesignFileKey("");
      setSelectedDesignFileData(null);
      return;
    }
    setLoadingDesignFiles(true);
    setDesignFilesError("");
    try {
      const response = await fetch(
        `/api/aps/projects/${encodeURIComponent(selectedProject)}/design-files?hubId=${encodeURIComponent(selectedHub)}&projectNumber=${encodeURIComponent(selectedProjectNumber)}`,
      );
      const json = (await response.json()) as {
        files?: DesignFileListItem[];
        storage?: "oss" | "local-fallback";
        bucket?: string;
        prefix?: string;
        diagnostics?: string[];
        error?: string;
        details?: string;
      };
      if (!response.ok) {
        throw new Error(json.error ?? json.details ?? "Failed to load design files");
      }
      const files = json.files ?? [];
      setDesignFiles(files);
      setDesignFilesStorage(json.storage ?? "");
      setDesignFilesBucket(json.bucket ?? "");
      setDesignFilesPrefix(json.prefix ?? "");
      setDesignFilesDiagnostics(json.diagnostics ?? []);
      setSelectedDesignFileBase((prevBase) => {
        const groups = new Map<string, DesignFileListItem[]>();
        for (const file of files) {
          const base = file.baseName ?? file.name;
          const list = groups.get(base) ?? [];
          list.push(file);
          groups.set(base, list);
        }
        if (prevBase && groups.has(prevBase)) return prevBase;
        return files[0]?.baseName ?? files[0]?.name ?? "";
      });
      setSelectedDesignFileKey((prevKey) => {
        if (prevKey && files.some((f) => f.objectKey === prevKey)) return prevKey;
        const firstLatest = files.find((f) => f.isLatest) ?? files[0];
        return firstLatest?.objectKey ?? "";
      });
    } catch (err) {
      setDesignFiles([]);
      setSelectedDesignFileBase("");
      setSelectedDesignFileKey("");
      setSelectedDesignFileData(null);
      setDesignFilesStorage("");
      setDesignFilesBucket("");
      setDesignFilesPrefix("");
      setDesignFilesDiagnostics([]);
      setDesignFilesError(
        err instanceof Error ? err.message : "Failed to load design files",
      );
    } finally {
      setLoadingDesignFiles(false);
    }
  }, [selectedProject, selectedProjectNumber, selectedHub]);

  const loadDesignFileData = useCallback(async () => {
    if (!selectedProject || !selectedDesignFileKey) {
      setSelectedDesignFileData(null);
      setAnalysisResults(null);
      return;
    }
    try {
      const response = await fetch(
        `/api/aps/projects/${encodeURIComponent(selectedProject)}/design-files?hubId=${encodeURIComponent(selectedHub)}&projectNumber=${encodeURIComponent(selectedProjectNumber)}&objectKey=${encodeURIComponent(selectedDesignFileKey)}`,
      );
      const json = (await response.json()) as {
        data?: DesignFileData;
        error?: string;
        details?: string;
      };
      if (!response.ok) {
        throw new Error(json.error ?? json.details ?? "Failed to load design file");
      }
      const data = json.data ?? null;
      setSelectedDesignFileData(data);
      if (data?.productInput) {
        setAnalysisInputs((prev) => ({ ...prev, ...data.productInput }));
      }
      setAnalysisResults(data?.analysisResults ?? null);
    } catch (err) {
      setSelectedDesignFileData(null);
      setDesignFilesError(
        err instanceof Error ? err.message : "Failed to load design file",
      );
    }
  }, [selectedProject, selectedDesignFileKey, selectedProjectNumber, selectedHub]);

  useEffect(() => {
    void loadSession();
  }, [loadSession]);

  useEffect(() => {
    if (!auth?.authenticated) return;
    void loadHubs().catch((err) => {
      setError(err instanceof Error ? err.message : "Failed loading hubs");
    });
  }, [auth, loadHubs]);

  useEffect(() => {
    if (!selectedHub) {
      setProjects([]);
      setSelectedProject("");
      return;
    }
    void loadProjects(selectedHub).catch((err) => {
      setLoadingProjects(false);
      setError(err instanceof Error ? err.message : "Failed loading projects");
    });
  }, [selectedHub, loadProjects]);

  useEffect(() => {
    if (!selectedHub || !selectedProject) {
      setModels([]);
      setSelectedModel("");
      return;
    }
    if (workspaceMode === "model" && workspaceTab === "folders") {
      return;
    }
    void loadModels(selectedHub, selectedProject).catch((err) => {
      setLoadingModels(false);
      setError(err instanceof Error ? err.message : "Failed loading models");
    });
  }, [selectedHub, selectedProject, workspaceMode, workspaceTab, loadModels]);

  useEffect(() => {
    if (!selectedProject || !selectedModelData?.versionId) {
      setSelectedModelDaContext(null);
      setLoadingSelectedModelDaContext(false);
      return;
    }
    let cancelled = false;
    const fetchDaContext = async () => {
      setLoadingSelectedModelDaContext(true);
      try {
        const response = await fetch(
          `/api/aps/da-config-check?projectId=${encodeURIComponent(selectedProject)}&versionId=${encodeURIComponent(selectedModelData.versionId)}`,
        );
        const json = (await response.json()) as {
          cloud_model?: {
            region?: string;
            projectGuid?: string;
            modelGuid?: string;
            revitVersionMajor?: number;
          } | null;
          resolved_activity?: {
            activityId?: string;
            source?: string;
            revitVersionMajor?: number;
          } | null;
        };
        if (cancelled) return;
        const cloudModel =
          json.cloud_model &&
          typeof json.cloud_model.region === "string" &&
          typeof json.cloud_model.projectGuid === "string" &&
          typeof json.cloud_model.modelGuid === "string"
            ? {
                region: json.cloud_model.region,
                projectGuid: json.cloud_model.projectGuid,
                modelGuid: json.cloud_model.modelGuid,
                ...(Number.isFinite(Number(json.cloud_model.revitVersionMajor))
                  ? { revitVersionMajor: Number(json.cloud_model.revitVersionMajor) }
                  : {}),
              }
            : undefined;
        const resolvedActivityId =
          typeof json.resolved_activity?.activityId === "string"
            ? json.resolved_activity.activityId.trim()
            : "";
        const resolvedActivitySource =
          typeof json.resolved_activity?.source === "string"
            ? json.resolved_activity.source.trim()
            : "";
        const revitVersionMajor = Number(json.resolved_activity?.revitVersionMajor);
        setSelectedModelDaContext({
          ...(resolvedActivityId ? { resolvedActivityId } : {}),
          ...(resolvedActivitySource ? { resolvedActivitySource } : {}),
          ...(Number.isFinite(revitVersionMajor) ? { revitVersionMajor } : {}),
          ...(cloudModel ? { cloudModel } : {}),
        });
      } catch {
        if (cancelled) return;
        setSelectedModelDaContext(null);
      } finally {
        if (!cancelled) {
          setLoadingSelectedModelDaContext(false);
        }
      }
    };
    void fetchDaContext();
    return () => {
      cancelled = true;
    };
  }, [selectedProject, selectedModelData]);

  useEffect(() => {
    if (!selectedHub || !selectedProject) {
      setProjectBrowserRoots([]);
      setProjectBrowserError("");
      setProjectBrowserWarning("");
      return;
    }
    if (workspaceMode !== "model" || workspaceTab !== "folders") return;
    void loadProjectBrowser(selectedHub, selectedProject);
  }, [selectedHub, selectedProject, workspaceMode, workspaceTab, loadProjectBrowser]);

  useEffect(() => {
    if (workspaceMode !== "form-builder") return;
    if (formBuilderSourceType !== "acc-version") return;
    if (!selectedHub || !formBuilderProjectId) return;
    void loadProjectBrowser(selectedHub, formBuilderProjectId);
  }, [
    workspaceMode,
    formBuilderSourceType,
    selectedHub,
    formBuilderProjectId,
    loadProjectBrowser,
  ]);

  useEffect(() => {
    setFormBuilderVersionId("");
  }, [formBuilderProjectId]);

  useEffect(() => {
    void loadModelData();
  }, [loadModelData]);

  useEffect(() => {
    void loadElementData();
  }, [loadElementData]);

  useEffect(() => {
    if (!(workspaceMode === "model" && workspaceTab === "issues")) return;
    void loadIssues();
  }, [loadIssues, workspaceMode, workspaceTab]);

  useEffect(() => {
    void loadDesignFiles();
  }, [loadDesignFiles]);

  useEffect(() => {
    void loadDesignFileData();
  }, [loadDesignFileData]);

  useEffect(() => {
    if (!selectedDesignFileBase) return;
    const versions = groupedDesignFiles.find(
      (g) => g.baseName === selectedDesignFileBase,
    )?.versions;
    if (!versions || versions.length === 0) return;
    const latest = versions.find((v) => v.isLatest) ?? versions[0];
    if (latest && latest.objectKey !== selectedDesignFileKey) {
      setSelectedDesignFileKey(latest.objectKey);
    }
  }, [groupedDesignFiles, selectedDesignFileBase, selectedDesignFileKey]);

  useEffect(() => {
    if (!selectedDesignFileKey) return;
    const selected = designFiles.find((f) => f.objectKey === selectedDesignFileKey);
    const base = selected?.baseName ?? selected?.name ?? "";
    if (base && base !== selectedDesignFileBase) {
      setSelectedDesignFileBase(base);
    }
  }, [designFiles, selectedDesignFileBase, selectedDesignFileKey]);

  useEffect(() => {
    if (!activeResizeHandle) return;
    const onMouseMove = (event: MouseEvent) => {
      const delta = event.clientY - resizeStartYRef.current;
      if (activeResizeHandle === "top") {
        setProductTopPaneHeight(Math.max(340, resizeStartHeightRef.current + delta));
      } else {
        setProductAnalysisPaneHeight(
          Math.max(240, resizeStartHeightRef.current + delta),
        );
      }
    };
    const onMouseUp = () => setActiveResizeHandle(null);
    const priorCursor = document.body.style.cursor;
    const priorSelect = document.body.style.userSelect;
    document.body.style.cursor = "row-resize";
    document.body.style.userSelect = "none";
    window.addEventListener("mousemove", onMouseMove);
    window.addEventListener("mouseup", onMouseUp);
    return () => {
      document.body.style.cursor = priorCursor;
      document.body.style.userSelect = priorSelect;
      window.removeEventListener("mousemove", onMouseMove);
      window.removeEventListener("mouseup", onMouseUp);
    };
  }, [activeResizeHandle]);

  const startPaneResize = useCallback(
    (handle: "top" | "analysis", clientY: number) => {
      resizeStartYRef.current = clientY;
      resizeStartHeightRef.current =
        handle === "top" ? productTopPaneHeight : productAnalysisPaneHeight;
      setActiveResizeHandle(handle);
    },
    [productAnalysisPaneHeight, productTopPaneHeight],
  );

  const productLayoutHandleHeight = 10;
  const productLayoutFixedHeight =
    productTopPaneHeight +
    productAnalysisPaneHeight +
    productDataPaneHeight +
    productLayoutHandleHeight * 2;
  const productLayoutHeight = workspaceExpanded
    ? `max(calc(100vh - 12rem), ${productLayoutFixedHeight}px)`
    : `${productLayoutFixedHeight}px`;

  useEffect(() => {
    if (!productLayoutRef.current) return;
    productLayoutRef.current.style.height = productLayoutHeight;
    productLayoutRef.current.style.minHeight = "780px";
    productLayoutRef.current.style.gridTemplateRows = `${productTopPaneHeight}px ${productLayoutHandleHeight}px ${productAnalysisPaneHeight}px ${productLayoutHandleHeight}px ${productDataPaneHeight}px`;
  }, [
    productAnalysisPaneHeight,
    productDataPaneHeight,
    productLayoutHandleHeight,
    productLayoutHeight,
    productTopPaneHeight,
  ]);

  const publishBlockedByReview = useMemo(() => {
    if (!formBuilderAnalysis) return true;
    if (formBuilderAnalysis.fields.length === 0) return true;
    return formBuilderAnalysis.fields.some((field) => field.reviewState === "needs_review");
  }, [formBuilderAnalysis]);

  const removeFormBuilderField = useCallback((fieldId: string) => {
    setFormBuilderAnalysis((prev) =>
      prev
        ? { ...prev, fields: prev.fields.filter((f) => f.id !== fieldId) }
        : prev,
    );
  }, []);

  const moveFormBuilderField = useCallback(
    (fieldId: string, direction: -1 | 1) => {
      setFormBuilderAnalysis((prev) => {
        if (!prev) return prev;
        const idx = prev.fields.findIndex((f) => f.id === fieldId);
        if (idx < 0) return prev;
        const target = idx + direction;
        if (target < 0 || target >= prev.fields.length) return prev;
        const next = [...prev.fields];
        const [moved] = next.splice(idx, 1);
        next.splice(target, 0, moved);
        return { ...prev, fields: next };
      });
    },
    [],
  );

  /**
   * Live Forma builder preview mirroring the server translator: repeated-row or
   * calculated groups become tables; heading + repeating fillable rows become
   * multiple-entries sections; everything else stays a single field.
   */
  const formBuilderStructurePreview = useMemo(() => {
    const fields = formBuilderAnalysis?.fields ?? [];
    type PreviewElement =
      | { kind: "field"; label: string; type: string }
      | {
          kind: "section";
          title: string;
          entryMode: "single" | "multiple";
          fieldCount: number;
        }
      | {
          kind: "table";
          title: string;
          columnCount: number;
          calculatedColumns: number;
        };
    const elements: PreviewElement[] = [];
    const groups = new Map<string, FormBuilderField[]>();
    const order: string[] = [];
    for (const field of fields) {
      const key = field.groupKey?.trim();
      if (!key) continue;
      if (!groups.has(key)) {
        groups.set(key, []);
        order.push(key);
      }
      groups.get(key)!.push(field);
    }
    const emitted = new Set<string>();
    let sectionCount = 0;
    let tableCount = 0;
    let calculatedColumnCount = 0;
    for (const field of fields) {
      const key = field.groupKey?.trim();
      if (!key) {
        elements.push({ kind: "field", label: field.label, type: field.type });
        continue;
      }
      if (emitted.has(key)) continue;
      emitted.add(key);
      const members = groups.get(key) ?? [];
      const heading = members.find((m) => m.type === "section_heading");
      const fillable = members.filter((m) => m.type !== "section_heading");
      const distinctRows = new Set(
        members
          .map((m) => m.rowIndex)
          .filter((r): r is number => typeof r === "number"),
      );
      const calculated = members.filter((m) => m.calculated || m.readOnly);
      const isTable = distinctRows.size >= 2 || calculated.length > 0;
      if (isTable) {
        const cols = new Set(
          (fillable.length > 0 ? fillable : members).map(
            (m) => (m.columnKey || m.label || m.sourceName).toLowerCase(),
          ),
        );
        const calcCols = calculated.length;
        tableCount += 1;
        calculatedColumnCount += calcCols;
        elements.push({
          kind: "table",
          title: heading?.label || key,
          columnCount: cols.size,
          calculatedColumns: calcCols,
        });
        continue;
      }
      const homogeneous =
        fillable.length >= 2 && new Set(fillable.map((m) => m.type)).size === 1;
      const entryMode: "single" | "multiple" =
        distinctRows.size >= 2 || (Boolean(heading) && homogeneous)
          ? "multiple"
          : "single";
      sectionCount += 1;
      elements.push({
        kind: "section",
        title: heading?.label || key,
        entryMode,
        fieldCount: fillable.length,
      });
    }
    return { elements, sectionCount, tableCount, calculatedColumnCount };
  }, [formBuilderAnalysis]);

  const formBuilderStatusTone = useMemo(() => {
    if (!formBuilderAnalysis) return "text-gray-700";
    if (formBuilderAnalysis.status === "ready") return "text-green-700";
    return "text-amber-700";
  }, [formBuilderAnalysis]);

  const onUploadFormBuilderPdf = useCallback(
    async (file: File | null) => {
      if (!file || formBuilderUploadPending) return;
      setFormBuilderUploadPending(true);
      setFormBuilderError("");
      setFormBuilderMessage("");
      try {
        const body = new FormData();
        body.set("file", file);
        if (selectedHub) body.set("hubId", selectedHub);
        if (selectedProject) body.set("projectId", selectedProject);
        const response = await fetch("/api/admin/forms/upload", {
          method: "POST",
          body,
        });
        const json = (await response.json()) as {
          uploadToken?: string;
          fileName?: string;
          error?: string;
        };
        if (!response.ok || !json.uploadToken) {
          throw new Error(json.error || "Upload failed.");
        }
        setFormBuilderUploadToken(json.uploadToken);
        setFormBuilderSelectedFileName(json.fileName || file.name);
        setFormBuilderMessage("PDF uploaded. Ready for analysis.");
      } catch (error) {
        setFormBuilderError(error instanceof Error ? error.message : "Upload failed.");
      } finally {
        setFormBuilderUploadPending(false);
      }
    },
    [formBuilderUploadPending, selectedHub, selectedProject],
  );

  const onAnalyzeFormBuilder = useCallback(async () => {
    if (formBuilderAnalyzing) return;
    setFormBuilderAnalyzing(true);
    setFormBuilderError("");
    setFormBuilderMessage("");
    try {
      const source =
        formBuilderSourceType === "upload"
          ? {
              kind: "upload_token" as const,
              uploadToken: formBuilderUploadToken.trim(),
            }
          : {
              kind: "acc_version" as const,
              projectId: formBuilderProjectId.trim(),
              versionId: formBuilderVersionId.trim(),
            };
      if (
        (source.kind === "upload_token" && !source.uploadToken) ||
        (source.kind === "acc_version" && (!source.projectId || !source.versionId))
      ) {
        throw new Error("Provide a PDF source before running analysis.");
      }
      const response = await fetch("/api/admin/forms/analyze", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          source,
          hubId: selectedHub || undefined,
          templateName: formBuilderTemplateName.trim() || undefined,
          templateType: formBuilderTemplateType,
        }),
      });
      const json = (await response.json()) as {
        success?: boolean;
        analysis?: FormBuilderAnalysis;
        error?: string;
      };
      if (!response.ok || !json.analysis) {
        throw new Error(json.error || "Analyze request failed.");
      }
      setFormBuilderAnalysis(json.analysis);
      setFormBuilderTemplateName(json.analysis.templateName);
      setFormBuilderTemplateType(json.analysis.templateType);
      setFormBuilderMessage(
        json.analysis.status === "ready"
          ? "Analysis complete. Template is ready to publish."
          : "Analysis complete. Review required fields before publish.",
      );
    } catch (error) {
      setFormBuilderError(error instanceof Error ? error.message : "Analyze failed.");
    } finally {
      setFormBuilderAnalyzing(false);
    }
  }, [
    formBuilderAnalyzing,
    formBuilderProjectId,
    formBuilderSourceType,
    formBuilderTemplateName,
    formBuilderTemplateType,
    formBuilderUploadToken,
    formBuilderVersionId,
    selectedHub,
  ]);

  const onPublishFormBuilder = useCallback(async () => {
    if (!formBuilderAnalysis || formBuilderPublishing || publishBlockedByReview) return;
    setFormBuilderPublishing(true);
    setFormBuilderError("");
    setFormBuilderMessage("");
    try {
      const response = await fetch("/api/admin/forms/create-template", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          analysisId: formBuilderAnalysis.analysisId,
          hubId: selectedHub || undefined,
          templateName: formBuilderTemplateName.trim() || undefined,
          templateType: formBuilderTemplateType,
          orderedFieldIds: formBuilderAnalysis.fields.map((field) => field.id),
          fieldOverrides: formBuilderAnalysis.fields.map((field) => ({
            id: field.id,
            label: field.label,
            type: field.type,
            required: field.required,
            options: field.options,
            reviewState: field.reviewState,
            calculated: field.calculated ?? false,
            ...(field.formula ? { formula: field.formula } : {}),
            ...(field.groupKey ? { groupKey: field.groupKey } : {}),
          })),
        }),
      });
      const json = (await response.json()) as {
        success?: boolean;
        templateId?: string;
        message?: string;
        sectionsCreated?: number;
        tablesCreated?: number;
        calculatedColumns?: number;
        error?: string;
      };
      if (!response.ok || !json.success) {
        throw new Error(json.error || json.message || "Template publish failed.");
      }
      const structureSummary = `${json.sectionsCreated ?? 0} section(s), ${
        json.tablesCreated ?? 0
      } table(s), ${json.calculatedColumns ?? 0} calculated column(s)`;
      setFormBuilderMessage(
        json.templateId
          ? `Template created (templateId: ${json.templateId}) — ${structureSummary}.`
          : `Template created — ${structureSummary}.`,
      );
    } catch (error) {
      setFormBuilderError(error instanceof Error ? error.message : "Publish failed.");
    } finally {
      setFormBuilderPublishing(false);
    }
  }, [
    formBuilderAnalysis,
    formBuilderPublishing,
    formBuilderTemplateName,
    formBuilderTemplateType,
    publishBlockedByReview,
    selectedHub,
  ]);

  async function onLogout() {
    await fetch("/api/auth/logout", { method: "POST" });
    setAuth(null);
    setHubs([]);
    setProjects([]);
    setModels([]);
    setSelectedHub("");
    setSelectedProject("");
    setSelectedModel("");
    setChatLog([]);
    setViewerActions([]);
  }

  async function onSendChat() {
    if (chatPending) return;
    const msg = chatInput.trim();
    if (!msg) return;
    const requestId = activeChatRequestIdRef.current + 1;
    activeChatRequestIdRef.current = requestId;
    const controller = new AbortController();
    chatAbortControllerRef.current = controller;
    setChatInput("");
    setChatLog((prev) => [...prev, `You: ${msg}`]);
    setError("");
    setChatPending(true);

    try {
      const chatBody = {
        message: msg,
        chatHistory: chatLog.slice(-8),
        selectedModelName: selectedModelData?.name ?? "no model selected",
        selectedModelUrn: selectedModelData?.viewerUrn ?? undefined,
        selectedHubId: selectedHub || undefined,
        selectedProjectId: selectedProject || undefined,
        selectedItemId: selectedModelData?.itemId ?? undefined,
        selectedDbIds,
        selectedCount: selectedDbIds.length,
        selectedElements,
        bimSelectionContext: chatBimSelectionContext,
        discoveryCachedSelection: discoveryCachedSelection ?? undefined,
        lastDaJob: lastDaJob ?? undefined,
        daExecutionContext: selectedModelDaContext ?? undefined,
        assistantMode,
        aiProvider,
        aiModel,
        workspaceMode,
        formBuilder:
          workspaceMode === "form-builder"
            ? {
                sourceType: formBuilderSourceType,
                uploadToken: formBuilderUploadToken || undefined,
                projectId:
                  formBuilderProjectId || selectedProject || undefined,
                versionId: formBuilderVersionId || undefined,
                templateName: formBuilderTemplateName || undefined,
                templateType: formBuilderTemplateType,
                analysis: formBuilderAnalysis ?? undefined,
              }
            : undefined,
        productAnalysis: {
          rulesText: "",
          selectedDesignFile: selectedDesignFileData,
          selectedProduct: primaryDesignProduct,
          analysisInputs,
          selectionContext: {
            hubId: selectedHub || "",
            hubName: selectedHubData?.name ?? "",
            projectId: selectedProject || "",
            projectNumber: selectedProjectNumber,
            projectName: selectedProjectData?.name ?? "",
            modelItemId: selectedModelData?.itemId ?? "",
            modelVersionId: selectedModelData?.versionId ?? "",
            modelName: selectedModelData?.name ?? "",
            modelUrn: selectedModelData?.viewerUrn ?? "",
          },
        },
    };
      aiBrowserDebug("chat:request", {
        message: msg,
        selectedCount: selectedDbIds.length,
        selectedElementsSample: selectedElements.slice(0, 5).map((e) => ({
          dbId: e.dbId,
          externalId: e.externalId,
          name: e.name,
          propCount: e.properties?.length ?? 0,
        })),
        bimSelectionContext: chatBimSelectionContext
          ? {
              count: chatBimSelectionContext.count,
              withControlMark: chatBimSelectionContext.withControlMark,
              withControlNumber: chatBimSelectionContext.withControlNumber,
              sampleCount: chatBimSelectionContext.sample.length,
            }
          : null,
        discovery: discoveryCachedSelection
          ? {
              cache_id: discoveryCachedSelection.cache_id,
              externalIds: discoveryCachedSelection.externalIds.length,
              selection_rules: discoveryCachedSelection.selection_rules,
            }
          : null,
        lastDaJob: lastDaJob ?? null,
        daExecutionContext: selectedModelDaContext
          ? {
              source: selectedModelDaContext.resolvedActivitySource ?? "",
              activityId: selectedModelDaContext.resolvedActivityId ?? "",
              revitVersionMajor: selectedModelDaContext.revitVersionMajor ?? null,
              cloudModelReady: Boolean(selectedModelDaContext.cloudModel),
              loading: loadingSelectedModelDaContext,
            }
          : null,
      });

      const response = await fetch("/api/chat", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(chatBody),
        signal: controller.signal,
      });

      if (!response.ok) {
        setError("Chat request failed");
        return;
      }

      const json = (await response.json()) as {
        message: string;
        actions: ViewerAction[];
        requestId?: string;
        queryResult?: { views?: Array<{ guid: string; name: string; role: string }> };
        discoveryCachedSelection?: DiscoveryCachedSelection | null;
        lastDaJob?: {
          workitem_id: string;
          submitted_at?: string;
          cache_id?: string;
          operation?: string;
        } | null;
      };
      aiBrowserDebug("chat:response", {
        requestId: json.requestId,
        messagePreview: json.message?.slice(0, 400),
        actions: json.actions,
        discoveryOut: json.discoveryCachedSelection
          ? {
              cache_id: json.discoveryCachedSelection.cache_id,
              externalIds: json.discoveryCachedSelection.externalIds.length,
              selection_rules: json.discoveryCachedSelection.selection_rules,
            }
          : null,
        lastDaJob: json.lastDaJob ?? null,
      });
      setDiscoveryCachedSelection((prev) =>
        "discoveryCachedSelection" in json
          ? (json.discoveryCachedSelection ?? null)
          : prev,
      );
      setLastDaJob((prev) =>
        "lastDaJob" in json ? (json.lastDaJob ?? null) : prev,
      );
      setChatLog((prev) => [...prev, `AI: ${json.message}`]);
      if (json.queryResult?.views?.length) {
        const viewCount = json.queryResult.views.length;
        const preview = json.queryResult.views
          .slice(0, 5)
          .map((v) => `${v.name} (${v.role})`)
          .join(", ");
        setChatLog((prev) => [
          ...prev,
          `AI Views: ${preview}${viewCount > 5 ? " ..." : ""}`,
        ]);
      }
      setViewerActions(json.actions ?? []);
    } catch (err) {
      if (err instanceof DOMException && err.name === "AbortError") {
        return;
      }
      setError("Chat request failed");
    } finally {
      if (activeChatRequestIdRef.current === requestId) {
        chatAbortControllerRef.current = null;
        setChatPending(false);
      }
    }
  }

  function onStopChat() {
    if (!chatPending) return;
    activeChatRequestIdRef.current += 1;
    chatAbortControllerRef.current?.abort();
    chatAbortControllerRef.current = null;
    setChatPending(false);
    setChatLog((prev) => [...prev, "AI: Request interrupted by user."]);
  }

  const resizeChatTextarea = useCallback((el: HTMLTextAreaElement | null) => {
    if (!el) return;
    el.style.height = `${CHAT_INPUT_MIN_HEIGHT_PX}px`;
    const nextHeight = Math.min(
      Math.max(el.scrollHeight, CHAT_INPUT_MIN_HEIGHT_PX),
      CHAT_INPUT_MAX_HEIGHT_PX,
    );
    el.style.height = `${nextHeight}px`;
    el.style.overflowY =
      el.scrollHeight > CHAT_INPUT_MAX_HEIGHT_PX ? "auto" : "hidden";
  }, []);

  const handleChatInputChange = useCallback(
    (value: string, source: "model" | "admin") => {
      setChatInput(value);
      if (source === "model") {
        resizeChatTextarea(modelChatInputRef.current);
      } else {
        resizeChatTextarea(adminChatInputRef.current);
      }
    },
    [resizeChatTextarea],
  );

  function handleChatInputKeyDown(e: KeyboardEvent<HTMLTextAreaElement>) {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      void onSendChat();
    }
  }

  useEffect(() => {
    if (workspaceMode === "admin") {
      resizeChatTextarea(adminChatInputRef.current);
      return;
    }
    resizeChatTextarea(modelChatInputRef.current);
  }, [chatInput, resizeChatTextarea, workspaceMode]);

  async function onCreateIssue() {
    const title = newIssueTitle.trim();
    if (!selectedProject || !title) return;
    setCreatingIssue(true);
    setIssuesError("");
    try {
      const response = await fetch(
        `/api/aps/projects/${encodeURIComponent(selectedProject)}/issues`,
        {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            title,
            description: newIssueDescription.trim() || undefined,
          }),
        },
      );
      const json = (await response.json()) as {
        error?: string;
        details?: string;
      };
      if (!response.ok) {
        throw new Error(json.error ?? json.details ?? "Failed to create issue");
      }
      setNewIssueTitle("");
      setNewIssueDescription("");
      await loadIssues();
    } catch (err) {
      setIssuesError(err instanceof Error ? err.message : "Failed to create issue");
    } finally {
      setCreatingIssue(false);
    }
  }

  function pickPropertyValue(
    properties: SelectedElementSnapshot["properties"],
    candidates: string[],
  ): string {
    for (const key of candidates) {
      const hit = properties.find(
        (p) => p.displayName.toLowerCase() === key.toLowerCase(),
      );
      if (hit?.displayValue) return hit.displayValue;
    }
    return "";
  }

  function parseNumber(value: string): number | null {
    const n = Number(String(value).replace(/[^0-9.-]/g, ""));
    return Number.isFinite(n) ? n : null;
  }

  async function onDesignProduct(fileNameRaw: string) {
    const fileName = fileNameRaw.trim();
    if (!fileName) {
      setDesignFileModalError("Enter a file name.");
      setError("Enter a file name.");
      return;
    }
    if (!selectedProject) {
      setDesignFileModalError("Select a project first.");
      setError("Select a project first.");
      return;
    }
    if (selectedElements.length === 0) {
      setDesignFileModalError("Select one or more elements in the model first.");
      setError("Select one or more elements in the model first.");
      return;
    }

    const products = selectedElements.map((el) => {
      const length = pickPropertyValue(el.properties, ["Length", "DIM_LENGTH"]);
      const width = pickPropertyValue(el.properties, ["Width", "DIM_WIDTH"]);
      const height = pickPropertyValue(el.properties, ["Height", "DIM_HEIGHT"]);
      const volume = pickPropertyValue(el.properties, ["Volume", "DIM_VOLUME"]);
      const level = pickPropertyValue(el.properties, [
        "Level",
        "Reference Level",
        "Base Level",
      ]);
      const baseElevation = pickPropertyValue(el.properties, ["Base Offset", "Base Elevation"]);
      const topElevation = pickPropertyValue(el.properties, ["Top Offset", "Top Elevation"]);
      return {
        dbId: el.dbId,
        name: el.name ?? "",
        externalId: el.externalId ?? "",
        controlMark: pickPropertyValue(el.properties, ["CONTROL_MARK"]),
        controlNumber: pickPropertyValue(el.properties, ["CONTROL_NUMBER"]),
        constructionProduct: pickPropertyValue(el.properties, ["CONSTRUCTION_PRODUCT"]),
        designNumber: pickPropertyValue(el.properties, ["DESIGN_NUMBER"]),
        level,
        elevations: {
          base: baseElevation,
          top: topElevation,
        },
        dimensions: {
          length,
          width,
          height,
          volume,
          lengthNum: parseNumber(length),
          widthNum: parseNumber(width),
          heightNum: parseNumber(height),
          volumeNum: parseNumber(volume),
        },
        levelCrossings: {
          note: "Placeholder: level crossing geometry extraction to be refined.",
          points: [],
        },
        properties: el.properties,
      };
    });

    const payload: DesignFileData = {
      fileName,
      createdAt: new Date().toISOString(),
      pieceLinks: selectedPieceRefs,
      productInput: analysisInputs,
      selectionContext: {
        hubId: selectedHub || "",
        hubName: selectedHubData?.name ?? "",
        projectId: selectedProject || "",
        projectNumber: selectedProjectNumber,
        projectName: selectedProjectData?.name ?? "",
        modelItemId: selectedModelData?.itemId ?? "",
        modelVersionId: selectedModelData?.versionId ?? "",
        modelName: selectedModelData?.name ?? "",
        modelUrn: selectedModelData?.viewerUrn ?? "",
      },
      products,
    };

    setSavingDesignFile(true);
    setDesignFilesError("");
    setDesignFileModalError("");
    try {
      const response = await fetch(
        `/api/aps/projects/${encodeURIComponent(selectedProject)}/design-files`,
        {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            fileName,
            hubId: selectedHub,
            projectNumber: selectedProjectNumber,
            data: payload,
          }),
        },
      );
      const json = (await response.json()) as {
        objectKey?: string;
        baseName?: string;
        version?: number;
        storage?: "oss" | "local-fallback";
        diagnostics?: string[];
        error?: string;
        details?: string;
      };
      if (!response.ok) {
        throw new Error(json.error ?? json.details ?? "Failed to save design file");
      }
      // Close modal immediately on successful save.
      setWorkspaceMode("product-analysis");
      setShowDesignFileModal(false);
      setDesignFileNameInput("");
      if ((json.storage ?? "") === "local-fallback") {
        setDesignFilesError(
          `Design file saved using local fallback storage. ${
            json.diagnostics?.join(" | ") ?? ""
          }`,
        );
      }
      try {
        await loadDesignFiles();
        if (json.baseName) setSelectedDesignFileBase(json.baseName);
        if (json.objectKey) setSelectedDesignFileKey(json.objectKey);
      } catch (refreshError) {
        setDesignFilesError(
          refreshError instanceof Error
            ? `Saved file, but refresh failed: ${refreshError.message}`
            : "Saved file, but refresh failed.",
        );
      }
    } catch (err) {
      const msg = err instanceof Error ? err.message : "Failed to save design file";
      setDesignFileModalError(msg);
      setDesignFilesError(
        msg,
      );
      setError(msg);
    } finally {
      setSavingDesignFile(false);
    }
  }

  function computeAnalysisResult(input: ProductInputState): AnalysisResults {
    const span = Math.max(0.001, parseNumber(input.spanFt) ?? 20);
    const load = parseNumber(input.uniformLoadKipFt) ?? 1;
    const points: AnalysisPoint[] = Array.from({ length: 31 }, (_, i) => {
      const x = (span * i) / 30;
      const ratio = x / span;
      const parab = 4 * ratio * (1 - ratio);
      return {
        x,
        deflection: load * parab,
        shear: load * (1 - 2 * ratio),
        stress: load * parab * 0.6,
        warp: load * parab * 0.35,
      };
    });
    return {
      span,
      uniformLoad: load,
      points,
      generatedAt: new Date().toISOString(),
    };
  }

  async function onCommitAndRun() {
    if (!selectedProject || !selectedDesignFileData || !selectedDesignFileKey) {
      setDesignFilesError("Select a design file first.");
      return;
    }
    setRunningAnalysis(true);
    setDesignFilesError("");
    try {
      const result = computeAnalysisResult(analysisInputs);
      setAnalysisResults(result);
      const fileName =
        selectedDesignFileData.fileName ||
        selectedDesignFileKey.split("/").pop()?.replace(/\.json$/i, "") ||
        "DesignFile";
      const updated: DesignFileData = {
        ...selectedDesignFileData,
        productInput: analysisInputs,
        analysisResults: result,
      };
      const response = await fetch(
        `/api/aps/projects/${encodeURIComponent(selectedProject)}/design-files`,
        {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            fileName,
            hubId: selectedHub,
            projectNumber: selectedProjectNumber,
            data: updated,
          }),
        },
      );
      const json = (await response.json()) as {
        objectKey?: string;
        baseName?: string;
        error?: string;
        details?: string;
      };
      if (!response.ok) {
        throw new Error(json.error ?? json.details ?? "Failed to commit analysis");
      }
      setSelectedDesignFileData(updated);
      await loadDesignFiles();
      if (json.baseName) setSelectedDesignFileBase(json.baseName);
      if (json.objectKey) setSelectedDesignFileKey(json.objectKey);
    } catch (err) {
      setDesignFilesError(
        err instanceof Error ? err.message : "Failed to commit and run",
      );
    } finally {
      setRunningAnalysis(false);
    }
  }

  function updateAnalysisInput(name: keyof typeof analysisInputs, value: string) {
    setAnalysisInputs((prev) => ({ ...prev, [name]: value }));
  }

  return (
    <main className="mx-auto flex w-full max-w-7xl flex-col gap-4 px-6 py-6">
      <header className="rounded-lg border border-black/10 bg-white p-4 shadow-sm">
        <h1 className="text-xl font-semibold">APS Viewer + AI (Phase 1)</h1>
        <p className="mt-1 text-sm text-gray-600">
          Login, select hub/project/model, and query the viewer with chat.
        </p>
        <div className="mt-3 flex items-center gap-3">
          {loadingAuth ? (
            <span className="text-sm text-gray-600">Checking auth...</span>
          ) : auth?.authenticated ? (
            <>
              <span className="text-sm text-green-700">Authenticated</span>
              <button
                onClick={onLogout}
                className="rounded border px-3 py-1 text-sm hover:bg-gray-100"
              >
                Logout
              </button>
            </>
          ) : (
            <a
              href="/auth/login"
              className="rounded bg-black px-3 py-1 text-sm text-white hover:bg-gray-800"
            >
              Login with Autodesk
            </a>
          )}
        </div>
      </header>

      {error ? (
        <div className="rounded border border-red-300 bg-red-50 px-3 py-2 text-sm text-red-700">
          {error}
        </div>
      ) : null}

      <section className="grid grid-cols-1 gap-3 rounded-lg border border-black/10 bg-white p-4 shadow-sm md:grid-cols-3">
        <label className="flex flex-col gap-1 text-sm">
          <span className="font-medium">Hub</span>
          <select
            value={selectedHub}
            onChange={(e) => setSelectedHub(e.target.value)}
            disabled={!auth?.authenticated || loadingHubs}
            className="rounded border px-2 py-2 text-black disabled:text-gray-500 bg-white"
          >
            <option value="">
              {loadingHubs ? "Loading hubs..." : "Select a hub"}
            </option>
            {hubs.map((hub) => (
              <option key={hub.id} value={hub.id}>
                {hub.name}
              </option>
            ))}
          </select>
          {hubsError ? (
            <div className="flex items-center gap-2 pt-1">
              <span className="text-xs text-red-600">{hubsError}</span>
              <button
                onClick={() => void loadHubs()}
                className="rounded border px-2 py-0.5 text-xs hover:bg-gray-100"
              >
                Retry
              </button>
            </div>
          ) : (
            <span className="text-xs text-gray-500">
              {hubs.length > 0 ? `${hubs.length} hubs loaded` : "No hubs loaded yet"}
            </span>
          )}
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="font-medium">Project</span>
          <select
            value={selectedProject}
            onChange={(e) => setSelectedProject(e.target.value)}
            disabled={!selectedHub || loadingProjects || workspaceMode === "admin"}
            className="rounded border px-2 py-2 text-black disabled:text-gray-500 bg-white"
          >
            <option value="">
              {loadingProjects ? "Loading projects..." : "Select a project"}
            </option>
            {projects.map((project) => (
              <option key={project.id} value={project.id}>
                {project.name}
              </option>
            ))}
          </select>
        </label>

        <label className="flex flex-col gap-1 text-sm">
          <span className="font-medium">Model</span>
          <select
            value={selectedModel}
            onChange={(e) => setSelectedModel(e.target.value)}
            disabled={
              !selectedProject || loadingModels || workspaceMode === "admin"
            }
            className="rounded border px-2 py-2 text-black disabled:text-gray-500 bg-white"
          >
            <option value="">
              {loadingModels ? "Loading models..." : "Select a model"}
            </option>
            {models.map((model) => (
              <option key={model.itemId} value={model.itemId}>
                {model.name}
              </option>
            ))}
          </select>
        </label>
      </section>

      <div
        className={
          workspaceExpanded
            ? "fixed inset-4 z-50 overflow-hidden rounded-lg border border-black/15 bg-white p-3 shadow-2xl text-black"
            : "overflow-hidden rounded-lg border border-black/10 bg-white p-3 shadow-sm text-black"
        }
      >
        <div className="mb-2 flex items-center justify-between">
          <div className="flex items-center gap-2">
            <button
              onClick={() => setWorkspaceMode("model")}
              className={`rounded border px-2 py-1 text-xs ${
                workspaceMode === "model" ? "bg-black text-white" : "bg-white"
              }`}
            >
              Project
            </button>
            <button
              onClick={() => setWorkspaceMode("product-analysis")}
              className={`rounded border px-2 py-1 text-xs ${
                workspaceMode === "product-analysis"
                  ? "bg-black text-white"
                  : "bg-white"
              }`}
            >
              Product Analysis
            </button>
            <button
              onClick={() => setWorkspaceMode("admin")}
              className={`rounded border px-2 py-1 text-xs ${
                workspaceMode === "admin" ? "bg-black text-white" : "bg-white"
              }`}
            >
              Admin
            </button>
            <button
              onClick={() => setWorkspaceMode("form-builder")}
              className={`rounded border px-2 py-1 text-xs ${
                workspaceMode === "form-builder" ? "bg-black text-white" : "bg-white"
              }`}
            >
              Form Builder
            </button>
          </div>
          <button
            onClick={() => setWorkspaceExpanded((prev) => !prev)}
            className="rounded border px-3 py-1 text-xs font-medium hover:bg-gray-100"
          >
            {workspaceExpanded ? "Restore" : "Maximize"}
          </button>
        </div>
        <div
          className={`${
            workspaceMode === "model" ? "grid" : "hidden"
          } min-h-0 grid-rows-[minmax(0,1fr)_minmax(200px,38%)] gap-3 ${
            workspaceExpanded ? "h-[calc(100%-2.25rem)]" : "h-[72vh] min-h-[780px]"
          }`}
        >
          <div className="grid h-full min-h-0 grid-cols-[minmax(280px,1fr)_minmax(0,3fr)] gap-3 overflow-hidden">
            <section className="flex h-full min-h-0 flex-col overflow-hidden rounded border border-black/10 bg-white p-3 text-black">
              <h3 className="mb-2 text-sm font-semibold">AI Chat</h3>
              <div className="mb-2 rounded border border-black/10 bg-gray-50 px-2 py-1 text-xs text-black">
                Selection: {selectedDbIds.length} element{selectedDbIds.length === 1 ? "" : "s"}
              </div>
              <div className="mb-2 grid grid-cols-3 gap-2">
              <div className="flex flex-col gap-1 text-xs">
                <span className="font-medium">Assistant</span>
                <div className="rounded border px-2 py-1.5 text-black bg-white">
                  Monty
                </div>
              </div>
              <label className="flex flex-col gap-1 text-xs">
                <span className="font-medium">Service</span>
                <select
                  value={aiProvider}
                  onChange={(e) => {
                    const nextProvider = e.target.value as AiProvider;
                    setAiProvider(nextProvider);
                    setAiModel(AI_MODEL_OPTIONS[nextProvider][0]);
                  }}
                  className="rounded border px-2 py-1.5 text-black bg-white"
                >
                  <option value="xai">xAI (Grok)</option>
                  <option value="openai">OpenAI</option>
                  <option value="cursor">Cursor Beta</option>
                </select>
              </label>
              <label className="flex flex-col gap-1 text-xs">
                <span className="font-medium">Model</span>
                <select
                  value={aiModel}
                  onChange={(e) => setAiModel(e.target.value)}
                  className="rounded border px-2 py-1.5 text-black bg-white"
                >
                  {aiModelOptions.map((modelName) => (
                    <option key={modelName} value={modelName}>
                      {modelName}
                    </option>
                  ))}
                </select>
              </label>
            </div>
            <div className="mb-2 min-h-0 flex-1 overflow-y-auto rounded border border-black/10 bg-gray-50 p-2 text-sm text-black">
              {chatLog.length === 0 ? (
                <p className="text-black">
                  Ask things like: &quot;find walls&quot;, &quot;fit view&quot;, or
                  &quot;clear selection&quot;.
                </p>
              ) : (
                chatLog.map((line, idx) => (
                  <p key={`${line}-${idx}`} className="mb-1">
                    {line}
                  </p>
                ))
              )}
              {chatPending ? (
                <p className="mt-2 text-xs text-gray-600">AI: Monty is thinking...</p>
              ) : null}
            </div>
            <div className="flex gap-2">
              <textarea
                ref={modelChatInputRef}
                value={chatInput}
                rows={1}
                onChange={(e) => handleChatInputChange(e.target.value, "model")}
                disabled={chatPending}
                onKeyDown={handleChatInputKeyDown}
                className="min-h-[40px] flex-1 resize-none rounded border px-3 py-2 text-sm text-black placeholder:text-gray-600"
                placeholder="Ask the model..."
              />
              <button
                onClick={() => {
                  if (chatPending) onStopChat();
                  else void onSendChat();
                }}
                className={`rounded px-3 py-2 text-sm text-white ${
                  chatPending
                    ? "bg-red-700 hover:bg-red-800"
                    : "bg-black hover:bg-gray-800"
                }`}
              >
                {chatPending ? "Stop" : "Send"}
              </button>
            </div>
            </section>

            <section className="flex h-full min-h-0 flex-col overflow-hidden rounded border border-black/10 bg-white p-3 text-black">
            <div className="mb-2 flex items-center gap-2">
              <span className="mr-auto text-xs font-medium text-gray-600">
                Project Tabs
              </span>
              <button
                onClick={() => setWorkspaceTab("folders")}
                className={`rounded border px-2 py-1 text-xs ${
                  workspaceTab === "folders" ? "bg-black text-white" : "bg-white"
                }`}
              >
                Folders
              </button>
              <button
                onClick={() => setWorkspaceTab("viewer")}
                className={`rounded border px-2 py-1 text-xs ${
                  workspaceTab === "viewer" ? "bg-black text-white" : "bg-white"
                }`}
              >
                Model
              </button>
              <button
                onClick={() => setWorkspaceTab("model-data")}
                className={`rounded border px-2 py-1 text-xs ${
                  workspaceTab === "model-data" ? "bg-black text-white" : "bg-white"
                }`}
              >
                Model Data
              </button>
              <button
                onClick={() => setWorkspaceTab("element-data")}
                className={`rounded border px-2 py-1 text-xs ${
                  workspaceTab === "element-data" ? "bg-black text-white" : "bg-white"
                }`}
              >
                Element Data
              </button>
              <button
                onClick={() => setWorkspaceTab("issues")}
                className={`rounded border px-2 py-1 text-xs ${
                  workspaceTab === "issues" ? "bg-black text-white" : "bg-white"
                }`}
              >
                Issues
              </button>
              <button
                onClick={() => {
                  setError("");
                  setDesignFileModalError("");
                  setShowDesignFileModal(true);
                }}
                disabled={savingDesignFile}
                className="rounded border px-2 py-1 text-xs hover:bg-gray-100 disabled:cursor-not-allowed disabled:opacity-60"
              >
                {savingDesignFile ? "Saving..." : "Design Product"}
              </button>
            </div>

            {workspaceTab === "folders" ? (
              <div className="min-h-0 flex-1 overflow-hidden rounded border border-black/10 bg-gray-50 p-2">
                <div className="grid h-full min-h-0 grid-cols-[minmax(250px,1fr)_minmax(0,2fr)] gap-2">
                  <div className="min-h-0 overflow-auto rounded border border-black/10 bg-white p-2 text-xs">
                    {!selectedProject ? (
                      <p>Select a project to load folders and files.</p>
                    ) : loadingProjectBrowser ? (
                      <p>Loading folders and files...</p>
                    ) : projectBrowserError ? (
                      <p className="text-red-700">{projectBrowserError}</p>
                    ) : projectBrowserRoots.length === 0 ? (
                      <p>No folders were returned for this project.</p>
                    ) : (
                      <div className="space-y-1">
                        {projectBrowserWarning ? (
                          <p className="mb-2 rounded border border-amber-300 bg-amber-50 px-2 py-1 text-[11px] text-amber-900">
                            {projectBrowserWarning}
                          </p>
                        ) : null}
                        {renderProjectBrowserNodes(projectBrowserRoots)}
                      </div>
                    )}
                  </div>
                  <div className="min-h-0 overflow-hidden rounded border border-black/10 bg-white p-2">
                    {selectedProjectBrowserNode?.kind === "file" ? (
                      <div className="mb-2 rounded border border-black/10 bg-gray-50 px-2 py-1 text-xs">
                        <div className="font-semibold">{selectedProjectBrowserNode.name}</div>
                        {selectedProjectBrowserFileExtension ? (
                          <div className="text-[11px] text-gray-700">
                            {selectedProjectBrowserFileExtension}
                          </div>
                        ) : null}
                      </div>
                    ) : (
                      <div className="mb-2 rounded border border-black/10 bg-gray-50 px-2 py-1 text-xs text-gray-700">
                        Select a file in the project browser to preview it.
                      </div>
                    )}
                    {selectedProjectBrowserViewerUrn ? (
                      <div className="h-[calc(100%-40px)] min-h-0">
                        <ViewerPanel
                          ref={viewerPanelRef}
                          mode="minimal"
                          viewerUrn={selectedProjectBrowserViewerUrn}
                          isActive={workspaceMode === "model" && workspaceTab === "folders"}
                          actions={
                            workspaceMode === "model" && workspaceTab === "folders"
                              ? viewerActions
                              : []
                          }
                          onActionComplete={() => setViewerActions([])}
                          onViewerFeedback={debouncedSetChatLog}
                          onSelectionChange={setSelectedDbIds}
                          onSelectionDetails={setSelectedElements}
                        />
                      </div>
                    ) : (
                      <div className="flex h-[calc(100%-40px)] min-h-0 items-center justify-center rounded border border-dashed border-black/20 text-xs text-gray-600">
                        Select a viewable model file to open preview.
                      </div>
                    )}
                  </div>
                </div>
              </div>
            ) : null}

            <div className={workspaceTab === "viewer" ? "min-h-0 flex-1" : "hidden min-h-0 flex-1"}>
              <ViewerPanel
                ref={viewerPanelRef}
                viewerUrn={selectedModelData?.viewerUrn ?? null}
                isActive={workspaceMode === "model" && workspaceTab === "viewer"}
                actions={
                  workspaceMode === "model" && workspaceTab === "viewer"
                    ? viewerActions
                    : []
                }
                onActionComplete={() => setViewerActions([])}
                onViewerFeedback={debouncedSetChatLog}
                onSelectionChange={setSelectedDbIds}
                onSelectionDetails={setSelectedElements}
              />
            </div>

            {workspaceTab === "model-data" ? (
              <div className="min-h-0 flex-1 overflow-auto rounded border border-black/10 bg-gray-50 p-2 text-xs">
                {loadingModelData ? (
                  <p>Loading model data...</p>
                ) : modelDataRows.length === 0 ? (
                  <p>No model data available.</p>
                ) : (
                  <table className="w-full border-collapse">
                    <thead>
                      <tr>
                        <th className="border border-black/10 bg-white px-2 py-1 text-left">
                          Entity
                        </th>
                        <th className="border border-black/10 bg-white px-2 py-1 text-left">
                          Parameter
                        </th>
                        <th className="border border-black/10 bg-white px-2 py-1 text-left">
                          Value
                        </th>
                        <th className="border border-black/10 bg-white px-2 py-1 text-left">
                          Source
                        </th>
                      </tr>
                    </thead>
                    <tbody>
                      {semanticModelRows.map((row, idx) => (
                        <tr key={`${row.entity}-${row.parameter}-${idx}`}>
                          <td className="border border-black/10 px-2 py-1 align-top">
                            {row.entity}
                          </td>
                          <td className="border border-black/10 px-2 py-1 align-top">
                            {row.parameter}
                          </td>
                          <td className="border border-black/10 px-2 py-1 align-top">
                            {row.value}
                          </td>
                          <td className="border border-black/10 px-2 py-1 align-top">
                            {row.source}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                )}
              </div>
            ) : null}

            {workspaceTab === "element-data" ? (
              <div className="min-h-0 flex-1 overflow-auto rounded border border-black/10 bg-gray-50 p-2 text-xs">
                {selectedElements.length === 0 ? (
                  <p>No selected element data yet. Select an element in Viewer tab.</p>
                ) : loadingElementData ? (
                  <p>Loading AEC element data...</p>
                ) : elementDataRows.length > 0 ? (
                  <div className="space-y-3">
                    <section className="rounded border border-black/10 bg-white p-2">
                      <h4 className="mb-2 text-sm font-semibold">Identity</h4>
                      {elementIdentityRows.length > 0 ? (
                        <table className="w-full border-collapse">
                          <thead>
                            <tr>
                              <th className="border border-black/10 px-2 py-1 text-left">
                                Field
                              </th>
                              <th className="border border-black/10 px-2 py-1 text-left">
                                Value
                              </th>
                            </tr>
                          </thead>
                          <tbody>
                            {elementIdentityRows.map((r, idx) => (
                              <tr key={`${r.key}-${idx}`}>
                                <td className="border border-black/10 px-2 py-1 align-top">
                                  {r.key}
                                </td>
                                <td className="border border-black/10 px-2 py-1 align-top">
                                  {r.value}
                                </td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      ) : (
                        <p className="text-xs text-gray-600">No identity rows returned.</p>
                      )}
                    </section>

                    <section className="rounded border border-black/10 bg-white p-2">
                      <h4 className="mb-2 text-sm font-semibold">Parameters</h4>
                      {elementParameterRows.length > 0 ? (
                        <table className="w-full border-collapse">
                          <thead>
                            <tr>
                              <th className="border border-black/10 px-2 py-1 text-left">
                                Parameter
                              </th>
                              <th className="border border-black/10 px-2 py-1 text-left">
                                Value
                              </th>
                            </tr>
                          </thead>
                          <tbody>
                            {elementParameterRows.map((r, idx) => (
                              <tr key={`${r.key}-${idx}`}>
                                <td className="border border-black/10 px-2 py-1 align-top">
                                  {r.key.replace(/^parameter\./, "")}
                                </td>
                                <td className="border border-black/10 px-2 py-1 align-top">
                                  {r.value}
                                </td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      ) : (
                        <p className="text-xs text-gray-600">No parameter rows returned.</p>
                      )}
                    </section>

                    {elementDiagnosticRows.length > 0 ? (
                      <section className="rounded border border-black/10 bg-white p-2">
                        <h4 className="mb-2 text-sm font-semibold">Diagnostics</h4>
                        <table className="w-full border-collapse">
                          <thead>
                            <tr>
                              <th className="border border-black/10 px-2 py-1 text-left">
                                Key
                              </th>
                              <th className="border border-black/10 px-2 py-1 text-left">
                                Value
                              </th>
                              <th className="border border-black/10 px-2 py-1 text-left">
                                Source
                              </th>
                            </tr>
                          </thead>
                          <tbody>
                            {elementDiagnosticRows.map((r, idx) => (
                              <tr key={`${r.key}-${idx}`}>
                                <td className="border border-black/10 px-2 py-1 align-top">
                                  {r.key}
                                </td>
                                <td className="border border-black/10 px-2 py-1 align-top">
                                  {r.value}
                                </td>
                                <td className="border border-black/10 px-2 py-1 align-top">
                                  {r.source}
                                </td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </section>
                    ) : null}
                  </div>
                ) : (
                  selectedElements.map((el) => (
                    <div
                      key={el.dbId}
                      className="mb-3 rounded border border-black/10 bg-white p-2"
                    >
                      <div className="mb-1 text-sm font-semibold">
                        dbId: {el.dbId}
                        {el.name ? ` - ${el.name}` : ""}
                      </div>
                      <table className="w-full border-collapse">
                        <thead>
                          <tr>
                            <th className="border border-black/10 px-2 py-1 text-left">
                              Property
                            </th>
                            <th className="border border-black/10 px-2 py-1 text-left">
                              Value
                            </th>
                            <th className="border border-black/10 px-2 py-1 text-left">
                              Units
                            </th>
                          </tr>
                        </thead>
                        <tbody>
                          {el.properties.map((p, idx) => (
                            <tr key={`${el.dbId}-${p.displayName}-${idx}`}>
                              <td className="border border-black/10 px-2 py-1 align-top">
                                {p.displayName}
                              </td>
                              <td className="border border-black/10 px-2 py-1 align-top">
                                {p.displayValue}
                              </td>
                              <td className="border border-black/10 px-2 py-1 align-top">
                                {p.units ?? ""}
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  ))
                )}
              </div>
            ) : null}

            {workspaceTab === "issues" ? (
              <div className="min-h-0 flex-1 overflow-auto rounded border border-black/10 bg-gray-50 p-2 text-xs">
                <div className="mb-3 flex items-center justify-between gap-2">
                  <div className="font-semibold">Project Issues</div>
                  <button
                    type="button"
                    onClick={() => void loadIssues()}
                    className="rounded border px-2 py-1 hover:bg-gray-100"
                  >
                    Refresh
                  </button>
                </div>

                <div className="mb-3 rounded border border-black/10 bg-white p-2">
                  <div className="mb-2 font-medium">Create Issue</div>
                  <div className="mb-2 flex flex-col gap-2">
                    <input
                      value={newIssueTitle}
                      onChange={(e) => setNewIssueTitle(e.target.value)}
                      placeholder="Issue title"
                      className="rounded border px-2 py-1 text-xs"
                    />
                    <textarea
                      value={newIssueDescription}
                      onChange={(e) => setNewIssueDescription(e.target.value)}
                      placeholder="Issue description (optional)"
                      className="min-h-16 rounded border px-2 py-1 text-xs"
                    />
                  </div>
                  <button
                    type="button"
                    onClick={() => void onCreateIssue()}
                    disabled={!selectedProject || creatingIssue || !newIssueTitle.trim()}
                    className="rounded border px-2 py-1 hover:bg-gray-100 disabled:cursor-not-allowed disabled:opacity-60"
                  >
                    {creatingIssue ? "Creating..." : "Create"}
                  </button>
                </div>

                {issuesError ? (
                  <div className="mb-2 rounded border border-red-300 bg-red-50 px-2 py-1 text-red-700">
                    {issuesError}
                  </div>
                ) : null}

                {loadingIssues ? (
                  <p>Loading issues...</p>
                ) : issues.length === 0 ? (
                  <p>No issues found for this project.</p>
                ) : (
                  <table className="w-full border-collapse">
                    <thead>
                      <tr>
                        <th className="border border-black/10 px-2 py-1 text-left">Title</th>
                        <th className="border border-black/10 px-2 py-1 text-left">Status</th>
                        <th className="border border-black/10 px-2 py-1 text-left">Due</th>
                        <th className="border border-black/10 px-2 py-1 text-left">ID</th>
                      </tr>
                    </thead>
                    <tbody>
                      {issues.map((issue) => (
                        <tr key={issue.id}>
                          <td className="border border-black/10 px-2 py-1 align-top">
                            {issue.title}
                          </td>
                          <td className="border border-black/10 px-2 py-1 align-top">
                            {issue.status ?? ""}
                          </td>
                          <td className="border border-black/10 px-2 py-1 align-top">
                            {issue.dueDate ?? ""}
                          </td>
                          <td className="border border-black/10 px-2 py-1 align-top">
                            {issue.id}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                )}
              </div>
            ) : null}
            </section>
          </div>

          <section className="flex min-h-0 flex-col overflow-hidden rounded border border-black/10 bg-white text-black">
            <div className="flex shrink-0 items-center justify-between gap-2 border-b border-black/10 px-3 py-2">
              <span className="text-sm font-semibold text-black">
                BIM selection (viewer)
              </span>
              <button
                type="button"
                onClick={() => void exportBimSelectionCsv()}
                disabled={bimTableRows.length === 0}
                className="rounded border border-black/20 bg-white px-3 py-1 text-sm font-medium text-black hover:bg-gray-100 disabled:cursor-not-allowed disabled:opacity-50"
              >
                Export CSV
              </button>
            </div>
            <div className="min-h-0 flex-1 overflow-auto p-2">
              {bimTableRows.length === 0 ? (
                <p className="py-4 text-center text-sm text-gray-600">
                  Select elements in the Model tab to populate this table.
                </p>
              ) : (
                <table className="w-full min-w-[720px] border-collapse text-sm text-black">
                  <thead>
                    <tr>
                      {BIM_TABLE_HEADERS.map((h) => (
                        <th
                          key={h}
                          className="sticky top-0 z-[1] border border-black/10 bg-gray-100 px-3 py-2 text-left text-sm font-semibold"
                        >
                          {h}
                        </th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {bimTableRows.map((row, idx) => (
                      <tr
                        key={`${row.elementId}-${idx}`}
                        className="hover:bg-gray-50/80"
                      >
                        <td className="border border-black/10 px-3 py-2 align-top">
                          {row.elementId}
                        </td>
                        <td className="border border-black/10 px-3 py-2 align-top">
                          {row.category}
                        </td>
                        <td className="border border-black/10 px-3 py-2 align-top">
                          {row.family}
                        </td>
                        <td className="border border-black/10 px-3 py-2 align-top">
                          {row.type}
                        </td>
                        <td className="border border-black/10 px-3 py-2 align-top">
                          {row.controlMark}
                        </td>
                        <td className="border border-black/10 px-3 py-2 align-top">
                          {row.controlNumber}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
            {bimExportNotice ? (
              <p className="shrink-0 border-t border-black/10 px-3 py-1.5 text-xs text-gray-600">
                {bimExportNotice}
              </p>
            ) : null}
          </section>
        </div>

          <div
            ref={productLayoutRef}
            className={`${
              workspaceMode === "product-analysis" ? "grid" : "hidden"
            } min-h-0 grid-cols-1`}
          >
            <div className="grid min-h-0 grid-cols-1 gap-3 md:grid-cols-3">
              <section className="flex min-h-0 flex-col overflow-hidden rounded border border-black/10 bg-white p-3 text-black">
                <h3 className="mb-2 text-sm font-semibold">Product Input</h3>
                <div className="min-h-0 flex-1 overflow-y-auto pr-1">
                  <label className="mb-2 flex flex-col gap-1 text-xs">
                    <span className="font-medium">Select Design File</span>
                    <select
                      value={selectedDesignFileBase}
                      onChange={(e) => setSelectedDesignFileBase(e.target.value)}
                      className="rounded border px-2 py-1.5 text-black bg-white"
                      disabled={loadingDesignFiles || groupedDesignFiles.length === 0}
                    >
                      <option value="">
                        {loadingDesignFiles ? "Loading files..." : "Select design file"}
                      </option>
                      {groupedDesignFiles.map((group) => (
                        <option key={group.baseName} value={group.baseName}>
                          {group.baseName}
                          {group.versions.some((v) => v.isLatest)
                            ? " (latest available)"
                            : ""}
                        </option>
                      ))}
                    </select>
                  </label>
                  <label className="mb-2 flex flex-col gap-1 text-xs">
                    <span className="font-medium">Version</span>
                    <select
                      value={selectedDesignFileKey}
                      onChange={(e) => setSelectedDesignFileKey(e.target.value)}
                      className="rounded border px-2 py-1.5 text-black bg-white"
                      disabled={!selectedDesignFileBase || selectedDesignFileVersions.length === 0}
                    >
                      <option value="">
                        {selectedDesignFileBase
                          ? "Select version"
                          : "Select design file first"}
                      </option>
                      {selectedDesignFileVersions.map((f) => (
                        <option key={f.objectKey} value={f.objectKey}>
                          {`v${String(f.version ?? 1).padStart(4, "0")}${
                            f.isLatest ? " (latest)" : ""
                          }${f.source === "local-fallback" ? " (local)" : ""}`}
                        </option>
                      ))}
                    </select>
                  </label>
                  <button
                    type="button"
                    onClick={() => void loadDesignFiles()}
                    className="mb-3 rounded border px-2 py-1 text-xs hover:bg-gray-100"
                  >
                    Refresh Design Files
                  </button>
                  <div className="mb-3 rounded border border-black/10 bg-gray-50 p-2 text-xs">
                    <div className="mb-1 font-medium">Design File Storage</div>
                    <div>Storage: {designFilesStorage || "(unknown)"}</div>
                    <div>Bucket: {designFilesBucket || "(not resolved)"}</div>
                    <div>Prefix: {designFilesPrefix || "(not resolved)"}</div>
                    {designFilesDiagnostics.length > 0 ? (
                      <div className="mt-1 text-red-700">
                        {designFilesDiagnostics.join(" | ")}
                      </div>
                    ) : null}
                  </div>
                  <div className="mb-3 rounded border border-black/10 bg-gray-50 p-2 text-xs">
                    <div className="mb-1 font-medium">Selection Context</div>
                    <div>Hub: {selectedHubData?.name ?? "(none selected)"}</div>
                    <div>Project: {selectedProjectData?.name ?? "(none selected)"}</div>
                    <div>Model: {selectedModelData?.name ?? "(none selected)"}</div>
                  </div>
                  {designFilesError ? (
                    <div className="mb-2 rounded border border-red-300 bg-red-50 px-2 py-1 text-xs text-red-700">
                      {designFilesError}
                    </div>
                  ) : null}
                  <div className="mb-2 grid gap-2 text-xs">
                <label className="flex flex-col gap-1">
                  <span>Governing Code</span>
                  <input
                    value={analysisInputs.governingCode}
                    onChange={(e) => updateAnalysisInput("governingCode", e.target.value)}
                    className="rounded border px-2 py-1"
                  />
                </label>
                <label className="flex flex-col gap-1">
                  <span>Risk Category</span>
                  <input
                    value={analysisInputs.riskCategory}
                    onChange={(e) => updateAnalysisInput("riskCategory", e.target.value)}
                    className="rounded border px-2 py-1"
                  />
                </label>
                <label className="flex flex-col gap-1">
                  <span>Exposure Category</span>
                  <input
                    value={analysisInputs.exposureCategory}
                    onChange={(e) => updateAnalysisInput("exposureCategory", e.target.value)}
                    className="rounded border px-2 py-1"
                  />
                </label>
                <label className="flex flex-col gap-1">
                  <span>Wind Speed (mph)</span>
                  <input
                    value={analysisInputs.windSpeedMph}
                    onChange={(e) => updateAnalysisInput("windSpeedMph", e.target.value)}
                    className="rounded border px-2 py-1"
                  />
                </label>
                <label className="flex flex-col gap-1">
                  <span>Seismic Design Category</span>
                  <input
                    value={analysisInputs.seismicDesignCategory}
                    onChange={(e) =>
                      updateAnalysisInput("seismicDesignCategory", e.target.value)
                    }
                    className="rounded border px-2 py-1"
                  />
                </label>
                <label className="flex flex-col gap-1">
                  <span>Concrete Strength f&apos;c (psi)</span>
                  <input
                    value={analysisInputs.concreteStrengthPsi}
                    onChange={(e) =>
                      updateAnalysisInput("concreteStrengthPsi", e.target.value)
                    }
                    className="rounded border px-2 py-1"
                  />
                </label>
                <label className="flex flex-col gap-1">
                  <span>Reinforcement Grade</span>
                  <input
                    value={analysisInputs.reinforcementGrade}
                    onChange={(e) =>
                      updateAnalysisInput("reinforcementGrade", e.target.value)
                    }
                    className="rounded border px-2 py-1"
                  />
                </label>
                <label className="flex flex-col gap-1">
                  <span>Fire Resistance (hours)</span>
                  <input
                    value={analysisInputs.fireResistanceHours}
                    onChange={(e) =>
                      updateAnalysisInput("fireResistanceHours", e.target.value)
                    }
                    className="rounded border px-2 py-1"
                  />
                </label>
                <label className="flex flex-col gap-1">
                  <span>Span (ft)</span>
                  <input
                    value={analysisInputs.spanFt}
                    onChange={(e) => updateAnalysisInput("spanFt", e.target.value)}
                    className="rounded border px-2 py-1"
                  />
                </label>
                <label className="flex flex-col gap-1">
                  <span>Uniform Load (kip/ft)</span>
                  <input
                    value={analysisInputs.uniformLoadKipFt}
                    onChange={(e) =>
                      updateAnalysisInput("uniformLoadKipFt", e.target.value)
                    }
                    className="rounded border px-2 py-1"
                  />
                </label>
                  </div>
                </div>
                <button
                  type="button"
                  onClick={() => void onCommitAndRun()}
                  disabled={!selectedDesignFileKey || runningAnalysis}
                  className="mt-2 rounded border border-black/20 bg-black px-2 py-1.5 text-xs font-medium text-white hover:bg-black/85 disabled:cursor-not-allowed disabled:opacity-60"
                >
                  {runningAnalysis ? "Running..." : "Commit and Run"}
                </button>
              </section>

              <section className="flex min-h-0 flex-col overflow-hidden rounded border border-black/10 bg-white p-3 text-black md:col-span-2">
                <h3 className="mb-2 text-sm font-semibold">Design View</h3>
                <div className="min-h-0 flex-1 overflow-auto rounded border border-black/10 bg-gray-50 p-2 text-xs">
                  {selectedDesignFileData?.products?.length ? (
                    <DesignViewSplit3D products={selectedDesignFileData.products} />
                  ) : (
                    <p>Select a design file with products to render the BIM design views.</p>
                  )}
                </div>
              </section>
            </div>

            <div
              role="separator"
              aria-label="Resize top panes"
              onMouseDown={(e) => startPaneResize("top", e.clientY)}
              className={`mx-1 flex cursor-row-resize items-center justify-center rounded transition-colors ${
                activeResizeHandle === "top"
                  ? "bg-gray-300"
                  : "bg-gray-200 hover:bg-gray-300"
              }`}
            >
              <div className="flex items-center gap-1 rounded-full border border-black/15 bg-white/85 px-2 py-0.5 shadow-sm">
                <span className="h-1 w-1 rounded-full bg-gray-500" />
                <span className="h-1 w-1 rounded-full bg-gray-500" />
                <span className="h-1 w-1 rounded-full bg-gray-500" />
              </div>
            </div>

            <section className="flex min-h-0 flex-col overflow-hidden rounded border border-black/10 bg-white p-3 text-black">
              <h3 className="mb-2 text-sm font-semibold">Product Analysis</h3>
              <div className="min-h-0 flex-1 overflow-auto text-xs">
                {(() => {
                  if (!analysisResults || analysisResults.points.length === 0) {
                    return (
                      <div className="rounded border border-black/10 bg-gray-50 p-3 text-gray-600">
                        Click <span className="font-medium">Commit and Run</span> to
                        persist product input and generate analysis graphs.
                      </div>
                    );
                  }
                  const points = analysisResults.points;
                  const line = (arr: number[]) =>
                    arr
                      .map((y, i) => {
                        const yy = Number.isFinite(y) ? y : 0;
                        return `${(i / (arr.length - 1)) * 280},${80 - yy * 10}`;
                      })
                      .join(" ");
                  const chart = (
                    title: string,
                    series: number[],
                    color: string,
                  ) => (
                    <div className="rounded border border-black/10 bg-white p-2">
                      <div className="mb-1 font-medium">{title}</div>
                      <svg viewBox="0 0 280 80" className="h-20 w-full">
                        <line x1="0" y1="80" x2="280" y2="80" stroke="#bbb" />
                        <polyline
                          fill="none"
                          stroke={color}
                          strokeWidth="2"
                          points={line(series)}
                        />
                      </svg>
                    </div>
                  );
                  return (
                    <div className="space-y-2">
                      <div className="rounded border border-black/10 bg-gray-50 p-2">
                        <div className="font-medium">
                          Span: {analysisResults.span.toFixed(2)} ft | Uniform Load:{" "}
                          {analysisResults.uniformLoad.toFixed(3)} kip/ft
                        </div>
                      </div>
                      <div className="grid grid-cols-1 gap-2 md:grid-cols-4">
                        {chart(
                          "Deflection (Parabola)",
                          points.map((p) => p.deflection),
                          "#0f766e",
                        )}
                        {chart(
                          "Shear",
                          points.map((p) => p.shear),
                          "#1d4ed8",
                        )}
                        {chart(
                          "Warp",
                          points.map((p) => p.warp),
                          "#7c3aed",
                        )}
                        {chart(
                          "Stress",
                          points.map((p) => p.stress),
                          "#b45309",
                        )}
                      </div>
                    </div>
                  );
                })()}
              </div>
            </section>

            <div
              role="separator"
              aria-label="Resize analysis pane"
              onMouseDown={(e) => startPaneResize("analysis", e.clientY)}
              className={`mx-1 flex cursor-row-resize items-center justify-center rounded transition-colors ${
                activeResizeHandle === "analysis"
                  ? "bg-gray-300"
                  : "bg-gray-200 hover:bg-gray-300"
              }`}
            >
              <div className="flex items-center gap-1 rounded-full border border-black/15 bg-white/85 px-2 py-0.5 shadow-sm">
                <span className="h-1 w-1 rounded-full bg-gray-500" />
                <span className="h-1 w-1 rounded-full bg-gray-500" />
                <span className="h-1 w-1 rounded-full bg-gray-500" />
              </div>
            </div>

            <section className="flex min-h-0 flex-col overflow-hidden rounded border border-black/10 bg-white p-3 text-black">
              <h3 className="mb-2 text-sm font-semibold">Product Data</h3>
              <div className="min-h-0 flex-1 overflow-auto rounded border border-black/10 bg-gray-50 p-2 text-xs">
                {primaryDesignProduct ? (
                  <table className="w-full border-collapse">
                    <tbody>
                      {Object.entries(primaryDesignProduct).map(([key, value]) => (
                        <tr key={key}>
                          <td className="border border-black/10 px-2 py-1 font-medium">
                            {key}
                          </td>
                          <td className="border border-black/10 px-2 py-1">
                            {typeof value === "object"
                              ? JSON.stringify(value)
                              : String(value ?? "")}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                ) : (
                  <p>Select a design file to view product data.</p>
                )}
              </div>
            </section>
          </div>

          <div
            className={`${
              workspaceMode === "admin" ? "grid" : "hidden"
            } min-h-0 grid-rows-1 ${
              workspaceExpanded ? "h-[calc(100%-2.25rem)]" : "h-[72vh] min-h-[780px]"
            }`}
          >
            <div className="grid h-full min-h-0 grid-cols-[minmax(280px,1fr)_minmax(0,3fr)] gap-3 overflow-hidden">
              <section className="flex h-full min-h-0 flex-col overflow-hidden rounded border border-black/10 bg-white p-3 text-black">
                <h3 className="mb-2 text-sm font-semibold">AI Chat</h3>
                <div className="mb-2 grid grid-cols-3 gap-2">
                  <div className="flex flex-col gap-1 text-xs">
                    <span className="font-medium">Assistant</span>
                    <div className="rounded border px-2 py-1.5 text-black bg-white">
                      Monty
                    </div>
                  </div>
                  <label className="flex flex-col gap-1 text-xs">
                    <span className="font-medium">Service</span>
                    <select
                      value={aiProvider}
                      onChange={(e) => {
                        const nextProvider = e.target.value as AiProvider;
                        setAiProvider(nextProvider);
                        setAiModel(AI_MODEL_OPTIONS[nextProvider][0]);
                      }}
                      className="rounded border px-2 py-1.5 text-black bg-white"
                    >
                      <option value="xai">xAI (Grok)</option>
                      <option value="openai">OpenAI</option>
                      <option value="cursor">Cursor Beta</option>
                    </select>
                  </label>
                  <label className="flex flex-col gap-1 text-xs">
                    <span className="font-medium">Model</span>
                    <select
                      value={aiModel}
                      onChange={(e) => setAiModel(e.target.value)}
                      className="rounded border px-2 py-1.5 text-black bg-white"
                    >
                      {aiModelOptions.map((modelName) => (
                        <option key={modelName} value={modelName}>
                          {modelName}
                        </option>
                      ))}
                    </select>
                  </label>
                </div>
                <div className="mb-2 min-h-0 flex-1 overflow-y-auto rounded border border-black/10 bg-gray-50 p-2 text-sm text-black">
                  {chatLog.length === 0 ? (
                    <p className="text-black">
                      Ask things like: &quot;list projects in this hub&quot;,
                      &quot;add user@company.com to Project A&quot;, or
                      &quot;show project admin summary&quot;.
                    </p>
                  ) : (
                    chatLog.map((line, idx) => (
                      <p key={`${line}-${idx}`} className="mb-1">
                        {line}
                      </p>
                    ))
                  )}
                  {chatPending ? (
                    <p className="mt-2 text-xs text-gray-600">AI: Monty is thinking...</p>
                  ) : null}
                </div>
                <div className="flex gap-2">
                  <textarea
                    ref={adminChatInputRef}
                    value={chatInput}
                    rows={1}
                    onChange={(e) => handleChatInputChange(e.target.value, "admin")}
                    disabled={chatPending}
                    onKeyDown={handleChatInputKeyDown}
                    className="min-h-[40px] flex-1 resize-none rounded border px-3 py-2 text-sm text-black placeholder:text-gray-600"
                    placeholder="Ask admin tasks..."
                  />
                  <button
                    onClick={() => {
                      if (chatPending) onStopChat();
                      else void onSendChat();
                    }}
                    className={`rounded px-3 py-2 text-sm text-white ${
                      chatPending
                        ? "bg-red-700 hover:bg-red-800"
                        : "bg-black hover:bg-gray-800"
                    }`}
                  >
                    {chatPending ? "Stop" : "Send"}
                  </button>
                </div>
              </section>

              <section className="flex h-full min-h-0 flex-col overflow-hidden rounded border border-black/10 bg-white p-3 text-black">
                <h3 className="mb-2 text-sm font-semibold">Admin Workspace</h3>
                <div className="min-h-0 flex-1 rounded border border-dashed border-black/20 bg-gray-50" />
              </section>
            </div>
          </div>
          <div
            className={`${
              workspaceMode === "form-builder" ? "grid" : "hidden"
            } min-h-0 grid-rows-1 ${
              workspaceExpanded ? "h-[calc(100%-2.25rem)]" : "h-[72vh] min-h-[780px]"
            }`}
          >
            <div className="grid h-full min-h-0 grid-cols-[minmax(280px,1fr)_minmax(0,3fr)] gap-3 overflow-hidden">
              <section className="flex h-full min-h-0 flex-col overflow-hidden rounded border border-black/10 bg-white p-3 text-black">
                <h3 className="mb-2 text-sm font-semibold">AI Chat</h3>
                <div className="mb-2 rounded border border-indigo-300 bg-indigo-50 px-2 py-1 text-[11px] text-indigo-900">
                  AI Form Builder Mode is active.
                </div>
                <div className="mb-2 grid grid-cols-3 gap-2">
                  <div className="flex flex-col gap-1 text-xs">
                    <span className="font-medium">Assistant</span>
                    <div className="rounded border px-2 py-1.5 text-black bg-white">
                      Monty
                    </div>
                  </div>
                  <label className="flex flex-col gap-1 text-xs">
                    <span className="font-medium">Service</span>
                    <select
                      value={aiProvider}
                      onChange={(e) => {
                        const nextProvider = e.target.value as AiProvider;
                        setAiProvider(nextProvider);
                        setAiModel(AI_MODEL_OPTIONS[nextProvider][0]);
                      }}
                      className="rounded border px-2 py-1.5 text-black bg-white"
                    >
                      <option value="xai">xAI (Grok)</option>
                      <option value="openai">OpenAI</option>
                      <option value="cursor">Cursor Beta</option>
                    </select>
                  </label>
                  <label className="flex flex-col gap-1 text-xs">
                    <span className="font-medium">Model</span>
                    <select
                      value={aiModel}
                      onChange={(e) => setAiModel(e.target.value)}
                      className="rounded border px-2 py-1.5 text-black bg-white"
                    >
                      {aiModelOptions.map((modelName) => (
                        <option key={modelName} value={modelName}>
                          {modelName}
                        </option>
                      ))}
                    </select>
                  </label>
                </div>
                <div className="mb-2 min-h-0 flex-1 overflow-y-auto rounded border border-black/10 bg-gray-50 p-2 text-sm text-black">
                  {chatLog.length === 0 ? (
                    <p className="text-black">
                      Ask things like: &quot;analyze this PDF for a safety form&quot; or
                      &quot;create the template from the latest analysis&quot;.
                    </p>
                  ) : (
                    chatLog.map((line, idx) => (
                      <p key={`${line}-${idx}`} className="mb-1">
                        {line}
                      </p>
                    ))
                  )}
                  {chatPending ? (
                    <p className="mt-2 text-xs text-gray-600">AI: Monty is thinking...</p>
                  ) : null}
                </div>
                <div className="flex gap-2">
                  <textarea
                    ref={adminChatInputRef}
                    value={chatInput}
                    rows={1}
                    onChange={(e) => handleChatInputChange(e.target.value, "admin")}
                    disabled={chatPending}
                    onKeyDown={handleChatInputKeyDown}
                    className="min-h-[40px] flex-1 resize-none rounded border px-3 py-2 text-sm text-black placeholder:text-gray-600"
                    placeholder="Ask form-builder tasks..."
                  />
                  <button
                    onClick={() => {
                      if (chatPending) onStopChat();
                      else void onSendChat();
                    }}
                    className={`rounded px-3 py-2 text-sm text-white ${
                      chatPending
                        ? "bg-red-700 hover:bg-red-800"
                        : "bg-black hover:bg-gray-800"
                    }`}
                  >
                    {chatPending ? "Stop" : "Send"}
                  </button>
                </div>
              </section>

              <section className="flex h-full min-h-0 flex-col overflow-hidden rounded border border-black/10 bg-white p-3 text-black">
                <div className="mb-2 flex items-center justify-between">
                  <h3 className="text-sm font-semibold">Admin Form Builder</h3>
                  <span className={`text-xs font-medium ${formBuilderStatusTone}`}>
                    {formBuilderAnalysis
                      ? `Status: ${formBuilderAnalysis.status}`
                      : "Status: Waiting"}
                  </span>
                </div>
                <div className="mb-3 grid grid-cols-2 gap-2 text-xs">
                  <label className="flex flex-col gap-1">
                    <span className="font-medium">Template Name</span>
                    <input
                      value={formBuilderTemplateName}
                      onChange={(e) => setFormBuilderTemplateName(e.target.value)}
                      className="rounded border px-2 py-1.5"
                      placeholder="e.g. Daily Safety Walk"
                    />
                  </label>
                  <label className="flex flex-col gap-1">
                    <span className="font-medium">Template Type</span>
                    <select
                      value={formBuilderTemplateType}
                      onChange={(e) =>
                        setFormBuilderTemplateType(e.target.value as FormTemplateType)
                      }
                      className="rounded border px-2 py-1.5 bg-white"
                    >
                      <option value="custom">custom</option>
                      <option value="inspection">inspection</option>
                      <option value="quality">quality</option>
                      <option value="safety">safety</option>
                      <option value="daily_report">daily_report</option>
                    </select>
                  </label>
                </div>

                <div className="mb-3 rounded border border-black/10 bg-gray-50 p-2 text-xs">
                  <div className="mb-2 flex flex-wrap gap-2">
                    <button
                      onClick={() => setFormBuilderSourceType("upload")}
                      className={`rounded border px-2 py-1 ${
                        formBuilderSourceType === "upload"
                          ? "bg-black text-white"
                          : "bg-white"
                      }`}
                    >
                      Upload PDF
                    </button>
                    <button
                      onClick={() => setFormBuilderSourceType("acc-version")}
                      className={`rounded border px-2 py-1 ${
                        formBuilderSourceType === "acc-version"
                          ? "bg-black text-white"
                          : "bg-white"
                      }`}
                    >
                      ACC Docs Version
                    </button>
                  </div>
                  {formBuilderSourceType === "upload" ? (
                    <div className="flex flex-wrap items-center gap-2">
                      <button
                        type="button"
                        onClick={() => formBuilderUploadInputRef.current?.click()}
                        disabled={formBuilderUploadPending}
                        className="rounded border bg-white px-3 py-1.5 hover:bg-gray-100 disabled:opacity-50"
                      >
                        {formBuilderUploadPending ? "Uploading..." : "Select PDF"}
                      </button>
                      <input
                        ref={formBuilderUploadInputRef}
                        type="file"
                        accept="application/pdf,.pdf"
                        onChange={(e) => {
                          const file = e.target.files?.[0] ?? null;
                          if (!file) return;
                          void onUploadFormBuilderPdf(file);
                          e.currentTarget.value = "";
                        }}
                        className="hidden"
                      />
                      <span className="text-[11px] text-gray-700">
                        {formBuilderSelectedFileName ||
                          (formBuilderUploadPending ? "Uploading..." : "No file uploaded")}
                      </span>
                    </div>
                  ) : (
                    <div className="grid grid-cols-2 gap-2">
                      <label className="flex flex-col gap-1">
                        <span className="font-medium">Project</span>
                        <select
                          value={formBuilderProjectId}
                          onChange={(e) => setFormBuilderProjectId(e.target.value)}
                          className="rounded border px-2 py-1 bg-white"
                        >
                          <option value="">
                            {selectedHub
                              ? loadingProjects
                                ? "Loading projects..."
                                : "Select project"
                              : "Select hub first"}
                          </option>
                          {formBuilderProjectOptions.map((project) => (
                            <option key={project.id} value={project.id}>
                              {project.label}
                            </option>
                          ))}
                        </select>
                      </label>
                      <label className="flex flex-col gap-1">
                        <span className="font-medium">File</span>
                        <select
                          value={formBuilderVersionId}
                          onChange={(e) => setFormBuilderVersionId(e.target.value)}
                          className="rounded border px-2 py-1 bg-white"
                        >
                          <option value="">
                            {!formBuilderProjectId
                              ? "Select project first"
                              : loadingProjectBrowser
                                ? "Loading files..."
                                : "Select file"}
                          </option>
                          {formBuilderFileOptions.map((file) => (
                            <option key={file.versionId} value={file.versionId}>
                              {file.label}
                            </option>
                          ))}
                        </select>
                      </label>
                    </div>
                  )}
                </div>

                <div className="mb-3 flex flex-wrap gap-2 text-xs">
                  <button
                    onClick={() => void onAnalyzeFormBuilder()}
                    disabled={formBuilderAnalyzing || formBuilderUploadPending}
                    className="rounded border bg-white px-3 py-1.5 hover:bg-gray-100 disabled:opacity-50"
                  >
                    {formBuilderAnalyzing ? "Analyzing..." : "Analyze PDF"}
                  </button>
                  <button
                    onClick={() => void onPublishFormBuilder()}
                    disabled={
                      formBuilderPublishing ||
                      !formBuilderAnalysis ||
                      publishBlockedByReview
                    }
                    className="rounded bg-black px-3 py-1.5 text-white hover:bg-gray-800 disabled:opacity-50"
                  >
                    {formBuilderPublishing ? "Publishing..." : "Create Template"}
                  </button>
                  <span className="self-center text-[11px] text-gray-700">
                    Target: Account-level form library
                  </span>
                </div>
                {formBuilderError ? (
                  <p className="mb-2 rounded border border-red-300 bg-red-50 px-2 py-1 text-xs text-red-700">
                    {formBuilderError}
                  </p>
                ) : null}
                {formBuilderMessage ? (
                  <p className="mb-2 rounded border border-green-300 bg-green-50 px-2 py-1 text-xs text-green-700">
                    {formBuilderMessage}
                  </p>
                ) : null}
                <div className="min-h-0 flex-1 overflow-auto rounded border border-black/10 bg-gray-50 p-2 text-xs">
                  {!formBuilderAnalysis ? (
                    <p>No analysis yet. Upload/select a PDF, then click Analyze PDF.</p>
                  ) : (
                    <div className="space-y-2">
                      <div className="rounded border border-black/10 bg-white px-2 py-1 text-[11px]">
                        {`Fields: ${formBuilderAnalysis.extractedFieldCount} | Confidence: ${formBuilderAnalysis.confidenceScore}`}
                      </div>
                      {formBuilderAnalysis.reviewNotes.length > 0 ? (
                        <div className="rounded border border-amber-300 bg-amber-50 px-2 py-1 text-[11px] text-amber-900">
                          {formBuilderAnalysis.reviewNotes.join(" ")}
                        </div>
                      ) : null}
                      <table className="w-full border-collapse">
                        <thead>
                          <tr>
                            <th className="border border-black/10 px-2 py-1 text-left">Field</th>
                            <th className="border border-black/10 px-2 py-1 text-left">Type</th>
                            <th className="border border-black/10 px-2 py-1 text-left">Group</th>
                            <th className="border border-black/10 px-2 py-1 text-left">Calc</th>
                            <th className="border border-black/10 px-2 py-1 text-left">State</th>
                            <th className="border border-black/10 px-2 py-1 text-left">Conf.</th>
                            <th className="border border-black/10 px-2 py-1 text-left">Actions</th>
                          </tr>
                        </thead>
                        <tbody>
                          {formBuilderAnalysis.fields.map((field, fieldIdx) => (
                            <tr key={field.id}>
                              <td className="border border-black/10 px-2 py-1">
                                <input
                                  value={field.label}
                                  onChange={(e) =>
                                    setFormBuilderAnalysis((prev) =>
                                      prev
                                        ? {
                                            ...prev,
                                            fields: prev.fields.map((row) =>
                                              row.id === field.id
                                                ? { ...row, label: e.target.value }
                                                : row,
                                            ),
                                          }
                                        : prev,
                                    )
                                  }
                                  className="w-full rounded border px-1 py-0.5"
                                />
                              </td>
                              <td className="border border-black/10 px-2 py-1">
                                <select
                                  value={field.type}
                                  onChange={(e) =>
                                    setFormBuilderAnalysis((prev) =>
                                      prev
                                        ? {
                                            ...prev,
                                            fields: prev.fields.map((row) =>
                                              row.id === field.id
                                                ? {
                                                    ...row,
                                                    type: e.target.value as FormBuilderField["type"],
                                                  }
                                                : row,
                                            ),
                                          }
                                        : prev,
                                    )
                                  }
                                  className="rounded border px-1 py-0.5 bg-white"
                                >
                                  <option value="text">text</option>
                                  <option value="multiline_text">multiline_text</option>
                                  <option value="number">number</option>
                                  <option value="checkbox">checkbox</option>
                                  <option value="radio">radio</option>
                                  <option value="dropdown">dropdown</option>
                                  <option value="date">date</option>
                                  <option value="signature">signature</option>
                                  <option value="section_heading">section_heading</option>
                                  <option value="table">table</option>
                                </select>
                              </td>
                              <td className="border border-black/10 px-2 py-1">
                                <input
                                  value={field.groupKey ?? ""}
                                  onChange={(e) =>
                                    setFormBuilderAnalysis((prev) =>
                                      prev
                                        ? {
                                            ...prev,
                                            fields: prev.fields.map((row) =>
                                              row.id === field.id
                                                ? { ...row, groupKey: e.target.value }
                                                : row,
                                            ),
                                          }
                                        : prev,
                                    )
                                  }
                                  placeholder="(none)"
                                  className="w-24 rounded border px-1 py-0.5"
                                  title="Section/table group key — fields sharing a key are grouped"
                                />
                              </td>
                              <td className="border border-black/10 px-2 py-1 text-center">
                                <input
                                  type="checkbox"
                                  checked={Boolean(field.calculated)}
                                  onChange={(e) =>
                                    setFormBuilderAnalysis((prev) =>
                                      prev
                                        ? {
                                            ...prev,
                                            fields: prev.fields.map((row) =>
                                              row.id === field.id
                                                ? { ...row, calculated: e.target.checked }
                                                : row,
                                            ),
                                          }
                                        : prev,
                                    )
                                  }
                                  title="Calculated cell (builds a calculated table column)"
                                />
                              </td>
                              <td className="border border-black/10 px-2 py-1">
                                <select
                                  value={field.reviewState}
                                  onChange={(e) =>
                                    setFormBuilderAnalysis((prev) =>
                                      prev
                                        ? {
                                            ...prev,
                                            fields: prev.fields.map((row) =>
                                              row.id === field.id
                                                ? {
                                                    ...row,
                                                    reviewState:
                                                      e.target.value as FormBuilderField["reviewState"],
                                                  }
                                                : row,
                                            ),
                                          }
                                        : prev,
                                    )
                                  }
                                  className="rounded border px-1 py-0.5 bg-white"
                                >
                                  <option value="auto">auto</option>
                                  <option value="needs_review">needs_review</option>
                                  <option value="approved">approved</option>
                                  <option value="rejected">rejected</option>
                                </select>
                              </td>
                              <td className="border border-black/10 px-2 py-1">
                                {field.confidence}
                              </td>
                              <td className="border border-black/10 px-2 py-1">
                                <div className="flex items-center gap-1">
                                  <button
                                    type="button"
                                    onClick={() => moveFormBuilderField(field.id, -1)}
                                    disabled={fieldIdx === 0}
                                    title="Move up"
                                    className="rounded border px-1.5 py-0.5 hover:bg-gray-100 disabled:opacity-40"
                                  >
                                    ↑
                                  </button>
                                  <button
                                    type="button"
                                    onClick={() => moveFormBuilderField(field.id, 1)}
                                    disabled={
                                      fieldIdx === formBuilderAnalysis.fields.length - 1
                                    }
                                    title="Move down"
                                    className="rounded border px-1.5 py-0.5 hover:bg-gray-100 disabled:opacity-40"
                                  >
                                    ↓
                                  </button>
                                  <button
                                    type="button"
                                    onClick={() => removeFormBuilderField(field.id)}
                                    title="Remove field"
                                    className="rounded border border-red-300 px-1.5 py-0.5 text-red-700 hover:bg-red-50"
                                  >
                                    ✕
                                  </button>
                                </div>
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>

                      <div className="rounded border border-indigo-200 bg-indigo-50 p-2">
                        <div className="mb-1 flex items-center justify-between">
                          <span className="font-semibold text-indigo-900">
                            Forma structure preview
                          </span>
                          <span className="text-[11px] text-indigo-800">
                            {`${formBuilderStructurePreview.sectionCount} section(s), ${formBuilderStructurePreview.tableCount} table(s), ${formBuilderStructurePreview.calculatedColumnCount} calc col(s)`}
                          </span>
                        </div>
                        {formBuilderStructurePreview.elements.length === 0 ? (
                          <p className="text-[11px] text-indigo-800">
                            No structure yet. Set a shared Group key on related rows
                            (and mark calculated cells) to form sections/tables.
                          </p>
                        ) : (
                          <ul className="space-y-1 text-[11px] text-indigo-900">
                            {formBuilderStructurePreview.elements.map((element, idx) => (
                              <li
                                key={`${element.kind}-${idx}`}
                                className="rounded border border-indigo-200 bg-white px-2 py-1"
                              >
                                {element.kind === "field" ? (
                                  <span>
                                    Field · {element.label}{" "}
                                    <span className="text-gray-500">({element.type})</span>
                                  </span>
                                ) : element.kind === "section" ? (
                                  <span>
                                    Section{" "}
                                    <span className="font-medium">
                                      {element.entryMode === "multiple"
                                        ? "(multiple entries)"
                                        : "(single)"}
                                    </span>{" "}
                                    · {element.title}{" "}
                                    <span className="text-gray-500">
                                      ({element.fieldCount} fields)
                                    </span>
                                  </span>
                                ) : (
                                  <span>
                                    Table · {element.title}{" "}
                                    <span className="text-gray-500">
                                      ({element.columnCount} cols
                                      {element.calculatedColumns > 0
                                        ? `, ${element.calculatedColumns} calculated`
                                        : ""}
                                      )
                                    </span>
                                  </span>
                                )}
                              </li>
                            ))}
                          </ul>
                        )}
                      </div>
                    </div>
                  )}
                </div>
              </section>
            </div>
          </div>
      </div>
      {showDesignFileModal ? (
        <div className="fixed inset-0 z-[70] flex items-center justify-center bg-black/30">
          <div className="w-full max-w-md rounded border border-black/20 bg-white p-4 text-black shadow-xl">
            <h3 className="mb-2 text-sm font-semibold">Create Design File</h3>
            <label className="mb-3 flex flex-col gap-1 text-xs">
              <span className="font-medium">File name</span>
              <input
                value={designFileNameInput}
                onChange={(e) => setDesignFileNameInput(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === "Enter") {
                    void onDesignProduct(designFileNameInput);
                  }
                }}
                autoFocus
                placeholder="e.g. L2_StackedPanels_SetA"
                className="rounded border bg-white px-2 py-1.5 text-sm text-black placeholder:text-gray-500"
              />
            </label>
            <div className="mb-3 max-h-40 overflow-auto rounded border border-black/10 bg-gray-50 p-2 text-xs">
              <div className="mb-1 font-medium">
                Selected Pieces (for naming/reference)
              </div>
              {selectedPieceRefs.length === 0 ? (
                <p>No elements selected.</p>
              ) : (
                <table className="w-full border-collapse">
                  <thead>
                    <tr>
                      <th className="border border-black/10 px-2 py-1 text-left">
                        PieceID (CONTROL_MARK)
                      </th>
                      <th className="border border-black/10 px-2 py-1 text-left">
                        dbId
                      </th>
                      <th className="border border-black/10 px-2 py-1 text-left">
                        External ID
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {selectedPieceRefs.map((p) => (
                      <tr key={`${p.dbId}-${p.externalId}`}>
                        <td className="border border-black/10 px-2 py-1">
                          PieceID:{p.pieceId}
                        </td>
                        <td className="border border-black/10 px-2 py-1">{p.dbId}</td>
                        <td className="border border-black/10 px-2 py-1">{p.externalId}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
            {designFileModalError ? (
              <div className="mb-3 rounded border border-red-300 bg-red-50 px-2 py-1 text-xs text-red-700">
                {designFileModalError}
              </div>
            ) : null}
            <div className="flex items-center justify-end gap-2">
              <button
                type="button"
                onClick={() => {
                  setShowDesignFileModal(false);
                  setDesignFileNameInput("");
                  setDesignFileModalError("");
                }}
                className="rounded border px-3 py-1 text-xs hover:bg-gray-100"
              >
                Cancel
              </button>
              <button
                type="button"
                onClick={() => void onDesignProduct(designFileNameInput)}
                disabled={savingDesignFile}
                className="rounded bg-black px-3 py-1 text-xs text-white hover:bg-gray-800 disabled:opacity-60"
              >
                {savingDesignFile ? "Saving..." : "Save Design File"}
              </button>
            </div>
          </div>
        </div>
      ) : null}
    </main>
  );
}

