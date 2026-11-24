namespace MYAcademy_IdentityEmailProject.Entities;

public class Message
{
    public int MessageId { get; set; }
    public string ReceiverEmail { get; set; }
    public string SenderEmail { get; set; }
    public string Subject { get; set; }
    public DateTime SendDate { get; set; }
    public string MessageDetail { get; set; }
    public bool IsRead { get; set; } = false;
    public int CategoryId { get; set; }
    public Category Category { get; set; }
}
