using family.api.Models;

namespace family.api.Data.Interfaces;

public interface IPageItemRepo
{
    Task SaveChanges();

    Task<IEnumerable<PageItem>> GetPageItemsByUserEmail(string userEmail);

    Task<PageItem> GetPageItemById(int id);

    Task AddPageItem(PageItem item);

    Task<PageItem> Update(PageItem item);

    void DeletePageItem(PageItem item);
}