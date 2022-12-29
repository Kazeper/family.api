using System.ComponentModel.DataAnnotations.Schema;

namespace family.api.Models;

public class User
{
    public int Id { get; set; }

    public string Email { get; set; }

    public bool EmailConfirmed { get; set; }

    public string PasswordHash { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public int AccessFailedCount { get; set; }

    public ICollection<PageItem> PageItems { get; set; }
}