namespace CoreBusiness.Models;

public class MenuItem
{
    public int MenuItemId { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public string Unit { get; set; } = "Phần";
    
    // Auxiliary property
    public string CategoryName { get; set; } = string.Empty;
}
