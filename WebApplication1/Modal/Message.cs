namespace WebApplication1.Modal
{
    public class Message
    {
        public int Id { get; set; }

        public int SenderId  { get; set; }
        public User Sender { get; set; }
        public int ReceiverId { get; set;}
        public User Receiver { get; set; }
        public string content { get; set; }

        public DateTime Timestemp { get; set; }
    }
}
