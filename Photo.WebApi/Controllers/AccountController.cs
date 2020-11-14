using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Photo.WebApi.Entities;
using Photo.WebApi.Helpers;
using Photo.WebApi.Services;
using static Photo.WebApi.Models.UserViewModel;

namespace Photo.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly EFContext _context;
        private readonly UserManager<DbUser> _userManager;
        private readonly SignInManager<DbUser> _signInManager;
        private readonly IJwtTokenService _IJwtTokenService;
        private readonly IWebHostEnvironment _env;

        public AccountController(EFContext context,
           UserManager<DbUser> userManager,
           SignInManager<DbUser> signInManager,
           IJwtTokenService IJwtTokenService,
           IWebHostEnvironment env)
        {
            _userManager = userManager;
            _context = context;
            _signInManager = signInManager;
            _IJwtTokenService = IJwtTokenService;
            _env = env;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Bad Model");
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                return BadRequest("Даний користувач не знайденний!");
            }

            var result = _signInManager
                .PasswordSignInAsync(user, model.Password, false, false).Result;

            if (!result.Succeeded)
            {
                return BadRequest("Невірно введений пароль!");
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            return Ok(
                 new
                 {
                     token = _IJwtTokenService.CreateToken(user)
                 });
        }

        [HttpPost("register")]
        [RequestSizeLimit(100 * 1024 * 1024)]     // set the maximum file size limit to 100 MB
        public async Task<IActionResult> Register([FromBody] UserRegisterViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest("Поганий запит");
            }

            var roleName = "User";
            var userReg = _context.Users.FirstOrDefault(x => x.Email == model.Email);
            if (userReg != null)
            {
                return BadRequest("Цей емейл вже зареєстровано!");
            }

            if (model.Email == null)
            {
                return BadRequest("Вкажіть пошту!");
            }
            else
            {
                var testmail = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                if (!testmail.IsMatch(model.Email))
                {
                    return BadRequest("Невірно вказана почта!");
                }
            }
            DbUser user = new DbUser
            {
                Email = model.Email,
                UserName = model.Email
            };
            string ext = ".jpg";
            string fileName = Path.GetRandomFileName() + ext;

            user.UserProfile = new UserProfile()
            {
                Name = "Empty",
                Surname = "Empty",
                DateOfBirth = new DateTime(2000, 1, 1),
                Phone = "+380000000000",
                RegistrationDate = DateTime.Now,
                Photo = null
            };

            if (!String.IsNullOrWhiteSpace(model.ImageBase64))
            {
                user.UserProfile.Photo = fileName;
                var bmp = model.ImageBase64.FromBase64StringToImage();
                var serverPath = _env.ContentRootPath; //Directory.GetCurrentDirectory(); //_env.WebRootPath;
                var folerName = "Uploaded";
                var path = Path.Combine(serverPath, folerName);

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string filePathSave = Path.Combine(path, fileName);
                bmp.Save(filePathSave, ImageFormat.Jpeg);
            }

            var res = _userManager.CreateAsync(user, model.Password).Result;
            if (!res.Succeeded)
            {
                return BadRequest("Код доступу має складатись з 8 символів, містити мінімум одну велику літеру!");
            }

            res = _userManager.AddToRoleAsync(user, roleName).Result;

            if (!res.Succeeded)
            {
                return BadRequest("Поганий запит!");
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            return Ok(
                 new
                 {
                     token = _IJwtTokenService.CreateToken(user)
                 });
        }
    }
}
