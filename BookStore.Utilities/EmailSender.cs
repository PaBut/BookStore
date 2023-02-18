using BookStore.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Utilities
{
    public class EmailSender : IEmailSender
    {
        readonly EmailSenderSettings _settings;

        public EmailSender(IOptionsMonitor<EmailSenderSettings> options)
        {
            _settings = options.CurrentValue;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailToSend = new MimeMessage();
            emailToSend.From.Add(MailboxAddress.Parse(_settings.Email));
            emailToSend.To.Add(MailboxAddress.Parse(email));
            emailToSend.Subject = subject;
            emailToSend.Body = new TextPart(MimeKit.Text.TextFormat.Html){ Text = htmlMessage };
            using(var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                client.Authenticate(_settings.Email, _settings.Password);
                client.Send(emailToSend);
                client.Disconnect(true);
            }

            return Task.CompletedTask;  
        }
    }
}
