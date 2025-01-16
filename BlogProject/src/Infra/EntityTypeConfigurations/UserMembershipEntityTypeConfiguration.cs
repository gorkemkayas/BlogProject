﻿using BlogProject.src.Infra.Entitites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogProject.src.Infra.EntityTypeConfigurations
{
    public class UserMembershipEntityTypeConfiguration : BaseEntityTypeConfiguration<UserMemberShipEntity>
    {
        public override void Configure(EntityTypeBuilder<UserMemberShipEntity> builder)
        {
            // StartDate
            builder.Property(e => e.StartDate)
                   .IsRequired()
                   .HasColumnType("datetime2");

            // EndDate
            builder.Property(e => e.EndDate)
                   .IsRequired()
                   .HasColumnType("datetime2");

            builder.HasOne(um => um.User)
                   .WithOne(u => u.UserMemberShipEntity)
                   .HasForeignKey<UserMemberShipEntity>(f => f.UserId);

            base.Configure(builder);
        }
    }
}
