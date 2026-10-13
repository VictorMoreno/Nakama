export function pluralWord(n: number, singular: string, plural: string): string {
  return `${n} ${n === 1 ? singular : plural}`;
}

export function botSubText(docsCount: number, teamCount: number): string {
  return `${pluralWord(docsCount, "documento", "documentos")} · ${pluralWord(teamCount, "persona", "personas")} con acceso`;
}
