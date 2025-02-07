namespace Foxnouns.Backend.Services;

public partial class ValidationService(Config config)
{
    private readonly Config.LimitsConfig _limits = config.Limits;
}
