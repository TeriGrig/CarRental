namespace CarRental.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public string CommenterId { get; set; }
        public User Commenter { get; set; }
        public string RecipientId { get; set; }
        public User Recipient { get; set; }
    }
}
