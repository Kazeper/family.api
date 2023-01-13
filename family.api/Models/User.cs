using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace family.api.Models;

public class User
{
    public int Id { get; set; }

    [MaxLength(50)]
    public string Email { get; set; }

    public bool EmailConfirmed { get; set; }

    [MaxLength(500)]
    public string PasswordHash { get; set; }

    [MaxLength(50)]
    public string FirstName { get; set; }

    [MaxLength(50)]
    public string LastName { get; set; }

    public int AccessFailedCount { get; set; }

    public ICollection<PageItem> PageItems { get; set; }
}