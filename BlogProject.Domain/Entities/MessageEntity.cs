﻿namespace BlogProject.Domain.Entities
{
    public class MessageEntity
    {
        public Guid Id { get; set; }    
        public string Content { get; set; }
        public bool IsRead { get; set; }
        public DateTime SentDate { get; set; }

        public Guid? SenderId { get; set; }
        public virtual AppUser? Sender { get; set; }

        public Guid? ReceiverId { get; set; }
        public virtual AppUser? Receiver { get; set; }

        public bool IsDeleted { get; set; }
    }
}
