namespace SkiControl.Models;

// Dane zbiorcze do statystyk na stronie głównej
public class SummaryModel
{
    public decimal TotalSales { get; set; }
    public int InvoicesCount { get; set; }
    public int UnpaidInvoicesCount { get; set; }
    public int KsefSentCount { get; set; }
}

// Punkt danych dla wykresu sprzedaży w ujęciu miesięcznym
public class MonthlySales
{
    public string Month { get; set; } = string.Empty;
    public double Amount { get; set; }
}