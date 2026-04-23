using ReviewFilms.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationDbContext(builder.Configuration);
builder.Services.AddApiControllers();
builder.Services.AddApiSwagger();
builder.Services.AddApiCors();

var app = builder.Build();

app.UseGlobalExceptionMiddleware();
app.UseApiSwagger();

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
