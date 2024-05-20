using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.Ocelot.Provider.AppConfiguration;
using Microsoft.AspNetCore.Mvc;
using ApiGateway.Interfaces;
using ApiGateway.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using MMLib.SwaggerForOcelot.DependencyInjection;
using System.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true).AddEnvironmentVariables();
builder.Services.AddOcelot(builder.Configuration)
    .AddCacheManager(x =>
    {
        x.WithDictionaryHandle();
    });


builder.Services.AddSwaggerForOcelot(builder.Configuration);

builder.Services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

builder.Services.AddScoped<IJwtService, JwtService>();






var app = builder.Build();



app.UseSwaggerForOcelotUI(builder.Configuration);


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    //app.UseSwaggerUI();
    app.UseSwaggerUI(setup =>
    {
        setup.SwaggerEndpoint("v1/swagger.json", "TelephoneServiceAPI");
        setup.SwaggerEndpoint("v2/swagger.json", "UserServiceAPI");
    });
}

app.UsePathBase("/gateway");

app.UseRouting();

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

//app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();

app.UseAuthorization();
app.MapControllers();

await app.UseOcelot();

app.Run();