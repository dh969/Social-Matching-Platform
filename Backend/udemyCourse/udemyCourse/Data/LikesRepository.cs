using Microsoft.EntityFrameworkCore;
using udemyCourse.DTOs;
using udemyCourse.Entities;
using udemyCourse.Extensions;
using udemyCourse.Helper;
using udemyCourse.Interfaces;

namespace udemyCourse.Data
{
    public class LikesRepository : ILikeRepository
    {
        private readonly DataContext context;

        public LikesRepository(DataContext context)
        {
            this.context = context;
        }
        public async Task<UserLike> GetUserLike(int sourceUserId, int targetUserId)
        {
            return await context.Likes.FindAsync(sourceUserId, targetUserId);
        }

        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams lp)
        {
            var users = context.Users.OrderBy(u => u.UserName).AsQueryable();
            var likes = context.Likes.AsQueryable();
            if (lp.Predicate == "liked")
            {
                likes = likes.Where(like => like.SourceUserId == lp.UserId);
                users = likes.Select(like => like.TargetUser);
            }
            if (lp.Predicate == "likedBy")
            {
                likes = likes.Where(like => like.TargetUserId == lp.UserId);
                users = likes.Select(like => like.SourceUser);
            }
            var likedUsers = users.Select(user => new LikeDto
            {
                UserName = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain).Url,
                City = user.City,
                Id = user.Id


            });
            return await PagedList<LikeDto>.CreateAsync(likedUsers, lp.PageNumber, lp.PageSize);
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await context.Users.Include(x => x.LikedUsers).FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
}
