using APIAggregationAssignment.Services;
using Serilog;

//with this line I'm initializing the Serilog logger. It's one of the many third party loggers available for free. I installed it via the NuGet Package Manager. After installing it, I ran the two following commands in the NuGet console to enable it to log into a file and the console, respectivelly: install-package serilog.sinks.file  install-package serilog.sinks.console
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/APIAggregate.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

//telling ASP.NET to use Serilog
builder.Host.UseSerilog();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//registering HttpClient along with IExternalAPIService and its implementation
builder.Services.AddHttpClient<IExternalAPIService, ExternalAPIService>();
//registering memory caching
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
