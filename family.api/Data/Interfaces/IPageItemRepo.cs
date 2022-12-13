using family.api.Models;

namespace family.api.Data.Interfaces;

public interface IPageItemRepo
{
    Task SaveChanges();

    Task<IEnumerable<PageItem>> GetPageItemsByUserId(int userId);

    Task<PageItem> GetPageItemById(int id);

    Task AddPageItem(PageItem item);

    Task AddPageItems(IEnumerable<PageItem> items);

    Task UpdatePageItems(IEnumerable<PageItem> items);

    void DeletePageItem(PageItem item);
}