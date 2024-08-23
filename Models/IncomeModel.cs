using System.ComponentModel.DataAnnotations;

namespace gelirGiderTakip.Models
{
    public class Income
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int Price { get; set; }
        [Required]
        public bool Type { get; set; }
        [Required]
        public int CategoryId { get; set; }
        [Required]
        public int UserId { get; set; }
    }
    public class Category
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
    public class IncomeModel
    {
        public Category Category { get; set; }
        public List<Income> Income { get; set; }
    }

    public class Total
    {
        public List<Income> Income { get; set; }
        public List<Income> Outcome { get; set; }
    }
}
