using Flunt.Notifications;
using Microsoft.AspNetCore.Mvc;

namespace src.Controllers
{
    public class BasePresenter
    {
        public IActionResult ViewModel { get; protected set; }

        public void NotFound()
        {
            ViewModel = new NotFoundResult();
        }

        public void ValidationOnErrorOcurred(Notifiable validation)
        {
            ViewModel = new UnprocessableEntityObjectResult(validation.Notifications);
        }
    }
}