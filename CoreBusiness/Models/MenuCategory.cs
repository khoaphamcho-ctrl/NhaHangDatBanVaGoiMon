namespace CoreBusiness.Models;

public class MenuCategory
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DisplayOrder { get; set; } = 0;
    public string Icon { get; set; } = "bi-cup-hot";
}
