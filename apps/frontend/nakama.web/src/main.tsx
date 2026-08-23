import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import App from "./App.tsx";
import { ChatbotsProvider } from "./context/ChatbotsContext.tsx";
import { CurriculumsProvider } from "./context/CurriculumsContext.tsx";
import { ThemeProvider } from "./context/ThemeContext.tsx";
import { ToastProvider } from "./context/ToastContext.tsx";
import "./index.css";

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <ThemeProvider>
      <ToastProvider>
        <ChatbotsProvider>
          <CurriculumsProvider>
            <BrowserRouter>
              <App />
            </BrowserRouter>
          </CurriculumsProvider>
        </ChatbotsProvider>
      </ToastProvider>
    </ThemeProvider>
  </StrictMode>
);
