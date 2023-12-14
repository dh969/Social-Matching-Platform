using System.ComponentModel.DataAnnotations.Schema;

namespace udemyCourse.Entities
{
    [Table("Photos")]
    public class Photo
    {

        public int Id { get; set; }
        public string Url { get; set; }
        public bool IsMain { get; set; }
        public string PublicId { get; set; }
        public int AppUserId { get; set; }
        public AppUser AppUser { get; set; }//one to many relationship as one user can upload many photos
    }
}