﻿var webpack = require('webpack');
var ExtractTextPlugin = require("extract-text-webpack-plugin");
var path = require("path");

// *************************************************************
// Build Configuration
var BUILD_CONFIG = process.env.BUILD_CONFIG || "";

var IS_RELEASE = /Prod|Release/i.test(BUILD_CONFIG);
var IS_STAGING = /Staging/i.test(BUILD_CONFIG);

var MINIFY = IS_RELEASE || IS_STAGING; // minify the code?
var SOURCEMAPS = !IS_RELEASE; // generate source maps?
var EMBEDDED_CSS = false; // true to embed the CSS within the JavaScript

var outputFolder = path.resolve('dist');
var libraryName = "libhive9";
// *************************************************************








var outputFileName = libraryName + ".js";
var entry = path.resolve("src/main.js");
var plugins = [
    new webpack.NoErrorsPlugin()
];

if (!EMBEDDED_CSS) {
    plugins.push(new ExtractTextPlugin("hive9lib.css"));
}

if (MINIFY) {
    outputFileName = libraryName + ".min.js";
    plugins.push(
        new webpack.optimize.OccurrenceOrderPlugin(),
        new webpack.optimize.DedupePlugin(),
        new webpack.optimize.UglifyJsPlugin({
            compress: {
                unused: true,
                dead_code: true,
                warnings: false
            }
        }));
}

function extractCSS(loaders) {
    /*
     If not running in webpack HMR, we need to extract compiled CSS into standalone files
     http://stackoverflow.com/questions/34133808/webpack-ots-parsing-error-loading-fonts/34133809#34133809
     */
    if (EMBEDDED_CSS) {
        return "style!" + loaders;
    }

    return ExtractTextPlugin.extract("style-loader", loaders);
}

var config = {
    entry: [entry],
    devtool: SOURCEMAPS ? "source-map" : undefined,
    output: {
        path: outputFolder,
        filename: outputFileName,
        publicPath: "",
        libraryTarget: 'umd',
        library: libraryName,
        umdNamedDefine: true
    },
    bail: true,
    // Declare libraries that are loaded globally so we can still "import" them
    externals: [
        {
            "jquery": "var $",
            "": "var x"
        },
        function (context, request, callback) {
            // Look for imports of the DHTMLX global objects
            if (/^dhtml/.test(request) || request === "dhx4") {
                return callback(null, "var " + request);
            }
            callback();
        }
    ],
    module: {
        loaders: [
            {
                test: /(\.jsx|\.js)$/,
                loader: 'babel',
                exclude: /node_modules/
            },
            {
                test: /\.scss$/,
                include: /src/,
                loader: extractCSS("css?modules&sourceMap&importLoaders=1&localIdentName=[name]__[local]___[hash:base64:5]!sass?sourceMap")
            }
        ]
    },
    resolve: {
        root: path.resolve('src'),
        extensions: ['', '.js', '.jsx']
    },
    plugins: plugins
};

module.exports = config;