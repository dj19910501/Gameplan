var webpack = require('webpack');
var ExtractTextPlugin = require("extract-text-webpack-plugin");
var HtmlWebpackPlugin = require('html-webpack-plugin');
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

var isWatchMode = process.argv && process.argv.indexOf("--watch") !== -1;

var jsSuffix = ".js";
var entry = path.resolve("src/main.js");
var plugins = [
    new webpack.NoErrorsPlugin(),
    // Emit an HTML snippet for including any CSS
    new HtmlWebpackPlugin({
        template: path.resolve(path.join("build", "include-css.htm")),
        hash: false,
        filename: "include-css.txt",
        inject: false,
        minify: false
    }),
    // Emit an HTML snippet for including any JavaScript
    new HtmlWebpackPlugin({
        template: path.resolve(path.join("build", "include-js.htm")),
        hash: false,
        filename: "include-js.txt",
        inject: false,
        minify: false
    })
];

if (!EMBEDDED_CSS) {
    plugins.push(new ExtractTextPlugin("[name]-[contenthash].css"));
}

if (MINIFY) {
    jsSuffix = ".min.js";
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
        filename: libraryName + "-[hash]" + jsSuffix,
        publicPath: undefined // ASP.NET app will inject a BASE_URL to tell us where the app is hosted and we will use this to set the path in main.js
    },
    bail: !isWatchMode,
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
            },
            {
                test: /\.ejs$/,
                loader: "underscore-template-loader",
                exclude: /\/build\//,
                query: {
                    engine: "util/templateEngine",
                    withImports: false,
                    prependFilenameComment: IS_RELEASE ? undefined : path.resolve("src"),
                    variable: "o"
                }
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
