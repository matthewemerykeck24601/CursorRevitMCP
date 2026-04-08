"use client";

import { useEffect, useMemo, useRef } from "react";
import * as THREE from "three";

type ProductRecord = Record<string, unknown>;

type ProductProperty = {
  displayName: string;
  displayValue: string;
};

type OpeningFeature = {
  kind: "void" | "penetration";
  width: number;
  height: number;
  diameter: number;
  x: number;
  y: number;
};

type ParsedProduct = {
  id: string;
  label: string;
  length: number;
  width: number;
  height: number;
  openings: OpeningFeature[];
};

type DesignViewSplit3DProps = {
  products: ProductRecord[];
};

function parseNumberLike(value: unknown): number | null {
  if (typeof value === "number" && Number.isFinite(value)) return value;
  if (typeof value !== "string") return null;
  const match = value.replace(/,/g, "").match(/-?\d+(\.\d+)?/);
  if (!match) return null;
  const parsed = Number.parseFloat(match[0]);
  return Number.isFinite(parsed) ? parsed : null;
}

function readDimension(
  dimensions: Record<string, unknown>,
  key: string,
  fallbackKey: string,
  defaultValue: number,
): number {
  const direct = parseNumberLike(dimensions[key]);
  if (direct && direct > 0) return direct;
  const fallback = parseNumberLike(dimensions[fallbackKey]);
  if (fallback && fallback > 0) return fallback;
  return defaultValue;
}

function normalizeProperties(raw: unknown): ProductProperty[] {
  if (!Array.isArray(raw)) return [];
  return raw
    .map((item) => {
      if (!item || typeof item !== "object") return null;
      const record = item as Record<string, unknown>;
      return {
        displayName: String(record.displayName ?? ""),
        displayValue: String(record.displayValue ?? ""),
      };
    })
    .filter((item): item is ProductProperty => !!item && !!item.displayName);
}

function detectOpenings(properties: ProductProperty[], length: number, width: number): OpeningFeature[] {
  const relevant = properties.filter((p) =>
    /(void|opening|penetration|hole|sleeve|cutout)/i.test(p.displayName),
  );
  const grouped = new Map<string, ProductProperty[]>();
  for (const prop of relevant) {
    const keyMatch = prop.displayName.match(/(?:^|[_\s-])(\d+)(?:$|[_\s-])/);
    const groupKey = keyMatch?.[1] ?? prop.displayName.toLowerCase().replace(/\s+/g, "_");
    const arr = grouped.get(groupKey) ?? [];
    arr.push(prop);
    grouped.set(groupKey, arr);
  }

  const features: OpeningFeature[] = [];
  const groups = Array.from(grouped.values()).slice(0, 12);
  groups.forEach((group, idx) => {
    const nameBlob = group.map((g) => g.displayName).join(" ").toLowerCase();
    const valueBlob = group.map((g) => g.displayValue).join(" ");
    const firstNumeric = parseNumberLike(valueBlob) ?? 0;
    let w = 0;
    let h = 0;
    let d = 0;
    let x = 0;
    let y = 0;
    for (const entry of group) {
      const n = entry.displayName.toLowerCase();
      const v = parseNumberLike(entry.displayValue);
      if (!v || v <= 0) continue;
      if (/diam|dia|radius/.test(n)) d = Math.max(d, v);
      if (/width|w\b/.test(n)) w = Math.max(w, v);
      if (/height|ht|h\b/.test(n)) h = Math.max(h, v);
      if (/x|offset_x|center_x|loc_x/.test(n)) x = v;
      if (/y|offset_y|center_y|loc_y/.test(n)) y = v;
    }
    if (d <= 0 && w <= 0 && h <= 0 && firstNumeric > 0) d = firstNumeric;
    if (w <= 0 && d > 0) w = d;
    if (h <= 0 && d > 0) h = d;
    if (w <= 0) w = Math.max(length * 0.06, 0.2);
    if (h <= 0) h = Math.max(width * 0.06, 0.2);
    const distributedX = ((idx + 1) / (groups.length + 1)) * length - length / 2;
    const distributedY = ((idx % 2 === 0 ? 0.35 : 0.65) * width) - width / 2;
    features.push({
      kind: /penetration|sleeve|hole/.test(nameBlob) ? "penetration" : "void",
      width: Math.min(w, length * 0.35),
      height: Math.min(h, width * 0.5),
      diameter: d > 0 ? Math.min(d, Math.min(length, width) * 0.35) : 0,
      x: Number.isFinite(x) && Math.abs(x) > 0.001 ? x : distributedX,
      y: Number.isFinite(y) && Math.abs(y) > 0.001 ? y : distributedY,
    });
  });
  return features;
}

function parseProducts(products: ProductRecord[]): ParsedProduct[] {
  return products.map((product, i) => {
    const dimensions = (product.dimensions ?? {}) as Record<string, unknown>;
    const properties = normalizeProperties(product.properties);
    const length = readDimension(dimensions, "lengthNum", "length", 20);
    const width = readDimension(dimensions, "widthNum", "width", 8);
    const height = readDimension(dimensions, "heightNum", "height", 2);
    const openings = detectOpenings(properties, length, width);
    return {
      id: String(product.dbId ?? i + 1),
      label:
        String(product.controlMark ?? "").trim() ||
        String(product.name ?? "").trim() ||
        `Product ${i + 1}`,
      length,
      width,
      height,
      openings,
    };
  });
}

function renderProfileScene(
  scene: THREE.Scene,
  parsed: ParsedProduct[],
  camera: THREE.OrthographicCamera,
) {
  for (const child of [...scene.children]) scene.remove(child);
  const maxLength = Math.max(...parsed.map((p) => p.length), 20);
  const stackHeight = parsed.reduce((sum, p) => sum + p.width, 0) + parsed.length * 1.2;
  let yCursor = stackHeight / 2;

  parsed.forEach((p) => {
    const panelY = yCursor - p.width / 2;
    yCursor -= p.width + 1.2;
    const shape = new THREE.Shape();
    shape.moveTo(-p.length / 2, -p.width / 2);
    shape.lineTo(p.length / 2, -p.width / 2);
    shape.lineTo(p.length / 2, p.width / 2);
    shape.lineTo(-p.length / 2, p.width / 2);
    shape.lineTo(-p.length / 2, -p.width / 2);
    for (const opening of p.openings) {
      const hole = new THREE.Path();
      if (opening.diameter > 0) {
        hole.absellipse(opening.x, opening.y, opening.diameter / 2, opening.diameter / 2, 0, Math.PI * 2, true, 0);
      } else {
        const hw = opening.width / 2;
        const hh = opening.height / 2;
        hole.moveTo(opening.x - hw, opening.y - hh);
        hole.lineTo(opening.x + hw, opening.y - hh);
        hole.lineTo(opening.x + hw, opening.y + hh);
        hole.lineTo(opening.x - hw, opening.y + hh);
        hole.lineTo(opening.x - hw, opening.y - hh);
      }
      shape.holes.push(hole);
    }
    const mesh = new THREE.Mesh(
      new THREE.ShapeGeometry(shape),
      new THREE.MeshBasicMaterial({ color: 0xbfdbfe, side: THREE.DoubleSide }),
    );
    mesh.position.y = panelY;
    scene.add(mesh);
    const border = new THREE.LineSegments(
      new THREE.EdgesGeometry(new THREE.ShapeGeometry(shape)),
      new THREE.LineBasicMaterial({ color: 0x1d4ed8 }),
    );
    border.position.y = panelY;
    scene.add(border);
  });

  const margin = 3;
  const halfW = maxLength / 2 + margin;
  const halfH = stackHeight / 2 + margin;
  camera.left = -halfW;
  camera.right = halfW;
  camera.top = halfH;
  camera.bottom = -halfH;
  camera.near = -100;
  camera.far = 100;
  camera.position.set(0, 0, 10);
  camera.lookAt(0, 0, 0);
  camera.updateProjectionMatrix();
}

function renderSectionScene(
  scene: THREE.Scene,
  parsed: ParsedProduct[],
  camera: THREE.OrthographicCamera,
) {
  for (const child of [...scene.children]) scene.remove(child);
  const maxThickness = Math.max(...parsed.map((p) => p.height), 1);
  const stackHeight = parsed.reduce((sum, p) => sum + p.width, 0) + parsed.length * 1.2;
  let yCursor = stackHeight / 2;
  parsed.forEach((p) => {
    const panelY = yCursor - p.width / 2;
    yCursor -= p.width + 1.2;
    const geometry = new THREE.PlaneGeometry(p.height, p.width);
    const mesh = new THREE.Mesh(
      geometry,
      new THREE.MeshBasicMaterial({ color: 0xdcfce7, side: THREE.DoubleSide }),
    );
    mesh.position.set(0, panelY, 0);
    scene.add(mesh);
    const border = new THREE.LineSegments(
      new THREE.EdgesGeometry(geometry),
      new THREE.LineBasicMaterial({ color: 0x15803d }),
    );
    border.position.set(0, panelY, 0);
    scene.add(border);
  });
  const margin = 2;
  const halfW = maxThickness / 2 + margin;
  const halfH = stackHeight / 2 + margin;
  camera.left = -halfW;
  camera.right = halfW;
  camera.top = halfH;
  camera.bottom = -halfH;
  camera.near = -100;
  camera.far = 100;
  camera.position.set(0, 0, 10);
  camera.lookAt(0, 0, 0);
  camera.updateProjectionMatrix();
}

function mountScene(
  container: HTMLDivElement,
  renderScene: (scene: THREE.Scene, camera: THREE.OrthographicCamera) => void,
): () => void {
  const renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });
  renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 2));
  renderer.setClearColor(0xffffff, 1);
  container.innerHTML = "";
  container.appendChild(renderer.domElement);
  const scene = new THREE.Scene();
  const camera = new THREE.OrthographicCamera(-10, 10, 10, -10, -100, 100);
  const resize = () => {
    const w = Math.max(container.clientWidth, 10);
    const h = Math.max(container.clientHeight, 10);
    renderer.setSize(w, h, false);
  };
  const draw = () => {
    renderScene(scene, camera);
    renderer.render(scene, camera);
  };
  resize();
  draw();
  const observer = new ResizeObserver(() => {
    resize();
    draw();
  });
  observer.observe(container);
  return () => {
    observer.disconnect();
    renderer.dispose();
    container.innerHTML = "";
  };
}

export function DesignViewSplit3D({ products }: DesignViewSplit3DProps) {
  const profileRef = useRef<HTMLDivElement | null>(null);
  const sectionRef = useRef<HTMLDivElement | null>(null);
  const parsed = useMemo(() => parseProducts(products), [products]);
  const totalOpenings = parsed.reduce((sum, p) => sum + p.openings.length, 0);

  useEffect(() => {
    if (!profileRef.current || !sectionRef.current) return;
    const stopProfile = mountScene(profileRef.current, (scene, camera) =>
      renderProfileScene(scene, parsed, camera),
    );
    const stopSection = mountScene(sectionRef.current, (scene, camera) =>
      renderSectionScene(scene, parsed, camera),
    );
    return () => {
      stopProfile();
      stopSection();
    };
  }, [parsed]);

  return (
    <div className="grid h-full min-h-0 grid-cols-1 gap-2 md:grid-cols-2">
      <div className="flex min-h-0 flex-col rounded border border-black/10 bg-white p-2">
        <div className="mb-1 text-xs font-medium">
          2D Profile (Length x Width) with detected voids/penetrations
        </div>
        <div ref={profileRef} className="min-h-[220px] flex-1 rounded border border-black/10" />
        <div className="mt-1 text-[11px] text-gray-600">
          Elements: {parsed.length} | Detected features: {totalOpenings}
        </div>
      </div>
      <div className="flex min-h-0 flex-col rounded border border-black/10 bg-white p-2">
        <div className="mb-1 text-xs font-medium">
          Cross Section (Width x Thickness)
        </div>
        <div ref={sectionRef} className="min-h-[220px] flex-1 rounded border border-black/10" />
        <div className="mt-1 text-[11px] text-gray-600">
          Thickness view from BIM `height`/`DIM_HEIGHT` parameters
        </div>
      </div>
    </div>
  );
}

