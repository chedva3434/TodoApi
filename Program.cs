using TodoApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;



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
//{
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
    c.RoutePrefix = string.Empty;
});
//}


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

app.MapPut("/items/{id}", async (ToDoDbContext db, int id, Item updatedItem) =>
{
    Console.WriteLine("Received updatedItem: " + JsonConvert.SerializeObject(updatedItem));  // הדפסת הנתונים
    var item = await db.Items.FirstOrDefaultAsync(i => i.Id == id);
    if (item != null)
    {
        // אם יש ערך, עדכן אותו
        if (updatedItem.IsCompleted.HasValue)
        {
            item.IsCompleted = updatedItem.IsCompleted.Value;
        }
        await db.SaveChangesAsync();
        return Results.Ok(item);
    }
    return Results.NotFound();
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


app.MapGet("/", () => "TodoApi Api is running!");
app.UseCors("AllowAll");

app.Run();
