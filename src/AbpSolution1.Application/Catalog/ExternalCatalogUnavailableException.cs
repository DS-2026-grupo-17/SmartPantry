using System;

namespace AbpSolution1.Catalog;

public class ExternalCatalogUnavailableException : Exception
{
    public ExternalCatalogUnavailableException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
