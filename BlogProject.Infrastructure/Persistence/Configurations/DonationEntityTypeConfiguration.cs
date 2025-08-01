﻿using BlogProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogProject.Infrastructure.Persistence.Configurations
{
    public class DonationEntityTypeConfiguration : IEntityTypeConfiguration<DonationEntity>
    {
        public void Configure(EntityTypeBuilder<DonationEntity> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Message)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(e => e.Amount)
                   .IsRequired()
                   .HasPrecision(18, 2); ;

            builder.Property(e => e.Currency)
                   .IsRequired();

            builder.Property(e => e.Status)
                  .IsRequired();

            builder.Property(e => e.IsDeleted)
                   .HasDefaultValue(false);

            builder.Property(e => e.CreatedTime)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(d => d.Receiver)
                   .WithMany(r => r.ReceivedDonations)
                   .HasForeignKey(f => f.ReceiverId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Sender)
                   .WithMany(s => s.SendedDonations)
                   .HasForeignKey(f => f.SenderId)
                   .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
