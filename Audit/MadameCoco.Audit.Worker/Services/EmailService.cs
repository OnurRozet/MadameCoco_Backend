using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MadameCoco.Audit.Worker.Interfaces;
using MadameCoco.Audit.Worker.Models;
using Microsoft.Extensions.Options;
using MadameCoco.Audit.Worker.Entities;
using System.Text;

namespace MadameCoco.Audit.Worker.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SmtpSettings> smtpSettings, ILogger<EmailService> logger)
    {
        _smtpSettings = smtpSettings.Value;
        _logger = logger;
    }

    public (string Subject, string Body) FormatReport(List<OrderLog> logs, DateTime reportDate)
    {
        var totalCount = logs.Count;
        var totalRevenue = logs.Sum(l => l.TotalPrice);

        var subject = $"Madame Coco Denetim Raporu - {reportDate:dd/MM/yyyy HH:mm}";

        // --- HTML Başlangıcı ve Özet ---
        var htmlBuilder = new StringBuilder();
        htmlBuilder.Append($"""
            <h1>Madame Coco Denetim Raporu (Son 10 Dakika)</h1>
            <p>Merhaba Yönetici,</p>
            <p>Son periyotta toplam <b>{totalCount}</b> adet yeni sipariş olayı gerçekleşmiştir.</p>
            <p>Tahmini Toplam Ciro: <b>{totalRevenue:N2} TL</b></p>
            <hr/>
            <h2>Detaylı İşlem Listesi</h2>
        """);

        // --- HTML Tablo Başlangıcı ---
        htmlBuilder.Append("<table border='1' cellpadding='10' cellspacing='0' style='width:100%; border-collapse: collapse;'>");

        // Tablo Başlıkları (Headers)
        htmlBuilder.Append("""
            <tr style='background-color: #f2f2f2;'>
                <th>Sipariş ID</th>
                <th>Müşteri ID</th>
                <th>Ürün ID</th>
                <th>Ürün Adı</th>
                <th>Olay Tipi</th>
                <th>Adet</th>
                <th>Toplam Tutar</th>
                <th>Oluşturulma Zamanı (UTC)</th>
            </tr>
        """);

        // --- Tablo Gövdesi (Data) ---
        foreach (var log in logs)
        {
            htmlBuilder.Append($"""
                <tr>
                    <td>{log.OrderId}</td>
                    <td>{log.CustomerId}</td>
                    <td>{log.ProductId}</td>
                    <td>{log.ProductName}</td>
                    <td>{log.EventType}</td>
                    <td>{log.Quantity}</td>
                    <td>{log.TotalPrice:N2} TL</td>
                    <td>{log.CreatedAt:yyyy-MM-dd HH:mm:ss}</td>
                </tr>
            """);
        }

        // --- HTML Kapanış ---
        htmlBuilder.Append("</table>");
        htmlBuilder.Append("<p>Bu rapor, Hangfire servisi tarafından otomatik olarak oluşturulmuştur.</p>");

        return (subject, htmlBuilder.ToString());
    }

    public async Task SendEmailAsync(string subject, string body)
    {
        try
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_smtpSettings.SenderEmail);
            email.To.Add(MailboxAddress.Parse(_smtpSettings.RecipientEmail));
            email.Subject = subject;

            // Mail gövdesini HTML olarak ayarlayalım
            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation($"[SMTP] '{subject}' konulu e-posta başarıyla gönderildi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SMTP HATA] E-posta gönderilirken bir hata oluştu.");
            // Hatanın Hangfire tarafından tekrar denenmesi için fırlatılması önerilir.
            throw;
        }
    }
}