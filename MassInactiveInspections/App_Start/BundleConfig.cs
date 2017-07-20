using System.Web;
using System.Web.Optimization;

namespace MassInactiveInspections
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        //public static void RegisterBundles(BundleCollection bundles)
        //{
        //    bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
        //                "~/Scripts/jquery-1.10.2.min.js"));

        //    bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
        //                "~/Scripts/bootstrap.min.js"));

        //    bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
        //                "~/Scripts/Datatables/jquery.dataTables.min.js",
        //                "~/Scripts/Datatables/dataTables.bootstrap.min.js",
        //                "~/Scripts/Datatables/dataTables.colReorder.min.js"));

        //    bundles.Add(new StyleBundle("~/Content/css").Include(
        //              "~/Content/bootstrap.min.css",
        //              "~/Content/bootstrap-theme.min.css",
        //              "~/Content/bootstrap.min.css",
        //              "~/Content/DataTables/css/jquery.dataTables.min.css",
        //              "~/Content/DataTables/css/jquery.dataTables_themeroller.css",
        //              "~/Content/DataTables/css/colReorder.bootstrap.css",
        //              "~/Content/DataTables/css/site.css"));
        //}



        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));
        }
    }
}
