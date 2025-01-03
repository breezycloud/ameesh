using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Server.Context;
using Shared.Enums;
using Shared.Helpers;
using Shared.Models.Auth;
using Shared.Models.Products;
using Shared.Models.Users;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private IConfiguration Configuration { get; }
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        Configuration = configuration;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        return await _context.Users.OrderByDescending(x => x.ModifiedDate).ToListAsync();
    }

    [HttpGet("staffonly")]
    public async Task<ActionResult<IEnumerable<StaffDto>?>> GetStaffNotPaid()
    {
        var month = DateTime.UtcNow.Month;
        var year = DateTime.UtcNow.Year;        
        return await _context.Users.AsNoTracking().Where(u => u.Role != UserRole.Master).Select(s => new StaffDto
        {
            Id = s.Id,
            FirstName = s.FirstName,
            LastName = s.LastName,
            Role = s.Role,
            CreatedDate = s.CreatedDate,
            ModifiedDate = s.ModifiedDate
        }).ToArrayAsync();
    }    

    [HttpGet("getoldpassword/{id}")]
    public async Task<ActionResult<NewPasswordModel?>> GetOldPassword(Guid id)
    {
        if (_context.Users == null)
        {
            return Problem("Entity set 'AppDbContext.Users'  is null.");
        }
        var user = await _context.UserCredentials.FirstOrDefaultAsync(x => x.UserId == id);
        if (user == null)
        {
            return NotFound();
        }
        return new NewPasswordModel
        {
            HashedPassword = user!.HashedPassword,
            OldPassword = "nil",
            NewPassword = "nil"
        };
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User?>> GetUser(Guid id)
    {
        if (_context.Users == null)
        {
            return Problem("Entity set 'AppDbContext.Users'  is null.");
        }
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return user;
    }       
    
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<User>>> GetActiveUsers()
    {
        return await _context.Users.Where(x => x.UserCredential!.IsActive).OrderByDescending(x => x.ModifiedDate).ToListAsync();
    }          
    
    [HttpGet("byStore/{StoreID}")]
    public async Task<ActionResult<IEnumerable<User>>> GetUsersByStore(Guid StoreID)
    {
        return await _context.Users.AsNoTracking().AsSplitQuery().Include(x => x.UserCredential).Where(x => x.Role != UserRole.Master).ToListAsync();
    }

    [HttpGet("username/{username}")]
    public async Task<ActionResult<IEnumerable<User>>> GetUserByUsername(string username)
    {
        return await _context.Users.Where(x => EF.Functions.ILike(x.UserCredential!.Username!, $"%{username}%")).ToListAsync();
    }

    // PUT: api/Users/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutUser(Guid id, User user)
    {
        if (id != user.Id)
        {
            return BadRequest();
        }
        var store = _context.Stores.FirstOrDefault();
        user!.StoreId = store!.Id;
        _context.Entry(user).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }
    

    // POST: api/Users
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<User>> PostUser(User user)
    {
        if (_context.Users == null)
        {
            return Problem("Entity set 'AppDbContext.Users'  is null.");
        }
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return CreatedAtAction("GetUser", new { id = user.Id }, user);
    }

    // POST: api/Users/ChangePassword
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost("changepassword/{id}")]
    public async Task<ActionResult<User>> ChangePassword(Guid id, NewPasswordModel model)
    {
        if (_context.Users == null)
        {
            return Problem("Entity set 'AppDbContext.Users'  is null.");
        }
        try
        {
            await _context.UserCredentials.Where(x => x.UserId == id).ExecuteUpdateAsync(s => s.SetProperty(p => p.HashedPassword, model.HashedPassword));                            
        }
        catch (System.Exception ex)
        {
            return BadRequest(ex);
        }
        return Ok();
    }
    

    [HttpPost("Login")]
    public async Task<ActionResult<LoginResponse>?> Login(LoginModel model)
    {
        if (_context.Users is null)
        {
            return NotFound();
        }
        string hashedPassword = Security.Encrypt(model.Password);
        var user = await _context.Users.AsNoTracking()
                                       .AsSplitQuery()
                                       .Include(x => x.Store)
                                       .Include(x => x.UserCredential)
                                       .Where(u => EF.Functions.ILike(u.UserCredential!.Username!, $"%{model.Username}%") && u.UserCredential!.HashedPassword == hashedPassword && u.UserCredential!.IsActive == true)
                                       .SingleOrDefaultAsync();

        if (user is null)
        {
            return BadRequest("User not found");
        }

        var claim = new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user!.UserCredential!.Username!),
            new Claim(ClaimTypes.Role, user!.Role!.ToString()!)
        };

        var key = Configuration["App:Key"]!;
        if (string.IsNullOrEmpty(key))
            return BadRequest("Invalid Key");

        var token = new JwtSecurityToken(
            null,
            null,
            claim,
            expires: user.Role == UserRole.Admin || user.Role == UserRole.Master ? DateTime.UtcNow.AddDays(180) : DateTime.UtcNow.AddDays(1),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
            SecurityAlgorithms.HmacSha512Signature));

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        var result = new LoginResponse
        {
            Id = user.Id,
            Username = user.UserCredential!.Username,
            Token = jwt,            
            StoreId = user.Role == UserRole.Admin ? null : user.StoreId,
            Role = user!.Role.ToString(),
            IsActive = user.UserCredential!.IsActive
        };
        return result;
    }

    [HttpDelete("revokeuseraccount/{id}")]
    public async Task<IActionResult> DeleteUserAccount(Guid id)
    {
        var record = await _context.UserCredentials.FindAsync(id);
        if (record is null)
            return NotFound();
                    
        _context.UserCredentials.Remove(record);        
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var record = await _context.Users.FindAsync(id);
        if (record is null)
            return NotFound();

        if (_context.UserCredentials.Any(x=>x.UserId == id))
            _context.UserCredentials.Where(x => x.UserId == id).ExecuteDelete();
            
        _context.Users.Remove(record);        
        await _context.SaveChangesAsync();
        return NoContent();
    }
    private bool UserExists(Guid id)
    {
        return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
    }
}
