using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bogus;
using Bogus.DataSets;
using Microsoft.EntityFrameworkCore;
using Server.Context;
using Shared.Enums;
using Shared.Helpers;
using Shared.Models.Company;
using Shared.Models.Customers;

using Shared.Models.Products;
using Shared.Models.Users;

namespace Server.Data;

public class SeedData
{
    private static DateTime Now = DateTime.UtcNow;
    public static void EnsureSeeded(IServiceProvider services, bool IsDev)
    {
        var factory = services.GetRequiredService<IServiceScopeFactory>();
        using var scope = factory.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (IsDev)
        {
            //db.Database.EnsureDeleted();
            if (db.Database.EnsureCreated())    
            {
                AddUsers(db);
                AddCustomers(db);
                AddExpenseTypes(db);
                ImportCustomers(services);
            }
            //ImportData(services);
            //RemoveQuantities(services);
            //AddExpenseTypes(db);
            //ImportCustomers(services);
        }
    }


    public async static void ImportData(IServiceProvider service)
    {
        var file = await File.ReadAllBytesAsync("/home/nerdyamin/Downloads/tables.json");
        if (file is null)
            Console.WriteLine("File not found!");
        
        using var stream = File.OpenRead("/home/nerdyamin/Downloads/tables.json");
        stream.Position = 0;        
        
        var options = new JsonSerializerOptions()
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
        var date = DateTime.UtcNow;
        var data = await JsonSerializer.DeserializeAsync<ProductData[]?>(stream, options);
        var categories = data!.AsParallel().GroupBy(x => x.Category!.Trim()).Select(c => new Category
        {
            Id = Guid.NewGuid(),
            CategoryName = c.Key,
            CreatedDate = date
        });        
        var brand = new Brand
        {
            Id = Guid.NewGuid(),
            BrandName = "Shop 2",
            CreatedDate = date
        };
        var store = new Store
        {
            Id = Guid.NewGuid(),
            BranchName = "Ameesh Luxury II",
            CreatedDate = date,
            PhoneNo1 = "08033000000"
        };
        var factory = service.GetRequiredService<IServiceScopeFactory>();
        using var scope = factory.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var fabricID = db.Categories.Where(x => EF.Functions.ILike(x.CategoryName!, $"%fabric%")).FirstOrDefault()?.Id;
        await db.Stores.AddAsync(store);
        await db.Brands.AddAsync(brand);

        var otherCategories = categories.Where(x => !x.CategoryName!.Contains("fabric", StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var category in otherCategories)
            Console.WriteLine("Other categories {0} {1}", category.Id, category.CategoryName);

        Console.WriteLine("Categories {0}", fabricID);                
        
        await db.Categories.AddRangeAsync(otherCategories);

        foreach(var category in categories)
        {
            foreach (var item in data!.Where(x => x.Category!.Trim() == category.CategoryName))
            {                

                var StoreItem = new Item
                {
                    Id = Guid.NewGuid(),
                    ProductName = item.Name?.Trim(),
                    BrandID   = brand.Id,
                    CategoryID = category.CategoryName!.Contains("fabric", StringComparison.OrdinalIgnoreCase) ? 
                        fabricID!.Value : category.Id,
                    CreatedDate = date,
                    Category = null,
                    Brand = null            
                };                
                var SellPrice = decimal.Parse(item.Sell);
                var CostPrice = decimal.Parse(item.Cost);
                var MarkupAmount = SellPrice - CostPrice;
                var Product = new Product
                {
                    Id = Guid.NewGuid(),
                    ItemId = StoreItem.Id,
                    StoreId = store.Id,
                    SellPrice = SellPrice,
                    MarkupAmount = MarkupAmount,
                    Item = null,
                    Store = null,                    
                    Stocks =
                    [
                        new Stock 
                        {
                            id = Guid.NewGuid(),
                            Date = date,
                            Quantity = (decimal)item.Quantity,
                            BuyPrice = CostPrice,                            
                        }
                    ]                    
                };
                Console.WriteLine($"Item {StoreItem.Id} {StoreItem.ProductName} {StoreItem.BrandID} {StoreItem.CategoryID}");
                Console.WriteLine($"Product {Product.Id} {Product.SellPrice} {Product.StoreId}");

                await db.Items.AddAsync(StoreItem);
                await db.Products.AddAsync(Product);

            }
        }        

        await db.SaveChangesAsync();
    }

    private static void RemoveQuantities(IServiceProvider services)
    {
        var factory = services.GetRequiredService<IServiceScopeFactory>();
        using var scope = factory.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var products = db.Products.Where(x => x.Stocks.Count > 1).ToList();
        var count = products.Count;
        Console.WriteLine("{0} products with multiple stocks", count);
        int index = 1;
        foreach (var product in products)
        {
            Console.WriteLine("{0} of {1} products with multiple stocks", index, count);
            product.Stocks.RemoveAt(0);
            db.Products.Update(product);
            db.SaveChanges();
            index++;
        }
    }

    private static async void ImportCustomers(IServiceProvider services)
    {

        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Server.Data.data.json");
        var fs = new StreamReader(stream!);
        var contents = await fs.ReadToEndAsync();
        var customers = JsonSerializer.Deserialize<CustomerJson[]?>(contents);
        if (customers!.Any())
        {
            var factory = services.GetRequiredService<IServiceScopeFactory>();
            using var scope = factory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            int count = customers!.Length;
            int index = 1;
            Console.WriteLine("found {0} customers", count);
            Thread.Sleep(100);
            foreach (var item in customers)
            {
                await db.Customers.AddAsync(
                    new Customer 
                    { Id = Guid.NewGuid(), CustomerName = item.name, PhoneNo = item!.phone, ContactAddress = item.address2}
                );
                Console.WriteLine("Imported {0} of {1} row", index, count);
                //Thread.Sleep(100);
                index++;
            }
            await db.SaveChangesAsync();
        }
        else
        {
            Console.WriteLine("No customers found yet");

        }
    }

    private static void AddExpenseTypes(AppDbContext db)
    {
        db.ExpenseTypes.Add(new()
        {
            Id = Guid.Parse("30bef2c8-fb10-447b-8ac2-9a651d91088b"),
            Expense = "Order Expense",
            CreatedDate = Now,
            ModifiedDate = Now
        });
        db.SaveChanges();
    }

    private static void AddCustomers(AppDbContext db)
    {
        try
        {
            // var bogus = new Faker<Customer>()
            //                     .RuleFor(g => g.Gender, f => f.PickRandom<Gender>())
            //                     .RuleFor(p => p.CustomerName, (f, c) => f.Name.FullName((Name.Gender?)c.Gender))                                
            //                     .RuleFor(p => p.PhoneNo, f => f.Person.Phone.Substring(0, 11))
            //                     .RuleFor(p => p.ContactAddress, f => f.Person.Address.ToString())
            //                     .RuleFor(p => p.CreatedDate, f => f.Date.Recent());
            // var customers = bogus.Generate(50);
            // db.Customers.AddRange(customers);
            db.Customers.Add(new Customer
            {
                Id = Guid.NewGuid(),
                CustomerName = "Regular Customer",
                PhoneNo = "09023920202",
                ContactAddress = "123, online",
                Regular = true,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            });
            db.SaveChanges();
        }
        catch (Exception)
        {

            throw;
        }
    }    

    private static void AddItems(AppDbContext db)
    {
        Guid antibiotics = Guid.NewGuid();
        var categories = new List<Category>()
        {
            new Category
            {
                Id = antibiotics,
                CategoryName = "Antibiotics",
                CreatedDate = Now,
                ModifiedDate = Now
            }
        };
        var items = new List<Item>()
        {
            new Item
            {
                Id = Guid.NewGuid(),
                ProductName = "Tetracycline",
                CategoryID = antibiotics,
                CreatedDate = Now,
                ModifiedDate = Now,                
            },
            new Item
            {
                Id = Guid.NewGuid(),
                ProductName = "Amoxicilin",                
                CategoryID = antibiotics,                
                CreatedDate = Now,
                ModifiedDate = Now,                
            }
        };

        var _ExpiryDate = DateTime.UtcNow.AddDays(90);
        var products = new List<Product>()
        {
            new Product
            {
                Id = Guid.NewGuid(),
                ItemId = items!.FirstOrDefault(x => x!.ProductName == "Amoxicilin")!.Id,
                StoreId = StoreID,
                SellPrice = 0,
                ReorderLevel = 100,                                
                CreatedDate = Now,
                ModifiedDate = Now,                
            },
            new Product
            {
                Id = Guid.NewGuid(),
                ItemId = items!.FirstOrDefault(x => x!.ProductName == "Tetracycline")!.Id,
                StoreId = StoreID,
                SellPrice = 0,
                ReorderLevel = 100,                               
                CreatedDate = Now,
                ModifiedDate = Now,
            }
        };
        db.Categories.AddRange(categories);
        db.Items.AddRange(items);
        db.Products.AddRange(products);
        db.SaveChanges();

    }

    
    static Guid StoreID = Guid.NewGuid();
    private static void AddUsers(AppDbContext context)
    {
        var stores = new Store
        {
            Id = StoreID,
            BranchName = "Ameesh Luxury",
            BranchAddress = "address",
            PhoneNo1 = "08000000000",
            CreatedDate = Now,
            ModifiedDate = Now
        };
        var id = Guid.NewGuid();
        var users = new User[]
        {
            new User
            {
                Id = id,
                FirstName = "System",
                LastName = "Administrator",
                Role = UserRole.Master,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                UserCredential = new UserCredential
                {
                    Id = Guid.NewGuid(),
                    UserId = id,
                    Username = "admin",
                    HashedPassword = Security.Encrypt("jacubox123*"),                
                    IsActive = true,
                    IsNew = false                
                }
            },
            // new User
            // {
            //     Id = Guid.NewGuid(),
            //     StoreId = StoreID,
            //     FirstName = "Manager",
            //     LastName = "Office",
            //     Username = UserRole.Manager.ToString(),
            //     HashedPassword = Security.Encrypt("12345678"),
            //     Role = UserRole.Manager,
            //     IsActive = true,
            //     IsNew = false,
            //     CreatedDate = DateTime.UtcNow,
            //     ModifiedDate = DateTime.UtcNow,
            // },
            // new User
            // {
            //     Id = Guid.NewGuid(),
            //     StoreId = StoreID,
            //     FirstName = "Pharmacy Sell",
            //     LastName = "Point",
            //     Username = UserRole.Pharmacy.ToString(),
            //     HashedPassword = Security.Encrypt("12345678"),
            //     Role = UserRole.Pharmacy,
            //     IsActive = true,
            //     IsNew = false,
            //     CreatedDate = DateTime.UtcNow,
            //     ModifiedDate = DateTime.UtcNow,
            // },
            // new User
            // {
            //     Id = Guid.NewGuid(),
            //     StoreId = StoreID,
            //     FirstName = "Lab Sell",
            //     LastName = "Point",
            //     Username = UserRole.Lab.ToString(),
            //     HashedPassword = Security.Encrypt("12345678"),
            //     Role = UserRole.Lab,
            //     IsActive = true,
            //     IsNew = false,
            //     CreatedDate = DateTime.UtcNow,
            //     ModifiedDate = DateTime.UtcNow,
            // },
            // new User
            // {
            //     Id = Guid.NewGuid(),
            //     StoreId = StoreID,
            //     FirstName = "Store",
            //     LastName = "Cashier",
            //     Username = "cashier",
            //     HashedPassword = Security.Encrypt("12345678"),
            //     Role = UserRole.Cashier,
            //     IsActive = true,
            //     IsNew = false,
            //     CreatedDate = DateTime.UtcNow,
            //     ModifiedDate = DateTime.UtcNow,
            // },
            // new User
            // {
            //     Id = Guid.NewGuid(),
            //     StoreId = StoreID,
            //     FirstName = "Store",
            //     LastName = "Manager",
            //     Username = "store",
            //     HashedPassword = Security.Encrypt("12345678"),
            //     Role = UserRole.Store,
            //     IsActive = true,
            //     IsNew = false,
            //     CreatedDate = DateTime.UtcNow,
            //     ModifiedDate = DateTime.UtcNow,
            // },
            // new User
            // {
            //     Id = Guid.NewGuid(),
            //     StoreId = StoreID,
            //     FirstName = "Dispenser",
            //     LastName = "Manager",
            //     Username = "dispenser",
            //     HashedPassword = Security.Encrypt("12345678"),
            //     Role = UserRole.Dispenser,
            //     IsActive = true,
            //     IsNew = false,
            //     CreatedDate = DateTime.UtcNow,
            //     ModifiedDate = DateTime.UtcNow,
            // }
        };
        // var referers = new List<Referer>()
        // {
        //     new()
        //     {
        //         Id = Guid.NewGuid(),
        //         RefererName = "Escort",
        //         PhoneNo = "08012345678",
        //         Type = RefererType.Escort,
        //         CreatedDate = Now,
        //         ModifiedDate = Now
        //     },
        //     new()
        //     {
        //         Id = Guid.NewGuid(),
        //         RefererName = "Doctor",
        //         PhoneNo = "08012345678",
        //         Type = RefererType.Doctor,
        //         CreatedDate = Now,
        //         ModifiedDate = Now
        //     },
        // };
        context.Stores.Add(stores);
        context.Users.AddRange(users);
        // context.Referers.AddRange(referers);
        context.SaveChanges();
    }

    public class MedicalTest
    {
        [JsonPropertyName("TestName")]
        public string TestName { get; set; }

        [JsonPropertyName("Rate")]
        [JsonConverter(typeof(NullableIntConverter))]
        public int? Rate { get; set; }  // Use nullable int for cases where the rate might be empty
    }
}
