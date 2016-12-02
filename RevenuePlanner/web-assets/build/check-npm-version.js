var execSync = require('child_process').execSync;

/**
 * Verifies that the installed npm version is >= the supplied major version
 * @param {} minMajorVersion 
 * @returns {} 
 */
module.exports = function(minMajorVersion) {
    var npmVersion = execSync("npm --version");

    if (Buffer.isBuffer(npmVersion)) {
        npmVersion = npmVersion.toString();
    }

    npmVersion = npmVersion.trim();

    var match = /^(\d+)\.\d+\.\d+/.exec(npmVersion);
    var major = match && match.length > 1 && parseInt(match[1]);

    if (!(major >= minMajorVersion)) {
        console.error("invalid npm version: " + npmVersion);
        console.error("npm version " + minMajorVersion + " or better required");
        process.exit(1);
    }

    console.log("npm is up to date (version " + npmVersion + ")");
};
