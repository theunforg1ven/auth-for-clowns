using Microsoft.IdentityModel.Tokens;
using StudyAuthApp.WebApi.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StudyAuthApp.WebApi.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _accessKey;
        private readonly SymmetricSecurityKey _refreshKey;
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
            _accessKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Secrets:JwtAccessKey"]));
            _refreshKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Secrets:JwtRefreshKey"]));
        }

        public string CreateAccessToken(int id)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString())
            };

            var creds = new SigningCredentials(_accessKey, SecurityAlgorithms.HmacSha512Signature);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddSeconds(60),
                SigningCredentials = creds,
                Issuer = _config["Secrets:JwtTokenIssuer"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public string CreateRefreshToken(int id)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString())
            };

            var creds = new SigningCredentials(_refreshKey, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds,
                Issuer = _config["Secrets:JwtTokenIssuer"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public int DecodeToken(string token, out bool hasTokenExpired)
        {
            var jwtToken = new JwtSecurityToken(token);
            var hasValue = int.TryParse(jwtToken.Claims
                                    .FirstOrDefault(claim => claim.Type == "nameid")
                                    .Value, out int id);

            if (hasValue == false)
            {
                hasTokenExpired = false;
                return -1;
            }

            hasTokenExpired = jwtToken.ValidTo < DateTime.UtcNow;

            return id;
        }
    }
}
