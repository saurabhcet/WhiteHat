using Azure;
using Azure.Communication.Email;
using Azure.Communication.Email.Models;
using WhiteHat.API.Model;

namespace WhiteHat.API.Email
{
    public class EmailSender : IEmailSender
    {
        string connectionString = @"endpoint=https://bot-framework-communication-resource.communication.azure.com/;accesskey=VOO8D+jwocyGSmYaueFl2JBzZsIqDN8oZMu9hw5P6nFrcClFmpnUvqVtMdC7JUpQwATIZIKXL74gCsN3stNsMg==";
        string senderMailAddr = @"donotreply@2a497475-2a9d-43f3-9c0c-a8b07706d60e.azurecomm.net";
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(ILogger<EmailSender> logger)
        {
            _logger = logger;
        }

        public bool SendTestMail(string subject)
        {

            EmailClient emailClient = new EmailClient(connectionString);


            //Replace with your domain and modify the content, recipient details as required

            EmailContent emailContent = new EmailContent($"{subject}");
            emailContent.PlainText = "Minutes of meeting test mail content";
            List<EmailAddress> emailAddresses = new List<EmailAddress> { new EmailAddress("vishal.gawai@gmail.com") { DisplayName = "Vishal Gawai" } };
            EmailRecipients emailRecipients = new EmailRecipients(emailAddresses);
            EmailMessage emailMessage = new EmailMessage(senderMailAddr, emailContent, emailRecipients);
            //var emailResult = emailClient.SendAsync(emailMessage, CancellationToken.None);

            SendEmailResult emailResult = emailClient.Send(emailMessage, CancellationToken.None);

            _logger.LogInformation($"email messageId - {emailResult.MessageId}");

            //Getting the status of the email

            Response<SendStatusResult> messageStatus = null;
            messageStatus = emailClient.GetSendStatus(emailResult.MessageId);
            _logger.LogInformation($"MessageStatus = {messageStatus.Value.Status}");
            TimeSpan duration = TimeSpan.FromMinutes(3);
            long start = DateTime.Now.Ticks;
            do
            {
                messageStatus = emailClient.GetSendStatus(emailResult.MessageId);
                if (messageStatus.Value.Status != SendStatus.Queued)
                {
                    _logger.LogInformation($"MessageStatus = {messageStatus.Value.Status}");
                    break;
                }
                Thread.Sleep(10000);
               _logger.LogInformation($"...");

            } while (DateTime.Now.Ticks - start < duration.Ticks);

            return true;

        }

        public bool SendMail(Minute mom)
        {
            try
            {
                EmailClient emailClient = new EmailClient(connectionString);
                //Replace with your domain and modify the content, recipient details as required


                EmailContent emailContent = new EmailContent($"{mom.Subject}");
                emailContent.PlainText = GetEmailBody(mom);
                var emailRecipients = GetEmailRecipients(mom);
                if (emailRecipients == null) throw new Exception($"Email recipient list is emppty");

                EmailMessage emailMessage = new EmailMessage(senderMailAddr, emailContent, emailRecipients);
                var emailResult = emailClient.SendAsync(emailMessage, CancellationToken.None);


                //SendEmailResult emailResult = emailClient.Send(emailMessage, CancellationToken.None);

                //_logger.LogInformation($"email messageId - {emailResult.MessageId}");

                ////Getting the status of the email

                //Response<SendStatusResult> messageStatus = null;
                //messageStatus = emailClient.GetSendStatus(emailResult.MessageId);
                //_logger.LogInformation($"MessageStatus = {messageStatus.Value.Status}");
                //TimeSpan duration = TimeSpan.FromMinutes(3);
                //long start = DateTime.Now.Ticks;
                //do
                //{
                //    messageStatus = emailClient.GetSendStatus(emailResult.MessageId);
                //    if (messageStatus.Value.Status != SendStatus.Queued)
                //    {
                //        _logger.LogInformation($"MessageStatus = {messageStatus.Value.Status}");
                //        break;
                //    }
                //    Thread.Sleep(10000);
                //    _logger.LogInformation($"...");

                //} while (DateTime.Now.Ticks - start < duration.Ticks);


                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message, null);
                throw;
            }
        }

        private string GetEmailBody(Minute mom)
        {
            string startLine = "Following are the minutes of meeting: \n";
            if (mom == null) return string.Empty;
            var msg = startLine + string.Join("\n", mom.Notes);
            return msg;
        }

        private EmailRecipients GetEmailRecipients(Minute mom)
        {
            if (mom == null || string.IsNullOrWhiteSpace(mom.ParticipantsColSeparated)) return null;

            var participants = mom.ParticipantsColSeparated.Split(';').ToList();

            var emailAddressList = new List<EmailAddress>();
            foreach (var participant in participants)
            {
                var emailAddr = new EmailAddress(participant);
                emailAddressList.Add(emailAddr);
            }

            EmailRecipients emailRecipients = new EmailRecipients(emailAddressList);
            return emailRecipients;
        }
    }
}
