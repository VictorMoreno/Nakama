import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  css: {
    modules: {
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
