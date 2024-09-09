using Coravel.Mailer.Mail;

namespace Foxnouns.Backend.Mailables;

public class AccountCreationMailable(Config config, AccountCreationMailableView view) : Mailable<AccountCreationMailableView>
{
    public override void Build()
    {
        To(view.To)
            .From(config.EmailAuth.From!)
            .View("~/Views/Mail/AccountCreation.cshtml", view);
    }
}

public class AccountCreationMailableView
{
    public required string To { get; init; }
    public required string Code { get; init; }
}