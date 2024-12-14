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

public class ResetPasswordMailable(Config config, ResetPasswordMailableView view)
    : Mailable<ResetPasswordMailableView>
{
    private string PlainText() =>
        $"""
            Somebody (hopefully you!) has requested a password reset.
            You can use the following link to do this:
            {view.BaseUrl}/auth/forgot-password/{view.Code}
            Note that this link will expire in one hour.

            If you weren't expecting this email, you don't have to do anything.
            Your password can't be changed without the above link.
            """;

    public override void Build()
    {
        To(view.To)
            .From(config.EmailAuth.From!)
            .Subject("Reset your account's password")
            .View("~/Views/Mail/ResetPassword.cshtml", view)
            .Text(PlainText());
    }
}

public class ResetPasswordMailableView : BaseView
{
    public required string Code { get; init; }
}
