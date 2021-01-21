using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using SendGrid;
using SendGrid.Helpers.Mail;
using Zeats.Legacy.Email.Anexos;
using Zeats.Legacy.Email.SendGrid.Helpers;
using Zeats.Legacy.Email.SendGrid.Models;
using Zeats.Legacy.Email.Servicos;

namespace Zeats.Legacy.Email.SendGrid.Servicos
{
    public class ServicoEmailSendGrid : ServicoEmail
    {
        private readonly ConfiguracaoSendGrid _configuracaoSendGrid;

        public ServicoEmailSendGrid(ConfiguracaoSendGrid configuracaoSendGrid)
        {
            _configuracaoSendGrid = configuracaoSendGrid;
        }

        public override void EnviarEmail(string assunto, string corpo, List<string> destinatarios, List<Anexo> anexos = null)
        {
            var sendGridClient = new SendGridClient(_configuracaoSendGrid.ApiKey);
            var from = new EmailAddress(_configuracaoSendGrid.Email, _configuracaoSendGrid.Name);
            var tos = destinatarios.Select(destinatario => new EmailAddress(destinatario)).ToList();

            var message = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, assunto, corpo, corpo);

            if (anexos?.Any() ?? false)
            {
                message.Attachments = anexos.Select(anexo => new Attachment
                {
                    Content = Convert.ToBase64String(anexo.Conteudo.ReadFully()),
                    Filename = anexo.Nome,
                    Type = anexo.Tipo,
                    Disposition = "attachment"
                }).ToList();
            }

            var response = sendGridClient.SendEmailAsync(message).Result;

            if (response.StatusCode != HttpStatusCode.OK
                && response.StatusCode != HttpStatusCode.Accepted)
                throw new Exception($"{response.StatusCode} - {response.Body.ReadAsStringAsync().Result}");
        }
    }
}