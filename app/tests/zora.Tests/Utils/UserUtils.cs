#region

using zora.Core.Domain;

#endregion

namespace zora.Tests.Utils;

public static class UserUtils
{
    public static IEnumerable<User> GetValidUsers()
    {
        List<User> users = new List<User>
        {
            new()
            {
                Id = 1,
                Username = "john.doe",
                Email = "john.doe@example.com",
                Password = "password"
            },
            new()
            {
                Id = 2,
                Username = "jane.doe",
                Email = "jane.doe@example.com",
                Password = "password"
            },
            new()
            {
                Id = 3,
                Username = "jim.doe",
                Email = "jim.doe@example.com",
                Password = "password"
            }
        };

        return users;
    }
}
