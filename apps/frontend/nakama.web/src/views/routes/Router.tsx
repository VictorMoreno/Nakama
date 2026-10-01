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

/** Bounces non-admins back to Chat — gates the admin-only sections (Chatbots, Analítica). */
function RequireAdmin({ children }: { children: ReactNode }) {
  const { isAdmin } = useUser();
  return isAdmin ? <>{children}</> : <Navigate to="/chat" replace />;
}

export default function Router() {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/login" replace />} />
      <Route path="login" element={<LoginPage />} />
      <Route element={<AppShell />}>
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
        <Route path="*" element={<Navigate to="/chat" replace />} />
      </Route>
    </Routes>
  );
}
