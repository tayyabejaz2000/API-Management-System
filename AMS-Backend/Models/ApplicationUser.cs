using System.Collections.Generic;

using Microsoft.AspNetCore.Identity;

namespace AMS.Models
{
    public class ApplicationUser : IdentityUser
    {
        public UserWallet Wallet { get; set; }
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
        public virtual ICollection<BoughtAPIs> BoughtApis { get; set; }
    }
}