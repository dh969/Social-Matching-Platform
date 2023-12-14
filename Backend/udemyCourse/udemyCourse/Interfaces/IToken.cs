using udemyCourse.Entities;

namespace udemyCourse.Interfaces
{
    public interface IToken
    {
        Task<string> CreateToken(AppUser user);

    }
}
