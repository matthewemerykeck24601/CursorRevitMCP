import { apsGet, apsPost } from "@/lib/aps";
import { env } from "@/lib/env";

export type IssueRecord = {
  id: string;
  title: string;
  status?: string;
  description?: string;
  dueDate?: string;
};

type IssuesListResponse = {
  results?: Array<{
    id?: string;
    title?: string;
    status?: string;
    description?: string;
    due_date?: string;
  }>;
};

type IssueCreateResponse = {
  id?: string;
  title?: string;
  status?: string;
  description?: string;
  due_date?: string;
};

function toIssueRecord(input: {
  id?: string;
  title?: string;
  status?: string;
  description?: string;
  due_date?: string;
}): IssueRecord {
  return {
    id: String(input.id ?? "unknown"),
    title: String(input.title ?? "Untitled issue"),
    status: input.status,
    description: input.description,
    dueDate: input.due_date,
  };
}

export function getIssuesContainerId(): string {
  return env.apsIssuesContainerId;
}

export async function listProjectIssues(
  accessToken: string,
  containerId: string,
): Promise<IssueRecord[]> {
  const data = await apsGet<IssuesListResponse>(
    `/issues/v2/containers/${encodeURIComponent(containerId)}/issues?page[limit]=50`,
    accessToken,
  );
  return (data.results ?? []).map((r) => toIssueRecord(r));
}

export async function createProjectIssue(
  accessToken: string,
  containerId: string,
  input: { title: string; description?: string },
): Promise<IssueRecord> {
  const payload = {
    title: input.title,
    description: input.description ?? "",
    issue_type_id: env.apsIssuesDefaultTypeId || undefined,
    status: "open",
  };
  const created = await apsPost<IssueCreateResponse>(
    `/issues/v2/containers/${encodeURIComponent(containerId)}/issues`,
    accessToken,
    payload,
  );
  return toIssueRecord(created);
}
