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
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IRazorViewEngine razorViewEngine;
        private readonly ITempDataProvider tempDataProvider;
        private readonly IOptions<MvcViewOptions> viewOptions;

        public ViewRenderer(IOptions<MvcViewOptions> viewOptions, IRazorViewEngine razorViewEngine, ITempDataProvider tempDataProvider, IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.razorViewEngine = razorViewEngine;
            this.tempDataProvider = tempDataProvider;
            this.viewOptions = viewOptions;
        }

        public async Task<string> RenderPartialViewToString<T>(T model, string partialViewPath)
        {
            // we need action context that we will use to find given view path and to construct view context...            
            IRoutingFeature routingFeature = httpContextAccessor.HttpContext.Features.Get<IRoutingFeature>();
            ActionContext actionContext = new ActionContext(httpContextAccessor.HttpContext, routingFeature.RouteData, new ActionDescriptor());

            ViewEngineResult findViewResult = razorViewEngine.FindView(actionContext, partialViewPath, false);
            IView view;
            if (findViewResult.Success)
            {
                view = findViewResult.View;
            }
            else
            {
                // view not found ... throw exception...
                ViewEngineResult getViewResult = razorViewEngine.GetView(null, partialViewPath, false);
                IEnumerable<string> searchedLocations = getViewResult.SearchedLocations.Concat(findViewResult.SearchedLocations);
                string errorMessage = string.Join(Environment.NewLine, new[] { $"Unable to find view '{ partialViewPath }'. The following locations were searched:" }.Concat(searchedLocations)); ;
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
                        Model = model // view data... this is our model that will be used to populate the view.
                    }, 
                    new TempDataDictionary(actionContext.HttpContext, tempDataProvider),
                    writer, // text writer.
                    viewOptions.Value.HtmlHelperOptions
                );

                // render the view now using view context that will populate the string writer.
                await view.RenderAsync(viewContext);
                return writer.ToString();
            }
        }        
    }
}
