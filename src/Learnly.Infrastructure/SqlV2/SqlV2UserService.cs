using Learnly.Application.SqlV2;
using Learnly.Application.SqlV2.Dtos;
using Learnly.Domain.Entities;
using Learnly.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Infrastructure.SqlV2;

public sealed class SqlV2UserService : ISqlV2UserService
{
    private readonly LearnlyDbContext _db;

    public SqlV2UserService(LearnlyDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<SqlUserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _db.DbUsers
            .AsNoTracking()
            .OrderBy(x => x.UserId)
            .Select(x => new SqlUserDto(x.UserId, x.RoleId, x.Email, x.Name, x.Surname))
            .ToListAsync(cancellationToken);
    }

    public async Task<(SqlUserDto? user, string? error)> CreateUserAsync(CreateSqlUserRequest request, CancellationToken cancellationToken = default)
    {
        var roleExists = await _db.DbRoles.AnyAsync(x => x.RoleId == request.RoleId, cancellationToken);
        if (!roleExists)
        {
            return (null, "Role does not exist.");
        }

        var emailExists = await _db.DbUsers.AnyAsync(x => x.Email == request.Email, cancellationToken);
        if (emailExists)
        {
            return (null, "Email already exists.");
        }

        var user = new DbUser
        {
            RoleId = request.RoleId,
            Email = request.Email.Trim(),
            Name = request.Name.Trim(),
            Surname = request.Surname.Trim(),
            Password = request.Password
        };

        _db.DbUsers.Add(user);
        await _db.SaveChangesAsync(cancellationToken);
        return (new SqlUserDto(user.UserId, user.RoleId, user.Email, user.Name, user.Surname), null);
    }
}
