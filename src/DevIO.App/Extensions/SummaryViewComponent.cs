using DevIO.Business.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DevIO.App.Extensions
{
    public class SummaryViewComponent : ViewComponent
    {
        private readonly INotificador _notification;

        public SummaryViewComponent(INotificador notification)
        {
            _notification = notification;
        }

        public async Task<IViewComponentResult> InvokeAsync() {

            var notificacoes = await Task.FromResult(_notification.ObterNotificacoes());
            notificacoes.ForEach(c => ViewData.ModelState.AddModelError(string.Empty, c.Mensagem));
            return View();
        }
    }
}
