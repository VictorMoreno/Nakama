/** @type {import('stylelint').Config} */
export default {
  extends: ["stylelint-config-standard-scss"],
  rules: {
    // CSS Modules files reference classes that only exist via camelCase JS access
    // (styles.fooBar) — stylelint can't see that, so relax the naming pattern rather
    // than fighting a false positive on every module file.
    "selector-class-pattern": null,
    // design-system.css / tokens.scss deliberately use `--custom-property-name` tokens
    // (--surface-strong, --glass-fill, ...), not the stricter kebab-case stylelint expects
    // by default in some configs — already kebab-case here, but keep this off to avoid
    // false positives on the very long/composite token names.
    "custom-property-pattern": null,
    "no-descending-specificity": null,
    // -webkit-backdrop-filter is still required for Safari's glassmorphism support
    // (unprefixed backdrop-filter alone silently drops the blur there) — not a stray prefix.
    "property-no-vendor-prefix": null,
    // CSS Modules' :global()/:local() escape hatch, used to target dynamically-added
    // classNames (e.g. from a third-party lib) that aren't scoped by the module.
    "selector-pseudo-class-no-unknown": [true, { ignorePseudoClasses: ["global", "local"] }],
    // Established compact style in this codebase: short single-line rules with a few
    // declarations (e.g. `.chevron { width: 11px; height: 11px; }`) read better here than
    // forcing every rule onto multiple lines.
    "declaration-block-single-line-max-declarations": null
  }
};
