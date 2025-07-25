﻿using BlogProject.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogProject.Infrastructure.Persistence.Configurations
{
    public class LikeEntityTypeConfiguration : BaseEntityTypeConfiguration<LikeEntity>
    {
        public override void Configure(EntityTypeBuilder<LikeEntity> builder)
        {
            builder.Property(e => e.UserId).IsRequired();

            builder.HasOne(l => l.User)
                   .WithMany(u => u.Likes)
                   .HasForeignKey(f => f.UserId)
                   .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Restrict);

            builder.HasOne(e => e.Post)
                   .WithMany(p => p.Likes)
                   .HasForeignKey(e => e.PostId)
                   .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Restrict);

            builder.HasOne(e => e.Comment)
                   .WithMany(c => c.Likes)
                   .HasForeignKey(f => f.CommentId)
                   .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Restrict);

            base.Configure(builder);
        }
    }
}
