declare global {
  interface Window {
    Autodesk?: {
      Viewing: {
        Initializer: (
          options: {
            env: string;
            api: string;
            getAccessToken: (
              callback: (token: string, expiresIn: number) => void,
            ) => void;
          },
          callback: () => void,
        ) => void;
        GuiViewer3D: new (container: HTMLElement) => AutodeskViewer;
        Document: {
          load: (
            urn: string,
            onSuccess: (doc: AutodeskDocument) => void,
            onFailure: (code: number, message: string) => void,
          ) => void;
        };
      };
    };
  }
}

type AutodeskDocument = {
  getRoot: () => {
    getDefaultGeometry: () => unknown;
  };
};

type AutodeskViewer = {
  start: () => number;
  finish: () => void;
  setTheme: (theme: string) => void;
  loadDocumentNode: (doc: AutodeskDocument, node: unknown) => Promise<unknown>;
  fitToView: () => void;
  clearSelection: () => void;
  isolate: (ids?: number[]) => void;
  search: (
    text: string,
    onSuccess: (dbIds: number[]) => void,
    onError: (error: unknown) => void,
  ) => void;
  select: (ids: number[]) => void;
};

export {};

