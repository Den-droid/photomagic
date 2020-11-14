using Photo.WebApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Photo.WebApi.Models
{
    public class UserPhotoView
    {
        public string ImageBase64 { get; set; }
    }

    public class UserProfileView
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Photo { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Phone { get; set; }

        public string Email { get; set; }

        public DateTime RegistrationDate { get; set; }

        public UserProfileView() { }

        public UserProfileView(DbUser user)
        {
            UserProfile profile = user.UserProfile;
            Email = user.Email;
            Name = profile.Name;
            Surname = profile.Surname;
            Photo = profile.Photo;
            DateOfBirth = profile.DateOfBirth;
            Phone = profile.Phone;
            RegistrationDate = profile.RegistrationDate;
        }
    }
}