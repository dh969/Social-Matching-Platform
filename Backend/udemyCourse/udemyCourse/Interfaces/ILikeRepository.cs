using udemyCourse.DTOs;
using udemyCourse.Entities;
using udemyCourse.Helper;

namespace udemyCourse.Interfaces
{
    public interface ILikeRepository
    {
        Task<UserLike> GetUserLike(int sourceUserId, int targetUserId);
        Task<AppUser> GetUserWithLikes(int userId);
        Task<PagedList<LikeDto>> GetUserLikes(LikesParams lp);
    }
}
