# Componentes Flat UI — referencia detallada

Basado en el inventario del kit Flat UI de Designmodo. Colores omitidos a propósito: usa variables CSS (ver SKILL.md).

## Botones

Tipos por función, todos con el mismo tratamiento visual (fondo sólido, sin gradiente, sin sombra, texto en blanco o en color de alto contraste):

- Primary, Default, Success, Warning, Danger, Info, Inverse, Disabled (opacidad reducida + `cursor: not-allowed`, sin cambiar la forma).

Reglas de forma:
- `border-radius`: pequeño (~4px), consistente en todos los botones.
- Padding generoso: aprox. `12px 24px` para botón normal; variantes `small`/`large` cambian solo tamaño de fuente y padding, nunca añaden efectos.
- Sin `border` visible o borde del mismo color que el fondo (no hay contorno separado salvo en variante "outline" si se pide).
- Estado `hover`/`active`: oscurecer el color de fondo un 8–12%, sin sombra ni desplazamiento (`transform` no se usa para dar profundidad).
- Tipografía en mayúsculas o normal según el diseño, peso seminegrita (600).

## Inputs de texto

- Fondo plano (blanco o gris muy claro), borde fino de 1–2px de un gris suave.
- `border-radius` pequeño, igual que los botones.
- Sin sombra interior. En `focus`, cambiar el color del borde (no añadir `box-shadow` de glow, o si se añade, muy sutil y coherente con "flat": preferible solo cambio de color de borde).
- Placeholder en gris medio, texto en gris oscuro/negro.
- Padding vertical generoso para que el input se vea "alto" y cómodo.

## Dropdown / menú desplegable

- **Verificado con `getComputedStyle` contra el kit real: `.dropdown-menu` tiene `border: 0` Y `box-shadow: none`.** Se separa del `body` blanco únicamente por un fondo de un gris apenas distinto (`#f3f4f5` sobre `#fff`) — ni borde ni sombra, solo tono. Si el proyecto apila paneles sobre contenido que YA es de ese mismo tono (por ejemplo un dropdown abierto sobre una tarjeta, no sobre la página), una sombra sutil como único recurso de separación es una adaptación razonable — pero el borde no es parte del patrón del kit en ningún caso.
- Ítems en lista vertical, con `hover` de color de fondo sólido (no degradado, y distinto del fondo real que se ve detrás del ítem — no solo distinto del valor por defecto declarado en la regla; ver checklist).
- Separadores como líneas finas de 1px, sin relieve.
- Puede incluir subniveles (ítems con submenú) — mismo tratamiento visual, indicador simple (flecha o triángulo plano) de que hay más opciones.

## Select personalizado

- **Verificado con `getComputedStyle` contra el kit real** (usa Select2, no
  bootstrap-select nativo — clases `.select2-choice`/`.select2-drop`/`.select2-results`):
  el control **cerrado** lleva **fondo sólido del color de acento** (`background:
  rgb(26,188,156)` — el teal del kit — con `color: #fff`), radio 4px, sin borde,
  padding asimétrico (`10px 39px 10px 15px`, más espacio a la derecha para la flecha).
  Es visualmente un botón de acento, no un input neutro. Aplicado: el trigger usa el
  color de acento del proyecto (no necesariamente el teal del kit) con su texto/ícono en
  el color de contraste correspondiente (blanco o el que el proyecto use para texto
  sobre acento).
- El panel desplegado (`.select2-drop`) tiene el mismo tono apenas-distinto-del-blanco
  que cualquier dropdown (ver sección "Dropdown" arriba) — no el mismo blanco que una
  tarjeta.
- El ítem "highlighted" (el que está bajo el cursor o navegado por teclado, clase
  `.select2-highlighted`) lleva **relleno sólido del color de acento + texto blanco** —
  no un tinte suave. Esto aplica tanto al hover como al estado de "seleccionado
  actualmente" cuando se reabre el desplegable.
- **Excepción real encontrada en un proyecto**: un control que reutiliza esta misma caja
  pero para comunicar un ESTADO semántico propio (ok/pendiente/error, con su propio
  color por estado, no el de acento genérico) NO debe heredar el relleno sólido de
  acento — necesita su propio fondo neutro y su propio color de texto por estado,
  declarado con suficiente especificidad CSS para ganarle a la regla genérica del select
  (un selector compuesto tipo `.wrapper .trigger.estado` es más robusto que confiar en
  el orden de imports entre dos archivos con la misma especificidad).
- Radio de borde pequeño, igual que botones/inputs.

## Tags input

- Campo de texto donde cada tag introducido se muestra como una "píldora" o etiqueta plana (fondo sólido, texto contrastado, radio de borde pequeño o completamente redondeado tipo pill) con una "x" para eliminar.
- Sin sombra ni gradiente en las etiquetas.

## Navbar

- Barra superior de fondo sólido, sin sombra debajo (o sombra mínima si se quiere separar del contenido).
- Logo/marca a la izquierda, ítems de menú a la derecha o centrados, tipografía en mayúsculas o peso medio.
- Ítems con submenú (mega-dropdown simple): incluir clase de indicador y usar reglas de "Dropdown".
- Estado activo del ítem de menú: color de fondo o subrayado sólido, no relieve.

## Barras de progreso y sliders

- Barra de progreso: contenedor (`.progress`) de fondo gris claro plano, relleno interior (`.progress-bar`) de color sólido (sin gradiente), altura consistente (~8–14px). **Verificado con `getComputedStyle` contra el kit real: `.progress` tiene `border-radius: 32px` — en una barra de ~14px de alto eso SÍ es un pill completo** (un radio mayor a la mitad del alto siempre renderiza como pill). `.progress-bar` (el relleno) en sí tiene `border-radius: 0` — no necesita radio propio porque el `overflow: hidden` del track ya lo recorta con la forma de pill. Corrección sobre una nota anterior de este mismo archivo: llegué a escribir que NO era pill basándome en una lectura visual de una captura, no en el CSS real — queda corregido aquí con evidencia directa.
- Slider: pista fina plana, "thumb" (control) como círculo o cuadrado plano sólido, sin sombra; al arrastrar, solo cambia el color, no aparece relieve 3D.

## Navegación / paginación

- Lista horizontal de números de página como "píldoras" o cuadrados con radio pequeño.
- Página activa: fondo sólido de color de énfasis; el resto, fondo transparente o gris muy claro con texto oscuro.
- Flechas prev/next como iconos planos simples.

## Checkboxes y radio buttons

- Checkbox: cuadrado con `border-radius` pequeño, borde fino; al marcarse, se rellena de color sólido con un icono de check simple (sin sombra).
- Radio button: círculo con borde fino; al marcarse, punto interior sólido centrado.
- Estado disabled: opacidad reducida, sin cambiar forma.
- Siempre acompañados de una etiqueta de texto clicable (el área de click incluye el texto).

## Switches (toggles)

- Forma de píldora horizontal (`border-radius: 999px`), con un círculo ("thumb") que se desliza de un extremo a otro. Esta es una de las pocas excepciones explícitas al radio chico general (ver principio 3 en SKILL.md).
- Fondo de la pista cambia de color sólido según esté activado o no; sin gradiente ni sombra, solo transición de color/posición.
- Detalle literal del kit real: el propio switch lleva texto "ON"/"OFF" dentro de la píldora (no solo el thumb deslizante) — es un detalle de fidelidad estricta, no un requisito duro de la skill; aplicarlo o no depende de si el proyecto prioriza fidelidad visual al kit sobre la simplicidad del control.

## Compartir / botones sociales

- Botones o iconos planos de una sola tonalidad, cuadrados o circulares, sin relieve, con icono vectorial simple centrado.

## Tipografía

- Familia sans-serif geométrica (el kit original usa *Lato*).
- Jerarquía clara:
  - H1: grande y muy contundente (para titulares hero).
  - H2–H6: descienden de forma progresiva y consistente en tamaño y peso.
- Texto en negrita (`<strong>`) para resaltar frases clave dentro de un párrafo, sin cambiar de color salvo que sea intencional.
- "Lead text": párrafo introductorio con tamaño de fuente mayor que el cuerpo normal, peso normal o ligero.
- Cita ("Quote"): bloque con una barra vertical sólida de color de énfasis a la izquierda (`border-left` grueso, sin sombra), texto en cursiva o tamaño mayor, atribución del autor debajo en tamaño menor.
- "Small font": variante de texto secundario/nota, de menor tamaño y color más apagado (gris), usada para aclaraciones.

## Iconos y glifos

- Todos los iconos son de trazo o relleno plano, monocromos, en SVG, sin efectos de sombra, brillo o degradado.
- Tamaño consistente dentro de un mismo contexto (ej. todos los iconos de una toolbar al mismo tamaño).

## Tarjetas / "samples"

- El kit de referencia (designmodo.github.io/Flat-UI) no incluye un componente de tarjeta/panel — no hay `.panel`/`.well`/`.card` en su demo, así que no hay un patrón "oficial" del kit para copiar aquí literalmente. Aplicando el mismo principio verificado que en dropdowns (ver arriba): **sin borde**, fondo plano, sin sombra pronunciada. La separación de la página viene de un tono de fondo distinto entre la página y la tarjeta (necesita un `--bg` de página y un `--surface` de tarjeta con contraste suficiente entre sí — si están casi al mismo tono, la tarjeta se pierde sin borde ni sombra que la delimite).
- Icono o avatar a un lado, texto en negrita para el título/acción principal, texto secundario en gris debajo.

## Checklist rápido antes de entregar

- [ ] Ningún `box-shadow` ni `text-shadow` decorativo (más allá de una separación mínima si es imprescindible).
- [ ] Ningún `linear-gradient`/`radial-gradient` en fondos.
- [ ] Ningún `border` fuera de los inputs de texto (y el tags-input). Botones, tarjetas, dropdowns, navbar: relleno sólido o tono de fondo distinto, nunca borde.
- [ ] Radios de borde consistentes entre componentes del mismo tipo.
- [ ] Una sola familia tipográfica sans-serif, con jerarquía de pesos clara.
- [ ] Estados hover/active resueltos con color, no con relieve.
- [ ] El color de hover es realmente distinto del fondo sobre el que se apoya el elemento — no solo distinto del `background` declarado en su propia regla. Un ítem de lista transparente dentro de un panel ya coloreado, con `hover: background: var(--surface)`, es un hover invisible si el panel mismo ya es `--surface` (bug real encontrado y corregido varias veces en esta guía: el color de "cambio" tiene que diferir del fondo que el ojo ve, no solo del valor por defecto de la propiedad).
- [ ] Colores definidos como variables CSS reutilizables, no hardcodeados por todo el código.
