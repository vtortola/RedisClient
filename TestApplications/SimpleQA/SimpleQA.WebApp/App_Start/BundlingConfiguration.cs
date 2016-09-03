using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace SimpleQA.WebApp
{
    public static class BundlingConfiguration
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            // anonymous users only see this bundle
            bundles.Add(new ScriptBundle("~/bundles/base")
                    .Include("~/Scripts/jquery-2.2.3.min.js")
                    .Include("~/Scripts/app.global.errorhandling.js")
                    .Include("~/Scripts/app.feature.login.js")
                    .Include("~/Scripts/app.action.visitquestion.js")
                    .Include("~/Scripts/app.scenebase.anonymous.js")
                    .Include("~/Scripts/app.scene.questionanonymous.js")
                    .Include("~/Scripts/app.js")
                    );

            // authenticated users see base bundle + this one
            bundles.Add(new ScriptBundle("~/bundles/user")
                    .Include("~/Scripts/jquery.validate.min.js")
                    .Include("~/Scripts/jquery.validate.unobtrusive.min.js")
                    .Include("~/Scripts/bootstrap.min.js")
                    .Include("~/Scripts/bootstrap3-typeahead.min.js")
                    .Include("~/Scripts/bootstrap-tagsinput.js")
                    .Include("~/Scripts/simplemde.min.js")
                    .Include("~/Scripts/showdown.min.js")
                                                                
                    .Include("~/Scripts/app.action.subscription.js")
                    .Include("~/Scripts/app.action.replaceHtml.js")

                    .Include("~/Scripts/app.feature.notification.js")
                    .Include("~/Scripts/app.feature.autonotifications.js")
                    .Include("~/Scripts/app.feature.usernotification.js")
                    .Include("~/Scripts/app.feature.markdown-editor.js")
                    .Include("~/Scripts/app.feature.tags-editor.js")

                    .Include("~/Scripts/app.validation.js")

                    .Include("~/Scripts/app.scenebase.authenticated.js")
                    .Include("~/Scripts/app.scene.ask.js")
                    .Include("~/Scripts/app.scene.question.js")
                    .Include("~/Scripts/app.scene.home.js")
                    .Include("~/Scripts/app.scene.tagbrowsing.js")

                    );

#if(DEBUG)
            BundleTable.EnableOptimizations = false;
#else
            BundleTable.EnableOptimizations = true;
#endif
        }
    }
}