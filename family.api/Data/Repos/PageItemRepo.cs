using family.api.Data.Interfaces;
using family.api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace family.api.Data.Repos;

public class PageItemRepo : IPageItemRepo
{
    private readonly AppDataContext _dataContext;

    public PageItemRepo(AppDataContext appDataContext)
    {
        _dataContext = appDataContext;
    }

    public async Task AddPageItem(PageItem item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        await _dataContext.AddAsync(item);
    }

    public async Task AddPageItems(IEnumerable<PageItem> items)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        foreach (var item in items)
        {
            _dataContext.AddAsync(item);
        }

        await SaveChanges();
    }

    public async Task UpdatePageItems(IEnumerable<PageItem> items)
    {
        foreach (var item in items)
        {
            var itemToUpdate = await _dataContext.PageItems.FirstOrDefaultAsync(x => x.Id == item.Id);

            itemToUpdate.IsVisible = item.IsVisible;
            itemToUpdate.ElementName = item.ElementName;
            itemToUpdate.ElementContent = item.ElementContent;
        }
    }

    public void DeletePageItem(PageItem item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        _dataContext.Remove(item);
    }

    public async Task<PageItem> GetPageItemById(int id)
    {
        return await _dataContext.PageItems.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<PageItem>> GetPageItemsByUserId(int userId)
    {
        return await _dataContext.PageItems.AsQueryable().Where(x => x.UserId == userId).ToListAsync();
    }

    public async Task SaveChanges()
    {
        await _dataContext.SaveChangesAsync();
    }
}