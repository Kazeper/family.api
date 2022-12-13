using family.api.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace family.api.Dtos;

public class PageItemDto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public bool IsVisible { get; set; }

    public string ElementName { get; set; }

    //consider using obj?
    public string ElementContent { get; set; }
}