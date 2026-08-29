---
name: flat-ui-design
description: >-
  Guía de estilo visual "Flat UI" (basada en el kit de Designmodo) para diseñar o maquetar páginas web y componentes de interfaz. Úsala siempre que el usuario pida diseñar, maquetar, rediseñar o generar código (HTML/CSS/React) de una web, landing page, dashboard o componente de UI con estilo "flat", "diseño plano", "flat design" o "Flat UI", o cuando pida aplicar esta guía de estilo a botones, formularios, navbars, tarjetas, tipografía u otros elementos de interfaz. No define una paleta de colores concreta; los colores los decide el usuario o se usan variables CSS neutras.
---

# Flat UI Design

Guía de estilo para construir interfaces con estética **Flat UI**, inspirada en el kit gratuito de Designmodo (https://designmodo.github.io/Flat-UI/). Esta skill se centra en la forma, la tipografía y el comportamiento de los componentes — **no** en la paleta de colores, que queda a elección del usuario (ver sección "Colores" para cómo tratarlos).

## Principios fundamentales

1. **Sin gradientes, sin sombras.** Ningún elemento debe tener `box-shadow`, `text-shadow` ni fondos degradados. Los colores son planos y sólidos (`background-color` uniforme).
2. **Sin bordes 3D ni efectos de relieve.** Nada de `inset`, `outset`, bordes biselados o `border` con distintos tonos por lado para simular profundidad.
3. **Bordes rectos o ligeramente redondeados.** Radio de borde pequeño y consistente (aprox. `3px`–`6px`) en botones, inputs y tarjetas. Nunca completamente cuadrado si el kit original usa esquinas suaves, pero tampoco muy redondeado (evitar el estilo "pill" salvo en switches/toggles).
4. **Color como jerarquía, no como decoración.** El color de fondo de un botón o etiqueta comunica su función (primario, éxito, alerta, peligro, info, por defecto, inverso, deshabilitado), no es un adorno.
5. **Tipografía bold y geométrica.** Fuente sans-serif (el kit original usa *Lato*/*Source Sans Pro*); los encabezados usan peso seminegrita o negrita, buen contraste de tamaño entre niveles (H1 grande y contundente, bajando de forma clara hasta H6).
6. **Espaciado generoso.** Padding amplio dentro de botones e inputs, buen aire entre secciones. La sensación "flat" depende tanto del espacio en blanco como de la ausencia de efectos.
7. **Iconografía simple y vectorial.** Iconos de trazo/relleno plano a una sola tonalidad, sin efectos, en formato SVG.
8. **Interactividad mediante color, no mediante profundidad.** Los estados `hover`/`active`/`focus` se resuelven oscureciendo o aclarando ligeramente el color de fondo (no añadiendo sombra ni relieve).

## Colores

Esta skill **no fija una paleta**. Al generar código:

- Usa **variables CSS** (`--color-primary`, `--color-success`, `--color-warning`, `--color-danger`, `--color-info`, `--color-default`, `--color-text`, `--color-bg`, etc.) en vez de valores hexadecimales fijos, para que el usuario pueda sustituirlas fácilmente por su propia paleta.
- Si el usuario no da colores, usa un set neutro de marcador de posición (grises + un acento) y dilo explícitamente, invitándolo a definir su paleta.
- Si el usuario sí indica colores o pide "los colores originales de Flat UI", entonces sí puedes usar la paleta clásica del kit (turquesa `#1abc9c`, verde esmeralda `#2ecc71`, azul `#3498db`, amatista `#9b59b6`, gris asfalto `#34495e`, amarillo girasol `#f1c40f`, naranja `#e67e22`, alizarina `#e74c3c`, nubes `#ecf0f1`, etc.), pero solo bajo petición explícita.

## Componentes y cómo tratarlos

Consulta `references/componentes.md` para el detalle de cada componente (botones, inputs, dropdowns, selects, navbar, barras de progreso y sliders, paginación, checkboxes, radio buttons, switches, tarjetas/"samples", tipografía). Ese archivo describe la estructura, estados y reglas CSS de cada uno tal como aparecen en el kit original, sin colores concretos.

Léelo siempre que vayas a construir alguno de estos componentes para no improvisar la forma o el comportamiento.

## Flujo de trabajo sugerido

1. **Aclara el alcance**: ¿página completa, un componente suelto, o una librería de componentes reutilizables?
2. **Aclara la paleta**: si no la ha dado, pregunta o usa placeholders neutros con variables CSS (ver arriba).
3. **Genera con variables CSS** para tipografía (una sola familia sans-serif, pesos 400/600/700), radios de borde, y colores — así el resultado es fácil de retocar.
4. **Aplica los principios fundamentales** a cada elemento: nada de sombras/gradientes, bordes finos y consistentes, buen padding, iconos planos.
5. **Verifica antes de entregar**: revisa que no se haya colado ningún `box-shadow`, `text-shadow`, `linear-gradient` ni borde con apariencia 3D.

## Notas técnicas

- Si el entregable es HTML/CSS o un artefacto React, usa la skill/guía `frontend-design` en combinación con esta para tipografía y tokens de diseño generales del entorno; esta skill aporta las reglas específicas del estilo Flat UI.
- Framework de referencia original: Bootstrap 3 + Sass, pero el estilo es igual de aplicable a CSS puro, Tailwind o React sin depender de Bootstrap.
