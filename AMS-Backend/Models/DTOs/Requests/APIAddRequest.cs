using System.ComponentModel.DataAnnotations;

namespace AMS.Models.DTOs.Requests
{
    public class APIAddRequest
    {
        [Required]
        public string name { get; set; }
        [Required]
        public string url { get; set; }
        public string sampleCall { get; set; }
        public string desc { get; set; }
        [Required]
        public uint price { get; set; }
    }
}