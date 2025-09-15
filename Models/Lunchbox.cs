using MarmitaBackend.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RequiredAttribute = System.ComponentModel.DataAnnotations.RequiredAttribute;


namespace MarmitaBackend.Models
{
    public class Lunchbox
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero.")]
        [Column(TypeName = "decimal(10,2)")] // precisão total 10, 2 casas decimais
        public decimal Price { get; set; }
        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        public int PortionGram { get; set; } = 0;

        [Required]
        public int CategoryId { get; set; }

        [ValidateNever]
        public Category Category { get; set; } = null!;

        public List<KitLunchbox> KitLunchboxes { get; set; } = new();
    }
}
