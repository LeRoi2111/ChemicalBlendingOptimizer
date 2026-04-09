using Google.OrTools.LinearSolver;
using ChemicalMixtureOptimizer.Models;

namespace ChemicalMixtureOptimizer.Services
{
    public class OptimizationService
    {
        public OptimizationResult Solve(OptimizationInput input)
        {
            var result = new OptimizationResult();

            // НАЧАЛО ВАЛИДАЦИИ 

            // Проверка 1: вес смеси должен быть положительным
            if (input.RequiredMixtureKg <= 0)
            {
                result.Success = false;
                result.ErrorMessage = "Ошибка: вес смеси должен быть больше 0";
                return result;
            }

            // Проверка 2: проверка каждого материала
            if (input.Materials == null || input.Materials.Count == 0)
            {
                result.Success = false;
                result.ErrorMessage = "Ошибка: не добавлено ни одного материала";
                return result;
            }

            for (int i = 0; i < input.Materials.Count; i++)
            {
                var material = input.Materials[i];

                // Стоимость не может быть отрицательной
                if (material.Cost < 0)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Ошибка: стоимость материала {material.Name} не может быть отрицательной";
                    return result;
                }

                // Проверка состава (неотрицательные значения)
                if (material.Composition == null)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Ошибка: состав материала {material.Name} не задан";
                    return result;
                }

                for (int j = 0; j < material.Composition.Length; j++)
                {
                    if (material.Composition[j] < 0)
                    {
                        result.Success = false;
                        result.ErrorMessage = $"Ошибка: содержание вещества B{j + 1} в материале {material.Name} не может быть отрицательным";
                        return result;
                    }
                }
            }

            // Проверка 3: минимальные требования к веществам не могут быть отрицательными
            if (input.MinSubstances == null)
            {
                result.Success = false;
                result.ErrorMessage = "Ошибка: минимальные требования к веществам не заданы";
                return result;
            }

            for (int j = 0; j < input.MinSubstances.Length; j++)
            {
                if (input.MinSubstances[j] < 0)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Ошибка: минимальное содержание вещества B{j + 1} не может быть отрицательным";
                    return result;
                }
            }

            // Проверка 4: проверка на слишком высокие требования (опционально)
            double maxPossible = 0;
            for (int j = 0; j < input.MinSubstances.Length; j++)
            {
                maxPossible = 0;
                for (int i = 0; i < input.Materials.Count; i++)
                {
                    if (input.Materials[i].Composition[j] > maxPossible)
                        maxPossible = input.Materials[i].Composition[j];
                }
                if (input.MinSubstances[j] > maxPossible * 1.5)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Ошибка: минимальное содержание вещества B{j + 1} ({input.MinSubstances[j]}) слишком высокое. Максимальное содержание в одном материале: {maxPossible}";
                    return result;
                }
            }

            // ========== КОНЕЦ ВАЛИДАЦИИ ==========

            try
            {
                // Создаем солвер (GLOP - симплекс-метод)
                Solver solver = Solver.CreateSolver("GLOP");
                if (solver == null)
                {
                    result.Success = false;
                    result.ErrorMessage = "Не удалось создать солвер";
                    return result;
                }

                int n = input.Materials.Count; // количество переменных
                int m = input.MinSubstances.Length; // количество веществ

                // Переменные x1...xn (количества материалов в кг)
                Variable[] x = new Variable[n];
                for (int i = 0; i < n; i++)
                {
                    x[i] = solver.MakeNumVar(0.0, double.PositiveInfinity, $"x{i + 1}");
                }

                // Целевая функция: минимизация стоимости
                Objective objective = solver.Objective();
                for (int i = 0; i < n; i++)
                {
                    objective.SetCoefficient(x[i], input.Materials[i].Cost);
                }
                objective.SetMinimization();

                // Ограничение 1: сумма = RequiredMixtureKg
                Constraint sumConstraint = solver.MakeConstraint(input.RequiredMixtureKg, input.RequiredMixtureKg, "sum");
                for (int i = 0; i < n; i++)
                {
                    sumConstraint.SetCoefficient(x[i], 1.0);
                }

                // Ограничения по веществам (нижняя граница)
                for (int j = 0; j < m; j++)
                {
                    Constraint constraint = solver.MakeConstraint(input.MinSubstances[j], double.PositiveInfinity, $"substance{j + 1}");
                    for (int i = 0; i < n; i++)
                    {
                        constraint.SetCoefficient(x[i], input.Materials[i].Composition[j]);
                    }
                }

                // Решение
                Solver.ResultStatus resultStatus = solver.Solve();

                if (resultStatus == Solver.ResultStatus.OPTIMAL)
                {
                    result.Success = true;
                    result.TotalCost = objective.Value();

                    // Получаем значения переменных
                    for (int i = 0; i < n; i++)
                    {
                        result.Quantities[i] = x[i].SolutionValue();
                    }

                    // Рассчитываем фактическое содержание веществ
                    result.MinLimits = input.MinSubstances;
                    for (int j = 0; j < m; j++)
                    {
                        double sum = 0;
                        for (int i = 0; i < n; i++)
                        {
                            sum += input.Materials[i].Composition[j] * result.Quantities[i];
                        }
                        result.SubstanceAmounts[j] = sum;
                    }
                }
                else
                {
                    result.Success = false;
                    result.ErrorMessage = "Оптимальное решение не найдено. Статус: " + resultStatus;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Ошибка: {ex.Message}";
            }

            return result;
        }
    }
}
