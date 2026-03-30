import js from '@eslint/js'
import vuePlugin from 'eslint-plugin-vue'
import prettier from 'eslint-config-prettier'
import tseslint from '@typescript-eslint/eslint-plugin'
import tsparser from '@typescript-eslint/parser'

export default [
  js.configs.recommended,
  ...vuePlugin.configs['flat/essential'],
  prettier,
  {
    files: ['**/*.vue'],
    languageOptions: {
      parserOptions: {
        ecmaVersion: 'latest',
        sourceType: 'module'
      },
      globals: {
        browser: true,
        node: true,
        es2021: true,
        localStorage: 'readonly',
        sessionStorage: 'readonly',
        navigator: 'readonly',
        console: 'readonly',
        FormData: 'readonly',
        setTimeout: 'readonly',
        setInterval: 'readonly',
        clearTimeout: 'readonly',
        clearInterval: 'readonly',
        fetch: 'readonly',
        Blob: 'readonly',
        File: 'readonly',
        Image: 'readonly'
      }
    },
    rules: {
      'vue/multi-word-component-names': 'off',
      'vue/no-v-html': 'off',
      'no-unused-vars': 'warn',
      'no-console': 'warn',
      'no-useless-assignment': 'warn'
    }
  },
  {
    files: ['**/*.ts'],
    languageOptions: {
      parser: tsparser,
      parserOptions: {
        ecmaVersion: 'latest',
        sourceType: 'module'
      },
      globals: {
        browser: true,
        node: true,
        es2021: true,
        localStorage: 'readonly',
        sessionStorage: 'readonly',
        navigator: 'readonly',
        console: 'readonly',
        FormData: 'readonly',
        fetch: 'readonly',
        Blob: 'readonly',
        File: 'readonly'
      }
    },
    rules: {
      '@typescript-eslint/no-unused-vars': 'warn',
      'no-console': 'warn'
    }
  },
  {
    files: ['**/*.js'],
    languageOptions: {
      ecmaVersion: 'latest',
      sourceType: 'module',
      globals: {
        browser: true,
        node: true,
        es2021: true,
        localStorage: 'readonly',
        sessionStorage: 'readonly',
        navigator: 'readonly',
        console: 'readonly',
        FormData: 'readonly',
        fetch: 'readonly'
      }
    },
    rules: {
      'no-unused-vars': 'warn',
      'no-console': 'warn',
      'no-useless-assignment': 'warn'
    }
  }
]
