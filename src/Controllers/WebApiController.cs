using System.Linq;
using System.Security.Claims;
using Flunt.Notifications;
using Microsoft.AspNetCore.Mvc;

namespace PlusUltra.WebApi.Controllers
{
    [ApiController]
    public class WebApiController : ControllerBase
    {
        /// <summary>
        /// Obtém o ID do usuário logado
        /// </summary>
        protected string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier).Value;

        /// <summary>
        /// Obtém o e-mail do usuário logado
        /// </summary>
        /// <returns></returns>
        protected string GetUserEmail() => User.FindFirst(ClaimTypes.Email).Value;

        protected UnprocessableEntityObjectResult ValidationError(Notifiable validation)
        {
            return UnprocessableEntity(validation.Notifications);
        }
    }
}