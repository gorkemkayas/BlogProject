﻿using BlogProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogProject.Infrastructure.Persistence.Configurations
{
    public class PostEntityTypeConfiguration : BaseEntityTypeConfiguration<PostEntity>
    {
        public override void Configure(EntityTypeBuilder<PostEntity> builder)
        {
            // Title
            builder.Property(e => e.Title)
                   .IsRequired()
                   .HasMaxLength(200);

            // Subtitle
            builder.Property(e => e.Subtitle)
                   .IsRequired()
                   .HasMaxLength(500);

            // Content
            builder.Property(e => e.Content)
                   .IsRequired()
                   .HasMaxLength(20000);

            // CoverImageUrl
            
            // IsDraft
            builder.Property(e => e.IsDraft).HasDefaultValue(false);

            // ViewCount
            builder.Property(e => e.ViewCount).HasDefaultValue(0);

            // One to many relation
            builder.HasOne(e => e.Author)
                   .WithMany(a => a.Posts)
                   .HasForeignKey(f => f.AuthorId)
                   .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Restrict);

            base.Configure(builder);
        }
    }
}
