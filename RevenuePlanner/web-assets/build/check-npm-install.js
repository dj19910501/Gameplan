var crypto = require('crypto');
var fs = require('fs');
var execSync = require('child_process').execSync;

var SAVED_CHECKSUM_FILE = '.package.json.checksum';

function checksum(str) {
    return crypto.createHash('md5').update(str).digest('hex');
}

function calculateChecksum() {
    return checksum(fs.readFileSync('package.json'));
}

function getSavedChecksum() {
    try {
        return fs.readFileSync(SAVED_CHECKSUM_FILE, "utf8");
    }
    catch (e) {
        return undefined;
    }
}

/**
 * Calculate a checksum of package.json
 * if it does not match the checksum in SAVED_CHECKSUM_FILE
 * then do a "npm install" then save the new checksum
 * 
 * @returns {} 
 */
module.exports = function() {

    var saved = getSavedChecksum();
    var calced = calculateChecksum();

    if (saved !== calced) {
        console.log("performing npm install");

        execSync("npm install");

        fs.writeFileSync(SAVED_CHECKSUM_FILE, calced, "utf8");
    } else {
        console.log("npm install up to date");
    }
};
