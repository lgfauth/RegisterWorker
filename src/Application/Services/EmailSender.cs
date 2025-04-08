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
        /// Send email to user with confirmation link
        /// </summary>
        /// <param name="userQueueRegister"></param>
        /// <returns></returns>
        public async Task<IResponse<bool>> SendSubscriptionConfirmationEmailAsync(UserQueueRegister userQueueRegister)
        {
            string tokenData = EncriptorAndDecriptor.TokenGenAndEncprtor(userQueueRegister.Email, userQueueRegister.Name!, _protector);
            string body = BuildEmailTemplate(userQueueRegister.Name!, _linkConfirmation, "pt");

            _linkConfirmation = string.Format(_linkConfirmation, tokenData);

            try
            {
                bool response = await SendEmailAsync(userQueueRegister.Email, "LFauth Dev Hub | Registration Confirmation", body);

                return new ResponseOk<bool>(response);
            }
            catch (Exception ex)
            {
                return new ResponseError<bool>(new ResponseModel { Code = "ES503", Message = $"{ex.Message} | {ex.InnerException?.Message}" });
            }
        }

        /// <summary>
        /// Send email to user for confirmation of account deletion
        /// </summary>
        /// <param name="userQueueRegister"></param>
        /// <returns></returns>
        public async Task<IResponse<bool>> SendDeletionConfirmationEmailAsync(UserQueueRegister userQueueRegister)
        {
            string tokenData = EncriptorAndDecriptor.TokenGenAndEncprtor(userQueueRegister.Email, userQueueRegister.Name!, _protector);
            string body = BuildEmailTemplate(userQueueRegister.Name!, _linkConfirmation, "pt");

            _linkConfirmation = string.Format(_linkConfirmation, tokenData);

            try
            {
                bool response = await SendEmailAsync(userQueueRegister.Email, "LFauth Dev Hub | Account Deletion Confirmation", body);

                return new ResponseOk<bool>(response);
            }
            catch (Exception ex)
            {
                return new ResponseError<bool>(new ResponseModel { Code = "ES503", Message = $"{ex.Message} | {ex.InnerException?.Message}" });
            }
        }

        /// <summary>
        /// Send email using SMTP client
        /// </summary>
        /// <returns></returns>
        private async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            
            using (var mensagem = new MailMessage(_fromAddress, to, subject, body))
            {
                var smtpClient = new SmtpClient(_host, 587)
                {
                    Credentials = new NetworkCredential(_sender, _phaseKey),
                    EnableSsl = true
                };

                mensagem.IsBodyHtml = true;
                await smtpClient.SendMailAsync(mensagem);

                return true;
            }
        }

        private string BuildEmailTemplate(string nome, string link, string idioma)
        {
            string titulo, saudacao, mensagem, textoBotao, instrucaoFallback, aviso;

            if (idioma.ToLower() == "pt")
            {
                titulo = "Confirmação de Cadastro";
                saudacao = $"Olá {nome},";
                mensagem = "Seja bem-vindo ao <strong>LFauth Dev Hub</strong>! Estamos felizes em ter você conosco.<br>Para concluir seu cadastro e ativar sua conta, clique no botão abaixo:";
                textoBotao = "Confirmar Cadastro";
                instrucaoFallback = "Se o botão acima não funcionar, copie e cole o link abaixo no seu navegador:";
                aviso = "Se você não solicitou este cadastro, por favor ignore este e-mail.";
            }
            else
            {
                titulo = "Registration Confirmation";
                saudacao = $"Hello {nome},";
                mensagem = "Welcome to <strong>LFauth Dev Hub</strong>! We are thrilled to have you with us.<br>To complete your registration and activate your account, please click the button below:";
                textoBotao = "Confirm Registration";
                instrucaoFallback = "If the button above does not work, copy and paste the following link into your browser:";
                aviso = "If you did not initiate this registration, please disregard this email.";
            }

            return $@"
<!DOCTYPE html>
<html lang=""{idioma}"">
  <head>
    <meta charset=""UTF-8"" />
    <title>{titulo}</title>
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
  </head>
  <body style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;"">
    <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
      <tr>
        <td align=""center"">
          <table border=""0"" cellpadding=""20"" cellspacing=""0"" width=""600"" style=""background-color: #ffffff; border-radius: 8px; box-shadow: 0 0 5px rgba(0,0,0,0.1);"">
            <tr>
              <td align=""center"" style=""padding-bottom: 0;"">
                <h2 style=""margin: 0; color: #333;"">LFauth Dev Hub</h2>
              </td>
            </tr>
            <tr>
              <td style=""padding-top: 0;"">
                <p style=""font-size: 16px; color: #333;"">{saudacao}</p>
                <p style=""font-size: 16px; color: #333;"">{mensagem}</p>
                <p style=""text-align: center; margin: 30px 0;"">
                  <a href=""{link}"" style=""background-color: #4CAF50; color: white; padding: 14px 24px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;"">{textoBotao}</a>
                </p>
                <p style=""font-size: 14px; color: #555;"">{instrucaoFallback}</p>
                <p style=""word-break: break-all;""><a href=""{link}"" style=""color: #3366cc;"">{link}</a></p>
                <p style=""font-size: 14px; color: #777; margin-top: 40px;"">{aviso}</p>
                <p style=""font-size: 14px; color: #555;"">Sincerely,<br><strong>Luan Faith | LFauth Dev Hub</strong></p>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </body>
</html>
";
        }
    }
}