namespace RaceDayApi.Models;

public class Category
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    public ICollection<Event> Events { get; set; } = new List<Event>();
}