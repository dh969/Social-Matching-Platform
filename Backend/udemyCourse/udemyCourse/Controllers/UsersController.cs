using AutoMapper;
using AutoMapper.Configuration.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using udemyCourse.DTOs;
using udemyCourse.Entities;
using udemyCourse.Extensions;
using udemyCourse.Helper;
using udemyCourse.Interfaces;

namespace udemyCourse.Controllers
{
    [Authorize]
    public class UsersController : BaseController
    {
        private readonly IUserRepository user;
        private readonly IMapper mapper;
        private readonly IPhotoSerrvice photo;

        public UsersController(IUserRepository user, IMapper mapper, IPhotoSerrvice photo)
        {
            this.user = user;
            this.mapper = mapper;
            this.photo = photo;
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
        {
            var currentUser = await user.GetUserByUsernameAsync(User.GetUsername());
            userParams.CurrentUserName = currentUser.UserName;
            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = currentUser.Gender == "male" ? "female" : "male";
            }
            var users = await user.GetMembersAsync(userParams);
            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));


            return Ok(users);


        }

        [HttpGet("id/{id}")]
        public async Task<ActionResult<MemberDto>> GetUserById(int id)
        {
            var user = await this.user.GetUserByIdAsync(id);

            return Ok(mapper.Map<MemberDto>(user));
        }
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await user.GetMemberAsync(username);
        }
        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto member)
        {
            var userName = User.GetUsername();
            var user = await this.user.GetUserByUsernameAsync(userName);
            if (user == null) { return NotFound(); }
            this.mapper.Map(member, user);//update properties of all the properties in member to user from member to user
            if (await this.user.SaveAllAsync()) return NoContent();//status of 204 i.e everything was okay but i have nothing to return to you 
            return BadRequest("Failed to update User");
        }
        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var username = User.GetUsername();
            var user = await this.user.GetUserByUsernameAsync(username);
            if (user == null) { return NotFound(); }
            var result = await this.photo.AddPhotoAsync(file);
            if (result.Error != null) return BadRequest(result.Error.Message);
            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };
            if (user.Photos.Count == 0) photo.IsMain = true;
            user.Photos.Add(photo);
            if (await this.user.SaveAllAsync())
            {
                return CreatedAtAction(nameof(GetUser), new { username }, this.mapper.Map<PhotoDto>(photo));//sends a 201 created response
            }
            return BadRequest("Cannot upload photo");
        }
        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await this.user.GetUserByUsernameAsync(User.GetUsername());
            if (user == null) return NotFound();
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo == null) return NotFound();
            if (photo.IsMain) return BadRequest("This is already your main photo");
            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true;
            if (await this.user.SaveAllAsync()) return NoContent();
            return BadRequest("Problems occured in setting This photo to main");
        }
        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await this.user.GetUserByUsernameAsync(User.GetUsername());
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo == null) return NotFound();
            if (photo.IsMain) return BadRequest("You cannot delete your main photo");
            if (photo.PublicId != null)
            {
                var result = await this.photo.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);


            }
            user.Photos.Remove(photo);
            if (await this.user.SaveAllAsync()) return Ok();
            return BadRequest("Problem deleting photo");


        }
    }
}