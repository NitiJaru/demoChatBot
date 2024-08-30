using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace demoChatBot.Models
{
    public class RestaurantShortResponse
    {
        public string _id { get; set; }
        public string LogoImage { get; set; }
        public string CoverUrl { get; set; }
        public string Name { get; set; }
        public string ManaEndpointUrl { get; set; }
        public string Classification { get; set; }
        public double PredictDeliveryTotalMinutes { get; set; }
        public bool IsStandby { get; set; }
        public string BusinessAccountId { get; set; }

    }

    public class ManaDisplayResList : Paging
    {
        public IEnumerable<RestaurantShortResponse> List { get; set; }
    }
}
