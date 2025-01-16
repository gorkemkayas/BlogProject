﻿using BlogProject.src.Infra.Entitites;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogProject.src.Infra.EntityTypeConfigurations
{
    public class SettingsEntityTypeConfiguration : BaseEntityTypeConfiguration<SettingsEntity>
    {
        public override void Configure(EntityTypeBuilder<SettingsEntity> builder)
        {
            // Theme
            builder.Property(e => e.Theme)
                   .IsRequired();

            // NotificationPreferences
            builder.Property(e => e.NotificationPreferences)
                   .IsRequired();

            // Settings - User One to One relation
            builder.HasOne(s => s.User)
                   .WithOne(u => u.Settings)
                   .HasForeignKey<SettingsEntity>(f => f.UserId);

            base.Configure(builder);
        }
    }
}
