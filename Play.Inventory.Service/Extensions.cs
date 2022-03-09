using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service;

public static class Extensions
{
    public static InventoryItemDto AsDto(this InventoryItem @this, string name, string description)
        => new(@this.CatalogItemId, name, description, @this.Quantity, @this.AcquiredDate);
}
