using ChemicalMixtureOptimizer.Models;
using ChemicalMixtureOptimizer.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace ChemicalMixtureOptimizer.Controllers
{
    public class HomeController : Controller
    {
        private readonly OptimizationService _optimizationService;

        public HomeController(OptimizationService optimizationService)
        {
            _optimizationService = optimizationService;
        }

        public IActionResult Index()
        {
            var model = new OptimizationInput();
            return View(model);
        }

        [HttpPost]
        public IActionResult Optimize(OptimizationInput input)
        {
            // Проверка: есть ли вообще материалы
            if (input.Materials == null || input.Materials.Count == 0)
            {
                ViewBag.Error = "Ошибка: не добавлено ни одного материала";
                return View("Index", input);
            }

            // Проверка: минимальные требования к веществам
            if (input.MinSubstances == null || input.MinSubstances.Length == 0)
            {
                ViewBag.Error = "Ошибка: не заданы минимальные требования к веществам";
                return View("Index", input);
            }

            // Вызов сервиса для решения задачи оптимизации
            var result = _optimizationService.Solve(input);

            // Если есть ошибка от сервиса, передаем её в представление
            if (!result.Success && !string.IsNullOrEmpty(result.ErrorMessage))
            {
                ViewBag.Error = result.ErrorMessage;
                return View("Index", input);
            }

            return View("Result", result);
        }

        [HttpPost]
        public IActionResult ExportToExcel(OptimizationResult result)
        {
            // Создаем MemoryStream для CSV файла
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, Encoding.UTF8);

            // Заголовок
            writer.WriteLine("Результаты оптимизации состава смеси");
            writer.WriteLine($"Дата расчета: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            writer.WriteLine();

            // Общая стоимость
            writer.WriteLine($"Общая стоимость смеси, {result.TotalCost:F4} у.е.");
            writer.WriteLine();

            // Состав смеси
            writer.WriteLine("Состав смеси:");
            writer.WriteLine("Компонент,Количество (кг)");
            writer.WriteLine($"A1,{result.Quantities[0]:F4}");
            writer.WriteLine($"A2,{result.Quantities[1]:F4}");
            writer.WriteLine($"A3,{result.Quantities[2]:F4}");
            writer.WriteLine($"A4,{result.Quantities[3]:F4}");
            writer.WriteLine($"A5,{result.Quantities[4]:F4}");
            writer.WriteLine($"Сумма,{result.Quantities.Sum():F4}");
            writer.WriteLine();

            // Содержание веществ
            writer.WriteLine("Содержание веществ:");
            writer.WriteLine("Вещество,Требуется (≥),Фактически,Выполнение");

            string[] substanceNames = { "B1", "B2", "B3", "B4" };
            for (int i = 0; i < 4; i++)
            {
                string status = result.SubstanceAmounts[i] >= result.MinLimits[i] ? "Выполнено" : "НЕ выполнено";
                writer.WriteLine($"{substanceNames[i]},{result.MinLimits[i]:F2},{result.SubstanceAmounts[i]:F2},{status}");
            }

            writer.Flush();
            stream.Position = 0;

            // Возвращаем файл
            string fileName = $"OptimizationResult_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            return File(stream, "text/csv; charset=utf-8", fileName);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}