using ReviewFilms.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationDbContext(builder.Configuration);
builder.Services.AddApiControllers();
builder.Services.AddApiSwagger();

var app = builder.Build();

app.UseGlobalExceptionMiddleware();
app.UseApiSwagger();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
