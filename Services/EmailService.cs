using InventarioWEB.Configurations;
using Microsoft.Extensions.Options;

using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace InventarioWEB.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;


        public EmailService(
            IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }


        public async Task EnviarCorreoAsync(
            string destinatario,
            string asunto,
            string mensaje,
            byte[] archivoAdjunto,
            string nombreArchivo)
        {

            var email = new MimeMessage();


            // =====================================
            // REMITENTE
            // =====================================

            email.From.Add(
                new MailboxAddress(
                    _settings.Name,
                    _settings.From));


            // =====================================
            // DESTINATARIO
            // =====================================

            email.To.Add(
                MailboxAddress.Parse(destinatario));


            // =====================================
            // ASUNTO
            // =====================================

            email.Subject = asunto;



            // =====================================
            // CUERPO + ADJUNTO
            // =====================================

            var cuerpo = new BodyBuilder
            {
                TextBody = mensaje
            };


            cuerpo.Attachments.Add(
                nombreArchivo,
                archivoAdjunto,
                ContentType.Parse("application/pdf"));


            email.Body = cuerpo.ToMessageBody();



            // =====================================
            // ENVÍO SMTP GMAIL
            // =====================================

            using var smtp = new SmtpClient();


            await smtp.ConnectAsync(
                _settings.Host,
                _settings.Port,
                SecureSocketOptions.StartTls);


            await smtp.AuthenticateAsync(
                _settings.User,
                _settings.Password);


            await smtp.SendAsync(email);


            await smtp.DisconnectAsync(true);
        }
    }
}