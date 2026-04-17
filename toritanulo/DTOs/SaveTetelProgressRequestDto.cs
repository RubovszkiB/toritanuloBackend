using System.ComponentModel.DataAnnotations;

namespace toritanulo.DTOs;

public class SaveTetelProgressRequestDto
{
    [Range(0, 100)]
    public int HaladasSzazalek { get; set; }
}
