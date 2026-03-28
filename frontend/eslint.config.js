import js from '@eslint/js'
import vuePlugin from 'eslint-plugin-vue'
import prettier from 'eslint-config-prettier'

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
    ignores: ['**/*.ts', '**/*.d.ts', 'node_modules/**', 'src/env.d.ts'],
    languageOptions: {
      ecmaVersion: 'latest',
      sourceType: 'module',
      globals: {
        browser: true,
        node: true,
        es2021: true
      }
    },
    rules: {
      'no-unused-vars': 'warn',
      'no-console': 'warn',
      'no-useless-assignment': 'warn'
    }
  }
]
