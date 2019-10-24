using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ASC.Web.Filters;

namespace ASC.Web.Controllers
{
    [Authorize]
    [UserActivityFilter]
    public class BaseController : Controller
    {
      
    }
}