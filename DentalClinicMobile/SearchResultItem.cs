namespace DentalClinicMobile;

public class SearchResultItem
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string PriceFormatted => $"{Price:N0} руб.";
}