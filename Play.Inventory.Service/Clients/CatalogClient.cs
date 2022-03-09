namespace Play.Inventory.Service.Clients;

public class CatalogClient
{
    #region Fields :
    private readonly HttpClient _httpClient;
    #endregion

    #region CTORS :
    public CatalogClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    #endregion

    #region Operations :
    public async Task<IReadOnlyCollection<CatalogItemDto>> GetCatalogItemsAsync()
        => await _httpClient.GetFromJsonAsync<IReadOnlyCollection<CatalogItemDto>>("/items");

    #endregion
}
