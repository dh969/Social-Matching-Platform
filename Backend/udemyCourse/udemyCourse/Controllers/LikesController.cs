using Microsoft.AspNetCore.Mvc;
using udemyCourse.Data;
using udemyCourse.DTOs;
using udemyCourse.Entities;
using udemyCourse.Extensions;
using udemyCourse.Helper;
using udemyCourse.Interfaces;

namespace udemyCourse.Controllers
{
    public class LikesController : BaseController
    {
        private readonly IUserRepository userRepository;
        private readonly ILikeRepository likeRepository;

        public LikesController(IUserRepository userRepository, ILikeRepository likeRepository)
        {
            this.userRepository = userRepository;
            this.likeRepository = likeRepository;
        }
        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId = int.Parse(User.GetUserId());
            var likedUser = await userRepository.GetUserByUsernameAsync(username);
            var sourceUser = await likeRepository.GetUserWithLikes(sourceUserId);
            if (likedUser == null)
            {
                return NotFound();
            }
            if (sourceUser.UserName == username) return BadRequest("you cannot like yourself");
            var userLike = await likeRepository.GetUserLike(sourceUserId, likedUser.Id);
            if (userLike != null) return BadRequest("you already liked this user");
            userLike = new UserLike
            {
                SourceUserId = sourceUserId,
                TargetUserId = likedUser.Id
            };
            sourceUser.LikedUsers.Add(userLike);
            if (await userRepository.SaveAllAsync()) return Ok();
            return BadRequest("Failed to like user");
        }
        [HttpGet]
        public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery] LikesParams lp)
        {
            lp.UserId = int.Parse(User.GetUserId());
            var users = await likeRepository.GetUserLikes(lp);
            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));
            return Ok(users);
        }
    }
}
