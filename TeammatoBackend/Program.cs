using Microsoft.EntityFrameworkCore;
using TeammatoBackend;
using TeammatoBackend.Database;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDBContext>(optionsAction: options=>
{
    options.UseNpgsql("Host=localhost;Username=teammato;Password=12345;Database=teammato;");
});
builder.Services.AddAuthentication("access-jwt-token").AddJwtBearer("refresh-jwt-token", (options)=>
{
    
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();



app.MapControllers();
// Configure the HTTP request pipeline.


app.Run();