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
    private static DateTime Now = DateTime.Now;
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
                //AddExpenseTypes(db);                
            }
        }        

        // var payments = db.Payments.Where(x => x.UserId == Guid.Parse("cccaa529-1300-40dc-9697-b4ea989b4e8d") && x.PaymentDate!.Value.Date == DateTime.Now.Date);
        // decimal collected = 0M;
        // foreach (var payment in payments)
        //     collected += payment!.Amount;
        
        // Console.WriteLine(collected.ToString("N2"));
            
        //var stocks = db.Products.AsNoTracking().AsParallel().SelectMany(x => x.Stocks).AsEnumerable().Where(x => Convert.IsDBNull(x.ExpiryDate));
        //db.Products.AsNoTracking().AsParallel().SelectMany(x => x.Stocks).AsEnumerable().Where(x => x.ExpiryDate!.Value!.Date.Subtract(DateTime.Now.Date).Days <= 90).Count();

        //var products = db.Products.AsNoTracking().AsParallel().ToList();
        //foreach (var product in products)
        //{           

        //   if (product.Stocks.Count > 1)
        //   {
        //       Console.WriteLine("id:{0}", product.Id);
        //       var count = product.Stocks.Count - 1;
        //       product.Stocks.RemoveRange(1, count);
        //       db.Entry(product).State = EntityState.Modified;
        //       db.SaveChanges();
        //   }
        //   else
        //        continue;
        //}

        // var date = new DateOnly(2024, 06, 8);
        // var time = new TimeOnly(10, 50, 00);
        // var lastModified = new DateTime(date, time);
        // var Now = DateTime.Now;
        //db.LabTests.ExecuteUpdate(s => s.SetProperty(p => p.Parameters, new List<TestParameter>())
        //           .SetProperty(p => p.Tests, new List<TestEntry>()));
    }

    private static void AddExpenseTypes(AppDbContext db)
    {
        db.ExpenseTypes.Add(new()
        {
            Id = Guid.NewGuid(),
            Expense = "Referral Payment",
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
                CustomerName = "Walk-in customer",
                PhoneNo = "09023920202",
                ContactAddress = "123, online",
                Regular = true,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
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

        var _ExpiryDate = DateTime.Now.AddDays(90);
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
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
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
            //     CreatedDate = DateTime.Now,
            //     ModifiedDate = DateTime.Now,
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
            //     CreatedDate = DateTime.Now,
            //     ModifiedDate = DateTime.Now,
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
            //     CreatedDate = DateTime.Now,
            //     ModifiedDate = DateTime.Now,
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
            //     CreatedDate = DateTime.Now,
            //     ModifiedDate = DateTime.Now,
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
            //     CreatedDate = DateTime.Now,
            //     ModifiedDate = DateTime.Now,
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
            //     CreatedDate = DateTime.Now,
            //     ModifiedDate = DateTime.Now,
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
