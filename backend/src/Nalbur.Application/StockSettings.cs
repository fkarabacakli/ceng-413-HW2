namespace Nalbur.Application;

/// <summary>
/// Settings the use cases need. The API fills these from environment variables;
/// the Application layer does not know where they came from.
/// </summary>
/// <param name="DefaultCriticalLevel">Critical level used when a new product does not give one (CRITICAL_STOCK_DEFAULT).</param>
public sealed record StockSettings(decimal DefaultCriticalLevel);
