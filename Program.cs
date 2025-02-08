using TodoApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;


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


var app = builder.Build();

//if (app.Environment.IsDevelopment())
{
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
    c.RoutePrefix = string.Empty;
});
}

app.MapGet("/Items", async (ToDoDbContext db) => { return await db.Items.ToListAsync(); });

app.MapGet("/Items/{id}", async(ToDoDbContext context, int id) =>
{
    var item = await context.Items.FirstOrDefaultAsync(i => i.Id == id);
    if (item == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(item); 
});

app.MapPost("/Items", async (ToDoDbContext db, Item newItem) =>
{ 
    newItem.IsCompleted = false;
    db.Items.Add(newItem);
    await db.SaveChangesAsync(); 
});

app.MapPut("/Items/{id}", async (int id, [FromBody]Item item, ToDoDbContext Db) =>
{
    var itemToUpdate = await Db.Items.FirstOrDefaultAsync(a => a.Id == id);
    Console.WriteLine(item);
    if (itemToUpdate != null)
    {
        itemToUpdate.IsCompleted = item.IsCompleted;
    }
    await Db.SaveChangesAsync();
});


app.MapDelete("/Items/{id}", async(ToDoDbContext db, int id) =>
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


app.MapGet("/", () => "TodoApi Api is running!");
app.UseCors("AllowAll");

app.Run();
