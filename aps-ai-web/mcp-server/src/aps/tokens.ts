export type ApsSession = {
  accessToken: string;
  expiresAt: number;
  refreshToken?: string;
};

export function isExpired(session: ApsSession, bufferMs = 60_000): boolean {
  return Date.now() >= session.expiresAt - bufferMs;
}
