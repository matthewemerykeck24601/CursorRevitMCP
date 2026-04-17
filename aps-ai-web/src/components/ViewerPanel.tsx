"use client";

import {
  forwardRef,
  useCallback,
  useEffect,
  useImperativeHandle,
  useRef,
  useState,
} from "react";
import type { MetromontPieceKind } from "@/lib/metromont-viewer-filter";
import {
  matchesMetromontPieceKind,
  propsMapFromViewerProperties,
} from "@/lib/metromont-viewer-filter";
import {
  FORGE_VIEWER_BULK_PROPERTY_NAMES,
  mergeForgeBulkProperties,
} from "@/lib/forge-revit-object-name";

const VIEWER_SCRIPT_URL =
  "https://developer.api.autodesk.com/modelderivative/v2/viewers/7.*/viewer3D.min.js";
const VIEWER_STYLE_URL =
  "https://developer.api.autodesk.com/modelderivative/v2/viewers/7.*/style.min.css";

type ViewerInstance = {
  start: () => number;
  finish: () => void;
  resize: () => void;
  addEventListener: (eventId: number, callback: (...args: unknown[]) => void) => void;
  removeEventListener: (
    eventId: number,
    callback: (...args: unknown[]) => void,
  ) => void;
  getSelection: () => number[];
  getProperties: (
    dbId: number,
    onSuccess: (result: {
      dbId: number;
      name?: string;
      externalId?: string;
      properties?: Array<{
        displayName?: string;
        displayValue?: unknown;
        units?: string;
      }>;
    }) => void,
    onError: (error: unknown) => void,
  ) => void;
  setTheme: (theme: string) => void;
  loadDocumentNode: (doc: unknown, node: unknown) => Promise<unknown>;
  fitToView: () => void;
  clearSelection: () => void;
  isolate: (ids?: number[]) => void;
  hide?: (ids: number[]) => void;
  showAll?: () => void;
  setGhosting?: (enabled: boolean) => void;
  search: (
    text: string,
    onSuccess: (dbIds: number[]) => void,
    onError: (error: unknown) => void,
  ) => void;
  select: (ids: number[]) => void;
  loadExtension?: (extensionId: string, options?: unknown) => Promise<unknown>;
  getExtension?: (extensionId: string) => unknown;
};

type ViewOption = {
  key: string;
  name: string;
  node: unknown;
};

type MarkupsCoreExtension = {
  enterEditMode?: () => void;
  leaveEditMode?: () => void;
  generateData?: () => string;
  loadMarkups?: (data: string, layerName?: string) => void;
  clear?: () => void;
};

export type ViewerAction =
  | { type: "viewer.fitToView" }
  | { type: "viewer.clearSelection" }
  | { type: "viewer.isolateSelection" }
  | { type: "viewer.search"; query: string }
  | { type: "viewer.isolateByQuery"; query: string }
  | { type: "viewer.hideByQuery"; query: string }
  | { type: "viewer.showAll" }
  | { type: "viewer.setGhosting"; enabled: boolean }
  | { type: "viewer.markupsSave" }
  | { type: "viewer.markupsLoad" }
  | { type: "viewer.markupsClear" }
  | {
      type: "viewer.selectDbIds";
      dbIds: number[];
      clearFirst?: boolean;
      fitToView?: boolean;
    }
  | { type: "viewer.isolateDbIds"; dbIds: number[] }
  | {
      type: "viewer.searchAndSelectMetromontPieces";
      query: string;
      pieceKind: MetromontPieceKind;
      clearFirst?: boolean;
      fitToView?: boolean;
      /** Cap Forge text-search hits before property filter (default 3500). */
      maxSearchMatches?: number;
    };

export type ViewerPanelHandle = {
  /** APS Viewer selection by dbId; same behavior as chat `viewer.selectDbIds` actions. */
  selectElements: (
    dbIds: Array<string | number>,
    options?: { clearFirst?: boolean; zoomToSelection?: boolean },
  ) => void;
};

export type SelectedElementSnapshot = {
  dbId: number;
  name?: string;
  externalId?: string;
  properties: Array<{
    displayName: string;
    displayValue: string;
    units?: string;
  }>;
};

type ViewerPanelProps = {
  viewerUrn: string | null;
  isActive?: boolean;
  mode?: "full" | "minimal";
  title?: string;
  actions: ViewerAction[];
  onActionComplete: () => void;
  onViewerFeedback?: (message: string) => void;
  onSelectionChange?: (ids: number[]) => void;
  onSelectionDetails?: (elements: SelectedElementSnapshot[]) => void;
};

export const ViewerPanel = forwardRef<ViewerPanelHandle, ViewerPanelProps>(
  function ViewerPanel(
    {
      viewerUrn,
      isActive = true,
      mode = "full",
      title = "Model Viewer",
      actions,
      onActionComplete,
      onViewerFeedback,
      onSelectionChange,
      onSelectionDetails,
    },
    ref,
  ) {
  const onViewerFeedbackRef = useRef(onViewerFeedback);
  onViewerFeedbackRef.current = onViewerFeedback;

  const containerRef = useRef<HTMLDivElement | null>(null);
  const viewerRef = useRef<ViewerInstance | null>(null);
  const documentRef = useRef<unknown>(null);
  const selectionEventIdRef = useRef<number | null>(null);
  const lastSelectionKeyRef = useRef<string>("");
  const [status, setStatus] = useState("Viewer idle");
  const [viewOptions, setViewOptions] = useState<ViewOption[]>([]);
  const [activeViewKey, setActiveViewKey] = useState("");
  const [markupsEnabled, setMarkupsEnabled] = useState(false);
  const [markupSnapshot, setMarkupSnapshot] = useState("");
  const runSelectElements = useCallback(
    (
      dbIds: Array<string | number>,
      options: { clearFirst: boolean; zoomToSelection: boolean },
    ) => {
      const viewer = viewerRef.current;
      if (!viewer) return;
      const ids = dbIds
        .map((x) => (typeof x === "number" ? x : Number(x)))
        .filter((n) => Number.isFinite(n))
        .map((n) => Math.trunc(n))
        .slice(0, 500);
      if (options.clearFirst) {
        viewer.clearSelection();
      }
      if (ids.length > 0) {
        viewer.select(ids);
      }
      if (options.zoomToSelection && ids.length > 0) {
        viewer.fitToView();
      }
      onViewerFeedbackRef.current?.(
        ids.length === 0
          ? "selectElements: no valid dbIds."
          : `Selected ${ids.length} element(s) by dbId${options.zoomToSelection ? " (fit view)." : "."}`,
      );
    },
    [],
  );

  useImperativeHandle(
    ref,
    () => ({
      selectElements: (
        dbIds: Array<string | number>,
        options?: { clearFirst?: boolean; zoomToSelection?: boolean },
      ) => {
        runSelectElements(dbIds, {
          clearFirst: options?.clearFirst !== false,
          zoomToSelection: options?.zoomToSelection === true,
        });
      },
    }),
    [runSelectElements],
  );

  const emitSelection = useCallback(() => {
    const ids = viewerRef.current?.getSelection() ?? [];
    const key = ids.join(",");
    if (key === lastSelectionKeyRef.current) return;
    lastSelectionKeyRef.current = key;
    onSelectionChange?.(ids);

    if (!onSelectionDetails) return;
    if (ids.length === 0 || !viewerRef.current) {
      onSelectionDetails([]);
      return;
    }

    // Keep this comfortably above common piece-set sizes so DA cache
    // captures the full viewer selection rather than a truncated subset.
    const idsToLoad = ids.slice(0, 500);
    type PropRow = {
      displayName: string;
      displayValue: string;
      units?: string;
    };
    const readOne = (dbId: number) =>
      new Promise<SelectedElementSnapshot>((resolve) => {
        const viewer = viewerRef.current;
        if (!viewer) {
          resolve({ dbId, properties: [] });
          return;
        }
        viewer.getProperties(
          dbId,
          (result) => {
            let properties: PropRow[] = (result.properties ?? [])
              .filter((p) => p.displayName)
              .map((p) => ({
                displayName: String(p.displayName),
                displayValue: String(p.displayValue ?? ""),
                units: p.units,
              }));
            const norm = (s: string) =>
              s.trim().toLowerCase().replace(/\s+/g, " ");
            const keys = new Set(properties.map((p) => norm(p.displayName)));
            if (result.externalId && !keys.has("external id")) {
              properties.unshift({
                displayName: "External ID",
                displayValue: String(result.externalId),
                units: undefined,
              });
            }

            const deliver = () => {
              resolve({
                dbId: result.dbId,
                name: result.name,
                externalId: result.externalId,
                properties,
              });
            };

            const model = (
              viewer as unknown as {
                model?: {
                  getBulkProperties?: (
                    dbIds: number[],
                    spec: readonly string[] | { propFilter: readonly string[] },
                    onSuccess: (
                      rows: Array<{
                        dbId: number;
                        properties?: Array<{
                          displayName?: string;
                          displayValue?: unknown;
                          attributeName?: string;
                          units?: string;
                        }>;
                      }>,
                    ) => void,
                    onError?: (err?: unknown) => void,
                  ) => void;
                };
              }
            ).model;

            const mergeFromBulk = (
              bulkResults: Array<{
                dbId: number;
                properties?: Array<{
                  displayName?: string;
                  displayValue?: unknown;
                  attributeName?: string;
                  units?: string;
                }>;
              }>,
            ) => {
              const row = bulkResults?.[0];
              if (row?.properties?.length) {
                properties = mergeForgeBulkProperties(properties, row.properties);
              }
              deliver();
            };

            const bulk = model?.getBulkProperties;
            const names = [...FORGE_VIEWER_BULK_PROPERTY_NAMES];
            if (model && typeof bulk === "function") {
              bulk.call(
                model,
                [dbId],
                { propFilter: names },
                mergeFromBulk,
                () => {
                  bulk.call(
                    model,
                    [dbId],
                    names,
                    mergeFromBulk,
                    deliver,
                  );
                },
              );
            } else {
              deliver();
            }
          },
          () => {
            resolve({
              dbId,
              properties: [],
            });
          },
        );
      });

    void Promise.all(idsToLoad.map((id) => readOne(id))).then((elements) => {
      onSelectionDetails(elements);
    });
  }, [onSelectionChange, onSelectionDetails]);

  const loadViewerAssets = useCallback(async () => {
    const ensureViewerScript = async () => {
      const existing = document.querySelector(
        `script[src="${VIEWER_SCRIPT_URL}"]`,
      ) as HTMLScriptElement | null;
      if (existing && window.Autodesk?.Viewing) return;

      if (existing && !window.Autodesk?.Viewing) {
        existing.remove();
      }

      await new Promise<void>((resolve, reject) => {
        const script = document.createElement("script");
        script.src = VIEWER_SCRIPT_URL;
        script.onload = () => {
          if (window.Autodesk?.Viewing) resolve();
          else reject(new Error("APS viewer script loaded but Autodesk API was unavailable"));
        };
        script.onerror = () => reject(new Error("Failed to load APS viewer script"));
        document.head.appendChild(script);
      });
    };

    await ensureViewerScript();

    if (!document.querySelector(`link[href="${VIEWER_STYLE_URL}"]`)) {
      const link = document.createElement("link");
      link.rel = "stylesheet";
      link.href = VIEWER_STYLE_URL;
      document.head.appendChild(link);
    }
  }, []);

  const getViewOptionsFromDocument = useCallback((doc: unknown): ViewOption[] => {
    const root = (doc as { getRoot?: () => unknown })?.getRoot?.();
    if (!root) return [];
    const docApi = (window.Autodesk?.Viewing as unknown as {
      Document?: {
        getSubItemsWithProperties?: (
          rootNode: unknown,
          filter: Record<string, unknown>,
          recursive: boolean,
        ) => unknown[];
      };
    })?.Document;
    const nodes =
      docApi?.getSubItemsWithProperties?.(root, { type: "geometry" }, true) ?? [];
    return nodes.map((node, idx) => {
      const typed = node as { guid?: string; id?: string | number; name?: string };
      return {
        key: String(typed.guid ?? typed.id ?? idx),
        name: typed.name || `View ${idx + 1}`,
        node,
      };
    });
  }, []);

  const loadViewNode = useCallback(
    async (doc: unknown, node: unknown, viewKey: string) => {
      if (!viewerRef.current) return;
      await viewerRef.current.loadDocumentNode(doc, node);
      setActiveViewKey(viewKey);
      setStatus("Model loaded");
      emitSelection();
    },
    [emitSelection],
  );

  const initViewer = useCallback(async () => {
    if (!containerRef.current) return;
    if (viewerRef.current) return;

    await loadViewerAssets();
    if (!window.Autodesk?.Viewing) {
      throw new Error("Autodesk Viewer API unavailable");
    }

    const getAccessToken = async (
      callback: (token: string, expiresIn: number) => void,
    ) => {
      try {
        const response = await fetch("/api/aps/viewer-token", {
          method: "GET",
          credentials: "include",
        });
        if (!response.ok) {
          if (response.status === 401) {
            setStatus("Session expired. Please sign in again.");
            return;
          }
          setStatus(`Viewer token request failed (${response.status})`);
          return;
        }
        const json = (await response.json()) as {
          access_token: string;
          expires_in: number;
        };
        callback(json.access_token, json.expires_in);
      } catch {
        setStatus("Unable to fetch viewer token");
      }
    };

    await new Promise<void>((resolve) => {
      window.Autodesk!.Viewing.Initializer(
        {
          env: "AutodeskProduction2",
          api: "streamingV2",
          getAccessToken,
        },
        () => {
          viewerRef.current = new window.Autodesk!.Viewing.GuiViewer3D(
            containerRef.current!,
          ) as unknown as ViewerInstance;
          viewerRef.current.start();
          viewerRef.current.setTheme("light-theme");
          viewerRef.current.setGhosting?.(true);
          const selectionEventId = (window.Autodesk?.Viewing as unknown as {
            SELECTION_CHANGED_EVENT?: number;
          })?.SELECTION_CHANGED_EVENT;
          if (typeof selectionEventId === "number") {
            selectionEventIdRef.current = selectionEventId;
            viewerRef.current.addEventListener(selectionEventId, emitSelection);
          }
          resolve();
        },
      );
    });
  }, [loadViewerAssets, emitSelection]);

  useEffect(() => {
    return () => {
      if (viewerRef.current && selectionEventIdRef.current !== null) {
        viewerRef.current.removeEventListener(
          selectionEventIdRef.current,
          emitSelection,
        );
      }
      viewerRef.current?.finish();
      viewerRef.current = null;
    };
  }, [emitSelection]);

  useEffect(() => {
    const handleResize = () => {
      viewerRef.current?.resize();
    };

    window.addEventListener("resize", handleResize);
    const observer =
      typeof ResizeObserver !== "undefined"
        ? new ResizeObserver(() => handleResize())
        : null;

    if (observer && containerRef.current) {
      observer.observe(containerRef.current);
    }

    return () => {
      window.removeEventListener("resize", handleResize);
      observer?.disconnect();
    };
  }, []);

  useEffect(() => {
    const timer = window.setInterval(() => {
      emitSelection();
    }, 1000);
    return () => {
      window.clearInterval(timer);
    };
  }, [emitSelection]);

  useEffect(() => {
    if (!viewerUrn) return;
    let cancelled = false;

    const run = async () => {
      try {
        setStatus("Loading viewer...");
        await initViewer();
        if (cancelled || !viewerRef.current) return;

        setStatus("Checking model translation...");
        const manifestResponse = await fetch(
          `/api/aps/model-manifest?urn=${encodeURIComponent(viewerUrn)}`,
          {
            method: "GET",
            credentials: "include",
          },
        );
        if (!manifestResponse.ok) {
          const manifestJson = (await manifestResponse.json()) as {
            error?: string;
          };
          throw new Error(
            manifestJson.error ??
              "Model is not viewable yet. It may still be translating.",
          );
        }
        const manifestJson = (await manifestResponse.json()) as {
          status?: string;
          progress?: string;
        };
        if (manifestJson.status && manifestJson.status !== "success") {
          throw new Error(
            `Model translation status: ${manifestJson.status} (${manifestJson.progress ?? "unknown"})`,
          );
        }

        setStatus("Loading model...");
        await new Promise<void>((resolve, reject) => {
          window.Autodesk!.Viewing.Document.load(
            `urn:${viewerUrn}`,
            (doc) => {
              documentRef.current = doc;
              const options = getViewOptionsFromDocument(doc);
              setViewOptions(options);
              const node =
                options.length > 0
                  ? options[0].node
                  : (doc as { getRoot: () => { getDefaultGeometry: () => unknown } })
                      .getRoot()
                      .getDefaultGeometry();
              if (!node) {
                reject(
                  new Error(
                    "Model has no default viewable geometry yet. It may still be translating.",
                  ),
                );
                return;
              }
              void viewerRef.current!
                .loadDocumentNode(doc, node)
                .then(() => {
                  if (options.length > 0) {
                    setActiveViewKey(options[0].key);
                  }
                  resolve();
                })
                .catch((err: unknown) => reject(err));
            },
            (code, message) =>
              reject(
                new Error(
                  `Model load failed (${code}): ${
                    message ||
                    "Viewer could not fetch derivative resources for this model."
                  }`,
                ),
              ),
          );
        });

        setStatus("Model loaded");
        emitSelection();
      } catch (error) {
        setStatus(
          `Viewer error: ${error instanceof Error ? error.message : "Unknown error"}`,
        );
      }
    };

    void run();
    return () => {
      cancelled = true;
    };
  }, [viewerUrn, initViewer, emitSelection, getViewOptionsFromDocument]);

  const onChangeView = useCallback(
    async (viewKey: string) => {
      if (!viewerRef.current || !documentRef.current) return;
      const option = viewOptions.find((v) => v.key === viewKey);
      if (!option) return;
      setStatus("Loading selected view...");
      try {
        await loadViewNode(documentRef.current, option.node, option.key);
      } catch (error) {
        setStatus(
          `Viewer error: ${error instanceof Error ? error.message : "Unknown error"}`,
        );
      }
    },
    [viewOptions, loadViewNode],
  );

  const toggleMarkups = useCallback(async () => {
    const viewer = viewerRef.current;
    if (!viewer) return;
    try {
      await viewer.loadExtension?.("Autodesk.Viewing.MarkupsCore");
      await viewer.loadExtension?.("Autodesk.Viewing.MarkupsGui");
      const core = viewer.getExtension?.(
        "Autodesk.Viewing.MarkupsCore",
      ) as MarkupsCoreExtension | undefined;
      if (!markupsEnabled) {
        core?.enterEditMode?.();
        setMarkupsEnabled(true);
        setStatus("Markup tools enabled");
      } else {
        core?.leaveEditMode?.();
        setMarkupsEnabled(false);
        setStatus("Markup tools disabled");
      }
    } catch (error) {
      setStatus(
        `Viewer error: ${error instanceof Error ? error.message : "Failed to load markups"}`,
      );
    }
  }, [markupsEnabled]);

  const saveMarkupSnapshot = useCallback(() => {
    const viewer = viewerRef.current;
    const core = viewer?.getExtension?.(
      "Autodesk.Viewing.MarkupsCore",
    ) as MarkupsCoreExtension | undefined;
    const snapshot = core?.generateData?.();
    if (!snapshot) {
      setStatus("Markup save failed: no markup data");
      return;
    }
    setMarkupSnapshot(snapshot);
    if (viewerUrn) {
      window.localStorage.setItem(`markup:${viewerUrn}`, snapshot);
    }
    setStatus("Markup snapshot saved");
  }, [viewerUrn]);

  const loadMarkupSnapshot = useCallback(() => {
    const viewer = viewerRef.current;
    const core = viewer?.getExtension?.(
      "Autodesk.Viewing.MarkupsCore",
    ) as MarkupsCoreExtension | undefined;
    const snapshot =
      markupSnapshot ||
      (viewerUrn ? window.localStorage.getItem(`markup:${viewerUrn}`) ?? "" : "");
    if (!snapshot) {
      setStatus("No saved markup snapshot found");
      return;
    }
    core?.loadMarkups?.(snapshot, "alice-layer");
    setStatus("Markup snapshot loaded");
  }, [markupSnapshot, viewerUrn]);

  const clearMarkups = useCallback(() => {
    const viewer = viewerRef.current;
    const core = viewer?.getExtension?.(
      "Autodesk.Viewing.MarkupsCore",
    ) as MarkupsCoreExtension | undefined;
    if (core?.clear) {
      core.clear();
    } else {
      core?.loadMarkups?.('<svg xmlns="http://www.w3.org/2000/svg"></svg>', "alice-layer");
    }
    setStatus("Markup canvas cleared");
  }, []);

  useEffect(() => {
    if (!viewerUrn) return;
    const saved = window.localStorage.getItem(`markup:${viewerUrn}`) ?? "";
    setMarkupSnapshot(saved);
  }, [viewerUrn]);

  useEffect(() => {
    if (!isActive) return;
    const handle = window.setTimeout(() => {
      viewerRef.current?.resize();
    }, 0);
    return () => {
      window.clearTimeout(handle);
    };
  }, [isActive]);

  useEffect(() => {
    if (!viewerRef.current || actions.length === 0) return;
    const normalizeSearchQuery = (raw: string): string[] => {
      const q = raw.trim();
      if (!q) return [];
      const variants = [q];
      const eq = q.match(/^[a-z_ ]+\s*=\s*(.+)$/i);
      if (eq?.[1]) variants.push(eq[1].trim());
      const quoted = q.match(/"([^"]+)"/);
      if (quoted?.[1]) variants.push(quoted[1].trim());
      return Array.from(new Set(variants.filter(Boolean)));
    };

    const searchIds = (query: string) =>
      new Promise<number[]>((resolve) => {
        const attempts = normalizeSearchQuery(query);
        const runAttempt = (index: number) => {
          if (!viewerRef.current || index >= attempts.length) {
            resolve([]);
            return;
          }
          viewerRef.current.search(
            attempts[index],
            (ids: number[]) => {
              if (ids.length > 0 || index === attempts.length - 1) {
                resolve(ids);
              } else {
                runAttempt(index + 1);
              }
            },
            () => runAttempt(index + 1),
          );
        };
        runAttempt(0);
      });

    void (async () => {
      for (const action of actions) {
        if (!viewerRef.current) break;
        if (action.type === "viewer.fitToView") {
          viewerRef.current.fitToView();
          continue;
        }
        if (action.type === "viewer.clearSelection") {
          viewerRef.current.clearSelection();
          continue;
        }
        if (action.type === "viewer.setGhosting") {
          viewerRef.current.setGhosting?.(action.enabled);
          continue;
        }
        if (action.type === "viewer.markupsSave") {
          saveMarkupSnapshot();
          continue;
        }
        if (action.type === "viewer.markupsLoad") {
          loadMarkupSnapshot();
          continue;
        }
        if (action.type === "viewer.markupsClear") {
          clearMarkups();
          continue;
        }
        if (action.type === "viewer.showAll") {
          viewerRef.current.showAll?.();
          viewerRef.current.isolate([]);
          onViewerFeedbackRef.current?.("Visibility reset to show all elements.");
          continue;
        }
        if (action.type === "viewer.selectDbIds") {
          runSelectElements(action.dbIds, {
            clearFirst: action.clearFirst !== false,
            zoomToSelection: action.fitToView === true,
          });
          continue;
        }
        if (action.type === "viewer.searchAndSelectMetromontPieces") {
          const maxScan = Math.min(
            Math.max(action.maxSearchMatches ?? 3500, 1),
            8000,
          );
          const queryVariants = [action.query.trim()];
          if (
            action.pieceKind === "WPA" ||
            action.pieceKind === "WPB"
          ) {
            queryVariants.push("WALL_PANEL", "WALL PANEL");
          }
          const seen = new Set<number>();
          const mergedHits: number[] = [];
          for (const q of queryVariants) {
            if (!q) continue;
            const ids = await searchIds(q);
            for (const id of ids) {
              if (seen.has(id)) continue;
              seen.add(id);
              mergedHits.push(id);
              if (mergedHits.length >= maxScan) break;
            }
            if (mergedHits.length >= maxScan) break;
          }
          const candidates = mergedHits.slice(0, maxScan);
          const viewerNow = viewerRef.current;
          if (!viewerNow) break;
          if (candidates.length === 0) {
            onViewerFeedbackRef.current?.(
              `Metromont search: no viewer hits for "${action.query}".`,
            );
            continue;
          }

          const propNames = [
            "Category",
            "Category Name",
            "Family",
            "Family Name",
            "Type",
            "Type Name",
            "CONSTRUCTION_PRODUCT",
            "CONTROL_MARK",
          ];

          const kept: number[] = [];
          const processBulkResults = (
            results: Array<{
              dbId: number;
              properties?: Array<{
                displayName?: string;
                displayValue?: unknown;
              }>;
            }>,
          ) => {
            for (const row of results) {
              const m = propsMapFromViewerProperties(row.properties);
              if (matchesMetromontPieceKind(m, action.pieceKind)) {
                kept.push(row.dbId);
              }
            }
          };

          const rawModel = viewerNow as unknown as {
            model?: {
              getBulkProperties?: (
                dbIds: number[],
                opts: { propFilter: string[] },
                onSuccess: (
                  results: Array<{
                    dbId: number;
                    properties?: Array<{
                      displayName?: string;
                      displayValue?: unknown;
                    }>;
                  }>,
                ) => void,
                onError?: (err: unknown) => void,
              ) => void;
            };
          };
          const model = rawModel.model;
          const chunkSize = 120;
          let bulkOk = false;

          if (typeof model?.getBulkProperties === "function") {
            try {
              for (let i = 0; i < candidates.length; i += chunkSize) {
                const chunk = candidates.slice(i, i + chunkSize);
                await new Promise<void>((resolve, reject) => {
                  model.getBulkProperties!(
                    chunk,
                    { propFilter: propNames },
                    (results) => {
                      processBulkResults(results);
                      resolve();
                    },
                    (err) =>
                      reject(err ?? new Error("getBulkProperties failed")),
                  );
                });
              }
              bulkOk = true;
            } catch {
              kept.length = 0;
              bulkOk = false;
            }
          }

          if (!bulkOk && candidates.length > 0) {
            const cap = Math.min(candidates.length, 450);
            for (let i = 0; i < cap; i++) {
              const id = candidates[i];
              await new Promise<void>((r) => {
                viewerNow.getProperties(
                  id,
                  (result) => {
                    const m = propsMapFromViewerProperties(result.properties);
                    if (matchesMetromontPieceKind(m, action.pieceKind)) {
                      kept.push(id);
                    }
                    r();
                  },
                  () => r(),
                );
              });
            }
          }

          const clearFirst = action.clearFirst !== false;
          const zoom = action.fitToView === true;
          if (clearFirst) {
            viewerNow.clearSelection();
          }
          if (kept.length > 0) {
            viewerNow.select(kept);
          }
          if (zoom && kept.length > 0) {
            viewerNow.fitToView();
          }
          onViewerFeedbackRef.current?.(
            bulkOk
              ? `Metromont ${action.pieceKind}: kept ${kept.length} of ${candidates.length} search hits (Structural Framing + piece rules).`
              : `Metromont ${action.pieceKind}: kept ${kept.length} (limited scan: bulk properties unavailable or failed).`,
          );
          continue;
        }
        if (action.type === "viewer.isolateDbIds") {
          const ids = action.dbIds.filter((n) => Number.isFinite(n)).map((n) => Math.trunc(n));
          if (ids.length === 0) {
            onViewerFeedbackRef.current?.("isolateDbIds: no valid dbIds.");
          } else {
            viewerRef.current.setGhosting?.(true);
            viewerRef.current.select(ids);
            viewerRef.current.isolate(ids);
            viewerRef.current.fitToView();
            onViewerFeedbackRef.current?.(
              `Isolated ${ids.length} element(s) by dbId.`,
            );
          }
          continue;
        }
        if (action.type === "viewer.isolateSelection") {
          const currentIds = viewerRef.current.getSelection();
          viewerRef.current.setGhosting?.(true);
          if (currentIds.length > 0) {
            viewerRef.current.isolate(currentIds);
            viewerRef.current.fitToView();
            onViewerFeedbackRef.current?.(
              `Isolated ${currentIds.length} selected element${currentIds.length === 1 ? "" : "s"}.`,
            );
          } else {
            onViewerFeedbackRef.current?.("No selected elements available to isolate.");
          }
          continue;
        }
        if (action.type === "viewer.search") {
          const ids = await searchIds(action.query);
          if (ids.length === 0) {
            onViewerFeedbackRef.current?.(`I found 0 matches for "${action.query}".`);
          } else {
            viewerRef.current.select(ids);
            viewerRef.current.fitToView();
            onViewerFeedbackRef.current?.(
              `I found ${ids.length} match${ids.length === 1 ? "" : "es"} for "${action.query}" and selected them.`,
            );
          }
          continue;
        }
        if (action.type === "viewer.isolateByQuery") {
          const ids = await searchIds(action.query);
          if (ids.length === 0) {
            onViewerFeedbackRef.current?.(`I found 0 matches to isolate for "${action.query}".`);
          } else {
            viewerRef.current.setGhosting?.(true);
            viewerRef.current.select(ids);
            viewerRef.current.isolate(ids);
            viewerRef.current.fitToView();
            onViewerFeedbackRef.current?.(
              `Isolated ${ids.length} match${ids.length === 1 ? "" : "es"} for "${action.query}".`,
            );
          }
          continue;
        }
        if (action.type === "viewer.hideByQuery") {
          const ids = await searchIds(action.query);
          if (ids.length === 0) {
            onViewerFeedbackRef.current?.(`I found 0 matches to hide for "${action.query}".`);
          } else if (viewerRef.current.hide) {
            viewerRef.current.hide(ids);
            onViewerFeedbackRef.current?.(
              `Hid ${ids.length} match${ids.length === 1 ? "" : "es"} for "${action.query}".`,
            );
          } else {
            onViewerFeedbackRef.current?.("Hide is not supported by current viewer runtime.");
          }
        }
      }
      emitSelection();
      onActionComplete();
    })();
  }, [
    actions,
    onActionComplete,
    emitSelection,
    runSelectElements,
    saveMarkupSnapshot,
    loadMarkupSnapshot,
    clearMarkups,
  ]);

  return (
    <section className="flex h-full min-h-0 flex-col overflow-hidden rounded border border-black/10 bg-white p-3 text-black">
      {mode === "full" ? (
        <div className="mb-2 flex items-center justify-between text-sm">
          <h2 className="font-semibold">{title}</h2>
          <div className="flex items-center gap-2">
            {viewOptions.length > 0 ? (
              <select
                aria-label="Model view selector"
                value={activeViewKey}
                onChange={(e) => void onChangeView(e.target.value)}
                className="rounded border px-2 py-1 text-xs text-black bg-white"
              >
                {viewOptions.map((v) => (
                  <option key={v.key} value={v.key}>
                    {v.name}
                  </option>
                ))}
              </select>
            ) : null}
            <button
              type="button"
              onClick={() => void toggleMarkups()}
              className="rounded border px-2 py-1 text-xs hover:bg-gray-100"
            >
              {markupsEnabled ? "Disable Markup" : "Enable Markup"}
            </button>
            <button
              type="button"
              onClick={() => saveMarkupSnapshot()}
              className="rounded border px-2 py-1 text-xs hover:bg-gray-100"
            >
              Save Markup
            </button>
            <button
              type="button"
              onClick={() => loadMarkupSnapshot()}
              className="rounded border px-2 py-1 text-xs hover:bg-gray-100"
            >
              Load Markup
            </button>
            <button
              type="button"
              onClick={() => clearMarkups()}
              className="rounded border px-2 py-1 text-xs hover:bg-gray-100"
            >
              Clear Markup
            </button>
            <span className="text-xs text-black">{status}</span>
          </div>
        </div>
      ) : null}
      {mode === "minimal" ? (
        <div className="mb-2 rounded border border-black/10 bg-gray-50 px-2 py-1 text-[11px] text-gray-700">
          {status}
        </div>
      ) : null}
      <div
        ref={containerRef}
        className="relative min-h-0 flex-1 overflow-hidden rounded border border-black/10 bg-gray-50"
      />
    </section>
  );
});

ViewerPanel.displayName = "ViewerPanel";
