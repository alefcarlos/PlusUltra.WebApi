using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusUltra.WebApi.Controllers;

namespace sample.Controllers
{
    [Authorize(JwtBearerDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    public class ValuesController : WebApiController
    {
        [AllowAnonymous]
        [Route("any")]
        [HttpGet]
        public IActionResult GetAction()
        {
            return Ok("Foi carai any!");
        }

        [Route("auth")]
        [HttpGet]
        public IActionResult GetAction2()
        {
            return Ok($"{GetUserId()} -  {GetUserEmail()}");
        }
    }
}
