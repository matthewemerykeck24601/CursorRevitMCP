"use client";

import { useCallback, useEffect, useMemo, useRef, useState } from "react";
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
} from "@/lib/bimSelectionTable";
import type { DiscoveryCachedSelection } from "@/lib/discovery-cached-selection";

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

type SessionResponse = {
  authenticated: boolean;
  expiresAt: number;
  scope: string;
};
type WorkspaceTab = "viewer" | "model-data" | "element-data" | "issues";
type WorkspaceMode = "model" | "product-analysis";
type ModelDataRow = { key: string; value: string; source: string };
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

  const selectedModelData = useMemo(
    () => models.find((m) => m.itemId === selectedModel) ?? null,
    [models, selectedModel],
  );
  const aiModelOptions = AI_MODEL_OPTIONS[aiProvider];
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
    const fetchOne = async (el: SelectedElementSnapshot) => {
      const url = `/api/aps/projects/${projectPath}/aec-element?hubId=${hubQ}&itemId=${itemQ}&externalId=${encodeURIComponent(el.externalId ?? "")}&dbId=${encodeURIComponent(String(el.dbId))}&elementName=${encodeURIComponent(el.name ?? "")}`;
      const response = await fetch(url);
      if (!response.ok) return { dbId: el.dbId, rows: [] as ModelDataRow[] };
      const json = (await response.json()) as { rows?: ModelDataRow[] };
      return { dbId: el.dbId, rows: json.rows ?? [] };
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
    void loadModels(selectedHub, selectedProject).catch((err) => {
      setLoadingModels(false);
      setError(err instanceof Error ? err.message : "Failed loading models");
    });
  }, [selectedHub, selectedProject, loadModels]);

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
    const msg = chatInput.trim();
    if (!msg) return;
    setChatInput("");
    setChatLog((prev) => [...prev, `You: ${msg}`]);
    setError("");

    const response = await fetch("/api/chat", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
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
        discoveryCachedSelection: discoveryCachedSelection ?? undefined,
        lastDaJob: lastDaJob ?? undefined,
        assistantMode,
        aiProvider,
        aiModel,
        workspaceMode,
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
      }),
    });

    if (!response.ok) {
      setError("Chat request failed");
      return;
    }

    const json = (await response.json()) as {
      message: string;
      actions: ViewerAction[];
      queryResult?: { views?: Array<{ guid: string; name: string; role: string }> };
      discoveryCachedSelection?: DiscoveryCachedSelection | null;
      lastDaJob?: {
        workitem_id: string;
        submitted_at?: string;
        cache_id?: string;
      } | null;
    };
    if ("discoveryCachedSelection" in json) {
      setDiscoveryCachedSelection(json.discoveryCachedSelection ?? null);
    }
    if ("lastDaJob" in json) {
      setLastDaJob(json.lastDaJob ?? null);
    }
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
  }

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
            disabled={!selectedHub || loadingProjects}
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
            disabled={!selectedProject || loadingModels}
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
              Model
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
                  Alice
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
            </div>
            <div className="flex gap-2">
              <input
                value={chatInput}
                onChange={(e) => setChatInput(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === "Enter") void onSendChat();
                }}
                className="flex-1 rounded border px-3 py-2 text-sm text-black placeholder:text-gray-600"
                placeholder="Ask the model..."
              />
              <button
                onClick={() => void onSendChat()}
                className="rounded bg-black px-3 py-2 text-sm text-white hover:bg-gray-800"
              >
                Send
              </button>
            </div>
            </section>

            <section className="flex h-full min-h-0 flex-col overflow-hidden rounded border border-black/10 bg-white p-3 text-black">
            <div className="mb-2 flex items-center gap-2">
              <button
                onClick={() => setWorkspaceTab("viewer")}
                className={`rounded border px-2 py-1 text-xs ${
                  workspaceTab === "viewer" ? "bg-black text-white" : "bg-white"
                }`}
              >
                Viewer
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

            <div className={workspaceTab === "viewer" ? "min-h-0 flex-1" : "hidden min-h-0 flex-1"}>
              <ViewerPanel
                ref={viewerPanelRef}
                viewerUrn={selectedModelData?.viewerUrn ?? null}
                isActive={workspaceMode === "model" && workspaceTab === "viewer"}
                actions={viewerActions}
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
                      {elementDataRows.map((r, idx) => (
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
                  Select elements in the Viewer tab to populate this table.
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
            className={`${
              workspaceMode === "product-analysis" ? "grid" : "hidden"
            } min-h-0 grid-cols-1`}
            style={{
              height: productLayoutHeight,
              minHeight: "780px",
              gridTemplateRows: `${productTopPaneHeight}px ${productLayoutHandleHeight}px ${productAnalysisPaneHeight}px ${productLayoutHandleHeight}px ${productDataPaneHeight}px`,
            }}
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

