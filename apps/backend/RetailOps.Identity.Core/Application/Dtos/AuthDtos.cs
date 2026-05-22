namespace RetailOps.Identity.Core.Application.Dtos;

public sealed record AuthenticateUserInDto(string Login, string Password);

public sealed record AuthTokenOutDto(
    string AccessToken,
    DateTime ExpiresAt,
    Guid UserId,
    int TenantId,
    string UserLevel,
    IReadOnlyList<string> PermissionKeys);

public sealed record CurrentUserOutDto(
    Guid Id,
    int LegacyUserId,
    int TenantId,
    string Name,
    string? Email,
    string UserLevel,
    IReadOnlyList<string> PermissionKeys);

public sealed record RegisterUserInDto(string Name, string Email, string Password, int TenantId);

public sealed record AssignPermissionsInDto(Guid UserId, IReadOnlyList<string> PermissionKeys);

public sealed record PermissionCatalogItemDto(string Key, string Name, string GroupName);

public sealed record UserListItemDto(
    Guid Id,
    int LegacyUserId,
    string Name,
    string? Email,
    string UserLevel,
    IReadOnlyList<string> PermissionKeys);

public sealed record VerifyManagerPinInDto(string Pin);

public sealed record VerifyManagerPinCommand(Guid UserId, string Pin);
