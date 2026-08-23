# Guía de Entrevistas de Validación

**Fecha:** 2026-07-24  
**Duración objetivo:** 10 minutos por entrevista  
**Muestra recomendada:** 3-5 personas por perfil

---

## Hipótesis a Validar

| ID | Hipótesis | Señal de validación |
|----|-----------|---------------------|
| H1 | Juniors prefieren preguntar a bot vs senior | Expresan fricción/incomodidad al preguntar |
| H2 | El onboarding puede acelerarse | Identifican tiempo perdido buscando info |
| H3 | Seniors pierden >2h/semana en preguntas | Cuantifican interrupciones frecuentes |
| H4 | Equipos dispuestos a nutrir el bot | No rechazan invertir tiempo en documentar |
| H5 | Entender el "por qué" reduce errores | Reportan problemas por copiar sin entender |

---

## Entrevista: Junior (0-6 meses)

**5 preguntas - 10 min**

| # | Pregunta | Valida |
|---|----------|--------|
| 1 | Cuéntame la última vez que te quedaste atascado con algo del proyecto. ¿Qué hiciste primero? | H1, H2 |
| 2 | Cuando necesitas preguntar algo a un senior, ¿hay algo que te frene o te haga dudarlo? | H1 |
| 3 | ¿Usas ChatGPT o Copilot? ¿Alguna vez te dio código que luego causó problemas? | H5 |
| 4 | ¿Qué información del proyecto te ha costado más encontrar? | H2 |
| 5 | Si pudieras cambiar UNA cosa de cómo accedes a información, ¿cuál sería? | Oportunidad |

**Follow-up universal:** "¿Por qué?" o "Cuéntame más"

---

## Entrevista: Junior con experiencia (6-12 meses)

**4 preguntas - 8 min**

| # | Pregunta | Valida |
|---|----------|--------|
| 1 | Pensando en tus primeras semanas, ¿qué te hubiera ayudado a ser productivo más rápido? | H2 |
| 2 | ¿Hubo algo que aprendiste "a la mala" que podrías haber evitado si lo hubieras sabido antes? | H2, H5 |
| 3 | ¿Algún junior nuevo te ha preguntado algo que tú también preguntaste cuando llegaste? | H3 (patrón) |
| 4 | ¿Dónde está la información más útil del proyecto? ¿Y la más difícil de encontrar? | Oportunidad |

---

## Entrevista: Senior / Tech Lead

**5 preguntas - 10 min**

| # | Pregunta | Valida |
|---|----------|--------|
| 1 | En una semana típica, ¿cuántas veces te interrumpen con preguntas técnicas? ¿Cuánto tiempo te lleva cada una? | H3 |
| 2 | ¿Hay preguntas que recibes una y otra vez? Dame un ejemplo. | H3 |
| 3 | ¿Hay conocimiento importante que solo está en tu cabeza? ¿Por qué no está documentado? | H4 |
| 4 | Cuando un junior copia código de ChatGPT sin entender el contexto, ¿qué tipo de problemas genera? | H5 |
| 5 | Si una herramienta respondiera las preguntas básicas por ti, ¿qué necesitaría para que confiaras en ella? | Requisitos |

---

## Entrevista: Engineering Manager

**4 preguntas - 8 min**

| # | Pregunta | Valida |
|---|----------|--------|
| 1 | ¿Cuánto tiempo tarda alguien nuevo en ser productivo? ¿Te gustaría que fuera más rápido? | H2 |
| 2 | ¿Cuánto tiempo estimas que los seniors dedican a responder preguntas de juniors? | H3 |
| 3 | ¿Qué pasa cuando un senior clave se va de vacaciones? ¿Han tenido problemas por conocimiento no documentado? | H4 |
| 4 | Si existiera una herramienta que respondiera preguntas sobre vuestro código, ¿dedicaríais tiempo a alimentarla? ¿Cuánto sería razonable? | H4 |

---

## Registro de Respuestas

### Entrevista #___

| Campo | Valor |
|-------|-------|
| Fecha | |
| Perfil | Junior / Junior exp / Senior / Manager |
| Equipo/Depto | |
| Duración | |

**Respuestas clave:**

| Pregunta | Respuesta resumida |
|----------|-------------------|
| 1 | |
| 2 | |
| 3 | |
| 4 | |
| 5 | |

**Citas textuales relevantes:**
> 

**Observaciones:**


---

## Análisis Post-Entrevistas

### Resumen de hallazgos por hipótesis

| Hipótesis | Validada? | Evidencia |
|-----------|-----------|-----------|
| H1 | Sí | Los juniors valoran la disponibilidad instantánea del bot frente a la disponibilidad limitada de los seniors |
| H2 | Sí | El onboarding puede acelerarse al no depender de la disponibilidad de los seniors para resolver dudas |
| H3 | Sí | Liberar tiempo de los seniors les permitiría centrarse en planificar tareas con mayor detalle para los nuevos |
| H4 | Sí | Los equipos están dispuestos a nutrir el bot; se ve valor añadido en que también sirva como base de conocimiento entre seniors cuando un experto de un área no está disponible |
| H5 | Sí | Se ha detectado que juniors que usan IA sin entender el código no saben abordar cambios posteriores porque no comprendieron el código previo |

### Insights inesperados

- El bot no solo cubre la necesidad de los juniors: los seniors ven valor en usarlo como base de conocimiento compartido cuando un experto de un área no está disponible (surgió en H4).

### Decisiones derivadas

- Considerar ampliar el alcance del bot más allá de onboarding de juniors, para que también funcione como base de conocimiento consultable entre seniors (impacto potencial en RF1/RF5 y en el alcance del MVP).
