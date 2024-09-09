using Coravel.Mailer.Mail.Interfaces;
using Coravel.Queuing.Interfaces;
using Foxnouns.Backend.Mailables;

namespace Foxnouns.Backend.Services;

public class MailService(ILogger logger, IMailer mailer, IQueue queue, Config config)
{
    private readonly ILogger _logger = logger.ForContext<MailService>();

    public void QueueAccountCreationEmail(string to, string code)
    {
        _logger.Debug("Sending account creation email to {ToEmail}", to);
        
        queue.QueueAsyncTask(async () =>
        {
            await mailer.SendAsync(new AccountCreationMailable(config, new AccountCreationMailableView
            {
                To = to,
                Code = code
            }));
        });
    }
}