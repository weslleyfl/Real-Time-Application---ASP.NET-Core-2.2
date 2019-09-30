using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ASC.Web.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
      
    }
}