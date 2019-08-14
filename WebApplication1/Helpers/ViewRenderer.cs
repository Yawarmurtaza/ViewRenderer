using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace WebApplication1.Helpers
{
    public class ViewRenderer : IViewRenderer
    {
        private readonly IHttpContextAccessor httpAccessor;
        private readonly IRazorViewEngine razorEngine;
        private readonly ITempDataProvider tempDataProvider;
        private readonly IOptions<MvcViewOptions> options;

        public ViewRenderer(IOptions<MvcViewOptions> options, IRazorViewEngine razorEngine, ITempDataProvider tempDataProvider, IHttpContextAccessor httpAccessor)
        {
            this.httpAccessor = httpAccessor;
            this.razorEngine = razorEngine;
            this.tempDataProvider = tempDataProvider;
            this.options = options;
        }

        public async Task<string> RenderPartialViewToString<T>(T dataModel, string partialViewPath)
        {
            // we need action context that we will use to find given view path and to construct view context...            
            IRoutingFeature routingFeature = httpAccessor.HttpContext.Features.Get<IRoutingFeature>();
            ActionContext actionContext = new ActionContext(httpAccessor.HttpContext, routingFeature.RouteData, new ActionDescriptor());

            ViewEngineResult viewEngineResult = razorEngine.FindView(actionContext, partialViewPath, false);
            IView view;
            if (viewEngineResult.Success)
            {
                view = viewEngineResult.View;
            }
            else
            {
                // view not found ... throw exception with paths that were searched...
                ViewEngineResult getViewResult = razorEngine.GetView(null, partialViewPath, false);
                IEnumerable<string> searchedPaths = getViewResult.SearchedLocations.Concat(viewEngineResult.SearchedLocations);
                string errorMessage = string.Join(Environment.NewLine, new[] { $"Unable to find view '{ partialViewPath }'. The following locations were searched:" }.Concat(searchedPaths)); ;
                throw new InvalidOperationException(errorMessage);
            }
            
            using (StringWriter writer = new StringWriter())
            {
                // create view context that can be used for redering it later...
                ViewContext viewContext = new ViewContext(
                    actionContext,
                    view,
                    new ViewDataDictionary<T>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                    {
                        Model = dataModel // view data... this is our model that will be used to populate the view.
                    }, 
                    new TempDataDictionary(actionContext.HttpContext, tempDataProvider),
                    writer, // text writer.
                    options.Value.HtmlHelperOptions
                );

                // render the view now using view context that will populate the string writer.
                await view.RenderAsync(viewContext);
                return writer.ToString();
            }
        }        
    }
}
