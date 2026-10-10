import type { FileParameter } from "./GeneratedNakamaClient";

export async function dataUrlToFileParameter(dataUrl: string, fileName: string): Promise<FileParameter> {
  const blob: Blob = await (await fetch(dataUrl)).blob();
  return { data: blob, fileName };
}
