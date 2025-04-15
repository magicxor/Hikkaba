import { defineConfig } from 'vite';
import { resolve } from 'path';

export default defineConfig({
  build: {
    outDir: resolve(__dirname, '../wwwroot'), // Base output directory
    emptyOutDir: false, // Do not clean wwwroot entirely, only target assets
    sourcemap: true, // Generate sourcemaps for JS
    rollupOptions: {
      input: {
        site_js: resolve(__dirname, 'js/site.js'),
        site_css: resolve(__dirname, 'css/site.css'),
      },
      output: {
        // Configure JS output
        entryFileNames: 'js/site.min.js', // Output JS to wwwroot/js/site.min.js
        chunkFileNames: 'js/[name]-[hash].min.js', // Chunks if any
        assetFileNames: (assetInfo) => {
          // Check if the original asset name corresponds to our CSS entry point
          if (assetInfo.names.some((name) => name === 'site_css.css')) {
            // Force the CSS output name
            return 'css/site.min.css'; // Output CSS to wwwroot/css/site.min.css
          }
          // Default asset handling (images, fonts, etc.) - adjust if needed
          return 'assets/[name]-[hash][extname]';
        },
      },
    },
    minify: 'terser', // Use terser for minification (default)
    terserOptions: {
      // Vite enables these by default usually, but explicitly setting for clarity
      keep_classnames: true,
      keep_fnames: true,
      output: {
        comments: false, // Remove comments
      },
    },
  },
  // Ensure assets paths are relative to the server root in dev mode if needed
  // base: '/',
});
