import { Outlet } from "react-router-dom";
import styles from "./AppShell.module.scss";
import Rail from "./Rail";
import Topbar from "./Topbar";

export default function AppShell() {
  return (
    <div className={styles.app}>
      <Rail />
      <main className={styles.main}>
        <div className={styles.mainContent}>
          <Topbar />
          <Outlet />
        </div>
      </main>
    </div>
  );
}
