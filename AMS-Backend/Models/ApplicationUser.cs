using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace AMS.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
    }
}