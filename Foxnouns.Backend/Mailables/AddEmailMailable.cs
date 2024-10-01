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
