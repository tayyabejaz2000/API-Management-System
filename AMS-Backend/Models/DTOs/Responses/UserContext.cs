using AMS.Configuration;

namespace AMS.Models.DTOs.Responses
{
    public class UserContext
    {
        public string Username { get; set; }
        public string PhoneNumber { get; set; }
        public int WalletBalance { get; set; }
    }
}