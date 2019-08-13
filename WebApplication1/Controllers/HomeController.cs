using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using WebApplication1.Helpers;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private ICompositeViewEngine _viewEngine;
        private readonly IViewRenderer renderer;

        public HomeController(IViewRenderer renderer)
        {
            this.renderer = renderer;
            // Error CS1503  Argument 1: cannot convert from 'Microsoft.AspNetCore.Mvc.Razor.RazorViewEngineOptions' to 'Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Mvc.MvcViewOptions>' WebApplication1 C:\Users\yakhuwaj\source\repos\WebApplication1\WebApplication1\Controllers\HomeController.cs    24  Active

            //_viewEngine = viewEngine;

            //IOptions<MvcViewOptions> opt =

            //_viewEngine = new CompositeViewEngine();
        }

        public async Task<IActionResult> Index()
        {
            EmailDataModel model = new EmailDataModel { Body = "This is email body | You request has been <strong>submitted!</strong>" };
            string emailContent = await this.renderer.RenderPartialViewToString<EmailDataModel>(model, "Templates/Email/_EmailView");
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<string> RenderPartialViewToString(string viewName, EmailDataModel model)
        {

            if (string.IsNullOrEmpty(viewName))
            {
                viewName = ControllerContext.ActionDescriptor.ActionName;
            }

            ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                ViewEngineResult viewResult = _viewEngine.FindView(ControllerContext, viewName, false);

                ViewContext viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);

                return writer.GetStringBuilder().ToString();
            }
        }
    }
}
