using System;
using System.Linq;
using System.Text;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using AMS.Data;
using AMS.Models;
using AMS.Configuration;
using AMS.Models.DTOs.Requests;
using AMS.Models.DTOs.Responses;

namespace AMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthManagementController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtConfig _jwtConfig;

        private readonly ApiDbContext _dbContext;

        public AuthManagementController(
            UserManager<ApplicationUser> userManager,
            IOptionsMonitor<JwtConfig> optionsMonitor,
            ApiDbContext _context)
        {
            _userManager = userManager;
            _jwtConfig = optionsMonitor.CurrentValue;
            _dbContext = _context;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationDto user)
        {
            if (ModelState.IsValid)
            {
                // We can utilise the model
                var existingUser = await _userManager.FindByEmailAsync(user.Email);

                if (existingUser != null)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = new List<string>() {
                                "Email already in use"
                            },
                        Success = false
                    });
                }

                var newUser = new ApplicationUser()
                {
                    Email = user.Email,
                    UserName = user.Username,
                    Wallet = new UserWallet()
                    {
                        Balance = 1000
                    }
                };
                var isCreated = await _userManager.CreateAsync(newUser, user.Password);
                if (isCreated.Succeeded)
                {
                    var (jwtAccessToken, jwtRefreshToken) = GenerateJwtTokenPair(newUser);

                    _dbContext.RefreshTokens.Add(jwtRefreshToken);
                    _dbContext.SaveChanges();
                    return Ok(new RegistrationResponse()
                    {
                        Success = true,
                        Token = jwtAccessToken,
                        RefreshToken = jwtRefreshToken.Token
                    });
                }
                else
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = isCreated.Errors.Select(x => x.Description).ToList(),
                        Success = false
                    });
                }
            }

            return BadRequest(new RegistrationResponse()
            {
                Errors = new List<string>() {
                        "Invalid payload"
                    },
                Success = false
            });
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(user.Email);

                if (existingUser == null)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = new List<string>() {
                                "Invalid login request"
                            },
                        Success = false
                    });
                }

                var isCorrect = await _userManager.CheckPasswordAsync(existingUser, user.Password);

                if (!isCorrect)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = new List<string>() {
                                "Invalid login request"
                            },
                        Success = false
                    });
                }

                _dbContext.Entry(existingUser).Collection("RefreshTokens").Load();
                var jwtRefreshToken = existingUser.RefreshTokens.Where(x => x.IsActive).FirstOrDefault();
                if (jwtRefreshToken == null)
                {
                    jwtRefreshToken = GenerateJwtRefreshToken(existingUser);
                    _dbContext.RefreshTokens.Add(jwtRefreshToken);
                    _dbContext.SaveChanges();
                }
                var jwtAccessToken = GenerateJwtAccessToken(existingUser);
                return Ok(new RegistrationResponse()
                {
                    Success = true,
                    Token = jwtAccessToken,
                    RefreshToken = jwtRefreshToken.Token
                });
            }

            return BadRequest(new RegistrationResponse()
            {
                Errors = new List<string>() {
                        "Invalid payload"
                    },
                Success = false
            });
        }

        [HttpPost]
        [Route("Refresh-Token")]
        public async Task<IActionResult> RefreshToken()
        {
            var Token = Request.Cookies["refreshToken"];
            var jwtRefreshToken = _dbContext.RefreshTokens.Where(x => x.Token == Token).Include(x => x.User).FirstOrDefault();
            if (jwtRefreshToken == null)
            {
                return BadRequest(new RegistrationResponse()
                {
                    Errors = new List<string>() {
                        "Invalid Token"
                    },
                    Success = false
                });
            }
            else if (!jwtRefreshToken.IsActive)
            {
                return BadRequest(new RegistrationResponse()
                {
                    Errors = new List<string>() {
                        "Refresh Token Expired or Revoked"
                    },
                    Success = false
                });
            }
            var jwtAccessToken = GenerateJwtAccessToken(jwtRefreshToken.User);
            var newJwtRefreshToken = GenerateJwtRefreshToken(jwtRefreshToken.User);

            jwtRefreshToken.Revoked = DateTime.UtcNow;
            _dbContext.RefreshTokens.Update(jwtRefreshToken);
            await _dbContext.RefreshTokens.AddAsync(newJwtRefreshToken);
            await _dbContext.SaveChangesAsync();

            return Ok(new RegistrationResponse()
            {
                Success = true,
                Token = jwtAccessToken,
                RefreshToken = jwtRefreshToken.Token
            });
        }

        private string GenerateJwtAccessToken(ApplicationUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtAccessToken = jwtTokenHandler.WriteToken(token);

            return jwtAccessToken;
        }
        private RefreshToken GenerateJwtRefreshToken(ApplicationUser user)
        {
            var randomNumber = new byte[32];
            var generator = new RNGCryptoServiceProvider();
            generator.GetBytes(randomNumber);
            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                Expires = DateTime.UtcNow.AddDays(10),
                Created = DateTime.UtcNow,
                User = user
            };
        }
        private (string, RefreshToken) GenerateJwtTokenPair(ApplicationUser user)
        {
            return (GenerateJwtAccessToken(user), GenerateJwtRefreshToken(user));
        }
        private void SetRefreshTokenInCookie(RefreshToken refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = refreshToken.Expires,
            };
            Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
        }
    }
}