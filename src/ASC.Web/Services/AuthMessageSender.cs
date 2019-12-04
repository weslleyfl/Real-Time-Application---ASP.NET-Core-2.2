using ASC.Web.Configuration;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace ASC.Web.Services
{
    public class AuthMessageSender : IEmailSender, ISmsSender
    {
        private IOptions<ApplicationSettings> _settings;

        public AuthMessageSender(IOptions<ApplicationSettings> settings)
        {
            _settings = settings;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // TODO: Email padrao inserido manualmente - SendEmailAsync
            email = "weslleylopes@fundep.com.br";

            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(_settings.Value.SMTPAccount));
            emailMessage.To.Add(new MailboxAddress(email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("html") { Text = htmlMessage };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_settings.Value.SMTPServer, _settings.Value.SMTPPort, false);
                //await client.AuthenticateAsync(_settings.Value.SMTPAccount, _settings.Value.SMTPPassword);
                // TODO: Email nao enviado - desabilitado
                //await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }

        }

        public async Task SendSmsAsync(string number, string message)
        {

            //return await Task.Run(() => string.Empty).ConfigureAwait(false);
            await Task.Factory.StartNew(() => string.Empty );

            // TODO: SMS esta desabilitado

            TwilioClient.Init(_settings.Value.TwilioAccountSID, _settings.Value.TwilioAuthToken);

            var smsMessage = await MessageResource.CreateAsync(
                to: new PhoneNumber(number),
                from: new PhoneNumber(_settings.Value.TwilioPhoneNumber),
                body: message);
        }
    }
}
