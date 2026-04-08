type Level = "info" | "warn" | "error";

type Meta = Record<string, unknown>;

export function log(level: Level, message: string, meta: Meta = {}): void {
  const payload = {
    ts: new Date().toISOString(),
    level,
    message,
    ...meta,
  };

  const line = JSON.stringify(payload);
  if (level === "error") {
    console.error(line);
  } else if (level === "warn") {
    console.warn(line);
  } else {
    console.log(line);
  }
}

