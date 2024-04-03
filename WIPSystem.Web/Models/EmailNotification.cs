namespace WIPSystem.Web.Models
{
    public class EmailNotification
    {
        public string RecipientEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        // You can add more properties as needed, such as CC, BCC, etc.
    }
}
