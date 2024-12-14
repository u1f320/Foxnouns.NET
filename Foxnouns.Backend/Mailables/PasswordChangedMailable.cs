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
