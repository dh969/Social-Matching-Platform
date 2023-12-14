using Microsoft.AspNetCore.Mvc;
using udemyCourse.Helper;

namespace udemyCourse.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [ApiController]
    [Route("api/[controller]")]
    public class BaseController : ControllerBase
    {

    }
}
