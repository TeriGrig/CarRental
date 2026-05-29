using Microsoft.EntityFrameworkCore;

namespace CarRental.Models
{
    public class Report
    {
        public int ReportId { get; set; }
        public string ReporterId { get; set; }
        public User Reporter { get; set; }
        public string ReportRecipientId { get; set; }
        public User ReportRecipient { get; set; }
        public DateTime DateTime { get; set; }
        public bool Seen { get; set; } = false;
    }
}
