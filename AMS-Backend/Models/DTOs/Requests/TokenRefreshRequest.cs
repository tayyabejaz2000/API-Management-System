using System.ComponentModel.DataAnnotations;

namespace AMS.Models.DTOs.Requests
{
    class TokenRefreshRequest
    {
        [Required]
        public string Token { get; set; }
    }
}