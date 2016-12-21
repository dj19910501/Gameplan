using System;
using System.Web.Mvc;
using System.Collections;
using System.Web;

namespace RevenuePlanner.Controllers.Filters
{
    public class AntiForgeryTokenFilter : ActionFilterAttribute
    {
        /// <summary>
        /// We either generate token or validate token but not both
        /// </summary>
        private bool _generateToken;

        public const string ANTI_FORGERY_TOKENS_SESSION_KEY = "CSRF_TOKEN";
        public const string ANTI_FORGERY_TOKEN_CONTROL_NAME = "_AntiForgeryToken";

        public AntiForgeryTokenFilter(bool generateToken = true)
        {
            _generateToken = generateToken;
        }

        /// <summary>
        /// Validate token passed through header or by _
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!_generateToken)
            {
                Hashtable antiForgeryTokens = filterContext.HttpContext.Session[ANTI_FORGERY_TOKENS_SESSION_KEY] as Hashtable;

                var url = filterContext.RequestContext.HttpContext.Request.Url.ToString();
                var referral = filterContext.RequestContext.HttpContext.Request.UrlReferrer.ToString();
                string CSRFToken = filterContext.RequestContext.HttpContext.Request.Headers[ANTI_FORGERY_TOKENS_SESSION_KEY];

                if (antiForgeryTokens[url].ToString() != CSRFToken && antiForgeryTokens[referral].ToString() != CSRFToken)
                {
                    //check post data 
                    CSRFToken = filterContext.RequestContext.HttpContext.Request.Params[ANTI_FORGERY_TOKEN_CONTROL_NAME];
                    if (antiForgeryTokens[url].ToString() != CSRFToken)
                    {
                        throw new Exception("Anti forgery security violation");
                    }
                }
            } else
            {
                filterContext.RequestContext.HttpContext.Response.Headers.Set("Content-Type", "text/html");
                filterContext.RequestContext.HttpContext.Response.Write(string.Format("<script>var {0} = '{1}'</script>", 
                                                                                        ANTI_FORGERY_TOKEN_CONTROL_NAME, 
                                                                                        GetAntiForgeryToken(filterContext)));
            }
        }

        /// <summary>
        /// Get CSRF Token
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private string GetAntiForgeryToken(ActionExecutingContext context)
        {
            //could use a fancier algorithm for token generation
            Guid token = Guid.NewGuid();

            //keep the new token in the session 
            Hashtable antiForgeryTokens = context.HttpContext.Session[ANTI_FORGERY_TOKENS_SESSION_KEY] as Hashtable;
            if (antiForgeryTokens == null)
            {
                antiForgeryTokens = new Hashtable();
                context.HttpContext.Session[ANTI_FORGERY_TOKENS_SESSION_KEY] = antiForgeryTokens;
            }

            antiForgeryTokens[context.RequestContext.HttpContext.Request.Url.ToString()] = token;

            return token.ToString();
        }
    }
    
    /// <summary>
    /// Use this extension to get the antiforgery token from current session 
    /// The token should be populated by the action filter on request coming in 
    /// </summary>
    public static class HttpExtension
    {
        public static MvcHtmlString GetAntiForgeryToken(this HttpContextBase context)
        {
            var currentUrl = context.Request.Url.ToString();
            var tokens = context.Session[AntiForgeryTokenFilter.ANTI_FORGERY_TOKENS_SESSION_KEY] as Hashtable;
            return new MvcHtmlString(string.Format("<input type=hidden name=\"{0}\" value=\"{1}\"/>", AntiForgeryTokenFilter.ANTI_FORGERY_TOKEN_CONTROL_NAME, tokens[currentUrl]));
        }
    }
}