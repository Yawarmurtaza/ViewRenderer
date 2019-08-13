using System.Threading.Tasks;

namespace WebApplication1.Helpers
{
    public interface IViewRenderer
    {
        Task<string> RenderPartialViewToString<T>(T model, string viewPath);
    }
}
