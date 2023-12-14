using CloudinaryDotNet.Actions;

namespace udemyCourse.Interfaces
{
    public interface IPhotoSerrvice
    {
        Task<ImageUploadResult> AddPhotoAsync(IFormFile file);
        Task<DeletionResult> DeletePhotoAsync(string publicId);
    }
}
