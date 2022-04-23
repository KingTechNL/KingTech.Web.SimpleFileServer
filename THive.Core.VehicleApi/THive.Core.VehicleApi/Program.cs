using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using THive.Core.DeviceApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));



// Add Database connection
//builder.Configure<MongoDbSettings>();
//builder.AddMongoDbContext<IGenericMongoDbContext<WeatherForecast>, GenericMongoDbContext<WeatherForecast>>();

// Add REST endpoints
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Allow CORS
var useCors = builder.AddCorsPolicy<CorsSettings>("CorsPolicy");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (useCors)
    app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
