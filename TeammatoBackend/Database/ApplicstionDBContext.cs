

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TeammatoBackend.Abstractions;
using TeammatoBackend.WebSockets;

namespace TeammatoBackend.Database 
{
    // ApplicationDbContext for managing the database context of the application
    public class ApplicationDBContext: DbContext
    {
        // DbSets representing the tables in the database
        public DbSet<User> Users {get;set;}
        public DbSet<Language> Languages {get;set;}
        public DbSet<Chat> Chats {get;set;}

        public DbSet<GameSession> GameSessions {get;set;}

        public DbSet<Message> Messages {get;set;}
        public DbSet<FavoriteGame> FavoriteGames {get;set;}


        // Constructor to accept DbContextOptions
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options):base(options){}

        // Configure database connection and PostgreSQL version
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Username=teammato;Password=12345;Database=teammato;", (options)=>
            {
                options.SetPostgresVersion(17,2); // Set PostgreSQL version
            });
        }
        // Model configuration for entity relationships and constraints
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User entity configurations
            modelBuilder.Entity<User>().HasKey(u=>u.Id);
            modelBuilder.Entity<User>().Property(u=>u.NickName).IsRequired().HasMaxLength(32);
            modelBuilder.Entity<User>().HasIndex(u=>u.NickName).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u=>u.Email).IsUnique();
            modelBuilder.Entity<User>().Property(u=>u.Password).IsRequired();

            // Language entity configurations
            modelBuilder.Entity<Language>().HasOne(lang=>lang.User).WithMany(usr=>usr.Languages)
                .HasForeignKey(lang=>lang.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Language>().HasKey((lang)=>new {lang.ISOName, lang.UserId});

            // Chat entity configurations
            modelBuilder.Entity<Chat>().HasMany(chat=>chat.Participants).WithMany(usr=>usr.Chats);
            modelBuilder.Entity<Chat>().HasOne((chat)=>chat.Owner).WithMany((usr)=>usr.OwnedChats);
            modelBuilder.Entity<Chat>().HasKey(chat=>chat.Id);

            // FavoriteGame entity configurations
            modelBuilder.Entity<FavoriteGame>().HasOne(game=>game.User).WithMany(usr=>usr.FavoriteGames).HasForeignKey(usr=>usr.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<FavoriteGame>().HasKey(game=>new{ game.GameId, game.UserId});
          
            // Message entity configurations
            modelBuilder.Entity<Message>().HasKey(msg => msg.Id);
            modelBuilder.Entity<Message>()
                .HasOne(msg => msg.Sender)
                .WithMany(user => user.Messages)
                .HasForeignKey(msg => msg.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Message>()
                .HasOne(msg => msg.Chat)
                .WithMany(chat => chat.Messages)
                .HasForeignKey(msg => msg.ChatId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GameSession>().HasOne((gameSession)=>gameSession.Owner).WithMany((usr)=>usr.OwnedGameSessions);
            modelBuilder.Entity<GameSession>().HasMany((gameSession)=>gameSession.Participants).WithMany((usr)=>usr.ParticipatedGameSessions);
            
            modelBuilder.Entity<GameSession>().HasKey((gameSession)=>gameSession.Id);
            //modelBuilder.Entity<GameSession>().Property((gameSession)=>gameSession.Description).
            //modelBuilder.Entity()




        }

        
    }
}