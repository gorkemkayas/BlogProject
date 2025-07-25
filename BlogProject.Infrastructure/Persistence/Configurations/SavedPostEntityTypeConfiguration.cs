﻿using BlogProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogProject.Infrastructure.Persistence.Configurations
{
    public class SavedPostEntityTypeConfiguration : IEntityTypeConfiguration<SavedPostEntity>
    {
        public void Configure(EntityTypeBuilder<SavedPostEntity> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.IsDeleted)
                   .HasDefaultValue(false);

            builder.Property(e => e.SavedDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(sp => sp.User)
                   .WithMany(u => u.SavedPosts)
                   .HasForeignKey(sp => sp.UserId);
        }
    }
}
