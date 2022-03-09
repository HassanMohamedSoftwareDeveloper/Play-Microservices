using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Entities;
using Play.Shared;

namespace Play.Catalog.Service.Controllers;
[Route("items")]
[ApiController]
public class ItemController : ControllerBase
{
    #region Fileds :
    private readonly IRepository<Item> _itemsRepository;
    private static int _requestCounter = 0;
    #endregion

    #region CTORS :
    public ItemController(IRepository<Item> itemsRepository)
    {
        this._itemsRepository = itemsRepository;
    }
    #endregion

    #region Endpoints :
    //GET /items 
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync()
    {
        _requestCounter++;
        Console.WriteLine($"Request {_requestCounter}: Starting..");
        if (_requestCounter <= 2)
        {
            Console.WriteLine($"Request {_requestCounter}: Delaying..");
            await Task.Delay(TimeSpan.FromSeconds(10));
        }
        if (_requestCounter <= 4)
        {
            Console.WriteLine($"Request {_requestCounter}: 500 (Internal Server Error)..");
            return StatusCode(500);
        }
        Console.WriteLine($"Request {_requestCounter}: Ok..");
        return Ok((await _itemsRepository.GetAllAsync()).Select(x => x.AsDto()));
    }
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
