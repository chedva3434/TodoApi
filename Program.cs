using TodoApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()  
              .AllowAnyMethod()  
              .AllowAnyHeader(); 
    });
});

builder.Services.AddEndpointsApiExplorer();  
builder.Services.AddSwaggerGen(); 

builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("bgwslqsib9wgay8d1atw"),
                     Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.33-mysql")));

builder.Services.AddDbContext<ToDoDbContext>();

var app = builder.Build();



app.MapGet("/Items", async(ToDoDbContext db) => await db.Items.ToListAsync());

app.MapGet("/items/{id}", async(ToDoDbContext context, int id) =>
{
    var item = await context.Items.FirstOrDefaultAsync(i => i.Id == id);
    if (item == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(item); 
});

app.MapPost("/items", (ToDoDbContext db, Item newItem) =>
{
    newItem.IsCompleted = false;
    db.Items.Add(newItem);
    db.SaveChangesAsync(); 
});

app.MapPut("/items/{id}",async (ToDoDbContext db, int id, Item updatedItem) =>
{
    var item = db.Items.FirstOrDefault(i => i.Id == id);
    if (item != null)
    {
        item.IsCompleted = updatedItem.IsCompleted;
    }  
    return await db.SaveChangesAsync();
});

app.MapDelete("/items/{id}", async(ToDoDbContext db, int id) =>
{
    var item = await db.Items.FirstOrDefaultAsync(i => i.Id == id);
    if (item == null)
    {
        return Results.NotFound();
    }

    db.Items.Remove(item); 
    await db.SaveChangesAsync(); 
    return Results.Ok(); 
});

//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
    c.RoutePrefix = string.Empty;
});
//}

app.MapGet("/", () => "TodoApi Api is running!");
app.UseCors("AllowAll");

app.Run();
