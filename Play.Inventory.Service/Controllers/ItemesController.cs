using Microsoft.AspNetCore.Mvc;
using Play.Inventory.Service.Entities;
using Play.Shared;

namespace Play.Inventory.Service.Controllers;
[Route("items")]
[ApiController]
public class ItemesController : ControllerBase
{
    #region Fields :
    private readonly IRepository<InventoryItem> _itemsRepository;
    #endregion

    #region CTORS :
    public ItemesController(IRepository<InventoryItem> itemsRepository)
    {
        _itemsRepository = itemsRepository;
    }
    #endregion

    #region Endpoints :
    //GET /items/{userId}
    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
    {
        if (userId.Equals(Guid.Empty)) return BadRequest();
        var items = (await _itemsRepository.GetAllAsync(x => x.UserId.Equals(userId))).Select(x => x.AsDto());
        return Ok(items);
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
