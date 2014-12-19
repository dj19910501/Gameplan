using System.Web;
using System.Web.Optimization;

namespace RevenuePlanner
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {

            bundles.Add(new StyleBundle("~/Content/cssstyle").Include("~/Content/css/style.css"));

            bundles.Add(new StyleBundle("~/Content/bootstrap").Include(
                        "~/Content/css/bootstrap.css", 
                        "~/Content/css/bootstrap-responsive.css"));

            bundles.Add(new ScriptBundle("~/bundles/jsjquery").Include(
                        "~/Scripts/js/jquery.min.js",
                        "~/Scripts/js/jquery.slimscroll.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jsbootstrap").Include(
                        "~/Scripts/js/bootstrap.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jsscript").Include(
                        "~/Scripts/js/scripts.js"));

            bundles.Add(new ScriptBundle("~/bundles/nFormjs").Include(
                        "~/Scripts/js/NaturalLanguageForm/modernizr.custom.js",
                        "~/Scripts/js/NaturalLanguageForm/nlform.js"));

            bundles.Add(new StyleBundle("~/Content/nFormCss").Include(
                        "~/Content/css/NaturalLanguageForm/default.css",
                        "~/Content/css/NaturalLanguageForm/component.css"));

            bundles.Add(new StyleBundle("~/Content/css/GetCSS").Include(
                 "~/Content/css/font-awesome.css",
                "~/Content/css/bootstrap.css",
                //  "~/Content/css/style.css",
                     "~/Content/css/datepicker.css",
                //  "~/Content/css/style_extended.css",
                     "~/Content/css/tipsy.css",
                     "~/Content/css/dhtmlxgantt.css",
                     "~/Content/css/summernote.css",
                     "~/Content/css/jquery.multiselect.css"
                    ));

            bundles.Add(new StyleBundle("~/Content/css/GetCSSForLogin").Include(
               "~/Content/css/font-awesome.css",
              "~/Content/css/bootstrap.css",
                   "~/Content/css/summernote.css"
                  ));

            bundles.Add(new ScriptBundle("~/bundles/GetJSForLogin").Include(
                        "~/Scripts/js/jquery.js",
                        "~/Scripts/js/bootstrap.js",
                        "~/Scripts/js/jquery.slimscroll_min.js",
                        "~/Scripts/js/jquery.slidepanel_min.js",
                        //"~/Scripts/js/scripts.js",
                        "~/Scripts/summernote_min.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/GetJS").Include(
                        "~/Scripts/js/DHTMLX/dhtmlxgantt.js",
                        "~/Scripts/js/jquery.js",
                        "~/Scripts/js/jquery-migrate-1.2.1.js",
                        "~/Scripts/js/jquery.slimscroll_min.js",
                        "~/Scripts/js/jquery.slidepanel_min.js",
                        "~/Scripts/js/bootstrap.js",
                       // "~/Scripts/js/scripts.js",
                      //  "~/Scripts/js/scripts_extended.js",
                        "~/Scripts/jquery.form.js",
                         "~/Scripts/js/bootstrap-datepicker.js",
                        "~/Scripts/js/jquery.price_format.1.8.js",
                        "~/Scripts/js/slimScrollHorizontal.js",
                         "~/Scripts/js/jquery.tipsy.js",
                        "~/Scripts/jquery.selectbox-0.2.js",
                         "~/Scripts/summernote_min.js",
                        "~/Scripts/js/jquery-ui.js",
                        "~/Scripts/js/jquery.multiselect_v1.js",
                        "~/Scripts/js/jquery.multiselect.filter.js"
                        ));

            BundleTable.EnableOptimizations = true;
            
        }
    }
}