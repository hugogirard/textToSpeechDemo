using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorClient.Model
{
    public class Message
    {
        [Required(ErrorMessage = "Please enter a message")]
        [StringLength(100, ErrorMessage = "The message is too long")]
        [JsonProperty("TextToConvert")]
        public string Text { get; set; }
    }
}
