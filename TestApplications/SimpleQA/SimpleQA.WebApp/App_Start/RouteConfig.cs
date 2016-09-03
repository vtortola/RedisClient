using SimpleQA.WebApp.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SimpleQA.WebApp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // HOME
            routes.MapRoute(
                name: "Default",
                url: "",
                defaults: new { controller = "Home", action = "Get" },
                constraints: new 
                { 
                    httpMethod = new HttpMethodConstraint("GET"),
                    isNotWebSocket = new WebSocketRouteConstraint( handleWebSocket: false)
                }
            );

            routes.MapRoute(
                name: "DefaultPaginated",
                url: "questions",
                defaults: new { controller = "Home", action = "Get" },
                constraints: new { httpMethod = new HttpMethodConstraint("GET") }
            );

            routes.MapRoute(
                name: "Tag",
                url: "questions/{tag}",
                defaults: new { controller = "Home", action = "ByTag" },
                constraints: new { httpMethod = new HttpMethodConstraint("GET") }
            );

            routes.MapRoute( // this is a ChildAction route
                name: "PopularTags",
                url: "populartags",
                defaults: new { controller = "Tags", action = "Popular" }
            );

            // PUSH
            routes.MapRoute(
                name: "PushUpdates",
                url: "",
                defaults: new { controller = "Push", action = "Updates" },
                constraints: new
                {
                    httpMethod = new HttpMethodConstraint("GET")
                }
            );

            // SUGGEST TAG
            routes.MapRoute(
                name: "AskTag",
                url: "tags/suggest",
                defaults: new { controller = "Tags", action = "Suggest" }
            );

            // ASK
            routes.MapRoute(
                name: "AskQuestionRead",
                url: "ask/{tag}",
                defaults: new { controller = "QuestionRead", action = "ask", tag = UrlParameter.Optional },
                constraints: new { httpMethod = new HttpMethodConstraint("GET") }
            );

            routes.MapRoute(
                name: "AskQuestionWrite",
                url: "ask",
                defaults: new { controller = "QuestionWrite", action = "ask", tag = UrlParameter.Optional },
                constraints: new { httpMethod = new HttpMethodConstraint("POST") }
            );
         
            // QUESTION
            routes.MapRoute(
                name: "QuestionRead",
                url: "question/{id}/{slug}/{action}",
                defaults: new { controller = "QuestionRead", action="get", slug = UrlParameter.Optional},
                constraints: new 
                { 
                    action = "get|edit|close|delete", 
                    httpMethod = new HttpMethodConstraint("GET")
                }
            );

            routes.MapRoute(
                name: "QuestionWrite",
                url: "question/{id}/{action}",
                defaults: new { controller = "QuestionWrite" },
                constraints: new { action = "edit|close|delete|vote", httpMethod = new HttpMethodConstraint("POST") }
            );

            routes.MapRoute(
                name: "QuestionVisitCounter",
                url: "visit/{questionId}",
                defaults: new { controller = "QuestionVisitCounter", action = "Visit" },
                constraints: new { httpMethod = new HttpMethodConstraint("POST") }
            );

            // ADD ANSWER
            routes.MapRoute(
                name: "AddAnswer",
                url: "question/{questionId}/answers",
                defaults: new { controller = "AnswerWrite", action = "Add" },
                constraints: new { httpMethod = new HttpMethodConstraint("POST") }
            );

            // ANSWERS
            routes.MapRoute(
                name: "AnswerRead",
                url: "question/{questionId}/answer/{answerid}/{action}",
                defaults: new { controller = "AnswerRead" },
                constraints: new { action = "edit|delete|get", httpMethod = new HttpMethodConstraint("GET") }
            );

            routes.MapRoute(
                name: "AnswerWrite",
                url: "question/{questionId}/answer/{answerid}/{action}",
                defaults: new { controller = "AnswerWrite" },
                constraints: new { action = "vote|delete|edit", httpMethod = new HttpMethodConstraint("POST") }
            );

            // USER
            routes.MapRoute(
                name: "UserInbox",
                url: "user/inbox",
                defaults: new { controller = "User", action = "Inbox" },
                constraints: new { httpMethod = new HttpMethodConstraint("POST") }
            );

            routes.MapRoute(
                name: "User",
                url: "user/{user}",
                defaults: new { controller = "User", action = "Index" },
                constraints: new { httpMethod = new HttpMethodConstraint("GET") }
            );

            // AUTH
            routes.MapRoute(
                name: "Auth",
                url: "auth/{action}",
                defaults: new { controller = "Authentication", action = "Login" },
                constraints: new { action = "login|logout", httpMethod = new HttpMethodConstraint("POST") }
            );
        }
    }
}
