namespace AMS.Models
{
    public class UserWallet
    {
        public int id { get; set; }
        public string UserID { get; set; }
        public ApplicationUser User { get; set; }
        public int Balance { get; set; }
    }
}