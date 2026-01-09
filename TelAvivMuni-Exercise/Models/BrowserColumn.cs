namespace TelAvivMuni_Exercise.Models
{
    public class BrowserColumn
    {
        public string DataField { get; set; } = string.Empty;
        public string Header { get; set; } = string.Empty;
        public double Width { get; set; } = double.NaN;
        public string? Format { get; set; }
        public string? HorizontalAlignment { get; set; }
    }
}
