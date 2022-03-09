using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Entities;
using Play.Catalog.Service.Repositories;

namespace Play.Catalog.Service.Controllers;
[Route("items")]
[ApiController]
public class ItemController : ControllerBase
{
    #region Fileds :
    private readonly IItemsRepository _itemsRepository;
    #endregion

    #region CTORS :
    public ItemController(IItemsRepository itemsRepository)
    {
        this._itemsRepository = itemsRepository;
    } 
    #endregion

    #region Endpoints :
    //GET /items 
    [HttpGet]
    public async Task<IEnumerable<ItemDto>> GetAsync() => (await _itemsRepository.GetAllAsync()).Select(x => x.AsDto());
    //GET /items/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
    {
        var item = await _itemsRepository.GetAsync(id);
        if (item is null) return NotFound();
        return item.AsDto();
    }
    //POST /items
    [HttpPost]
    public async Task<ActionResult<ItemDto>> PostAsync(CreateItemDto itemDto)
    {
        Item item = new()
        {
            Id = Guid.NewGuid(),
            Name = itemDto.Name,
            Description = itemDto.Description,
            Price = itemDto.Price,
            CreatedDate = DateTimeOffset.UtcNow
        };
        await _itemsRepository.CreateAsync(item);
        return CreatedAtAction(nameof(GetByIdAsync), new { item.Id }, item.AsDto());
    }
    //PUT /items/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutAsync(Guid id, [FromBody] UpdateItemDto itemDto)
    {
        var item = await _itemsRepository.GetAsync(id);
        if (item is null) return NotFound();
        item.Name = itemDto.Name;
        item.Description = itemDto.Description;
        item.Price = itemDto.Price;
        await _itemsRepository.UpdateAsync(item);
        return NoContent();
    }
    //DELETE /items/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var item = await _itemsRepository.GetAsync(id);
        if (item is null) return NotFound();
        await _itemsRepository.DeleteAsync(id);
        return NoContent();
    }
    #endregion
}
