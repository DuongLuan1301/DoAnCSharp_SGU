namespace project_csharp_sgu.Models;
public class Poi
{
    public string Id { get; set; } // Thêm dòng này để định danh hàng quán [cite: 25]
    public string Name { get; set; }
    public string Description { get; set; }
    public string Address { get; set; }
    public string Distance { get; set; }
}