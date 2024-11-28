namespace Foxnouns.Backend.Utils;

/// <summary>
/// Static methods for validating user input (mostly making sure it's not too short or too long)
/// </summary>
public static partial class ValidationUtils
{
    public static void Validate(IEnumerable<(string, ValidationError?)> errors)
    {
        errors = errors.Where(e => e.Item2 != null).ToList();
        if (!errors.Any())
            return;

        var errorDict = new Dictionary<string, IEnumerable<ValidationError>>();
        foreach (var error in errors)
        {
            if (errorDict.TryGetValue(error.Item1, out var value))
                errorDict[error.Item1] = value.Append(error.Item2!);
            errorDict.Add(error.Item1, [error.Item2!]);
        }

        throw new ApiError.BadRequest("Error validating input", errorDict);
    }
}
