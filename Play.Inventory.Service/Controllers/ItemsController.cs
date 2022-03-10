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
    private readonly IRepository<InventoryItem> _inventoryItemsRepository;
    private readonly IRepository<CatalogItem> _catalogItemsRepository;
    #endregion

    #region CTORS :
    public ItemsController(IRepository<InventoryItem> itemsRepository, IRepository<CatalogItem> catalogItemsRepositoryt)
    {
        _inventoryItemsRepository = itemsRepository;
        _catalogItemsRepository = catalogItemsRepositoryt;
    }
    #endregion

    #region Endpoints :
    //GET /items/{userId}
    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
    {
        if (userId.Equals(Guid.Empty)) return BadRequest();
        var inventoryItemsEntities = (await _inventoryItemsRepository.GetAllAsync(x => x.UserId.Equals(userId)));
        var itemIds=inventoryItemsEntities.Select(x=>x.CatalogItemId);
        var catalogItemsEntities = await _catalogItemsRepository.GetAllAsync(x=>itemIds.Contains(x.Id));
        var inventoryItemsDtos = inventoryItemsEntities.Select(inventoryItem =>
        {
            var catalogItem = catalogItemsEntities.Single(catalogItem => catalogItem.Id.Equals(inventoryItem.CatalogItemId));
            return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
        });
        return Ok(inventoryItemsDtos);
    }
    //POST /items
    [HttpPost]
    public async Task<ActionResult> PostAsync(GrantItemDto itemDto)
    {
        var inventoryItem = await _inventoryItemsRepository.GetAsync(x => x.UserId.Equals(itemDto.UserId) && x.CatalogItemId.Equals(itemDto.CatalogItemId));
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
            await _inventoryItemsRepository.CreateAsync(inventoryItem);
        }
        else
        {
            inventoryItem.Quantity += itemDto.Quantity;
            await _inventoryItemsRepository.UpdateAsync(inventoryItem);
        }
        return Ok();
    }
    #endregion
}
