import test from "node:test";
import assert from "node:assert/strict";
import { formatDocAiError } from "@/lib/google-docai";

test("formatDocAiError surfaces grpc code and message", () => {
  const detail = formatDocAiError({
    code: 7,
    details: "Permission denied on processor.",
    message: "7 PERMISSION_DENIED: Permission denied on processor.",
  });
  assert.match(detail, /code=7/);
  assert.match(detail, /Permission denied on processor/);
});

test("formatDocAiError replaces opaque undefined grpc messages", () => {
  const detail = formatDocAiError({
    code: 2,
    message: "undefined undefined: undefined",
  });
  assert.doesNotMatch(detail, /undefined undefined: undefined/);
  assert.match(detail, /code=2/);
});

test("formatDocAiError extracts grpc retry note", () => {
  const detail = formatDocAiError({
    message: "undefined undefined: undefined",
    note: "Exception occurred in retry method that was not classified as transient",
  });
  assert.match(detail, /note=Exception occurred in retry method/);
  assert.doesNotMatch(detail, /undefined undefined: undefined/);
});

test("formatDocAiError extracts nested google api error payload", () => {
  const detail = formatDocAiError({
    response: {
      status: 403,
      statusText: "Forbidden",
      data: {
        error: {
          status: "PERMISSION_DENIED",
          message: "Caller does not have permission",
        },
      },
    },
  });
  assert.match(detail, /http=403/);
  assert.match(detail, /Caller does not have permission/);
  assert.match(detail, /status=PERMISSION_DENIED/);
});
