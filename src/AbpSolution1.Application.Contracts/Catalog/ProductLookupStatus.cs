using System.Text.Json.Serialization;

namespace AbpSolution1.Catalog;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProductLookupStatus
{
    Found,
    NotFound,
    RateLimited,
    Unavailable
}
