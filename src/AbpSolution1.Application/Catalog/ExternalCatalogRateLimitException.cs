using System;

namespace AbpSolution1.Catalog;

public class ExternalCatalogRateLimitException : Exception
{
    public ExternalCatalogRateLimitException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
