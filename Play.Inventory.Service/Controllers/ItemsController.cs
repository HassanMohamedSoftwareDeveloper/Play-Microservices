using Microsoft.AspNetCore.Mvc;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;
using Play.Shared;

namespace Play.Inventory.Service.Controllers;
[Route("items")]
[ApiController]
public class ItemsController : ControllerBase
{
    #region Fields :
    private readonly IRepository<InventoryItem> _itemsRepository;
    private readonly CatalogClient _catalogClient;
    #endregion

    #region CTORS :
    public ItemsController(IRepository<InventoryItem> itemsRepository, CatalogClient catalogClient)
    {
        _itemsRepository = itemsRepository;
        _catalogClient = catalogClient;
    }
    #endregion

    #region Endpoints :
    //GET /items/{userId}
    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
    {
        if (userId.Equals(Guid.Empty)) return BadRequest();
        var catalogItems = await _catalogClient.GetCatalogItemsAsync();
        var inventoryItemsEntities = (await _itemsRepository.GetAllAsync(x => x.UserId.Equals(userId)));
        var inventoryItemsDtos = inventoryItemsEntities.Select(inventoryItem =>
        {
            var catalogItem = catalogItems.Single(catalogItem => catalogItem.Id.Equals(inventoryItem.CatalogItemId));
            return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
        });
        return Ok(inventoryItemsDtos);
    }
    //POST /items
    [HttpPost]
    public async Task<ActionResult> PostAsync(GrantItemDto itemDto)
    {
        var inventoryItem = await _itemsRepository.GetAsync(x => x.UserId.Equals(itemDto.UserId) && x.CatalogItemId.Equals(itemDto.CatalogItemId));
        if (inventoryItem is null)
        {
            inventoryItem = new()
            {
                Id = Guid.NewGuid(),
                CatalogItemId = itemDto.CatalogItemId,
                UserId = itemDto.UserId,
                Quantity = itemDto.Quantity,
                AcquiredDate = DateTimeOffset.UtcNow
            };
            await _itemsRepository.CreateAsync(inventoryItem);
        }
        else
        {
            inventoryItem.Quantity += itemDto.Quantity;
            await _itemsRepository.UpdateAsync(inventoryItem);
        }
        return Ok();
    }
    #endregion
}
