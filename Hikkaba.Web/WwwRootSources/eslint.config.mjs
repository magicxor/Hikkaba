// eslint.config.js
import js from '@eslint/js';
import globals from 'globals';
import eslintConfigPrettier from 'eslint-config-prettier'; // Disables ESLint rules that conflict with Prettier

export default [
  js.configs.recommended, // Applies recommended rules from @eslint/js
  eslintConfigPrettier, // Applies Prettier configuration to disable conflicting rules
  {
    // Main configuration for project JS files
    files: ['js/**/*.js'], // Apply only to files in the js folder
    languageOptions: {
      ecmaVersion: 'latest', // Use the latest ECMAScript version
      globals: {
        ...globals.browser, // Define global browser environment variables
        $: 'readonly', // Add jQuery
        moment: 'readonly', // Add moment.js
        getSelectionText: 'readonly',
        wrapText: 'readonly',
        writeLineToInput: 'readonly',
        writeSelectionLineToInput: 'readonly',
        navigationFn: 'readonly',
        // Here you can add other global variables if they are used
        // myCustomGlobal: "readonly"
      },
    },
    rules: {
      // Here you can override recommended rules or add custom ones
      // Example:
      // "no-unused-vars": "warn"
      'no-undef': 'off',
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
    },
  },
  {
    // Global ignores
    ignores: ['node_modules/', 'dist/', '_output/', '**/*.config.js', '!vite.config.js'], // Ignore folders and configuration files, but not vite.config.js
  },
];
