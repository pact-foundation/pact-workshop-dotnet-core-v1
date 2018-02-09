using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Provider.Controllers
{
    [Route("api/[controller]")]
    public class ProviderController : Controller
    {
        private IConfiguration _Configuration { get; }

        public ProviderController(IConfiguration configuration)
        {
            this._Configuration = configuration;
        }

        // GET api/provider?validInt=[int]
        [HttpGet]
        public JsonResult Get(string validDateTime)
        {
            if(String.IsNullOrEmpty(validDateTime))
            {
                return new JsonResult(BadRequest(new { message = "validDateTime is required" }));
            }

            if(this.GetPactProviderThrowNotFoundFlag())
            {
                return new JsonResult(NotFound());
            }

            DateTime parsedDateTime;

            try
            {
                parsedDateTime = DateTime.Parse(validDateTime);
            }
            catch(Exception ex)
            {
                return new JsonResult(BadRequest(new { message = "validDateTime is not a date or time" }));
            }

            return new JsonResult(new {
                test = "NO",
                validDateTime = parsedDateTime
            });
        }

        private bool GetPactProviderThrowNotFoundFlag()
        {
            bool pactProviderShouldThrowNotFound = false;
            string pactProviderShouldThrowNotFoundEnvVarString = this._Configuration["PACT_PROVIDER_SHOULD_THROW_404"];
            Boolean.TryParse(pactProviderShouldThrowNotFoundEnvVarString, out pactProviderShouldThrowNotFound);

            return pactProviderShouldThrowNotFound;
        }
    }
}
