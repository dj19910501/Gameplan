/**
 * The ASP.NET app creates a global BASE_URL variable which tells us the virtual directory in which the app is
 * mounted.  We can make use of this to resolve paths we want relative to the ASP.NET app
 */
export default function resolveAppUri(relativeUri) {
    return (window.URL_BASE || "/") + relativeUri;
}
