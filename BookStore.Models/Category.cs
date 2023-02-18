using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
namespace BookStore.Models
{   
    [Serializable]
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        [DisplayName("Diplay Order")]
        [Range(1,100, ErrorMessage = "Display Order muust be between 1 and 100 included")]
        public int dispalyOrder { get; set; }

        public DateTime CreationTime { get; set; } = DateTime.Now;
    }
}
