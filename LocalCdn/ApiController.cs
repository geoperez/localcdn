using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Modules;

namespace LocalCdn
{
    public class ApiController : WebApiController
    {
        [WebApiHandler(HttpVerbs.Get, "/api/entries/*")]
        public bool Get(WebServer server, HttpListenerContext context)
        {
            try
            {
                var data = EntryRepository.GetAll();

                var lastSegment = context.Request.Url.Segments.Last();

                if (lastSegment.EndsWith("/"))
                    return context.JsonResponse(data.Select(x => x.Name));

                if (data.Any(p => p.Name == lastSegment))
                {
                    return context.JsonResponse(data.FirstOrDefault(p => p.Name == lastSegment));
                }

                throw new KeyNotFoundException("Key Not Found: " + lastSegment);
            }
            catch (Exception ex)
            {
                return HandleError(context, ex);
            }
        }

        [WebApiHandler(HttpVerbs.Get, "/api/hosts/*")]
        public bool GetHosts(WebServer server, HttpListenerContext context)
        {
            try
            {
                var data = LocalCdn.HostEntries;

                return context.JsonResponse(data);
            }
            catch (Exception ex)
            {
                return HandleError(context, ex);
            }
        }

        [WebApiHandler(HttpVerbs.Post, "/api/entries")]
        public bool Post(WebServer server, HttpListenerContext context)
        {
            try
            {
                var model = context.ParseJson<Entry>();
                LocalCdn.AddEntry(model.Url);

                return true;
            }
            catch (Exception ex)
            {
                return HandleError(context, ex);
            }
        }

        [WebApiHandler(HttpVerbs.Delete, "/api/entries")]
        public bool Delete(WebServer server, HttpListenerContext context)
        {
            try
            {
                var model = context.ParseJson<Entry>();
                // TODO: Complete
                return true;
            }
            catch (Exception ex)
            {
                return HandleError(context, ex);
            }
        }

        protected bool HandleError(HttpListenerContext context, Exception ex, int statusCode = 500)
        {
            var errorResponse = new
            {
                Title = "Unexpected Error",
                ErrorCode = ex.GetType().Name,
                Description = ex.ExceptionMessage(),
            };

            context.Response.StatusCode = statusCode;
            return context.JsonResponse(errorResponse);
        }
    }
}
