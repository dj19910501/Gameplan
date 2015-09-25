// stack trace logic URL: http://helephant.com/2007/05/diy-javascript-stack-trace/

//Added By: Maninder Singh Wadhva on 12/10/2014 to address ticket #900 Exception handling in client side scripting
// Summary:
//     Log current javascript exception.
function logError(ex, stack) {
    if (ex == null) return;
    if (logErrorUrl == null) {
        alert('logErrorUrl must be defined.');
        return;
    }

    var url = ex.fileName != null ? ex.fileName : document.location;
    if (stack == null && ex.stack != null) stack = ex.stack;

    // format output
    var out = ex.message != null ? ex.name + ": " + ex.message : ex;
    out += ": at document path '" + url + "'.";
    if (stack != null) out += "\n  at " + stack.join("\n  at ");

    // post/log error message to Elmah.
    $.ajax({
        type: 'POST',
        url: logErrorUrl,
        data: { message: out }
    });
}

// Summary:
//     Function to get stack trace.
Function.prototype.trace = function () {
    var trace = [];
    var current = this;
    while (current) {
        trace.push(current.signature());
        current = current.caller;
    }
    return trace;
}

// Summary:
//     Function to get signature details.
Function.prototype.signature = function () {
    var signature = {
        name: this.getName(),
        params: [],
        toString: function () {
            var params = this.params.length > 0 ? "'" + this.params.join("', '") + "'" : "";
            return this.name + "(" + params + ")"
        }
    };
    if (this.arguments) {
        for (var x = 0; x < this.arguments.length; x++)
            signature.params.push(this.arguments[x]);
    }
    return signature;
}

// Summary:
//     Function to extract from the function definition.
Function.prototype.getName = function () {
    if (this.name)
        return this.name;
    var definition = this.toString().split("\n")[0];
    var exp = /^function ([^\s(]+).+/;
    if (exp.test(definition))
        return definition.split("\n")[0].replace(exp, "$1") || "anonymous";
    return "anonymous";
}

// Summary:
//     Function to hide please wait dialog.
function hidePleaseWaitDialog()
{
    if (myApp != 'undefined' && myApp != null) {
        myApp.hidePleaseWait();
    }
}

// Summary:
//     Initiates logging of every unhandled javascript exception.
window.onerror = function (msg, url, line) {
    logError(msg, arguments.callee.trace());
    hidePleaseWaitDialog()
}