using System.ComponentModel.DataAnnotations;

namespace ChemicalMixtureOptimizer.Models
{
    public class Material
    {
        [Required(ErrorMessage = "Введите название материала")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Введите стоимость")]
        [Range(0.01, 1000, ErrorMessage = "Стоимость должна быть от 0,01 до 1000 у.е.")]
        [RegularExpression(@"^[0-9]+([.,][0-9]{1,2})?$", ErrorMessage = "Введите корректное число (например, 3,5 или 3.5)")]
        public double Cost { get; set; }

        [Required(ErrorMessage = "Введите состав материала")]
        public double[] Composition { get; set; } = new double[4];
    }
}