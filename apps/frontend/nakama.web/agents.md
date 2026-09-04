# React Frontend Agent Guide

Este documento define las reglas de implementación para proyectos React TypeScript con estructura y estilo como este frontend.

## 1) Stack y principios base

- React + TypeScript + Vite.
- TypeScript en modo `strict`.
- ESLint + Prettier + Stylelint.
- Enfoque por **features** con separación **domain / infrastructure / views**.
- Dependencias de infraestructura resueltas por inyección (`useInjection` + `Services`).

## 2) Arquitectura objetivo

```text
src/
  modules/
    <feature>/
      domain/
      infrastructure/
  shared/
    infrastructure/
      injection/
      multiLanguage/
  views/
    pages/
      <feature>/
        hooks/
    components/
      <feature>/
    shared/
      contexts/
      hooks/
      css/
    routes/
```

### Reglas

- `modules`: lógica de acceso a datos y contratos de dominio.
- `views/pages`: páginas contenedor (composición + inyección).
- `views/components`: UI reutilizable o específica de feature.
- `views/pages/*/hooks`: hooks de caso de uso para la página.
- `views/shared`: contexto global de UI, hooks transversales, estilos compartidos.

## 3) Naming conventions

- **Features/carpetas**: minúscula semántica (`quotes`, `sessions`, `vehicleInfo`).
- **Componentes y páginas**: `PascalCase.tsx` (`SessionsPage.tsx`, `SessionsLayout.tsx`).
- **Hooks**: `useXxx.ts` (`useSessions.ts`, `useApi.ts`).
- **Interfaces de dominio**: prefijo `I` (`ISessionRepository`).
- **Repositorios concretos**: `ApiXxxRepository.ts`.
- **Métodos de repositorio**: `PascalCase` para contratos (`Search`, `Validate`, `FindIndex`).
- **Estilos por componente**: `NombreComponente.module.scss`.
- **Contextos**: `XContext.tsx` + hook `useX`.

## 4) Pautas para componentes

- Crear `interface Props` explícita y tipada.
- Preferir función con `export default function NombreComponente(...)`.
- Mantener componentes presentacionales sin acceso directo a infraestructura.
- Inyectar dependencias en páginas/layouts y pasar datos/handlers por props.
- Usar `useEffect` solo para side effects reales (fetch, sync de contexto/storage).
- Evitar lógica de negocio compleja en JSX; moverla a hook de página.

## 5) Pautas para hooks

- Un hook = una responsabilidad funcional.
- Nombre obligatorio `useXxx`.
- Estado estándar para operaciones remotas: datos + `isLoading`.
- Exponer API mínima en retorno: `{ data, isLoading, action }`.
- Si hay navegación/gestión de errores API, reutilizar hook transversal (`useApi`).
- No capturar errores de forma silenciosa; usar flujo común de error (redirect/modal/context).

## 6) Dominio e infraestructura

- Definir contrato en `modules/<feature>/domain/I...Repository.ts`.
- Implementar en `modules/<feature>/infrastructure/Api...Repository.ts`.
- Adaptar DTO -> dominio en métodos privados de mapeo (`ToDomain`) cuando aplique.
- No consumir cliente HTTP generado desde componentes; siempre vía repositorio.

## 7) Inyección de dependencias

1. Registrar repositorio en `shared/infrastructure/injection/Services.ts`.
2. Verificar que queda disponible por `id` en `ServicesConfiguration`.
3. Consumirlo con:
   - `const repo = useInjection<IMiRepo>(Services.MiRepo.id);`

Regla: cualquier nueva dependencia de acceso a datos debe entrar por este mecanismo.

## 8) Routing y layouts

- Centralizar rutas en `views/routes/Router.tsx`.
- Componer páginas bajo layouts.
- Mantener rutas de error explícitas y navegación controlada por idioma/parámetros.

## 9) Estado compartido y contextos

- Ubicar contextos globales en `views/shared/contexts`.
- Exponer hook de consumo (`useCategory`, `useVehicle`, `useAlert`).
- Lanzar error si se usa fuera de su Provider.
- Si persiste estado de sesión/UI, encapsular acceso a `sessionStorage/localStorage` en el provider o hook.

## 10) Estilos e imports

- Usar CSS Modules SCSS para estilos por componente.
- Importar como `import styles from "./Componente.module.scss";`.
- Reutilizar estilos transversales en `views/shared/css`.
- Respetar orden de imports y reglas de `simple-import-sort`.

## 11) Checklist para crear una nueva feature

1. Crear `modules/<feature>/domain` con modelos + interfaz de repositorio.
2. Implementar `Api<Feature>Repository` en `infrastructure`.
3. Registrar servicio en `Services.ts`.
4. Crear hook en `views/pages/<feature>/hooks/use<Feature>.ts`.
5. Crear `Page` contenedora e inyectar repositorio.
6. Crear componentes UI en `views/components/<feature>`.
7. Añadir ruta en `views/routes/Router.tsx`.
8. Añadir estilos `.module.scss` colocalizados.
9. Ejecutar `npm run lint` y `npm run build`.

## 12) Comandos estándar

- Desarrollo: `npm run dev`
- Lint TS/React: `npm run lint`
- Auto-fix lint: `npm run lint-fix`
- Lint estilos: `npm run lint-css`
- Build: `npm run build`

