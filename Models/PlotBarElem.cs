using ScottPlot;
using SportStats.Enums;

namespace SportStats.Models
{
    public class PlotBarElem
    {
        public List<double> Values { get; set; } = null!;
        public BarPlotTypes Type { get; set; }
        public string? Title { get; set; }
        public Color color { get; set; }
    }
}
