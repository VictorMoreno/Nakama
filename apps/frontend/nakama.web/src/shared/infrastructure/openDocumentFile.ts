import type { DocumentFile } from "../domain/DocumentFile";

const BLOB_URL_LIFETIME_MS = 60_000;
const VIEWABLE_TYPES = ["application/pdf", "text/plain"];

function isViewableInBrowser(file: DocumentFile): boolean {
  return VIEWABLE_TYPES.some((type) => file.blob.type.startsWith(type));
}

function saveAsFile(objectUrl: string, fileName: string): void {
  const link: HTMLAnchorElement = document.createElement("a");
  link.href = objectUrl;
  link.download = fileName;
  link.click();
}

export async function openDocumentFile(download: () => Promise<DocumentFile>): Promise<void> {
  const viewer: Window | null = window.open("", "_blank");
  try {
    const file: DocumentFile = await download();
    const objectUrl: string = URL.createObjectURL(file.blob);

    if (viewer && isViewableInBrowser(file)) {
      viewer.location.href = objectUrl;
    } else {
      viewer?.close();
      saveAsFile(objectUrl, file.fileName);
    }

    setTimeout(() => URL.revokeObjectURL(objectUrl), BLOB_URL_LIFETIME_MS);
  } catch (error) {
    viewer?.close();
    throw error;
  }
}
