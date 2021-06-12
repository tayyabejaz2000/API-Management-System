using System.Collections.Generic;

namespace AMS.Configuration
{
    public class AuthResult : ResponseDTO
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}