
using Microsoft.EntityFrameworkCore;
using TeammatoBackend.Abstractions;
namespace TeammatoBackend.Database 
{
    public class ApplicationDBContext: DbContext
    {
        public DbSet<User> Users {get;set;}
        public  ApplicationDBContext(DbContextOptions options):base(options)
        {   

        }

        
    }
}