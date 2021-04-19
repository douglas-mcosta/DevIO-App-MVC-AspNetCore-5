using DevIO.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace DevIO.App.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly INotificador _noticador;

        protected BaseController(INotificador noticador)
        {
            _noticador = noticador;
        }

        protected bool OperacaoValida() {

            return !_noticador.TemNotificacao();
        }
    }
}
