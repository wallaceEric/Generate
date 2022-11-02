using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration; 
using generate.Services;
using generate.Helpers.MarkovChain;
using generate.Helpers.Settings;
using generate.Helpers.Errors;

namespace generate
{
    public class Program
    {
        
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            {
                builder.Services.AddControllers().AddJsonOptions(x =>
                {
                    // serialize enums as strings in api responses (e.g. Role)
                    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

                    // ignore omitted parameters on models to enable optional params (e.g. User update)
                    x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });
                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
                builder.Services.AddSingleton<IReviewService, ReviewService>();
                
            }
            var app = builder.Build();

            {
                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();
                app.UseAuthorization();

                // global error handler
                app.UseMiddleware<ErrorHandlerMiddleware>();
                app.MapControllers();
                app.Run();
            }
        }
    }
}