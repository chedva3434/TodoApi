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



app.MapGet("/Items", (ToDoDbContext db) => db.Items.ToList());

app.MapGet("/items/{id}", (ToDoDbContext context, int id) =>
{
    var item = context.Items.FirstOrDefault(i => i.Id == id);
    if (item == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(item); 
});

app.MapPost("/items", (ToDoDbContext db, Item newItem) =>
{
    db.Items.Add(newItem);
    db.SaveChanges(); 
});

app.MapPut("/items/{id}", (ToDoDbContext db, int id, Item updatedItem) =>
{
    var item = db.Items.FirstOrDefault(i => i.Id == id);
    if (item == null)
    {
        return Results.NotFound(); 
    }
    item.IsCompleted = updatedItem.IsCompleted;
    db.SaveChanges();
    return Results.Ok(item); 
});

app.MapDelete("/items/{id}", (ToDoDbContext db, int id) =>
{
    var item = db.Items.FirstOrDefault(i => i.Id == id);
    if (item == null)
    {
        return Results.NotFound();
    }

    db.Items.Remove(item); 
    db.SaveChanges(); 
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
