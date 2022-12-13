using AutoMapper;
using family.api.Data;
using family.api.Data.Interfaces;
using family.api.Data.Repos;
using family.api.Dtos;
using family.api.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var sqlConnBuilder = new SqlConnectionStringBuilder();
sqlConnBuilder.ConnectionString = builder.Configuration.GetConnectionString("SQLDbConnection");

builder.Services.AddDbContext<AppDataContext>(opt => opt.UseSqlServer(sqlConnBuilder.ConnectionString));
builder.Services.AddScoped<IPageItemRepo, PageItemRepo>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());//not sure whether use automapper or not

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("api/pageitems", async (IPageItemRepo pageItemRepo, IMapper mapper) =>
{
    var items = await pageItemRepo.GetPageItemsByUserId(1);//TODO get userId by token.
    return Results.Ok(mapper.Map<IEnumerable<PageItemDto>>(items));
});

app.MapGet("api/pageitems/{id}", async (IPageItemRepo pageItemRepo, IMapper mapper, [FromRoute] int id) =>
{
    var item = await pageItemRepo.GetPageItemById(id);//TODO get userId by token.
    return item is null ? Results.NotFound() : Results.Ok(mapper.Map<IEnumerable<PageItemDto>>(item));
});

app.MapPost("api/pageitems", async (IPageItemRepo pageItemRepo, IMapper mapper, [FromBody] PageItemDto item) =>
{
    var itemModel = mapper.Map<PageItem>(item);

    //TODO get userId by token and set PageItem.UserId

    await pageItemRepo.AddPageItem(itemModel);
    await pageItemRepo.SaveChanges();

    return Results.Created($"api/pageitems/{itemModel.Id}", mapper.Map<PageItemDto>(itemModel));
});

app.MapPut("api/pageitems", async (IPageItemRepo pageItemRepo, IMapper mapper, [FromBody] IEnumerable<PageItemDto> items) =>
{
    var models = mapper.Map<IEnumerable<PageItem>>(items);
    //TODO check token??

    await pageItemRepo.UpdatePageItems(models.Where(x => x.Id != 0).ToList());//not sure
    await pageItemRepo.AddPageItems(models.Where(x => x.Id == 0).ToList());//not sure
    await pageItemRepo.SaveChanges();

    return Results.Ok();
});

app.Run();