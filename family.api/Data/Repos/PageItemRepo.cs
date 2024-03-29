﻿using family.api.Data.Interfaces;
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

    public async Task<PageItem> Update(PageItem item)
    {
        var itemToUpdate = await _dataContext.PageItems.FirstOrDefaultAsync(x => x.Id == item.Id);

        if (itemToUpdate != null)
        {
            itemToUpdate.UserId = item.UserId;
            itemToUpdate.IsVisible = item.IsVisible;
            itemToUpdate.ElementName = item.ElementName;
            itemToUpdate.ElementContent = item.ElementContent;
        }

        return itemToUpdate;
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

    public async Task<IEnumerable<PageItem>> GetPageItemsByUserEmail(string userEmail)
    {
        //TODO check 2nd approach
        var user = _dataContext.Users.FirstOrDefault(x => x.Email == userEmail);
        if (user == null) return new List<PageItem>();

        return await _dataContext.PageItems.Where(x => x.UserId == user.Id).Include(a => a.User).ToListAsync();

        //return await _dataContext.PageItems.AsQueryable().Where(x => x.User.Email == userEmail).ToListAsync();
    }

    public async Task SaveChanges()
    {
        await _dataContext.SaveChangesAsync();
    }
}