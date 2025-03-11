#region

using FluentResults;
using zora.Core.Enums;

#endregion

namespace zora.Core.Utilities;

public static class ResultUtilities
{
    public static bool IsValidationError<T>(Result<T> result, ErrorType errorType)
    {
        if (result.Errors.Any(e => e.Metadata.ContainsKey(Constants.REASON) &&
                                   (ErrorType)e.Metadata[Constants.REASON] == errorType))
        {
            return true;
        }

        return false;
    }
}
