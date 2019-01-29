using System.Collections;
using System.Collections.Generic;

namespace TelerikPerformanceDemo.Utils
{
    public class PartialResult
    {
        private const string Aggr = "aggr";
        public IEnumerable Data { get; }
        public int Total { get; }
        public IDictionary<string, object> Aggregates { get; }
        public PartialResult(IEnumerable data, int total, object aggregates = null)
        {
            Data = data;
            Total = total;
            Aggregates = new Dictionary<string, object>();

            if (aggregates != null)
            {
                Aggregates.Add(Aggr, aggregates);
            }
        }
    }
}
