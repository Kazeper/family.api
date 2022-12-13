using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace family.api.Models;

public class PageItem
{
    [Key]
    public int Id { get; set; }

    public User User { get; set; }

    [ForeignKey("FKUser")]
    public int UserId { get; set; }

    public bool IsVisible { get; set; }

    [Required]
    [MaxLength(50)]
    public string ElementName { get; set; }

    //consider using obj?
    public string ElementContent { get; set; }
}