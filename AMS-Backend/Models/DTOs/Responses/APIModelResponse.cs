using System;

namespace AMS.Models.DTOs.Responses
{
    public class APIModelResponse
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public string desc { get; set; }
        public string sampleCall { get; set; }
        public uint price { get; set; }

    }
}
