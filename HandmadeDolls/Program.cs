using HandmadeDolls.Data;
using HandmadeDolls.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MiniValidation;
using NetDevPack.Identity.Jwt;
using NetDevPack.Identity.Model;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

#region Configure Services

builder.Services.AddIdentityEntityFrameworkContextConfiguration(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly("HandmadeDolls")));

builder.Services.AddDbContext<MinimalContextDb>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityConfiguration();
builder.Services.AddJwtConfiguration(builder.Configuration, "AppSettings");

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(opt =>
{
    opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ExcluirFornecedor", policy => policy.RequireClaim("ExcluirFornecedor"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Handmade Dolls API",
        Description = "Developed and Implemented by Igor de Moraes Silva Ribeiro",
        Contact = new OpenApiContact { Name = "Igor de Moraes Silva Ribeiro", Email = "igormoraes90@yahoo.com.br" },
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o Token JWT desta maneira: Bearer {seu token}",
        Name = "Authorization",
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

#endregion

#region Configure Pipeline

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthConfiguration();
app.UseHttpsRedirection();

MapActions(app);

app.Run();

#endregion


#region Actions

void MapActions(WebApplication app)
{
    #region JWT

    app.MapPost("/Api/Registro", [AllowAnonymous] async (SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IOptions<AppJwtSettings> appJwtSettings, RegisterUser registerUser) =>
    {
        if (registerUser == null)
            return Results.BadRequest("Usuário não informado.");

        if (!MiniValidator.TryValidate(registerUser, out var errors))
            return Results.ValidationProblem(errors);

        var user = new IdentityUser
        {
            UserName = registerUser.Email,
            Email = registerUser.Email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, registerUser.Password);

        if (!result.Succeeded)
            return Results.BadRequest(result.Errors);

        var jwt = new JwtBuilder().WithUserManager(userManager)
                                  .WithJwtSettings(appJwtSettings.Value)
                                  .WithEmail(user.Email)
                                  .WithJwtClaims()
                                  .WithUserClaims()
                                  .WithUserRoles()
                                  .BuildUserResponse();

        return Results.Ok(jwt);

    }).ProducesValidationProblem()
      .Produces(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest)
      .WithName("RegistroUsuario")
      .WithTags("Usuario");

    app.MapPost("/Api/Login", [AllowAnonymous] async (SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IOptions<AppJwtSettings> appJwtSettings, LoginUser loginUser) =>
    {
        if (loginUser == null)
            return Results.BadRequest("Usuário não informado.");

        if (!MiniValidator.TryValidate(loginUser, out var errors))
            return Results.ValidationProblem(errors);

        var result = await signInManager.PasswordSignInAsync(loginUser.Email, loginUser.Password, false, true);

        if (result.IsLockedOut)
            return Results.BadRequest("Usuário bloqueado.");

        if (!result.Succeeded)
            return Results.BadRequest("Usuário ou senha inválidos!");

        var jwt = new JwtBuilder()
                      .WithUserManager(userManager)
                      .WithJwtSettings(appJwtSettings.Value)
                      .WithEmail(loginUser.Email)
                      .WithJwtClaims()
                      .WithUserClaims()
                      .WithUserRoles()
                      .BuildUserResponse();

        return Results.Ok(jwt);

    }).ProducesValidationProblem()
      .Produces(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest)
      .WithName("LoginUsuario")
      .WithTags("Usuario");

    #endregion

    #region GET's

    app.MapGet("/", [AllowAnonymous] () => "Hello World =]").ExcludeFromDescription();

    app.MapGet("/Product", /*[Authorize]*/ async (MinimalContextDb context) =>
    {
        var products = await context.Products.Where(p => p.Active).ToListAsync();

        foreach (var item in products)
        {
            if (item.ProductType.ToString() == "DOLL")
            {
                var accessories = await context.DollsAccessories
                                               .AsNoTracking()
                                               .Where(a => a.DollId == item.Id)
                                               .ToListAsync();

                if (accessories.Count > 0)
                {
                    item.Accessories = new List<DollAcessory>();
                    
                    foreach (var itens in accessories)
                    {
                        var prod = await context.Products
                                                .Where(p => p.Active)
                                                .FirstOrDefaultAsync(p => p.Id == itens.AccessoryId);

                        var newProduct = new DollAcessory
                        {
                            Id = prod.Id,
                            Description = prod.Description
                        };

                        item.Accessories.Add(newProduct);
                    }
                }
            }
        }

        return products;

    }).WithName("GetProducts")
      .WithTags("Product");

    app.MapGet("/Product/{id}", /*[Authorize]*/ async (int id, MinimalContextDb context) =>
    {
        var productById = await context.Products
                                       .Where(p => p.Active)
                                       .FirstOrDefaultAsync(p => p.Id == id);

        if (productById?.ProductType.ToString() == "DOLL")
        {
            var accessories = await context.DollsAccessories
                                           .AsNoTracking()
                                           .Where(a => a.DollId == productById.Id)
                                           .ToListAsync();
            if (accessories.Count > 0)
            {
                productById.Accessories = new List<DollAcessory>();
                
                foreach (var item in accessories)
                {
                    var prod = await context.Products
                                            .Where(p => p.Active)
                                            .FirstOrDefaultAsync(p => p.Id == item.AccessoryId);

                    var newProduct = new DollAcessory
                    {
                        Id = prod.Id,
                        Description = prod.Description
                    };

                    productById.Accessories.Add(newProduct);
                }
            }
        }


        return productById is Product product ? Results.Ok(product) : Results.NotFound();
        
    }).Produces<Product>(StatusCodes.Status200OK)
      .Produces<Product>(StatusCodes.Status404NotFound)
      .WithName("GetProductById")
      .WithTags("Product");

    #endregion

    #region POST's

    app.MapPost("/Product", /*[Authorize]*/ async (MinimalContextDb context, Product product) =>
    {
        var result = 0;
        
        if (!MiniValidator.TryValidate(product, out var errors))
            return Results.ValidationProblem(errors);

        if (product.ProductType.ToString() == "DOLL")
        {
            var accessories = product.Accessories?.ToList();          

            if (accessories != null && accessories.Count > 0)
            {
                var newProduct = new Product
                {
                    Description = product.Description,
                    Price = product.Price,
                    Stock = product.Stock,
                    Image = product.Image,
                    Active = product.Active,
                    ProductType = product.ProductType,
                };

                context.Products.Add(newProduct);
                result = await context.SaveChangesAsync();

                foreach (var item in accessories)
                {
                    var accessory = await context.Products.FirstOrDefaultAsync(p => p.Id == item.AccessoryId);

                    if (accessory != null)
                    {
                        if (accessory.ProductType == ProductType.DOLL)
                        {
                            using (var connection = context.Database.GetDbConnection())
                            {
                                await connection.OpenAsync();
                                using (var command = connection.CreateCommand())
                                {
                                    command.CommandText = $"DELETE DollsAccessories WHERE DollId = {newProduct.Id}";
                                    await command.ExecuteNonQueryAsync();

                                    command.CommandText = $"DELETE Products WHERE Id = {newProduct.Id}";
                                    await command.ExecuteNonQueryAsync();
                                }
                            }

                            return Results.BadRequest("It is not possible to add " + accessory.Description + " as a gift.");
                        }
                        
                        if (!accessory.Active)
                        {
                            using (var connection = context.Database.GetDbConnection())
                            {
                                await connection.OpenAsync();
                                using (var command = connection.CreateCommand())
                                {
                                    command.CommandText = $"DELETE DollsAccessories WHERE DollId = {newProduct.Id}";
                                    await command.ExecuteNonQueryAsync();

                                    command.CommandText = $"DELETE Products WHERE Id = {newProduct.Id}";
                                    await command.ExecuteNonQueryAsync();
                                }
                            }

                            return Results.BadRequest("Sorry! The accessory " + accessory.Description + " chosen as a gift is no longer available.");
                        }

                        if (accessory.Stock <= 0)
                        {
                            using (var connection = context.Database.GetDbConnection())
                            {
                                await connection.OpenAsync();
                                using (var command = connection.CreateCommand())
                                {
                                    command.CommandText = $"DELETE DollsAccessories WHERE DollId = {newProduct.Id}";
                                    await command.ExecuteNonQueryAsync();

                                    command.CommandText = $"DELETE Products WHERE Id = {newProduct.Id}";
                                    await command.ExecuteNonQueryAsync();
                                }
                            }

                            return Results.BadRequest("Sorry! It will not be possible to add the accessory " + accessory.Description + " as a gift as there is no more stock.");
                        }

                        DollAcessory dollAcessory = new()
                        {
                            DollId = newProduct.Id,
                            AccessoryId = accessory.Id
                        };

                        context.DollsAccessories.Add(dollAcessory);
                        result = await context.SaveChangesAsync();
                    }                    
                }    
            }
            else
            {
                context.Products.Add(product);
                result = await context.SaveChangesAsync();
            }
        }
        else
        {
            context.Products.Add(product);
            result = await context.SaveChangesAsync();
        }

        return result > 0
            ? Results.CreatedAtRoute("GetProductById", new { id = product.Id }, product)
            : Results.BadRequest("There was a problem saving the record!");

    }).ProducesValidationProblem()
      .Produces<Product>(StatusCodes.Status201Created)
      .Produces(StatusCodes.Status400BadRequest)
      .WithName("PostProduct")
      .WithTags("Product");

    #endregion
}

#endregion