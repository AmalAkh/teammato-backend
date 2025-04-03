
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TeammatoBackend.Abstractions;

namespace TeammatoBackend.Database 
{
    public class ApplicationDBContext: DbContext
    {
        public DbSet<User> Users {get;set;}
        public DbSet<Language> Languages  {get;set;}

        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options):base(options){}
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Username=teammato;Password=12345;Database=teammato;", (options)=>
            {
                options.SetPostgresVersion(17,2);
            });
            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasKey(u=>u.Id);
            modelBuilder.Entity<User>().Property(u=>u.NickName).IsRequired().HasMaxLength(32);
            modelBuilder.Entity<User>().HasIndex(u=>u.NickName).IsUnique();
            
            modelBuilder.Entity<User>().HasIndex(u=>u.Email).IsUnique();
            
            modelBuilder.Entity<User>().Property(u=>u.Image).IsRequired();
            modelBuilder.Entity<User>().Property(u=>u.Password).IsRequired();

        
           
            
            modelBuilder.Entity<Language>().HasOne(lang=>lang.User).WithMany(usr=>usr.Languages)
            .HasForeignKey(lang=>lang.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Language>().HasKey((lang)=>new {lang.ISOName, lang.UserId});

            modelBuilder.Entity<Chat>().HasMany(chat=>chat.Participants).WithMany(usr=>usr.Chats);
            modelBuilder.Entity<Chat>().HasKey(chat=>chat.Id);
            
        }

        
    }
}