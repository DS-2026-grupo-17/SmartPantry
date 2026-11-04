using Volo.Abp.Application.Dtos;

namespace AbpSolution1.Pantries;

public class GetPantryItemsInput : PagedAndSortedResultRequestDto
{
    // Los consumidos se conservan como historial; el cliente puede ocultarlos.
    public bool IncludeConsumed { get; set; } = true;
}
