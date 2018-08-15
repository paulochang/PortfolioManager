using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using statgen.Models;

namespace statgen.Utils
{
    public static class RiskCalculator
    {
        public static async Task<RiskStatsViewDto> GetAssetRiskStats(List<double> fullReturnSet)
        {
            ImmutableList<double> roReturnSet = fullReturnSet.ToImmutableList();

            (double var, double es) varAndEs90 = await Task.Run(() => GetVarAndEsAtConfidenceLvl(roReturnSet, 90));
            (double var, double es) varAndEs95 = await Task.Run(() => GetVarAndEsAtConfidenceLvl(roReturnSet, 95));
            (double var, double es) varAndEs99 = await Task.Run(() => GetVarAndEsAtConfidenceLvl(roReturnSet, 99));

            RiskStatsViewDto partialResult = new RiskStatsViewDto(
                varAndEs90.var, varAndEs90.es,
                varAndEs95.var, varAndEs95.es,
                varAndEs99.var, varAndEs99.es
            );
            return partialResult;
        }

        private static (double var, double es) GetVarAndEsAtConfidenceLvl(ImmutableList<double> returnsSet,
            int confidenceLevel)
        {
            int fullCount = returnsSet.Count;
            int confidenceLvlCount = fullCount * (100 - confidenceLevel) / 100;

            ImmutableList<double> confidenceLvlReturnsList = returnsSet.Take(confidenceLvlCount).ToImmutableList();

            double var = confidenceLvlReturnsList.Last();
            double es = confidenceLvlReturnsList.AsParallel().Average();

            return (var, es);
        }
    }
}