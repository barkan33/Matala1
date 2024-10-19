namespace Matala1.Models.Entitys
{
    public class Notice
    {
        public int NoticeID { get; set; }  // Primary Key
        public string Title { get; set; }
        public string Content { get; set; }
        public string Preview { get; set; }
        public DateTime PublishDate { get; set; }
        public string Author { get; set; }
        public DateTime? ExpiryDate { get; set; }  // Nullable
        public string Category { get; set; }
    }
}
