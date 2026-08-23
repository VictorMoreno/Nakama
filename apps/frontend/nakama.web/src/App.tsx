import type { ReactNode } from "react";
import { Navigate, Route, Routes } from "react-router-dom";
import { AppShell } from "./components/layout/AppShell";
import { useChatbots } from "./context/ChatbotsContext";
import { AnalyticsView } from "./features/analytics/AnalyticsView";
import { ChatView } from "./features/chat/ChatView";
import { ChatbotDetailView } from "./features/chatbots/ChatbotDetailView";
import { ChatbotsListView } from "./features/chatbots/ChatbotsListView";
import { CreateChatbotView } from "./features/chatbots/CreateChatbotView";
import { CreateCurriculumView } from "./features/curriculum/CreateCurriculumView";
import { CurriculumDetailView } from "./features/curriculum/CurriculumDetailView";
import { CurriculumsListView } from "./features/curriculum/CurriculumsListView";

/** Bounces non-admins back to Chat — gates the admin-only sections (Chatbots, Analítica). */
function RequireAdmin({ children }: { children: ReactNode }) {
  const { isAdmin } = useChatbots();
  return isAdmin ? <>{children}</> : <Navigate to="/chat" replace />;
}

export default function App() {
  return (
    <Routes>
      <Route element={<AppShell />}>
        <Route index element={<Navigate to="/chat" replace />} />
        <Route path="chat" element={<ChatView />} />
        <Route
          path="chatbots"
          element={
            <RequireAdmin>
              <ChatbotsListView />
            </RequireAdmin>
          }
        />
        <Route
          path="chatbots/new"
          element={
            <RequireAdmin>
              <CreateChatbotView />
            </RequireAdmin>
          }
        />
        <Route
          path="chatbots/:key"
          element={
            <RequireAdmin>
              <ChatbotDetailView />
            </RequireAdmin>
          }
        />
        <Route path="formacion" element={<CurriculumsListView />} />
        <Route
          path="formacion/new"
          element={
            <RequireAdmin>
              <CreateCurriculumView />
            </RequireAdmin>
          }
        />
        <Route path="formacion/:key" element={<CurriculumDetailView />} />
        <Route
          path="analytics"
          element={
            <RequireAdmin>
              <AnalyticsView />
            </RequireAdmin>
          }
        />
        <Route path="*" element={<Navigate to="/chat" replace />} />
      </Route>
    </Routes>
  );
}
