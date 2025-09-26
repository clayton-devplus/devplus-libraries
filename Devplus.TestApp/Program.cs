using Devplus.Mail;
using Devplus.Messaging;
using Devplus.Security;
using Devplus.Messaging.Interfaces;
using Devplus.TestApp.Consumers;
using Devplus.TestApp.Interfaces;
using Devplus.TestApp.Services;
using Microsoft.OpenApi.Models;
using Devplus.Security.AspNetCore.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMessaging(builder.Configuration);
builder.Services.AddMail(builder.Configuration);
builder.Services.AddScoped<ITestMessageService, TestMessageService>();
builder.Services.AddScoped<IMessagingConsumer, TestConsumer>();
builder.Services.AddScoped<TestConsumer>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddDevplusSecurity(builder.Configuration);
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Devplus Ecco Connector API - Management Core.",
        Version = "v1",
        Description = "Core API Ecco Connector",
        Contact = new OpenApiContact
        {
            Name = "Dev+ Consultoria - Arquitetura",
            Email = "suporte@devplus.com.br"
        },
    });
});



var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();