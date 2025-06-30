using ContainerManager.Infrastructure.Docker;
using ContainerManager.Application.Commands;
using ContainerManager.Domain.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ✅ Register services
builder.Services.AddAuthorization();              
builder.Services.AddControllers();               
builder.Services.AddEndpointsApiExplorer();       
builder.Services.AddSwaggerGen();  
// builder.Services.AddScoped<DockerService>();   
builder.Services.AddScoped<IContainerService, DockerService>();


builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<StartContainerCommand>();
});

var app = builder.Build();

// ✅ Middleware pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();   

app.Run();
