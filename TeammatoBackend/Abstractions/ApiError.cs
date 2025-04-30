
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace TeammatoBackend.Abstractions
{
   // This class represents a simple API response format used for standardizing responses across the application.
    public class ApiSimpleResponse
    {
        // The code represents a status or type of the response. 
        // For example, it could be 'success', 'error', 'not_found', etc.
        public string Code{get;set;}

        // DevMessage is an optional field that contains detailed developer messages for debugging purposes.
        public string ?DevMessage{get;set;}

        // UserMessage is the message that will be sent to the end user. 
        // It describes the outcome or status of the operation in user-friendly terms.
        public string UserMessage{get;set;}

        // Constructor for initializing the ApiSimpleResponse with a code, user message, and an optional developer message.
        public ApiSimpleResponse(string code, string userMessage, string devMessage = null)
        {
            this.Code = code; // Set the status code (e.g., 'success', 'error')
            this.UserMessage = userMessage; // Set the user-friendly message that will be shown to the client
        }
    }
        
}