using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PGMSFront.Models
{
    public class LabDocModel
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceSpecification { get; set; }
        public int ServiceDocumentId { get; set; }
        public string DocumentDescription { get; set; }
        public string DocumentName { get; set; }
    }

    public class LabModel
    {
        public List<LabDocModel> labdocs { get; set; }
        public int ServiceId { get; set; }
        public int FacilityId { get; set; }

    }
    public static class Extenstion
    {
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
        {
            return items.GroupBy(property).Select(x => x.First());
        }
    }
}