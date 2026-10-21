using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace AbpSolution1.Products;

/*
 * RF-08, persistencia interna. Requiere un usuario autenticado; no exige permisos administrativos.
 * Si el código de barras ya está guardado, devuelve el producto existente en lugar de crear
 * un duplicado. El índice único sobre Barcode refuerza la regla en la base.
 */
[Authorize]
public class ProductAppService : ApplicationService, IProductAppService
{
    private readonly IRepository<Product, Guid> _productRepository;

    public ProductAppService(IRepository<Product, Guid> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ProductDto> GetAsync(Guid id)
    {
        var product = await _productRepository.GetAsync(id);
        return ObjectMapper.Map<Product, ProductDto>(product);
    }

    public async Task<PagedResultDto<ProductDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await _productRepository.GetQueryableAsync();
        var query = queryable
            .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? nameof(Product.Name) : input.Sorting)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var products = await AsyncExecuter.ToListAsync(query);
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        return new PagedResultDto<ProductDto>(
            totalCount,
            ObjectMapper.Map<List<Product>, List<ProductDto>>(products));
    }

    public async Task<ProductDto> SaveAsync(SaveProductDto input)
    {
        var barcode = input.Barcode.Trim();

        var product = await _productRepository.FindAsync(p => p.Barcode == barcode);
        if (product == null)
        {
            product = new Product(GuidGenerator.Create(), barcode, input.Name);
            await _productRepository.InsertAsync(product, autoSave: true);
        }

        return ObjectMapper.Map<Product, ProductDto>(product);
    }
}
