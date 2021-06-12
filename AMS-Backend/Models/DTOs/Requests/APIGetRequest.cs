using System;
using System.ComponentModel.DataAnnotations;

namespace AMS.Models.DTOs.Requests
{
    public class APIGetRequest
    {
        [Required]
        public Guid id { get; set; }
    }
}