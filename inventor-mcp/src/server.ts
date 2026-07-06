import { Server } from "@modelcontextprotocol/sdk/server/index.js";
import { z } from "zod";
import {
  CallToolRequestSchema,
  ListToolsRequestSchema,
} from "@modelcontextprotocol/sdk/types.js";
import { invokeILogicTool, ilogicToolDefs } from "./tools/ilogicTools.js";
import { invokeParameterTool, parameterToolDefs } from "./tools/parameterTools.js";
import { invokeRevitInteropTool, revitInteropToolDefs } from "./tools/revitInteropTools.js";
import { ExcelParameterSource } from "./data/ExcelParameterSource.js";
import { SqlParameterSourceStub } from "./data/SqlParameterSource.stub.js";
import type { IParameterSource } from "./data/IParameterSource.js";

const ArgsSchema = z.record(z.string(), z.unknown());

function result(payload: unknown) {
  return {
    content: [{ type: "text", text: JSON.stringify(payload) }],
  };
}

function buildParameterSource(): IParameterSource {
  const source = process.env.INVENTOR_PARAM_SOURCE?.trim().toLowerCase() ?? "excel";
  if (source === "sql") {
    return new SqlParameterSourceStub();
  }

  const workbookPath =
    process.env.INVENTOR_EXCEL_PATH?.trim() ||
    "d:/CursorRevitMCP/data/inventor-parameters.xlsx";
  return new ExcelParameterSource(workbookPath);
}

export function buildServer() {
  const parameterSource = buildParameterSource();
  const server = new Server(
    { name: "inventor-mcp", version: "0.1.0" },
    { capabilities: { tools: {} } },
  );

  server.setRequestHandler(ListToolsRequestSchema, async () => ({
    tools: [...ilogicToolDefs, ...parameterToolDefs, ...revitInteropToolDefs],
  }));

  server.setRequestHandler(CallToolRequestSchema, async (request) => {
    const name = request.params.name;
    const args = ArgsSchema.parse(request.params.arguments ?? {});

    const ilogicResult = await invokeILogicTool(name, args);
    if (ilogicResult) {
      return result(ilogicResult);
    }

    const parameterResult = await invokeParameterTool(name, args, parameterSource);
    if (parameterResult) {
      return result(parameterResult);
    }

    const revitInteropResult = await invokeRevitInteropTool(name, args);
    if (revitInteropResult) {
      return result(revitInteropResult);
    }

    throw new Error(`Unknown tool: ${name}`);
  });

  return server;
}
