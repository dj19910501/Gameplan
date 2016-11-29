// Each client-side template we compile into a function automatically imports this file.
import escape from 'lodash/escape';

// Export all of the things we want on the "_" that will be available to our templates

// Use old-style module.exports syntax since the template functions do not understand new ES6 module syntax
module.exports = {
    // This is required for the escape syntax (<%- %>) to work
    escape,
};
