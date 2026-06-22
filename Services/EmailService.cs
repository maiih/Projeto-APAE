using System.Net;
using System.Net.Mail;

namespace APAEApplication.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var host = _configuration["SmtpSettings:Host"];
        var portValue = _configuration["SmtpSettings:Port"];
        var user = _configuration["SmtpSettings:Email"];
        var password = _configuration["SmtpSettings:Password"];

        if (string.IsNullOrWhiteSpace(host))
            throw new InvalidOperationException("SMTP host não configurado.");
        if (string.IsNullOrWhiteSpace(portValue) || !int.TryParse(portValue, out var port))
            throw new InvalidOperationException("SMTP port inválida.");
        if (string.IsNullOrWhiteSpace(user))
            throw new InvalidOperationException("E-mail SMTP não configurado.");
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Senha de app SMTP não configurada.");

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(user, password)
        };

        using var message = new MailMessage
        {
            From = new MailAddress(user),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        message.To.Add(to);

        await client.SendMailAsync(message);
    }
}
