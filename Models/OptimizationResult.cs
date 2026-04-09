namespace ChemicalMixtureOptimizer.Models
{
    public class OptimizationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public double TotalCost { get; set; }
        public double[] Quantities { get; set; } = new double[5];
        public double[] SubstanceAmounts { get; set; } = new double[4];
        public double[] MinLimits { get; set; } = new double[4];
    }
}
