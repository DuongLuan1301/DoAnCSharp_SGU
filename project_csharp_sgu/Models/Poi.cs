namespace project_csharp_sgu.Models
{
    public class POI
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public string AudioPath { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Distance { get; set; }

        // 🟢 BẮT BUỘC THÊM
        public string DistanceText { get; set; }

        public string MapUrl => $"https://www.google.com/maps?q={Latitude},{Longitude}";
    }
}