import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import { ChatbotsProvider } from "./views/shared/contexts/ChatbotsContext.tsx";
import { FormationsProvider } from "./views/shared/contexts/FormationsContext.tsx";
import { ThemeProvider } from "./views/shared/contexts/ThemeContext.tsx";
import { ToastProvider } from "./views/shared/contexts/ToastContext.tsx";
import Router from "./views/routes/Router.tsx";
import "./index.css";

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <ThemeProvider>
      <ToastProvider>
        <ChatbotsProvider>
          <FormationsProvider>
            <BrowserRouter>
              <Router />
            </BrowserRouter>
          </FormationsProvider>
        </ChatbotsProvider>
      </ToastProvider>
    </ThemeProvider>
  </StrictMode>
);
