import type { ReactNode } from "react";
import { Navigate, Route, Routes } from "react-router-dom";
import AppShell from "../components/layout/AppShell";
import { useChatbots } from "../shared/contexts/ChatbotsContext";
import AnalyticsPage from "../pages/analytics/AnalyticsPage";
import ChatPage from "../pages/chat/ChatPage";
import ChatbotDetailPage from "../pages/chatbots/ChatbotDetailPage";
import ChatbotsListPage from "../pages/chatbots/ChatbotsListPage";
import CreateChatbotPage from "../pages/chatbots/CreateChatbotPage";
import CreateFormationPage from "../pages/formations/CreateFormationPage";
import FormationDetailPage from "../pages/formations/FormationDetailPage";
import FormationsListPage from "../pages/formations/FormationsListPage";

/** Bounces non-admins back to Chat — gates the admin-only sections (Chatbots, Analítica). */
function RequireAdmin({ children }: { children: ReactNode }) {
  const { isAdmin } = useChatbots();
  return isAdmin ? <>{children}</> : <Navigate to="/chat" replace />;
}

export default function Router() {
  return (
    <Routes>
      <Route element={<AppShell />}>
        <Route index element={<Navigate to="/chat" replace />} />
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
        <Route path="formacion/:key" element={<FormationDetailPage />} />
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
