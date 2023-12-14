using Microsoft.AspNetCore.Identity;

namespace udemyCourse.Entities
{
    public class AppRole : IdentityRole<int>//may to many relationship between roles and users as many users can have many roles and many roles can have many users
    {
        public ICollection<AppUserRole> UserRoles { get; set; } //List and icollection does not matter both serves the same purpose   
    }
}
