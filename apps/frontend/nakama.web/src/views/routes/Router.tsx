import type { ReactNode } from "react";
import { Navigate, Route, Routes } from "react-router-dom";
import AppShell from "../components/layout/AppShell";
import { useUser } from "../shared/contexts/UserContext";
import AnalyticsPage from "../pages/analytics/AnalyticsPage";
import LoginPage from "../pages/auth/LoginPage";
import ChatPage from "../pages/chat/ChatPage";
import ChatbotDetailPage from "../pages/chatbots/ChatbotDetailPage";
import ChatbotsListPage from "../pages/chatbots/ChatbotsListPage";
import CreateChatbotPage from "../pages/chatbots/CreateChatbotPage";
import CreateFormationPage from "../pages/formations/CreateFormationPage";
import FormationExecutePage from "../pages/formations/FormationExecutePage";
import FormationManagePage from "../pages/formations/FormationManagePage";
import FormationsListPage from "../pages/formations/FormationsListPage";
import HomePage from "../pages/home/HomePage";
import LlmProvidersPage from "../pages/llmProviders/LlmProvidersPage";

/** Bounces non-admins back to Chat — gates the admin-only sections (Chatbots, Analítica). */
function RequireAdmin({ children }: { children: ReactNode }) {
  const { isAdmin } = useUser();
  return isAdmin ? <>{children}</> : <Navigate to="/chat" replace />;
}

/** The learner's landing page is Inicio; admins keep landing on Chat. */
function LandingRedirect() {
  const { isAdmin } = useUser();
  return <Navigate to={isAdmin ? "/chat" : "/inicio"} replace />;
}

function RequireLearner({ children }: { children: ReactNode }) {
  const { isAdmin } = useUser();
  return isAdmin ? <Navigate to="/chat" replace /> : <>{children}</>;
}

function RequireAuth({ children }: { children: ReactNode }) {
  const { currentUser, isRestoring } = useUser();
  if (isRestoring) return null;
  return currentUser ? <>{children}</> : <Navigate to="/login" replace />;
}

function RedirectIfAuthenticated({ children }: { children: ReactNode }) {
  const { currentUser, isRestoring } = useUser();
  if (isRestoring) return null;
  return currentUser ? <LandingRedirect /> : <>{children}</>;
}

export default function Router() {
  return (
    <Routes>
      <Route path="/" element={<LandingRedirect />} />
      <Route
        path="login"
        element={
          <RedirectIfAuthenticated>
            <LoginPage />
          </RedirectIfAuthenticated>
        }
      />
      <Route
        element={
          <RequireAuth>
            <AppShell />
          </RequireAuth>
        }
      >
        <Route
          path="inicio"
          element={
            <RequireLearner>
              <HomePage />
            </RequireLearner>
          }
        />
        <Route path="chat" element={<ChatPage />} />
        <Route
          path="chatbots"
          element={
            <RequireAdmin>
              <ChatbotsListPage />
            </RequireAdmin>
          }
        />
        <Route
          path="chatbots/new"
          element={
            <RequireAdmin>
              <CreateChatbotPage />
            </RequireAdmin>
          }
        />
        <Route
          path="chatbots/:key"
          element={
            <RequireAdmin>
              <ChatbotDetailPage />
            </RequireAdmin>
          }
        />
        <Route path="formacion" element={<FormationsListPage />} />
        <Route
          path="formacion/new"
          element={
            <RequireAdmin>
              <CreateFormationPage />
            </RequireAdmin>
          }
        />
        <Route path="formacion/:key" element={<FormationExecutePage />} />
        <Route
          path="formacion/:key/gestionar"
          element={
            <RequireAdmin>
              <FormationManagePage />
            </RequireAdmin>
          }
        />
        <Route
          path="analytics"
          element={
            <RequireAdmin>
              <AnalyticsPage />
            </RequireAdmin>
          }
        />
        <Route
          path="proveedores"
          element={
            <RequireAdmin>
              <LlmProvidersPage />
            </RequireAdmin>
          }
        />
        <Route path="*" element={<LandingRedirect />} />
      </Route>
    </Routes>
  );
}
