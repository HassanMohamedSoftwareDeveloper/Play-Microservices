using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service;

public static class Extensions
{
    public static InventoryItemDto AsDto(this InventoryItem @this) 
        => new(@this.CatalogItemId, @this.Quantity, @this.AcquiredDate);
}
