using System;
using UsersClassLibrary.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Logging;
using System.Security.Claims;
using System.Text;
namespace ResponseUtils
{
    public class JWTGen : IJWTGen
    {

        private String issuer;
        private String audience;
        private SymmetricSecurityKey key;

        public JWTGen(String issuer, String audience, String key)
        {
            this.issuer = issuer;
            this.audience = audience;
            this.key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));
        }

        public string? getUserIdFromToken(String token)
        {
            if(token.ToLower().StartsWith("bearer "))
            {
                var splittedToken = token.Split(" ");
                if(splittedToken.Count() != 0)
                {
                    token = token.Split(" ")[1];
                }
            }
            IdentityModelEventSource.ShowPII = true;

            SecurityToken validatedToken;
            TokenValidationParameters validationParams = new TokenValidationParameters();
            validationParams.ValidateLifetime = true;
            validationParams.ValidAudience = this.audience.ToLower();
            validationParams.ValidIssuer = this.issuer.ToLower();
            validationParams.IssuerSigningKey = this.key;
            ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(token, validationParams, out validatedToken);
            Claim IdUserClaim = principal.FindFirst("IdUser");

            if (IdUserClaim == null)
            {
                return null;
            }
            return IdUserClaim.Value;
        }

        public string generateUserToken(User user)
        {
            if (user == null)
            {
                throw new ArgumentException("Cannot create token of null user.");
            }
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new Claim("IdUser", user.IdUser.ToString()),
                    new Claim("Username", user.Username)

                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = this.issuer,
                Audience = this.audience,
                SigningCredentials = new SigningCredentials(this.key, SecurityAlgorithms.HmacSha512Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var stringToken = tokenHandler.WriteToken(token);
            return stringToken;
        }
    }
}

