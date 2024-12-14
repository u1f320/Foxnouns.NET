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

namespace Foxnouns.Backend.Mailables;

public class PasswordChangedMailable(Config config, PasswordChangedMailableView view)
    : Mailable<PasswordChangedMailableView>
{
    private string PlainText() =>
        $"""
            Your password has been changed using a "forgot password" link.
            If this wasn't you, request a password reset immediately:
            {view.BaseUrl}/auth/forgot-password
            """;

    public override void Build()
    {
        To(view.To)
            .From(config.EmailAuth.From!)
            .Subject("Your password has been changed")
            .View("~/Views/Mail/PasswordChanged.cshtml", view)
            .Text(PlainText());
    }
}

public class PasswordChangedMailableView : BaseView;
