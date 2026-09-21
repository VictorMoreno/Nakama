import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  css: {
    modules: {
      // CSS Modules classes are accessed as styles.camelCaseName in components (e.g.
      // ".chat-panel-head" -> styles.chatPanelHead), matching the agents.md migration's
      // convention across every feature's .module.scss files.
      localsConvention: 'camelCaseOnly'
    }
  },
  server: {
   port: 3000,
   strictPort: true,
   proxy:{
     "/api/":{
       target: "https://localhost:7080",
       changeOrigin: true,
       secure: false,
     }
   } 
  }
})
