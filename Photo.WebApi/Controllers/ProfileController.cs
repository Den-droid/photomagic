using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Photo.WebApi.Entities;
using Photo.WebApi.Helpers;
using Photo.WebApi.Models;

namespace Photo.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly EFContext _context;
        private readonly IWebHostEnvironment _env;
        public ProfileController(EFContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpPost("info")]
        public IActionResult GetInfo()
        {
            string userName;
            try
            {
                userName = User.Claims.FirstOrDefault(x => x.Type == "name").Value;
            }
            catch (Exception)
            {
                return BadRequest("Потрібно спочатку залогінитися!");
            }

            if (string.IsNullOrEmpty(userName))
            {
                return BadRequest("Потрібно спочатку залогінитися!");

            }

            var query = _context.Users.Include(x => x.UserProfile).AsQueryable();
            var user = query.FirstOrDefault(c => c.UserName == userName);

            if (user == null)
            {
                return BadRequest("Поганий запит!");
            }

            UserProfileView userProfile = new UserProfileView(user);
            return Ok(userProfile);
        }

        [HttpPost("update")]
        public IActionResult ProfileUpdate([FromBody] UserProfileView model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Bad Model");
            }

            string userName;
            try
            {
                userName = User.Claims.FirstOrDefault(x => x.Type == "name").Value;
            }
            catch (Exception)
            {
                return BadRequest("Потрібно спочатку залогінитися!");
            }

            if (string.IsNullOrEmpty(userName))
            {
                return BadRequest("Потрібно спочатку залогінитися!");

            }

            var query = _context.Users.Include(x => x.UserProfile).AsQueryable();
            var user = query.FirstOrDefault(c => c.UserName == userName);

            if (user == null)
            {
                return BadRequest("Поганий запит!");
            }

            user.UserProfile.Name = model.Name;
            user.UserProfile.Surname = model.Surname;
            user.UserProfile.DateOfBirth = model.DateOfBirth;
            user.UserProfile.Phone = model.Phone;
            user.PhoneNumber = model.Phone;
            user.UserName = model.Email;
            user.Email = model.Email;
            _context.SaveChanges();

            var result = new UserProfileView(user);
            return Ok(result);
        }

        [HttpPost("update-photo")]
        public IActionResult UserPhotoUpdate([FromBody] UserPhotoView model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Bad Model");
            }

            string userName;
            try
            {
                userName = User.Claims.FirstOrDefault(x => x.Type == "name").Value;
            }
            catch (Exception)
            {
                return BadRequest("Потрібно спочатку залогінитися!");
            }

            if (string.IsNullOrEmpty(userName))
            {
                return BadRequest("Потрібно спочатку залогінитися!");

            }

            var query = _context.Users.Include(x => x.UserProfile).AsQueryable();
            var user = query.FirstOrDefault(c => c.UserName == userName);

            if (user == null)
            {
                return BadRequest("Поганий запит!");
            }

            string ext = ".jpg";
            string fileName = Path.GetRandomFileName() + ext;
            var bmp = model.ImageBase64.FromBase64StringToImage();
            var serverPath = _env.ContentRootPath;
            var folerName = "Uploaded";
            var path = Path.Combine(serverPath, folerName);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            try
            {
                string filePathSave = Path.Combine(path, fileName);
                bmp.Save(filePathSave, ImageFormat.Jpeg);

                // Check if file exists with its full path    
                if (!String.IsNullOrWhiteSpace(user.UserProfile.Photo) && System.IO.File.Exists(Path.Combine(path, user.UserProfile.Photo)))
                {
                    // If file found, delete it    
                    System.IO.File.Delete(Path.Combine(path, user.UserProfile.Photo));
                    Console.WriteLine("File deleted.");
                }
                else Console.WriteLine("File not found");
            }
            catch (IOException ioExp)
            {
                return BadRequest(ioExp.Message);
            }

            user.UserProfile.Photo = fileName;
            _context.SaveChanges();

            var result = new UserProfileView(user);
            return Ok(result);
        }
    }
}
