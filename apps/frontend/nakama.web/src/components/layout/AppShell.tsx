import { Outlet } from "react-router-dom";
import { Rail } from "./Rail";
import { Topbar } from "./Topbar";

export function AppShell() {
  return (
    <div className="app">
      <Rail />
      <main className="main">
        <div className="main-content">
          <Topbar />
          <Outlet />
        </div>
      </main>
    </div>
  );
}
