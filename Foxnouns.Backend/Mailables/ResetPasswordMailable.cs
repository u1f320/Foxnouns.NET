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
