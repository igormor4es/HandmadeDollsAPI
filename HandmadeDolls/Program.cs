using HandmadeDolls.Data;
using HandmadeDolls.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MiniValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Handmade Dolls API",
        Description = "Developed and Implemented by Igor de Moraes Silva Ribeiro",
        Contact = new OpenApiContact { Name = "Igor de Moraes Silva Ribeiro", Email = "igormoraes90@yahoo.com.br" },
    });
});

builder.Services.AddDbContext<MinimalContextDb>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();




app.MapGet("/Product", async (MinimalContextDb context) => 
   
    await context.Products
                 .Include("Dolls")
                 .Include("Accessories")
                 .ToListAsync()

).WithName("GetProducts")
 .WithTags("Product");





app.MapGet("/Product/{id}", async (int id, MinimalContextDb context) => 

    await context.Products
                 .Include("Dolls")
                 .Include("Accessories")
                 .FirstOrDefaultAsync(p => p.Id == id)
                 is Product product 
                 ? Results.Ok(product) 
                 : Results.NotFound()

).Produces<Product>(StatusCodes.Status200OK)
 .Produces<Product>(StatusCodes.Status404NotFound)
 .WithName("GetProductById")
 .WithTags("Product");



app.MapPost("/Product", async (MinimalContextDb context, Product product) =>
{
    if (!MiniValidator.TryValidate(product, out var errors))
        return Results.ValidationProblem(errors);

    context.Products.Add(product);
    var result = await context.SaveChangesAsync();

    return result > 0
        ? Results.CreatedAtRoute("GetProductById", new { id = product.Id }, product)
        : Results.BadRequest("Houve um problema ao salvar o registro!");

}).ProducesValidationProblem()
  .Produces<Product>(StatusCodes.Status201Created)
  .Produces(StatusCodes.Status400BadRequest)
  .WithName("PostProduct")
  .WithTags("Product");





app.Run();