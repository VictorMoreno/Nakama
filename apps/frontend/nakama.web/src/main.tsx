import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import { ErrorBoundary } from "./views/components/shared/ErrorBoundary.tsx";
import ServerErrorGate from "./views/components/shared/ServerErrorGate.tsx";
import { ChatbotsProvider } from "./views/shared/contexts/ChatbotsContext.tsx";
import { FormationsProvider } from "./views/shared/contexts/FormationsContext.tsx";
import { ThemeProvider } from "./views/shared/contexts/ThemeContext.tsx";
import { ToastProvider } from "./views/shared/contexts/ToastContext.tsx";
import { UserProvider } from "./views/shared/contexts/UserContext.tsx";
import { UsersProvider } from "./views/shared/contexts/UsersContext.tsx";
import Router from "./views/routes/Router.tsx";
import "./index.css";

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <ErrorBoundary>
      <ServerErrorGate>
        <ThemeProvider>
          <ToastProvider>
            <UserProvider>
              <UsersProvider>
                <ChatbotsProvider>
                  <FormationsProvider>
                    <BrowserRouter>
                      <Router />
                    </BrowserRouter>
                  </FormationsProvider>
                </ChatbotsProvider>
              </UsersProvider>
            </UserProvider>
          </ToastProvider>
        </ThemeProvider>
      </ServerErrorGate>
    </ErrorBoundary>
  </StrictMode>
);
