using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
    [Serializable]
    public class CoverType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}