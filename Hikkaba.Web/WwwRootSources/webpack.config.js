const path = require('path'); // node's built-in path module
const outputDir = path.join('_output', 'js-bundle');
const isDevelopment = process.env.NODE_ENV === 'development';
const TerserPlugin = require('terser-webpack-plugin');
const webpack = require('webpack');

module.exports = {
    entry: {
        site: './js/site.js',
    },
    output: {
        filename: '[name].min.js',
        path: path.join(__dirname, outputDir),
    },
    devtool: false,
    plugins: [
        new webpack.SourceMapDevToolPlugin({
            filename: '[file].map',
            publicPath: '/js/'
        })
    ],
    optimization: {
        minimize: !isDevelopment,
        minimizer: [
            new TerserPlugin({
                terserOptions: {
                    keep_classnames: true,
                    keep_fnames: true,
                    output: {
                        comments: false,
                    },
                },
                extractComments: false
            })
        ]
    }
};
