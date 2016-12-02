// This is a node script which runs during the VS build process to build and bundle the web assets
var checkNpmInstall = require('./check-npm-install');
var checkNpmVersion = require('./check-npm-version');

checkNpmVersion(3);
checkNpmInstall();
