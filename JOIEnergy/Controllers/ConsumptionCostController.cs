using System.Collections.Generic;
using System.Linq;
using JOIEnergy.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace JOIEnergy.Controllers
{
    [Route("consumption")]
    public class ConsumptionCostController : Controller
    {
        private readonly IPricePlanService _pricePlanService;
        //private readonly IAccountService _accountService;

        public ConsumptionCostController(IPricePlanService pricePlanService)
        {
            _pricePlanService = pricePlanService;
            //this._accountService = accountService;
        }

        [HttpGet("last-week/{smartMeterId}")]
        public ObjectResult CalculatedCostForLastWeekUsage(string smartMeterId)
        {
            var costOfLastWeekUsage = _pricePlanService.GetConsumptionCostOfElectricityReadingsForPricePlanForDateRange(smartMeterId);
            if (costOfLastWeekUsage == -1)
            {
                return new NotFoundObjectResult(string.Format("No Price Plan Found For Smart Meter Id ({0})", smartMeterId));
            }

           // dynamic response = JObject.FromObject(costOfLastWeekUsage);

            return new ObjectResult(costOfLastWeekUsage);
            //costOfLastWeekUsage ? 
            // : 
            //new NotFoundObjectResult(string.Format("Smart Meter ID ({0}) not found", smartMeterId));
        }


    }
}
