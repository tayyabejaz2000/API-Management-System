using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMS.Models
{
    public class APIModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string sampleCall { get; set; }
        public string desc { get; set; }
        public uint price { get; set; }
    }
}