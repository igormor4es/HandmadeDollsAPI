using HandmadeDolls.Data;
using HandmadeDolls.Models;
using HandmadeDolls.Repositorie;
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
    options.AddPolicy("Administrator", policy => policy.RequireClaim("Administrator"));
    options.AddPolicy("Customer", policy => policy.RequireClaim("Customer"));
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
            return Results.BadRequest("User not informed.");

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
      .WithName("RegisterUser")
      .WithTags("User");

    app.MapPost("/Api/Login", [AllowAnonymous] async (SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IOptions<AppJwtSettings> appJwtSettings, LoginUser loginUser) =>
    {
        if (loginUser == null)
            return Results.BadRequest("User not informed.");

        if (!MiniValidator.TryValidate(loginUser, out var errors))
            return Results.ValidationProblem(errors);

        var result = await signInManager.PasswordSignInAsync(loginUser.Email, loginUser.Password, false, true);

        if (result.IsLockedOut)
            return Results.BadRequest("Blocked user.");

        if (!result.Succeeded)
            return Results.BadRequest("Username or password is invalid!");

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
      .WithName("LoginUser")
      .WithTags("User");

    app.MapPost("/Api/UserClaim", [Authorize] async (SignInManager<IdentityUser> signInManager, MinimalContextDb context, string loginUser, string claim) =>
    {        
        if (loginUser == null)
            return Results.BadRequest("User not informed.");

        var currentUser = signInManager.UserManager.Users.FirstOrDefault(x => x.UserName == loginUser);
        
        if (currentUser == null)
            return Results.BadRequest("User not found!");

        var userClaim = await new UserRepository().UserClaim(currentUser, context, claim, builder.Configuration.GetConnectionString("DefaultConnection"));

        var result = userClaim switch
        {
            1 => Results.BadRequest("This user already has this Claim!"),
            2 => Results.Ok("Claim saved successfully!"),
            _ => Results.BadRequest("There was a problem saving the record!")
        };

        return result;

    }).ProducesValidationProblem()
      .Produces(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest)
      .RequireAuthorization("Administrator")
      .WithName("UserClaim")
      .WithTags("User");

    app.MapDelete("/Api/UserClaim", [Authorize] async (SignInManager<IdentityUser> signInManager, MinimalContextDb context, string loginUser, string claim) =>
    {
        if (loginUser == null)
            return Results.BadRequest("User not informed.");

        var currentUser = signInManager.UserManager.Users.FirstOrDefault(x => x.UserName == loginUser);

        if (currentUser == null)
            return Results.BadRequest("User not found!");

        var delete = await new UserRepository().DeleteUserClaim(currentUser, context, claim, builder.Configuration.GetConnectionString("DefaultConnection"));

        var result = delete switch
        {
            0 => Results.BadRequest("User does not have this claim."),
            1 => Results.Ok("Claim removed."),
            _ => Results.BadRequest("There was a problem deleting the record!")
        };

        return result;

    }).Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status404NotFound)
      .RequireAuthorization("Administrator")
      .WithName("DeleteUserClaim")
      .WithTags("User");

    #endregion

    #region GET's

    app.MapGet("/", [AllowAnonymous] () => "Hello World =]").ExcludeFromDescription();

    app.MapGet("/Product", /*[Authorize]*/ async (MinimalContextDb context) =>
    {
        var products = await new ProductRepository().GetAllProducts(context);

        return products;

    }).WithName("GetProducts")
      .WithTags("Product");

    app.MapGet("/Product/{id}", [Authorize] async (int id, MinimalContextDb context) =>
    {
        var productById = await new ProductRepository().GetProductById(id, context);

        return productById is Product product ? Results.Ok(product) : Results.NotFound();
        
    }).Produces<Product>(StatusCodes.Status200OK)
      .Produces<Product>(StatusCodes.Status404NotFound)
      .WithName("GetProductById")
      .WithTags("Product");

    #endregion

    #region POST's

    app.MapPost("/Product", [Authorize] async (MinimalContextDb context, Product product) =>
    {        
        if (!MiniValidator.TryValidate(product, out var errors))
            return Results.ValidationProblem(errors);

        var postProduct = await new ProductRepository().PostProduct(context, product);

        var result = postProduct switch
        {
            1 => Results.CreatedAtRoute("GetProductById", new { id = product.Id }, product),
            2 => Results.BadRequest("It is not possible to add the accessory as a gift."),
            3 => Results.BadRequest("Sorry! The accessory chosen as a gift is no longer available."),
            4 => Results.BadRequest("Sorry! It will not be possible to add the accessory as a gift, as the stock is out of stock."),
            5 => Results.BadRequest("Sorry! It will not be possible to add the accessory as a gift as there is no more stock available at the moment."),
            _ => Results.BadRequest("There was a problem saving the record!")
        };

        return result;

    }).ProducesValidationProblem()
      .Produces<Product>(StatusCodes.Status201Created)
      .Produces(StatusCodes.Status400BadRequest)
      .RequireAuthorization("Administrator")
      .WithName("PostProduct")
      .WithTags("Product");

    #endregion

    #region PUT's

    app.MapPut("/Api/Fornecedor/{id}", [Authorize] async (int id, MinimalContextDb context, Product product) =>
    {
        var putProduct = await context.Products.AsNoTracking<Product>().FirstOrDefaultAsync(p => p.Id == id);

        if (putProduct == null)
            return Results.NotFound();

        if (!MiniValidator.TryValidate(product, out var errors))
            return Results.ValidationProblem(errors);

        //É necessário verificar se na tabela DollsAccessories ele não possui registros nela. Caso ele já exista lá e vc deseje alterar o seu ProductType isso não será possível.
        context.Products.Update(product).State = EntityState.Modified;
        var result = await context.SaveChangesAsync();

        return result > 0
            ? Results.CreatedAtRoute("GetProductById", new { id = product.Id }, product)
            : Results.BadRequest("There was a problem editing the registry!");

    }).ProducesValidationProblem()
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .RequireAuthorization("Administrator")
      .WithName("PutProduct")
      .WithTags("Product");

    #endregion
}

#endregion