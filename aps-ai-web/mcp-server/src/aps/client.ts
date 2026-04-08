const APS_BASE = "https://developer.api.autodesk.com";

export async function apsGet<T>(
  path: string,
  accessToken: string,
): Promise<T> {
  const response = await fetch(`${APS_BASE}${path}`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${accessToken}`,
      "Content-Type": "application/json",
    },
    cache: "no-store",
  });
  if (!response.ok) {
    const text = await response.text();
    throw new Error(`APS GET failed (${response.status}): ${text}`);
  }
  return (await response.json()) as T;
}
