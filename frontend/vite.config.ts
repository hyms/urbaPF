import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'
import { quasar, transformAssetUrls } from '@quasar/vite-plugin'
import { fileURLToPath, URL } from 'node:url'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')

  return {
    plugins: [
      vue({
        template: { transformAssetUrls }
      }),
      quasar({
        sassVariables: fileURLToPath(new URL('./src/styles/quasar-variables.scss', import.meta.url))
      })
    ],
    resolve: {
      alias: {
        '@': fileURLToPath(new URL('./src', import.meta.url))
      }
    },
    server: {
      port: 5173,
      host: true,
      proxy: {
        '/api': {
          target: 'http://localhost:5000',
          changeOrigin: true
        }
      }
    },
    define: {
      'import.meta.env': JSON.stringify({
        VITE_API_URL: env.VITE_API_URL
      })
    }
  }
})
