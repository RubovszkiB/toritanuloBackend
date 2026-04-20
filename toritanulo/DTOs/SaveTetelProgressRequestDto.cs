using System.ComponentModel.DataAnnotations;

namespace toritanulo.DTOs;

public class SaveTetelProgressRequestDto
{
    [Range(0, 100)]
    public int HaladasSzazalek { get; set; }

    [Range(1, int.MaxValue)]
    public int LastPage { get; set; } = 1;

    [Range(0, 1)]
    public decimal ScrollProgress { get; set; }

    [Range(0, int.MaxValue)]
    public int PageCount { get; set; }

    public bool Completed { get; set; }
}
