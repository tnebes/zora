#region

using FluentResults;
using Moq;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;

#endregion

namespace zora.Tests.TestFixtures;

internal static class UserRepositoryMockBuilder
{
    internal static void SetupUserRepository(MockedRepositoryFixture fixture)
    {
        fixture.MockUserRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<long>(), It.IsAny<bool>()))
            .ReturnsAsync((long id, bool includeProperties) =>
            {
                User? user = fixture.Users.FirstOrDefault(u => u.Id == id);
                return user != null ? Result.Ok(user) : Result.Fail<User>(new Error("User not found"));
            });

        fixture.MockUserRepository
            .Setup(repo => repo.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync((string username, bool includeProperties) =>
            {
                User? user = fixture.Users.FirstOrDefault(u => u.Username == username);
                return user != null ? Result.Ok(user) : Result.Fail<User>(new Error("User not found"));
            });

        fixture.MockUserRepository
            .Setup(repo => repo.GetByEmailAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync((string email, bool includeProperties) =>
            {
                User? user = fixture.Users.FirstOrDefault(u => u.Email == email);
                return user != null ? Result.Ok(user) : Result.Fail<User>(new Error("User not found"));
            });

        fixture.MockUserRepository
            .Setup(repo => repo.GetUsersAsync(It.IsAny<QueryParamsDto>(), It.IsAny<bool>()))
            .ReturnsAsync((QueryParamsDto queryParams, bool includeProperties) =>
            {
                int skip = (queryParams.Page - 1) * queryParams.PageSize;
                IEnumerable<User> pagedUsers = fixture.Users.Skip(skip).Take(queryParams.PageSize);
                return (pagedUsers, fixture.Users.Count());
            });

        fixture.MockUserRepository
            .Setup(repo => repo.IsUsernameUniqueAsync(It.IsAny<string>()))
            .ReturnsAsync((string username) => !fixture.Users.Any(u => u.Username == username));

        fixture.MockUserRepository
            .Setup(repo => repo.IsEmailUniqueAsync(It.IsAny<string>()))
            .ReturnsAsync((string email) => !fixture.Users.Any(u => u.Email == email));

        fixture.MockUserRepository
            .Setup(repo => repo.Add(It.IsAny<User>()))
            .ReturnsAsync((User user) =>
            {
                long newId = fixture.Users.Any() ? fixture.Users.Max(u => u.Id) + 1 : 1;
                user.Id = newId;
                fixture.Users.Add(user);
                return user;
            });

        fixture.MockUserRepository
            .Setup(repo => repo.Update(It.IsAny<User>()))
            .ReturnsAsync((User user) =>
            {
                int index = fixture.Users.FindIndex(u => u.Id == user.Id);
                if (index >= 0)
                {
                    fixture.Users[index] = user;
                }

                return Result.Ok(user);
            });

        fixture.MockUserRepository
            .Setup(repo => repo.SoftDelete(It.IsAny<User>()))
            .Callback((User user) =>
            {
                User? existingUser = fixture.Users.FirstOrDefault(u => u.Id == user.Id);
                if (existingUser != null)
                {
                    fixture.Users.Remove(existingUser);
                }
            });
    }
}
