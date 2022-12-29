using AutoMapper;
using family.api;
using family.api.Data;
using family.api.Data.Interfaces;
using family.api.Data.Repos;
using family.api.Dtos;
using family.api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Bearer Authentication with JWT Token",
        Type = SecuritySchemeType.Http,
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme,
                }
            },
            new List<string>()
        }
    });
});

var sqlConnBuilder = new SqlConnectionStringBuilder();
sqlConnBuilder.ConnectionString = builder.Configuration.GetConnectionString("SQLDbConnection");

builder.Services.AddDbContext<AppDataContext>(opt => opt.UseSqlServer(sqlConnBuilder.ConnectionString));
builder.Services.AddScoped<IPageItemRepo, PageItemRepo>();//TODO consider using generic repo
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());//not sure whether use automapper or not

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey
        (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateActor = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.UseAuthentication();

app.UseHttpsRedirection();

#region User

app.MapPost("api/user/login", async (IUserRepo userRepo, [FromBody] UserLoginDto userLoginDto) =>
{
    var user = await userRepo.Get(userLoginDto.Email);

    if (user == null || !PasswordHasher.Check(user.PasswordHash, userLoginDto.Password).Verified)
        return Results.BadRequest();

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Email),
        new Claim(ClaimTypes.GivenName, user.FirstName),
        new Claim(ClaimTypes.Surname, user.LastName),
    };

    var token = new JwtSecurityToken
    (
        issuer: builder.Configuration["Jwt:Issuer"],
        audience: builder.Configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddDays(60),
        notBefore: DateTime.UtcNow,
        signingCredentials: new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            SecurityAlgorithms.HmacSha256)
    );

    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

    return Results.Ok(tokenString);
});

app.MapPost("api/user/register", async (IUserRepo userRepo, [FromBody] UserRegisterDto userRegisterDto) =>
{
    if (!userRegisterDto.Password.Equals(userRegisterDto.ConfirmPassword))
        return Results.BadRequest("Password and confirmation password do not match");

    await userRepo.Register(new User
    {
        Email = userRegisterDto.Email,
        PasswordHash = PasswordHasher.Hash(userRegisterDto.Password),
        FirstName = userRegisterDto.FirstName,
        LastName = userRegisterDto.LastName,
    });

    await userRepo.SaveChanges();

    return Results.Ok();
});

app.MapPost("api/user/logout", [Authorize] () =>
{
    //TODO implement
    return Results.Ok();
});

app.MapPost("api/user/changepassword",
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
async (IUserRepo userRepo, [FromHeader(Name = "Authorization")] string token, [FromBody] UserChangePasswordDto userChangePasswordDto) =>
{
    if (userChangePasswordDto.NewPassword.Equals(userChangePasswordDto.OldPassword))
        return Results.BadRequest("new password must be different than old one.");

    if (!userChangePasswordDto.NewPassword.Equals(userChangePasswordDto.ConfirmNewPassword))
        return Results.BadRequest("new password and confirmation new password do not match");

    var splittedToken = token.Split(' ');
    var pureToken = splittedToken.Length == 2 ? splittedToken[1] : null;

    if (string.IsNullOrEmpty(pureToken)) return Results.BadRequest();

    var jwt = new JwtSecurityTokenHandler().ReadJwtToken(pureToken);
    var email = jwt.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

    if (string.IsNullOrEmpty(email?.Value)) return Results.BadRequest();

    var user = await userRepo.Get(email.Value);

    if (user == null) return Results.BadRequest();

    user.PasswordHash = PasswordHasher.Hash(userChangePasswordDto.NewPassword);

    var updatedUser = await userRepo.Update(user);

    if (updatedUser == null) return Results.BadRequest();

    await userRepo.SaveChanges();

    return Results.Ok();
});

#endregion User

app.MapGet("api/pageitems",
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
async (IPageItemRepo pageItemRepo, [FromHeader(Name = "Authorization")] string token, IMapper mapper) =>
{
    var splittedToken = token.Split(' ');
    var pureToken = splittedToken.Length == 2 ? splittedToken[1] : null;

    if (string.IsNullOrEmpty(pureToken)) return Results.BadRequest();

    var jwt = new JwtSecurityTokenHandler().ReadJwtToken(pureToken);
    var email = jwt.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

    if (string.IsNullOrEmpty(email?.Value)) return Results.BadRequest();

    var items = await pageItemRepo.GetPageItemsByUserEmail(email.Value);
    return Results.Ok(mapper.Map<IEnumerable<PageItemDto>>(items));
});

app.MapGet("api/pageitems/{id}",
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
async (IPageItemRepo pageItemRepo, IMapper mapper, [FromRoute] int id) =>
{
    var item = await pageItemRepo.GetPageItemById(id);
    return item is null ? Results.NotFound() : Results.Ok(mapper.Map<IEnumerable<PageItemDto>>(item));
});

app.MapPost("api/pageitems",
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
async (IPageItemRepo pageItemRepo, IUserRepo userRepo, IMapper mapper,
[FromHeader(Name = "Authorization")] string token, [FromBody] PageItemDto item) =>
{
    var splittedToken = token.Split(' ');
    var pureToken = splittedToken.Length == 2 ? splittedToken[1] : null;

    if (string.IsNullOrEmpty(pureToken)) return Results.BadRequest();

    var jwt = new JwtSecurityTokenHandler().ReadJwtToken(pureToken);
    var email = jwt.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

    if (string.IsNullOrEmpty(email?.Value)) return Results.BadRequest();

    var user = await userRepo.Get(email.Value);
    if (user == null) return Results.BadRequest();

    var itemModel = mapper.Map<PageItem>(item);

    itemModel.UserId = user.Id;
    //add itemModel.User = user?

    await pageItemRepo.AddPageItem(itemModel);
    await pageItemRepo.SaveChanges();

    return Results.Created($"api/pageitems/{itemModel.Id}", mapper.Map<PageItemDto>(itemModel));
});

app.MapPut("api/pageitems",
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
async (IPageItemRepo pageItemRepo, IUserRepo userRepo, IMapper mapper,
[FromHeader(Name = "Authorization")] string token, [FromBody] IEnumerable<PageItemDto> items) =>
{
    var splittedToken = token.Split(' ');
    var pureToken = splittedToken.Length == 2 ? splittedToken[1] : null;

    if (string.IsNullOrEmpty(pureToken)) return Results.BadRequest();

    var jwt = new JwtSecurityTokenHandler().ReadJwtToken(pureToken);
    var email = jwt.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

    if (string.IsNullOrEmpty(email?.Value)) return Results.BadRequest();

    var user = await userRepo.Get(email.Value);
    if (user == null) return Results.BadRequest();

    var models = mapper.Map<IEnumerable<PageItem>>(items);

    bool elementNotFound = false;

    foreach (var item in models.Where(x => x.Id != 0))
    {
        item.UserId = user.Id;
        //add itemModel.User = user?

        var updated = await pageItemRepo.Update(item);

        if (updated == null)
        {
            elementNotFound = true;
            break;
        }
    }

    if (elementNotFound) return Results.NotFound("Element to update not found");

    models.Where(x => x.Id == 0).ToList()
    .ForEach(y =>
    {
        y.UserId = user.Id;
        pageItemRepo.AddPageItem(y);
    });

    await pageItemRepo.SaveChanges();

    return Results.Ok();
});

app.Run();