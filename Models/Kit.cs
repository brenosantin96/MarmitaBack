using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using RequiredAttribute = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace MarmitaBackend.Models
{
    public class Kit
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [ValidateNever]
        public Category Category { get; set; } = null!;


        //Marmitas incluidas neste kit
        [ValidateNever]
        public List<KitLunchbox> KitLunchboxes { get; set; }
    }
}
