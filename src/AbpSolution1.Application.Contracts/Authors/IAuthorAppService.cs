using System;
using System.Threading.Tasks;
using AbpSolution1.Shared;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace AbpSolution1.Authors;

public interface IAuthorAppService :
    ICrudAppService<
        AuthorDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateAuthorDto>
{
    Task<IRemoteStreamContent> GetListAsExcelFileAsync(AuthorExcelDownloadDto input);

    Task<DownloadTokenResultDto> GetDownloadTokenAsync();
}
