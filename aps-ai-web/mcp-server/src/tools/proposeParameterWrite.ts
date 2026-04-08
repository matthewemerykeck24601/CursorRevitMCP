import crypto from "node:crypto";
import { ProposeParameterWriteInput } from "../schemas/toolSchemas.js";

function tokenFor(changes: Array<{ dbId: number; parameter: string; value: string }>): string {
  return crypto
    .createHash("sha256")
    .update(JSON.stringify(changes))
    .digest("hex")
    .slice(0, 16);
}

export function proposeParameterWrite(rawInput: unknown) {
  const input = ProposeParameterWriteInput.parse(rawInput);
  const expected = tokenFor(input.changes);

  if (!input.apply) {
    return {
      mode: "preview",
      confirmationRequired: true,
      confirmationToken: expected,
      changes: input.changes,
      message:
        "Write preview generated. Re-submit with apply=true and matching confirmationToken to execute.",
    };
  }

  if (!input.confirmationToken || input.confirmationToken !== expected) {
    return {
      mode: "blocked",
      confirmationRequired: true,
      confirmationToken: expected,
      message: "Missing or invalid confirmationToken. No write applied.",
    };
  }

  return {
    mode: "applied",
    appliedCount: input.changes.length,
    changes: input.changes,
    message: "Guarded write confirmed and marked for execution.",
  };
}
