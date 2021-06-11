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
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApiDbContext _dbContext;

        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            ApiDbContext _context)
        {
            _userManager = userManager;
            _dbContext = _context;
        }

        [HttpPost]
        [Route("User-Context")]
        public IActionResult GetUserContext()
        {
            var Token = Request.Headers["refreshToken"].ToString();
            var userJwtToken = _dbContext.RefreshTokens.Where(x => x.Token == Token).Include(x => x.User).ThenInclude(x => x.Wallet).FirstOrDefault();
            if (userJwtToken == null)
            {
                return BadRequest(new RegistrationResponse()
                {
                    Errors = new List<string>() {
                        "Invalid Token"
                    },
                    Success = false
                });
            }
            else if (userJwtToken.User == null)
            {
                return BadRequest(new RegistrationResponse()
                {
                    Errors = new List<string>() {
                        "Bad Token"
                    },
                    Success = false
                });
            }
            var user = userJwtToken.User;
            return Ok(new UserContext()
            {
                Username = user.UserName,
                PhoneNumber = user.PhoneNumber,
                WalletBalance = user.Wallet.Balance
            });
        }

    }
}