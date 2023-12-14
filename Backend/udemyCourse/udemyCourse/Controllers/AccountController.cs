using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Security.Cryptography;
using System.Text;
using udemyCourse.Data;
using udemyCourse.DTOs;
using udemyCourse.Entities;
using udemyCourse.Interfaces;

namespace udemyCourse.Controllers
{
    public class AccountController : BaseController
    {
        private readonly DataContext _context;
        private readonly IToken _token;
        private readonly UserManager<AppUser> userManager;

        public AccountController(DataContext context, IToken token, UserManager<AppUser> userManager)
        {
            _context = context;
            _token = token;
            this.userManager = userManager;
        }
        [HttpPost("Register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto register)
        {
            if (await userExists(register.username))
            { return BadRequest("username is taken"); }
            //using var hmac = new HMACSHA512();//password salt "using is used here as the hmac class has a dispose method so that we can dispose the class when we are done with it
            //var user = new AppUser {
            //    UserName = register.username,
            //    PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(register.password)),
            //    PasswordSalt=hmac.Key
            //    };
            var user = new AppUser
            {
                UserName = register.username,

            };
            var result = await userManager.CreateAsync(user, register.password);
            if (!result.Succeeded) return BadRequest(result.Errors);
            var roleResult = await userManager.AddToRoleAsync(user, "Member");

            await userManager.CreateAsync(user, register.password);
            if (!roleResult.Succeeded) return BadRequest(roleResult.Errors);
            //  await _context.SaveChangesAsync();
            return new UserDto
            {
                Username = user.UserName,
                Token = await _token.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender

            };

        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto login)
        {
            // var user = await _context.Users.Include(x=>x.Photos).FirstOrDefaultAsync(x => x.UserName == login.username);
            var user = await userManager.Users.Include(x => x.Photos).FirstOrDefaultAsync(x => x.UserName == login.username);
            if (user == null)
            {
                return Unauthorized("User doesn't exist");
            }
            var result = await userManager.CheckPasswordAsync(user, login.password);
            if (!result) return Unauthorized("Invalid password");
            //using var hmac = new HMACSHA512(user.PasswordSalt);
            //var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(login.password));

            //for (int i = 0; i < computedHash.Length; i++)
            //{
            //    if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Wrong Password");
            //  }
            return new UserDto
            {
                Username = user.UserName,
                Token = await _token.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }
        private async Task<bool> userExists(string username)
        {
            return await _context.Users.AnyAsync(x => x.UserName == username);
        }
    }
}
