namespace project_csharp_sgu.Models;

public class Poi
{
    public string Id { get; set; } 
    public string Name { get; set; }
    public string Address { get; set; }
    
    // Hai thông số này sẽ dùng để định vị gian hàng
    public double Lat { get; set; } 
    public double Lng { get; set; }

    public string Distance { get; set; } 
    public string description { get; set; } // Đổi thành chữ D viết hoa nhé
}