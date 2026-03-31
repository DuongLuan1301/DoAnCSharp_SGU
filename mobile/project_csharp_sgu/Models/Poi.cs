namespace project_csharp_sgu.Models;

public class Poi
{
    public string name { get; set; }
    public string address { get; set; }
    public double lat { get; set; }
    public double lng { get; set; }
    public string description { get; set; } // đây là description đã được backend lọc theo lang
}
