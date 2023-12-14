using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using udemyCourse.Extensions;

namespace udemyCourse.Entities
{
    public class AppUser : IdentityUser<int>//will generate the id as integer instead of string 
    {
        //[Required]
        //public string UserName { get; set; }
        //public int Id { get; set; }
        //public byte[] PasswordHash { get; set; }
        //public byte[] PasswordSalt { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string KnownAs { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime LastActive { get; set; } = DateTime.UtcNow;
        public string Gender { get; set; }
        public string Introduction { get; set; }
        public string LookingFor { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public List<Photo> Photos { get; set; } = new List<Photo>();
        //public int GetAge()//this is causing automapper to access this full users db
        //{
        //    return DateOfBirth.CalculateAge();
        //}
        public List<UserLike> LikedByUsers { get; set; }
        public List<UserLike> LikedUsers { get; set; }
        public List<Message> MessagesSent { get; set; }
        public List<Message> MessageReceived { get; set; }//many to many relationship as user can send many messages and can receive many messages
        public ICollection<AppUserRole> UserRoles { get; set; }//icollection is just a parwent class of list and all
    }
}
