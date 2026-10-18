import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// In local development (npm run dev) /api is proxied to the API on port 5080.
// In Docker, nginx does the same job (see nginx/default.conf.template).
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': 'http://localhost:5080',
      '/health': 'http://localhost:5080',
    },
  },
})
