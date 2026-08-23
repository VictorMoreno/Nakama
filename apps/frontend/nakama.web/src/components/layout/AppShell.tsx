import { Outlet } from "react-router-dom";
import { Rail } from "./Rail";
import { Topbar } from "./Topbar";

export function AppShell() {
  return (
    <div className="app">
      <Rail />
      <main className="main">
        <Topbar />
        <Outlet />
      </main>
    </div>
  );
}
