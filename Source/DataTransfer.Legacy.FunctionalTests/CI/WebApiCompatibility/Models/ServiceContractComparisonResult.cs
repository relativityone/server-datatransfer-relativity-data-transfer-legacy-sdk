using System.Collections.Generic;
using System.Linq;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.CI.WebApiCompatibility.Models
{
    public class ServiceContractComparisonResult
    {
        public List<ServiceMethodComparisonResult> MethodComparisonResults { get; set; }

        public bool IsValid => MethodComparisonResults.All(r => r.IsValid);

        public ServiceContractComparisonResult()
        {
            MethodComparisonResults = new List<ServiceMethodComparisonResult>();
        }
    }
}
