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

public class AddEmailMailable(Config config, AddEmailMailableView view)
    : Mailable<AddEmailMailableView>
{
    public override void Build()
    {
        To(view.To).From(config.EmailAuth.From!).View("~/Views/Mail/AddEmail.cshtml", view);
    }
}

public class AddEmailMailableView : BaseView
{
    public required string Code { get; init; }
    public required string Username { get; init; }
}
