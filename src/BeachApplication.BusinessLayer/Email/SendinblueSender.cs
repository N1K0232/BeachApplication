using BeachApplication.BusinessLayer.Settings;
using FluentEmail.Core;
using FluentEmail.Core.Interfaces;
using FluentEmail.Core.Models;
using Microsoft.Extensions.Options;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;
using TinyHelpers.Extensions;

namespace BeachApplication.BusinessLayer.Email;

public class SendinblueSender : ISender
{
    public SendinblueSender(IOptions<SendinblueSettings> sendinblueSettingsOptions)
    {
        Configuration.Default.ApiKey.TryAdd("api-key", sendinblueSettingsOptions.Value.ApiKey);
    }

    public SendResponse Send(IFluentEmail email, CancellationToken? token = null)
        => SendAsync(email, token).GetAwaiter().GetResult();

    public async Task<SendResponse> SendAsync(IFluentEmail email, CancellationToken? token = null)
    {
        var sender = new TransactionalEmailsApi();
        var response = new SendResponse();

        try
        {
            var message = await CreateSmtpEmailAsync(email);
            var result = await sender.SendTransacEmailAsync(message);

            response.MessageId = result.MessageId;
        }
        catch (Exception ex)
        {
            response.ErrorMessages.Add(ex.Message);
        }

        return response;
    }

    private static async Task<SendSmtpEmail> CreateSmtpEmailAsync(IFluentEmail message)
    {
        var content = message.Data;
        var userName = content.FromAddress.Name.HasValue() ? content.FromAddress.Name : null;
        var sender = new SendSmtpEmailSender(userName, content.FromAddress.EmailAddress);

        var toAddressList = content.ToAddresses.Any() ? content.ToAddresses.Select(a => new SendSmtpEmailTo(a.EmailAddress, a.Name)).ToList() : null;
        var ccAddressList = content.CcAddresses.Any() ? content.CcAddresses.Select(a => new SendSmtpEmailCc(a.EmailAddress, a.Name)).ToList() : null;

        var bccAddressList = content.BccAddresses.Any() ? content.BccAddresses.Select(a => new SendSmtpEmailBcc(a.EmailAddress, a.Name)).ToList() : null;
        var replyToAddress = content.ReplyToAddresses.Any() ? new SendSmtpEmailReplyTo(content.ReplyToAddresses.First().EmailAddress, content.ReplyToAddresses.First().Name) : null;

        var email = new SendSmtpEmail(sender, toAddressList, cc: ccAddressList, bcc: bccAddressList, replyTo: replyToAddress)
        {
            Subject = content.Subject
        };

        if (content.IsHtml)
        {
            email.HtmlContent = content.Body;
            email.TextContent = content.PlaintextAlternativeBody;
        }
        else
        {
            email.TextContent = content.Body;
        }

        if (content.Attachments.Any())
        {
            email.Attachment = [];
            foreach (var attachment in content.Attachments)
            {
                var emailAttachment = await ConvertAttachmentAsync(attachment);
                email.Attachment.Add(emailAttachment);
            }
        }

        return email;
    }

    private static async Task<SendSmtpEmailAttachment> ConvertAttachmentAsync(Attachment attachment)
    {
        var sendinblueAttachment = new SendSmtpEmailAttachment
        {
            Content = await GetAttachmentByteArrayAsync(attachment.Data),
            Name = attachment.Filename
        };

        return sendinblueAttachment;
    }

    private static async Task<byte[]> GetAttachmentByteArrayAsync(Stream stream)
    {
        using var ms = new MemoryStream();

        await stream.CopyToAsync(ms);
        return ms.ToArray();
    }
}