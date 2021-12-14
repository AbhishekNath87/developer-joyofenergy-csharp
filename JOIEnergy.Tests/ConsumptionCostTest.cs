using JOIEnergy.Controllers;
using JOIEnergy.Domain;
using JOIEnergy.Enums;
using JOIEnergy.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json.Linq;

namespace JOIEnergy.Tests
{
    public class ConsumptionCostTest
    {
        private MeterReadingService meterReadingService;
        private ConsumptionCostController controller;
        private Dictionary<string, Supplier> smartMeterToPricePlanAccounts = new Dictionary<string, Supplier>();
        private static String SMART_METER_ID = "smart-meter-id";

        public ConsumptionCostTest()
        {
            var readings = new Dictionary<string, List<Domain.ElectricityReading>>();
            meterReadingService = new MeterReadingService(readings);
            var pricePlans = new List<PricePlan>() { 
                new PricePlan() { EnergySupplier = Supplier.DrEvilsDarkEnergy, UnitRate = 10, PeakTimeMultiplier = NoMultipliers() }, 
                new PricePlan() { EnergySupplier = Supplier.TheGreenEco, UnitRate = 2, PeakTimeMultiplier = NoMultipliers() },
                new PricePlan() { EnergySupplier = Supplier.PowerForEveryone, UnitRate = 1, PeakTimeMultiplier = NoMultipliers() } 
            };
            
            var accountService = new AccountService(smartMeterToPricePlanAccounts);
            var pricePlanService = new PricePlanService(pricePlans, meterReadingService,accountService);
            controller = new ConsumptionCostController(pricePlanService);
        }

        [Fact]
        public void ShouldCalculateCostForMeterReadingsForLastWeek()
        {
            var lstReadingsSamples = new List<ElectricityReading>();
            lstReadingsSamples.Add(new ElectricityReading { Time = DateTime.Now.AddDays(-1), Reading = 15.0m });
            lstReadingsSamples.Add(new ElectricityReading { Time = DateTime.Now.AddDays(-2), Reading = 5.0m });
            lstReadingsSamples.Add(new ElectricityReading { Time = DateTime.Now.AddDays(-3), Reading = 6.0m });
            lstReadingsSamples.Add(new ElectricityReading { Time = DateTime.Now.AddDays(-4), Reading = 7.0m });
            lstReadingsSamples.Add(new ElectricityReading { Time = DateTime.Now.AddDays(-5), Reading = 8.0m });
            lstReadingsSamples.Add(new ElectricityReading { Time = DateTime.Now.AddDays(-6), Reading = 9.0m });
            lstReadingsSamples.Add(new ElectricityReading { Time = DateTime.Now.AddDays(-7), Reading = 2.0m });

            meterReadingService.StoreReadings(SMART_METER_ID, lstReadingsSamples);

            var result = controller.CalculatedCostForLastWeekUsage(SMART_METER_ID).Value;

            //var actualCosts = ((JObject)result).ToObject<Dictionary<string, decimal>>();
            //Assert.Equal(3, actualCosts.Count);
            //Assert.Equal(100m, actualCosts["" + Supplier.DrEvilsDarkEnergy], 3);
            //Assert.Equal(20m, actualCosts["" + Supplier.TheGreenEco], 3);
            //Assert.Equal(10m, actualCosts["" + Supplier.PowerForEveryone], 3);
        }

        
        private static List<PeakTimeMultiplier> NoMultipliers()
        {
            return new List<PeakTimeMultiplier>();
        }
    }
}
