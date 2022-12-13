using AutoMapper;
using family.api.Dtos;
using family.api.Models;

namespace family.api.Profiles;

public class PageItemsProfile : Profile
{
    public PageItemsProfile()
    {
        //TODO not sure whether use automapper
        CreateMap<PageItem, PageItemDto>();
        CreateMap<PageItemDto, PageItem>();
    }
}