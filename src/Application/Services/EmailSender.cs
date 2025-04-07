using Application.Interfaces;
using Domain.Entities;
using Domain.Models;
using Domain.Models.Envelope;
using Domain.Settings;
using Domain.Utils;
using Microsoft.AspNetCore.DataProtection;
using System.Net;
using System.Net.Mail;

namespace Application.Services
{
    class EmailSender : IEmailSender
    {
        private string _linkConfirmation;

        private readonly string _fromAddress;
        private readonly IDataProtector _protector;
        private readonly string? _host;
        private readonly string? _sender;
        private readonly string? _phaseKey;

        public EmailSender(EnvirolmentVariables envirolment, IDataProtectionProvider protector)
        {
            _host = envirolment.EMAIL_SMTP_CLIENT!;
            _sender = envirolment.EMAIL_FROM_ADDRESS!;
            _phaseKey = envirolment.EMAIL_FROM_PASSWORD!;
            _fromAddress = envirolment.EMAIL_FROM_ADDRESS!;

            _protector = protector.CreateProtector(envirolment.JWTSETTINGS_ISSUER!);
            _linkConfirmation = "https://lfauthdevhub.up.railway.app/confirm-subscription/{0}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userQueueRegister"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<IResponse<bool>> SendConfirmationEmailAsync(UserQueueRegister userQueueRegister, string type)
        {
            string tokenData = EncriptorAndDecriptor.TokenGenAndEncprtor(userQueueRegister.Email, userQueueRegister.Name!, _protector);
            _linkConfirmation = string.Format(_linkConfirmation, tokenData);

            string body = SelectBodyByType(type, userQueueRegister.Name!);
            string title = type.Equals("Delete") ? "Account Deletion Confirmation" : "Registration Confirmation";

            using (var mensagem = new MailMessage(_fromAddress, userQueueRegister.Email, $"LFauth Dev Hub | {title}", body))
            {
                try
                {
                    var smtpClient = new SmtpClient(_host, 587)
                    {
                        Credentials = new NetworkCredential(_sender, _phaseKey),
                        EnableSsl = true
                    };

                    mensagem.IsBodyHtml = true;

                    smtpClient.Send(mensagem);

                    return new ResponseOk<bool>(true);
                }
                catch (Exception ex)
                {
                    return new ResponseError<bool>(new ResponseModel { Code = "ES503", Message = $"{ex.Message} | {ex.InnerException?.Message}" });
                }
            }
        }

        private string SelectBodyByType(string type, string name)
        {
            if (type.Equals("Delete"))
            {
                return string.Format(@"<!DOCTYPE html>
<html>
  <head>
    <meta charset=""utf-8"" />
    <title>Record Deletion Confirmation</title>
  </head>
  <body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 460px;"">
    <p>Hello {0},</p>
    <p>I would like to confirm that your account on the LFauth Dev Hub website has been successfully deleted.</p>
    <p>Your account has been completely removed, including all data related to interactions with the site's features.</p>
    <p>
      To register again, please visit our website and sign up for a new account.
    </p>
    <p>
      Sincerely,<br /><br /><br />
      Luan Faith | LFauth Dev Hub
    </p>
  </body>
</html>

", name);
            }
            else
            {
                return string.Format(@"<!DOCTYPE html>
<html>
  <head>
    <meta charset=""utf-8"" />
    <title>Registration Confirmation</title>
  </head>
  <body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 460px;"">
    <p>Hello {0},</p>
    <p>Welcome to <strong>LFauth Dev Hub</strong>! We are thrilled to have you with us.</p>
    <p>To complete your registration and activate your account, please click the button below:</p>
    <br>
    <p style=""text-align: center;"">
      <a href=""{1}"" style=""background-color: #4CAF50; color: #fff; padding: 12px 20px; text-decoration: none; border-radius: 4px; font-weight: bold;"">Confirm Registration</a>
    </p>
    <br>
    <p>If the button above does not work, copy and paste the following link into your browser:</p>
    <p><a href=""{1}"">{1}</a></p>
    <p>If you did not initiate this registration, please disregard this email.</p>
    <p>Sincerely,<br /><br /><br />
       Luan Faith | LFauth Dev Hub
    </p>
  </body>
</html>

", name, _linkConfirmation);
            }
        }
    }
}