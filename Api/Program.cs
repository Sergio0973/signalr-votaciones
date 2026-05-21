using Api.Extensions;
using Api.Hubs;
using Api.Services;
using Application.Abstractions;
using Application.UseCases;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureCors();
builder.Services.AddMapsterConfiguration();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreatePollCommand).Assembly));
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IPollHubService, PollHubService>();
builder.Services.AddSignalR();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHub<PollHub>("/pollHub");

app.Run();
