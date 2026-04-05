namespace project_csharp_sgu.Models;

public class Poi
{
<<<<<<< Updated upstream
    public string Id { get; set; } 
    public string Name { get; set; }
    public string Address { get; set; }
    
    // Hai thông số này sẽ dùng để định vị gian hàng
    public double Lat { get; set; } 
    public double Lng { get; set; }

    public string Distance { get; set; } 
    public string description { get; set; } // Đổi thành chữ D viết hoa nhé
=======
    public string id { get; set; } 
    public string qr_id { get; set; } // <--- THÊM DÒNG NÀY
    public string name { get; set; }
    public string address { get; set; }
    public double lat { get; set; }
    public double lng { get; set; }
    public string description { get; set; }
    public string distance { get; set; }
>>>>>>> Stashed changes
}