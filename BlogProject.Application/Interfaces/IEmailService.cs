﻿namespace BlogProject.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendResetPasswordEmailAsync(string resetEmailLink, string receiverEmail);
        Task SendPasswordChangedNotificationAsync(string subject, string receiverEmail);

        Task SendEmailConfirmationEmailAsync(string receiverEmail, string confirmationLink);

        Task SendSuspensionNotificationEmailAsync(string receiverEmail, string confirmationLink, bool suspendedRemoved = false);

        Task SendEmailNotificationNewsletterEmailAsync(string receiverEmail);
    }
}
