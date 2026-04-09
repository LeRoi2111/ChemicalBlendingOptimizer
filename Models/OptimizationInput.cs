using System.ComponentModel.DataAnnotations;

namespace ChemicalMixtureOptimizer.Models
{
    public class OptimizationInput
    {
        [Required(ErrorMessage = "Введите общий вес смеси")]
        [Range(0.1, 100, ErrorMessage = "Вес смеси должен быть от 0.1 до 100 кг")]
        public double RequiredMixtureKg { get; set; } = 1.0;

        [Required(ErrorMessage = "Введите минимальное содержание веществ")]
        public double[] MinSubstances { get; set; } = new double[4];

        [Required(ErrorMessage = "Данные о материалах обязательны")]
        public List<Material> Materials { get; set; } = new List<Material>();

        // Конструктор для инициализации данных по варианту 2
        public OptimizationInput()
        {
            // Инициализация требований к веществам (вариант 2)
            MinSubstances = new double[] { 15, 15, 50, 70 };

            // Инициализация материалов (вариант 2)
            Materials = new List<Material>
            {
                new Material { Name = "A1", Cost = 3.5, Composition = new double[] { 12, 12, 76, 76 } },
                new Material { Name = "A2", Cost = 3.5, Composition = new double[] { 20, 18, 62, 62 } },
                new Material { Name = "A3", Cost = 5.2, Composition = new double[] { 12, 18, 70, 70 } },
                new Material { Name = "A4", Cost = 4.0, Composition = new double[] { 20, 14, 54, 57 } },
                new Material { Name = "A5", Cost = 5.0, Composition = new double[] { 20, 14, 85, 75 } }
            };
        }
    }
}