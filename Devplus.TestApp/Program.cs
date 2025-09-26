using Devplus.Mail;
using Devplus.Messaging;
using Devplus.Security;
using Devplus.Messaging.Interfaces;
using Devplus.TestApp.Consumers;
using Devplus.TestApp.Interfaces;
using Devplus.TestApp.Services;
using Devplus.Security.AspNetCore.DependencyInjection;
using Devplus.TestApp.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMessaging(builder.Configuration);
builder.Services.AddMail(builder.Configuration);
builder.Services.AddScoped<ITestMessageService, TestMessageService>();
builder.Services.AddScoped<IMessagingConsumer, TestConsumer>();
builder.Services.AddScoped<TestConsumer>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddDevplusSecurity(builder.Configuration);
builder.Services.ConfigureSwagger();



var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();
app.Run();