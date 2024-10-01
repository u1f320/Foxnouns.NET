using Coravel.Mailer.Mail.Interfaces;
using Coravel.Queuing.Interfaces;
using Foxnouns.Backend.Mailables;

namespace Foxnouns.Backend.Services;

public class MailService(ILogger logger, IMailer mailer, IQueue queue, Config config)
{
    private readonly ILogger _logger = logger.ForContext<MailService>();

    public void QueueAccountCreationEmail(string to, string code)
    {
        queue.QueueAsyncTask(async () =>
        {
            _logger.Debug("Sending account creation email to {ToEmail}", to);
            try
            {
                await mailer.SendAsync(
                    new AccountCreationMailable(
                        config,
                        new AccountCreationMailableView
                        {
                            BaseUrl = config.BaseUrl,
                            To = to,
                            Code = code,
                        }
                    )
                );
            }
            catch (Exception exc)
            {
                _logger.Error(exc, "Sending account creation email");
            }
        });
    }

    public void QueueAddEmailAddressEmail(string to, string code, string username)
    {
        _logger.Debug("Sending add email address email to {ToEmail}", to);
        queue.QueueAsyncTask(async () =>
        {
            try
            {
                await mailer.SendAsync(
                    new AddEmailMailable(
                        config,
                        new AddEmailMailableView
                        {
                            BaseUrl = config.BaseUrl,
                            To = to,
                            Code = code,
                            Username = username,
                        }
                    )
                );
            }
            catch (Exception exc)
            {
                _logger.Error(exc, "Sending add email address email");
            }
        });
    }
}
