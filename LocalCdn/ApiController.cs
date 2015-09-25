using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
                    return context.JsonResponse(data);

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

        [WebApiHandler(HttpVerbs.Post, "/api/entries/*")]
        public bool Post(WebServer server, HttpListenerContext context)
        {
            try
            {
                var model = context.ParseJson<Entry>();
                EntryRepository.Add(model);

                return true;
            }
            catch (Exception ex)
            {
                return HandleError(context, ex);
            }
        }

        [WebApiHandler(HttpVerbs.Post, "/api/entries/*")]
        public bool Delete(WebServer server, HttpListenerContext context)
        {
            try
            {
                var model = context.ParseJson<Entry>();

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
