using System;

namespace AMS.Models
{
    public class BoughtAPIs
    {
        public DateTime boughtOn { get; set; }
        public DateTime? expiresOn { get; set; }
        public string UserID { get; set; }
        public ApplicationUser User { get; set; }
        public Guid apiID { get; set; }
        public APIModel api { get; set; }
        public bool IsExpired => DateTime.UtcNow >= expiresOn;
    }
}