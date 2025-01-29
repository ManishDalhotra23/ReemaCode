using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Optimization;

namespace PGMSFront
{

    public class BundleConfig
    {

        public static void RegisterBundles(BundleCollection bundles)
        {

            var scriptBundle = new ScriptBundle("~/Scripts/bundle");
            var styleBundle = new StyleBundle("~/Content/bundle");

            // jQuery
            scriptBundle
                .Include("~/Scripts/jquery-3.1.0.js");


            // Bootstrap
            scriptBundle
                .Include("~/Scripts/bootstrap.js");

            // My Scripts
            bundles.Add(new ScriptBundle("~/bundles/MyScripts").Include(
                            "~/MyScripts/aes.js",
                            "~/MyScripts/MyFunctions.js",
                            "~/MyScripts/jquery.maskedinput-1.3.1.min.js",
                             "~/Templates/assets/js/Load.js",
                            "~/Templates/Alert/jquery-confirm.min.js"));

            // Bootstrap
            styleBundle
                .Include
                (
                "~/Content/bootstrap.css",
                "~/Content/Site.css",
                "~/Templates/Alert/jquery-confirm.min.css"
                );


            // Custom site styles
            styleBundle
                .Include();

            //bundles.Add(new ScriptBundle("~/bundles/MyScripts").Include(
            //            //"~/JScripts/jquery.js",
            //            "~/MyScripts/aes.js",
            //           "~/MyScripts/MyFunctions.js"));

            bundles.Add(scriptBundle);
            bundles.Add(styleBundle);

#if !DEBUG
            BundleTable.EnableOptimizations = true;
#endif
        }
    }
}