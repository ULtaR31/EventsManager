﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EventManagement.Application.Services;

public class JwtService:IJwtService
{
    private readonly string _key;
    
    public JwtService(IConfiguration config)
    {
        _key = config["Jwt:Key"];
    }
    
    public string GenerateJwtToken(string email)
    {

        var Claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, email),
            new Claim(ClaimTypes.Role, "Participant")
        };
        var staticKey = _key;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(staticKey));
        var signingCred = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

        var securityToken = new JwtSecurityToken(
            claims:Claims,
            expires:DateTime.Now.AddSeconds(20),
            signingCredentials:signingCred
            );
        string tokenString = new JwtSecurityTokenHandler().WriteToken(securityToken);
        return tokenString;
    } 
    
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using (var numberGenerator = RandomNumberGenerator.Create())
        {
            numberGenerator.GetBytes(randomNumber);
        }
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal? GetTokenPrincipal(string token)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var validation = new TokenValidationParameters
        {
            IssuerSigningKey = securityKey,
            ValidateLifetime = false,
            ValidateActor = false,
            ValidateIssuer = false,
            ValidateAudience = false,
        };
        return new JwtSecurityTokenHandler().ValidateToken(token, validation, out _);
    }
}