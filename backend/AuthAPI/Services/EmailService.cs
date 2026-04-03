using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace AuthAPI.Services
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderPassword { get; set; } = string.Empty;
        public bool EnableSSL { get; set; }
    }

    public class EmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IConfiguration configuration)
        {
            _emailSettings = new EmailSettings();
            configuration.GetSection("EmailSettings").Bind(_emailSettings);
        }

        public async Task<bool> SendOtpEmailAsync(string toEmail, string otpCode)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Auth System", _emailSettings.SenderEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = "Password Reset OTP - Authentication System";

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <style>
                            body {{ font-family: 'Segoe UI', Arial, sans-serif; margin: 0; padding: 0; background-color: #f4f4f4; }}
                            .container {{ max-width: 500px; margin: 20px auto; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.1); }}
                            .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; }}
                            .content {{ padding: 30px; }}
                            .otp-code {{ font-size: 36px; font-weight: bold; color: #667eea; text-align: center; padding: 20px; background: #f8f9fa; border-radius: 8px; letter-spacing: 5px; }}
                            .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; border-top: 1px solid #eee; }}
                            .btn {{ display: inline-block; padding: 12px 24px; background: #667eea; color: white; text-decoration: none; border-radius: 5px; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h2>🔐 Password Reset Request</h2>
                            </div>
                            <div class='content'>
                                <p>Hello,</p>
                                <p>We received a request to reset your password. Use the following OTP code to complete the process:</p>
                                <div class='otp-code'>{otpCode}</div>
                                <p>This OTP is valid for <strong>10 minutes</strong>.</p>
                                <p>If you didn't request this, please ignore this email or contact support.</p>
                            </div>
                            <div class='footer'>
                                <p>© 2024 Authentication System. All rights reserved.</p>
                            </div>
                        </div>
                    </body>
                    </html>
                ";

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, 
                    _emailSettings.EnableSSL ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
                await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
                return false;
            }
        }
    }
}