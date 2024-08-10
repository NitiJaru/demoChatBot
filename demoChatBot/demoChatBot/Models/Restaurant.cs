using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace demoChatBot.Models
{
    public class Restaurant
    {
        public string _id { get; set; }
        public string BusinessAccountId { get; set; }
        public string ManaCode { get; set; }
        public string LogoImage { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CoverUrl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BannerUrl { get; set; }
        public string Name { get; set; }
        public string Classification { get; set; }
        public string Address { get; set; }
        public string Remark { get; set; }
        public GeoJsonPoint<GeoJson2DGeographicCoordinates> Coordinates { get; set; }
        public string PhoneNumber { get; set; }
        public double Rating { get; set; }
        public string Note { get; set; }
        public double PreparePerOrderAverageMinutes { get; set; }
        public List<Category> CategoryList { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SesionId { get; set; }
        public string OwnerManaId { get; set; }
        public string OwnerName { get; set; }
        public string ManaEndpointUrl { get; set; }
        /// <summary>
        /// Ready to Receive Order 
        /// </summary>
        public bool? IsStandby { get; set; } 
    }

    public class Category
    {
        public string _id { get; set; }
        public string Name { get; set; }
        public bool canNote { get; set; }
        public bool CanRemove { get; set; }
        public IEnumerable<string> MenuOptionIds { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
    }
}
