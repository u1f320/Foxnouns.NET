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
