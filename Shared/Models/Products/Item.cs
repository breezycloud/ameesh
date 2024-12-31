using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Shared.Models.Products;

public class Item
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BrandID { get; set; }
    public Guid CategoryID { get; set; }
    [Required(ErrorMessage = "Product Name is required")]
    public string? ProductName { get; set; }
    public string? Description { get; set; } = "";
    public string? Barcode { get; set; } = "";    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedDate { get; set; }
    [ForeignKey("CategoryID")]
    public virtual Category? Category { get; set; } = new Category();
    [ForeignKey("BrandID")]
    public virtual Brand? Brand { get; set; } = new Brand();

    public override string ToString()
    {
        return $"{Brand!.BrandName} {Category!.CategoryName} {ProductName}";
    }

}
