export function pluralWord(n: number, singular: string, plural: string): string {
  return `${n} ${n === 1 ? singular : plural}`;
}

export function botSubText(docsCount: number, teamCount: number): string {
  return `${pluralWord(docsCount, "documento", "documentos")} · ${pluralWord(teamCount, "persona", "personas")} con acceso`;
}

/** Turns a chatbot name into a URL/key-safe slug, avoiding collisions with existing keys. */
export function slugify(name: string, existingKeys: string[]): string {
  const base =
    name
      .toLowerCase()
      .normalize("NFD")
      .replace(/[̀-ͯ]/g, "")
      .replace(/[^a-z0-9]+/g, "-")
      .replace(/(^-+|-+$)/g, "") || "bot";

  let key = base;
  let i = 2;
  while (existingKeys.includes(key)) {
    key = `${base}-${i}`;
    i++;
  }
  return key;
}
