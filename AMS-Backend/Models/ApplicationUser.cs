using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;

namespace AMS.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public virtual UserWallet Wallet { get; set; }
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
    }
}