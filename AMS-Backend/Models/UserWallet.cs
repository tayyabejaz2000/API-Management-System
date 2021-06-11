using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AMS.Models
{
    public class UserWallet
    {
        [Key, ForeignKey("UserForeignKey")]
        public string UserID { get; set; }
        public virtual ApplicationUser User { get; set; }
        public int Balance { get; set; }
    }
}