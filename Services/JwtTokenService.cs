using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MeetingManagement.Config;
using MeetingManagement.Data.Context;
using MeetingManagement.Interface.IService;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace MeetingManagement.Service.Jwt;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtConfig _config;
    private ApplicationDbContext _context;
    public JwtTokenService(JwtConfig config, ApplicationDbContext context)
    {
        _config = config;
        _context = context;
    }

    // account Id, Username, 
    public async Task<string> GenerateAccessToken(string Id, string Username, string UserId)
    {
        var permissions = await _context.Permission.Where(x => x.UserId == UserId).ToListAsync();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, Id), 
            new Claim(ClaimTypes.Name, Username),
            new Claim("UserId", UserId)
        };
        var permissionList = new List<string>();

        foreach (var p in permissions)
        {
            var controller = p.Controller ?? "*";
            var action = string.IsNullOrEmpty(p.Action) ? "*" : p.Action;

            if (p.FullPermission)
            {
                permissionList.Add($"{controller}.*");
                continue;
            }

            if (p.View)
                permissionList.Add($"{controller}.{action}.View");
            if (p.Insert)
                permissionList.Add($"{controller}.{action}.Insert");
            if (p.Edit)
                permissionList.Add($"{controller}.{action}.Edit");
            if (p.Delete)
                permissionList.Add($"{controller}.{action}.Delete");
            if (p.InsertAll)
                permissionList.Add($"{controller}.{action}.InsertAll");
            if (p.EditAll)
                permissionList.Add($"{controller}.{action}.EditAll");
            if (p.DeleteAll)
                permissionList.Add($"{controller}.{action}.DeleteAll");
        }
        claims.Add(new Claim(
            "Permissions",
            JsonSerializer.Serialize(permissionList)
        ));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config.Issuer,
            audience: _config.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_config.AccessTokenExpirationMinutes),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}