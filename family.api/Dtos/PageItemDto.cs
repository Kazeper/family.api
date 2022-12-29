using System.ComponentModel.DataAnnotations;

namespace family.api.Dtos;

public class PageItemDto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required]
    public bool IsVisible { get; set; }

    [Required]
    public string ElementName { get; set; }

    //consider using obj?
    public string ElementContent { get; set; }
}