﻿using System.Web;
using System.Web.Optimization;

namespace RevenuePlanner
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {

            //bundles.Add(new StyleBundle("~/Content/css/GetCSSStyle").Include(
            //    "~/Content/css/bootstrap-responsive.css",  
            //    "~/Content/css/style_min_v1.css",
            //      "~/Content/css/style_extended_min_v1.css"
            // ));

            bundles.Add(new StyleBundle("~/Content/css/GetCSS").Include(
                 "~/Content/css/font-awesome.css",
                "~/Content/css/bootstrap.css",
                     "~/Content/css/datepicker.css",
                     "~/Content/css/dhtmlxgantt.css",
                     "~/Content/css/summernote.css",
                     "~/Content/css/jquery.multiselect.css"
                     //"~/Content/css/DHTMLX/dhtmlxtreegrid2_min.css"
                    ));

            bundles.Add(new StyleBundle("~/Content/css/GetCSSForLogin").Include(
               "~/Content/css/font-awesome.css",
              "~/Content/css/bootstrap.css",
                   "~/Content/css/summernote.css",
                    "~/Content/css/bootstrap-responsive.min.css",
                       "~/Content/css/style.css",
                     "~/Content/css/style_extended.css"
                  ));

            bundles.Add(new ScriptBundle("~/bundles/GetJSForLogin").Include(
                        "~/Scripts/js/jquery.js",
                        "~/Scripts/js/bootstrap.js",
                        "~/Scripts/js/jquery.slimscroll_min.js",
                        "~/Scripts/js/jquery.slidepanel_min.js",
                        "~/Scripts/js/scripts.js",
                        "~/Scripts/summernote_min.js",
                        "~/Scripts/js/mixpanel.init.js",
                        "~/Scripts/js/mixpanel.login.js" //ensure this script is the last called
                        ));

            bundles.Add(new ScriptBundle("~/bundles/GetJS").Include(
                        "~/Scripts/js/DHTMLX/dhtmlxgantt.js",
                        "~/Scripts/js/jquery.js",
                        "~/Scripts/js/jquery-migrate-1.2.1.js",
                        "~/Scripts/js/jquery.slimscroll_min.js",
                        "~/Scripts/js/jquery.slidepanel_min.js",
                        "~/Scripts/js/bootstrap.js",
                        "~/Scripts/jquery.form.js",
                         "~/Scripts/js/bootstrap-datepicker.js",
                        "~/Scripts/js/jquery.price_format.1.8_v2.js",
                        "~/Scripts/js/slimScrollHorizontal.js",
                        "~/Scripts/jquery.selectbox-0.2.js",
                         "~/Scripts/summernote_min.js",
                        "~/Scripts/js/jquery-ui.js",
                        "~/Scripts/js/jquery.multiselect_v1.js",
                        "~/Scripts/js/jquery.multiselect.filter.js",
                        "~/Scripts/MultiselectWeight.js",
                        "~/Scripts/jquery.cluetip.js",
                        "~/Scripts/js/scripts.js",
                        "~/Scripts/js/scripts_extended.js",
                        "~/Scripts/js/DHTMLX/dhtmlxtreegrid_min.js",
                        "~/Scripts/js/errorhandler.js",
                        "~/Scripts/dhtmlxchart.js",
                        "~/Scripts/js/ganttExportapi.js",
                        "~/Scripts/js/mixpanel.init.js",
                        "~/Scripts/js/mixpanel.layout.js"
                        ));
            bundles.Add(new ScriptBundle("~/bundles/GetJSReport").Include(
                       "~/Scripts/js/jquery.slimscroll.js",
                       "~/Scripts/js/slimScrollHorizontal.js",
                       "~/Scripts/js/jquery.mCustomScrollbar.concat.min.js",
                       "~/Scripts/js/jquery.actual.js",
                       "~/Scripts/js/highcharts.js"
                       ));
            BundleTable.EnableOptimizations = true;
            
        }
    }
}