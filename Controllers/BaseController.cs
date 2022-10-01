using RTGSWebApi.Infrastructure.Utility;
using RTGSWebApi.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RTGSWebApi.Transaction;
using RTGSWebApi.LoginSecurity;

namespace RTGSWebApi.Controllers
{
    public class BaseController : Controller
    {
        protected IUnitOfWork Uow { get; set; }
        
        protected ILogger Logger { get; set; }
        protected IOptions<ConfigurationOptions> Options { get; set; }
        protected ITransActionDP TDp { get; set; }
        protected IUnitOfWorkCbs UowCbs { get; set; }

        protected ILoginDP LDp { get; set; }
    }
}