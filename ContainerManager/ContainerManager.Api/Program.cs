using ContainerManager.Infrastructure.Docker;
using ContainerManager.Application.Commands;
using ContainerManager.Domain.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();              
builder.Services.AddControllers();               
builder.Services.AddEndpointsApiExplorer();       
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<StartContainerCommand>();
});


#if DEBUG
builder.Services.AddScoped<IContainerService, FakeDockerService>();
#else
builder.Services.AddScoped<IContainerService, DockerService>();
#endif

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();   

app.Run();
