using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RequiredAttribute = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace MarmitaBackend.Models
{
    public class Kit
    {
        public int Id { get; set; }

        public int TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;



        public string Name { get; set; }
        public string Description { get; set; }


        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero.")]
        [Column(TypeName = "decimal(10,2)")] // precisão total 10, 2 casas decimais
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
