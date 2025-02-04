﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Shared.Models.Products;
public class Category
{

    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required(ErrorMessage = "Name is required")]
    public string? CategoryName { get; set; }
    public string? Description { get; set; } = "";
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedDate { get; set; }
    public virtual List<Product> Products { get; set; } = new();
}
