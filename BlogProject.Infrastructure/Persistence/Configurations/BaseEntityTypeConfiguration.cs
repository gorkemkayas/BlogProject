﻿using BlogProject.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogProject.Infrastructure.Persistence.Configurations
{
    public class BaseEntityTypeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : BaseEntity
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.CreatedTime)
                   .HasColumnType("datetime2")
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.ModifiedTime)
                   .HasColumnType("datetime2")
                   .IsRequired(false);

            builder.Property(e => e.IsDeleted)
                   .HasDefaultValue(false);
        }
    }
}
