using System.Collections.Generic;

namespace AMS.Configuration
{
    public class ResponseDTO
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; }
    }
}