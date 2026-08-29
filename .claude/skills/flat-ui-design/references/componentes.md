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

- Contenedor con fondo sólido, sin sombra pronunciada (como mucho una sombra muy sutil de 1–2px para separarlo del fondo, si el usuario lo pide; el kit original es minimalista aquí).
- Ítems en lista vertical, con `hover` de color de fondo sólido (no degradado).
- Separadores como líneas finas de 1px, sin relieve.
- Puede incluir subniveles (ítems con submenú) — mismo tratamiento visual, indicador simple (flecha o triángulo plano) de que hay más opciones.

## Select personalizado

- Mismo tratamiento que un input (fondo plano, borde fino, radio pequeño).
- Icono indicador (flecha hacia abajo) simple y plano a la derecha.
- Al desplegar, la lista de opciones sigue las reglas de "Dropdown".

## Tags input

- Campo de texto donde cada tag introducido se muestra como una "píldora" o etiqueta plana (fondo sólido, texto contrastado, radio de borde pequeño o completamente redondeado tipo pill) con una "x" para eliminar.
- Sin sombra ni gradiente en las etiquetas.

## Navbar

- Barra superior de fondo sólido, sin sombra debajo (o sombra mínima si se quiere separar del contenido).
- Logo/marca a la izquierda, ítems de menú a la derecha o centrados, tipografía en mayúsculas o peso medio.
- Ítems con submenú (mega-dropdown simple): incluir clase de indicador y usar reglas de "Dropdown".
- Estado activo del ítem de menú: color de fondo o subrayado sólido, no relieve.

## Barras de progreso y sliders

- Barra de progreso: contenedor de fondo gris claro plano, relleno interior de color sólido (sin gradiente), altura consistente (~8–14px), esquinas ligeramente redondeadas o completamente redondeadas (pill).
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

- Forma de píldora horizontal (`border-radius: 999px`), con un círculo ("thumb") que se desliza de un extremo a otro.
- Fondo de la pista cambia de color sólido según esté activado o no; sin gradiente ni sombra, solo transición de color/posición.

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

- Bloques de contenido (tipo tarjeta de evento, chat, notificación) con fondo plano, sin sombra pronunciada (como mucho un borde fino de separación), radio de borde pequeño.
- Icono o avatar a un lado, texto en negrita para el título/acción principal, texto secundario en gris debajo.

## Checklist rápido antes de entregar

- [ ] Ningún `box-shadow` ni `text-shadow` decorativo (más allá de una separación mínima si es imprescindible).
- [ ] Ningún `linear-gradient`/`radial-gradient` en fondos.
- [ ] Radios de borde consistentes entre componentes del mismo tipo.
- [ ] Una sola familia tipográfica sans-serif, con jerarquía de pesos clara.
- [ ] Estados hover/active resueltos con color, no con relieve.
- [ ] Colores definidos como variables CSS reutilizables, no hardcodeados por todo el código.
