﻿using BlogProject.Domain.Enums;

namespace BlogProject.Domain.Entities
{
    public class DonationEntity
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public decimal Amount { get; set; }
        public Currency Currency { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedTime { get; set; }

        public Guid? SenderId { get; set; }
        public AppUser? Sender { get; set; }

        public Guid? ReceiverId { get; set; }
        public AppUser? Receiver { get; set; }
        public virtual ICollection<PaymentMethodEntity> PaymentMethods { get; set; }

        public bool IsDeleted { get; set; }
    }
}
