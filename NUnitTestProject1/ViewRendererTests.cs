using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using WebApplication1.Helpers;

namespace Tests
{
    public class ViewRendererTests
    {
        private Mock<IHttpContextAccessor> mockHttpContextAccessor;
        private Mock<IRazorViewEngine> mockRazorViewEngine;
        private Mock<ITempDataProvider> mockTempDataProvider;
        private Mock<IOptions<MvcViewOptions>> mockViewOptions;

        [SetUp]
        public void Setup()
        {
            mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockRazorViewEngine = new Mock<IRazorViewEngine>();
            mockTempDataProvider = new Mock<ITempDataProvider>();
            mockViewOptions = new Mock<IOptions<MvcViewOptions>>();
        }

        [Test]
        public async Task RenderPartialViewToString_should_successfully_render_view_into_string()
        {
            // Arrange.

            string data = "this is the data";
            string partialViewPath = "temp/view/myview";

            // Http context
            Mock<HttpContext> mockContext = new Mock<HttpContext>();
            Mock<IFeatureCollection> mockFeaturesColl = new Mock<IFeatureCollection>();
            Mock<IRoutingFeature> mockRoutingFeature = new Mock<IRoutingFeature>();
            Mock<RouteData> mockRoutData = new Mock<RouteData>();
            mockRoutingFeature.Setup(m => m.RouteData).Returns(mockRoutData.Object);
            mockFeaturesColl.Setup(m => m.Get<IRoutingFeature>()).Returns(mockRoutingFeature.Object);            
            mockContext.Setup(m => m.Features).Returns(mockFeaturesColl.Object);
            mockHttpContextAccessor.Setup(m => m.HttpContext).Returns(mockContext.Object);

            // Razor view engine mocks
            Mock<IViewEngine> mockEngine = new Mock<IViewEngine>();
            Mock<IView> mockView = new Mock<IView>();                       
            mockRazorViewEngine.Setup(m => m.FindView(It.IsAny<ActionContext>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(ViewEngineResult.Found(partialViewPath, mockView.Object));            
            IViewRenderer renderer = new ViewRenderer(mockViewOptions.Object, mockRazorViewEngine.Object, mockTempDataProvider.Object, mockHttpContextAccessor.Object);

            // IOptions

            Mock<MvcViewOptions> mockMvcOptions = new Mock<MvcViewOptions>();
            MvcViewOptions opt = new MvcViewOptions();
            opt.HtmlHelperOptions = new HtmlHelperOptions();
            //mockMvcOptions.Setup(m => m.HtmlHelperOptions).Returns(new HtmlHelperOptions());
            mockViewOptions.Setup(m => m.Value).Returns(opt);

            // Act.

            string htmlString = await renderer.RenderPartialViewToString(data, partialViewPath);

            // Assert.

            // since we are not passing in any view ... the result html string should be empty.
            Assert.IsEmpty(htmlString);
        }
    }
};