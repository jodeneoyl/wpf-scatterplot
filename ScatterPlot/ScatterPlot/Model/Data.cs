namespace ScatterPlot.Model
{
    public class Data
    {
        public string Series { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Weight { get; set; }
        
        public Data() { }

        public Data(string series, double x, double y, double weight)
        {
            Series = series;
            X = x;
            Y = y;
            Weight = weight;
        }
    }
}
