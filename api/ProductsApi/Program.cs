using Microsoft.EntityFrameworkCore;
using ProductsApi.Data;
using ProductsApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Sql"),
                   sql => sql.EnableRetryOnFailure()));
builder.Services.AddApplicationInsightsTelemetry();   // Application Insights (APM)
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(builder.Configuration["Cors:Origin"] ?? "http://localhost:5500")
    .AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated(); // MVP: sem migrations

app.MapHealthChecks("/health");

var api = app.MapGroup("/api/products");

api.MapGet("/", async (AppDbContext db, int page = 1, int size = 20) =>
    await db.Products.Where(p => p.Active).OrderBy(p => p.Name)
        .Skip((page - 1) * size).Take(size).ToListAsync());

api.MapGet("/{id:guid}", async (Guid id, AppDbContext db) =>
    await db.Products.FindAsync(id) is { } p ? Results.Ok(p) : Results.NotFound());

api.MapPost("/", async (Product input, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(input.Sku) || string.IsNullOrWhiteSpace(input.Name) || input.Price < 0)
        return Results.BadRequest("Sku, Name e Price (>= 0) são obrigatórios.");
    if (await db.Products.AnyAsync(p => p.Sku == input.Sku))
        return Results.Conflict("SKU já cadastrado.");
    input.Id = Guid.NewGuid();
    db.Products.Add(input);
    await db.SaveChangesAsync();
    return Results.Created($"/api/products/{input.Id}", input);
});

api.MapPut("/{id:guid}", async (Guid id, Product input, AppDbContext db) =>
{
    var p = await db.Products.FindAsync(id);
    if (p is null) return Results.NotFound();
    (p.Name, p.Description, p.Price, p.Stock) = (input.Name, input.Description, input.Price, input.Stock);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

api.MapDelete("/{id:guid}", async (Guid id, AppDbContext db) =>
{
    var p = await db.Products.FindAsync(id);
    if (p is null) return Results.NotFound();
    p.Active = false;                       // soft delete
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
