using System;
using System.ComponentModel.DataAnnotations;

namespace AMS.Models
{
    public class RefreshToken
    {
        [Key]
        [MaxLength(32)]
        public string Token { get; set; }
        public DateTime Created { get; set; }
        public DateTime Expires { get; set; }
        public DateTime? Revoked { get; set; }

        public virtual ApplicationUser User { get; set; }


        public bool IsExpired => DateTime.UtcNow >= Expires;
        public bool IsActive => Revoked == null && !IsExpired;
    }
}