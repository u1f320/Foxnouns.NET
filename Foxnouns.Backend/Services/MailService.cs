// Copyright (C) 2023-present sam/u1f320 (vulpine.solutions)
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
using Coravel.Mailer.Mail;
using Coravel.Mailer.Mail.Interfaces;
using Coravel.Queuing.Interfaces;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Mailables;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Foxnouns.Backend.Services;

public class MailService(
    ILogger logger,
    IMailer mailer,
    IQueue queue,
    IClock clock,
    Config config,
    IServiceProvider serviceProvider
)
{
    private readonly ILogger _logger = logger.ForContext<MailService>();

    public void QueueAccountCreationEmail(string to, string code)
    {
        queue.QueueAsyncTask(async () =>
        {
            await SendEmailAsync(
                to,
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
        });
    }

    public void QueueAddEmailAddressEmail(string to, string code, string username)
    {
        _logger.Debug("Sending add email address email to {ToEmail}", to);
        queue.QueueAsyncTask(async () =>
        {
            await SendEmailAsync(
                to,
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
        });
    }

    private async Task SendEmailAsync<T>(string to, Mailable<T> mailable)
    {
        try
        {
            // ReSharper disable SuggestVarOrType_SimpleTypes
            await using var scope = serviceProvider.CreateAsyncScope();
            await using var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            // ReSharper restore SuggestVarOrType_SimpleTypes

            Instant now = clock.GetCurrentInstant();

            int count = await db.SentEmails.CountAsync(e =>
                e.Email == to && e.SentAt > (now - Duration.FromHours(1))
            );
            if (count >= 2)
            {
                _logger.Information(
                    "Have already sent 2 or more emails to {ToAddress} in the past hour, not sending new email",
                    to
                );
                return;
            }

            await mailer.SendAsync(mailable);

            db.SentEmails.Add(new SentEmail { Email = to, SentAt = now });
            await db.SaveChangesAsync();
        }
        catch (Exception exc)
        {
            _logger.Error(exc, "Sending email");
        }
    }
}
