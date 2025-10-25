
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjectManager.Api.Data;
using ProjectManager.Api.DTOs;
using ProjectManager.Api.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ProjectManager.Api.Services
{
    public interface IAuthService
    {
        Task<AuthResultDto> Register(RegisterDto dto);
        Task<AuthResultDto> Login(LoginDto dto);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<AuthResultDto> Register(RegisterDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            var name = dto.Name.Trim();

            if (await _db.Users.AnyAsync(u => u.Email == email))
                throw new Exception("Email already exists");

            using var hmac = new HMACSHA512();
            var user = new User
            {
                DisplayName = name,
                Email = email,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password)),
                PasswordSalt = hmac.Key
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var token = GenerateToken(user);
            return new AuthResultDto(token, user.DisplayName, user.Email);
        }

        public async Task<AuthResultDto> Login(LoginDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null) throw new Exception("Invalid credentials");

            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.Password));
            if (!computed.SequenceEqual(user.PasswordHash)) throw new Exception("Invalid credentials");

            var token = GenerateToken(user);
            return new AuthResultDto(token, user.DisplayName, user.Email);
        }

        private string GenerateToken(User user)
        {
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"] ?? "DevSecretKeyChangeMe");
            var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.DisplayName),
                new Claim(ClaimTypes.Email, user.Email)
            };
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"] ?? "projectmanager.local",
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
