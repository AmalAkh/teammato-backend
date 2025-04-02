
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.WebSockets;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace TeammatoBackend.WebSockets
{
    
    public static class GameSessionsStorage
    {
        public static readonly GameSessionPool GameSessionPool = new GameSessionPool();




    }
     
}