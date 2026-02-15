using InventarioWEB.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

[Table("tallas")]
public class Talla
{
    [Key]
    public int ID_Tallas { get; set; }

    [Required]
    [StringLength(100)]
    public string DescripTalla { get; set; } = string.Empty;

    [Required]
    public int ID_Genero { get; set; }

    [ForeignKey(nameof(ID_Genero))]
    public Genero Genero { get; set; } = null!;

    public bool Activo { get; set; } = true;
}
