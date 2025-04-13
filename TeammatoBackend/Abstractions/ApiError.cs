
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace TeammatoBackend.Abstractions
{
   
    public class ApiSimpleResponse
    {
        public string Code{get;set;}
        public string ?DevMessage{get;set;}
        public string UserMessage{get;set;}

        public ApiSimpleResponse(string code, string userMessage, string devMessage = null)
        {
            this.Code = code;
            this.UserMessage = userMessage;
        }

    }
        
}