using System;
using System.Collections.Generic;
using System.Linq;
using JOIEnergy.Domain;

namespace JOIEnergy.Services
{
    public class PricePlanService : IPricePlanService
    {
        public interface Debug { void Log(string s); };

        private readonly List<PricePlan> _pricePlans;
        private IMeterReadingService _meterReadingService;
        private IAccountService _accountService;

        public PricePlanService(List<PricePlan> pricePlan, IMeterReadingService meterReadingService, IAccountService accountService)
        {
            _pricePlans = pricePlan;
            _meterReadingService = meterReadingService;
            _accountService = accountService;
        }

        private decimal calculateAverageReading(List<ElectricityReading> electricityReadings)
        {
            var newSummedReadings = electricityReadings.Select(readings => readings.Reading).Aggregate((reading, accumulator) => reading + accumulator);

            return newSummedReadings / electricityReadings.Count();
        }

        private decimal calculateTimeElapsed(List<ElectricityReading> electricityReadings)
        {
            var first = electricityReadings.Min(reading => reading.Time);
            var last = electricityReadings.Max(reading => reading.Time);

            return (decimal)(last - first).TotalHours;
        }
        private decimal calculateCost(List<ElectricityReading> electricityReadings, PricePlan pricePlan)
        {
            var average = calculateAverageReading(electricityReadings);
            var timeElapsed = calculateTimeElapsed(electricityReadings);
            var averagedCost = average / timeElapsed;
            return averagedCost * pricePlan.UnitRate;
        }

        public Dictionary<String, decimal> GetConsumptionCostOfElectricityReadingsForEachPricePlan(String smartMeterId)
        {
            List<ElectricityReading> electricityReadings = _meterReadingService.GetReadings(smartMeterId);

            if (!electricityReadings.Any())
            {
                return new Dictionary<string, decimal>();
            }
            return _pricePlans.ToDictionary(plan => plan.EnergySupplier.ToString(), plan => calculateCost(electricityReadings, plan));
        }

        //todo: add method to accept readings in addition to meter id

        public decimal GetConsumptionCostOfElectricityReadingsForPricePlanForDateRange(String smartMeterId)
        {
            var electricityReadings = _meterReadingService.GetReadings(smartMeterId);

            var pricePlanIdForSmartMeter = _accountService.GetPricePlanIdForSmartMeterId(smartMeterId);
            var pricePlan = _pricePlans.FirstOrDefault(a => a.EnergySupplier == pricePlanIdForSmartMeter);
            if (pricePlan == null)
            {
                return -1;
            }

            //have a way of filtering out
            var dateRanges = GetStartEndDatesForLastWeek().ToList();
            var filteredReadings = electricityReadings.Where(a => a.Time >= dateRanges[0] && a.Time <= dateRanges[1]).ToList();

            if (!filteredReadings.Any())
            {
                return 0;
            }

            return calculateCost(filteredReadings, pricePlan);

            //return _pricePlans.ToDictionary(plan => plan.EnergySupplier.ToString(), plan => calculateCost(filteredReadings, plan)).Where(a=>a.Key== pricePlanForSmartMeter.ToString());
        }

        public IEnumerable<DateTime> GetStartEndDatesForLastWeek()
        {
            var startDate = DateTime.Now.AddDays(-7);
            var endDate = DateTime.Now;

            return new List<DateTime>() { startDate, endDate };
        }
    }
}
