import js from '@eslint/js';
import globals from 'globals';
import stylistic from '@stylistic/eslint-plugin';

const filesArray = ['js/**/*.{js,jsx,ts,tsx}'];

export default [
  js.configs.recommended,
  stylistic.configs.recommended,
  {
    // Main configuration for project JS files
    files: filesArray,
    languageOptions: {
      ecmaVersion: 'latest',
      globals: {
        ...globals.browser, // Define global browser environment variables
        $: 'readonly', // Add jQuery
        moment: 'readonly', // Add moment.js
        getSelectionText: 'readonly',
        wrapText: 'readonly',
        writeLineToInput: 'readonly',
        writeSelectionLineToInput: 'readonly',
        navigationFn: 'readonly',
      },
    },
    plugins: {
      '@stylistic': stylistic,
    },
    rules: {
      ...js.configs.recommended.rules,
      ...stylistic.configs.recommended.rules,
      '@stylistic/semi': ['error', 'always'],
      '@stylistic/brace-style': ['error', '1tbs'],
    },
  },
  {
    // Configuration for vite.config.js
    files: ['vite.config.js'],
    languageOptions: {
      globals: {
        ...globals.node, // Define global Node.js environment variables
      },
    },
    rules: {
      // You can add/override rules specific to Vite config
      '@stylistic/semi': ['error', 'always'],
      '@stylistic/brace-style': ['error', '1tbs'],
    },
  },
  {
    // Global ignores
    ignores: ['node_modules/', 'dist/', '_output/', '**/*.config.js', '!vite.config.js'], // Ignore folders and configuration files, but not vite.config.js
  },
];
