using System;
using System.ComponentModel.DataAnnotations;

namespace AbpSolution1.Pantries;

// RF-10: agregar un producto guardado a la despensa del usuario autenticado.
public class CreatePantryItemDto : PantryItemDataDto
{
    [Required]
    public Guid ProductId { get; set; }
}
