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
    public class APIManagementController : Controller
    {
        private readonly ApiDbContext _dbContext;

        public APIManagementController(ApiDbContext _context)
        {
            _dbContext = _context;
        }
        [HttpPost]
        [Route("Add-API")]
        public async Task<IActionResult> AddAPI([FromBody] APIAddRequest request)
        {
            APIModel apiModel = new APIModel()
            {
                name = request.name,
                url = request.url,
                sampleCall = request.sampleCall,
                desc = request.desc,
                price = request.price,
            };

            try
            {
                await _dbContext.ApiModels.AddAsync(apiModel);
                await _dbContext.SaveChangesAsync();
                return Ok(new ResponseDTO()
                {
                    Success = true
                });
            }
            catch (DbUpdateException e)
            {
                Console.Write(e.Message);
                return BadRequest(new ResponseDTO()
                {
                    Success = false,
                    Errors = new List<string>{
                        "Cannot Add API to Database",
                        e.Message
                    }
                });
            }
        }

        [HttpGet]
        [Route("GetAPIs")]

        public async Task<IActionResult> GetAllAPIs()
        {
            var models = await _dbContext.ApiModels.Select(x => new APIModelResponse() { id = x.id, name = x.name, desc = x.desc, price = x.price, sampleCall = x.sampleCall }).ToListAsync();
            if (models.Count == 0)
                return BadRequest(new ResponseDTO()
                {
                    Success = false,
                    Errors = new List<string>
                    {
                        "No API Exist currently"
                    }
                });
            return Ok(models);
        }

        [HttpPost]
        [Route("GetAPIByID")]

        public async Task<IActionResult> GetAPIByID([FromBody] APIGetRequest id)
        {
            var model = await _dbContext.ApiModels.Where(x => x.id == id.id).FirstOrDefaultAsync();
            if (model == null)
                return BadRequest(new ResponseDTO()
                {
                    Success = false,
                    Errors = new List<string>
                        {
                            "API with Guid {" + id.ToString() + "} doesn't exist"
                        }
                });

            return Ok(model);
        }

        [HttpPost]
        [Route("BuyAPI")]
        public async Task<IActionResult> BuyAPI([FromBody] APIGetRequest id)
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

            var model = await _dbContext.ApiModels.Where(x => x.id == id.id).FirstOrDefaultAsync();
            if (model == null)
                return BadRequest(new ResponseDTO()
                {
                    Success = false,
                    Errors = new List<string>
                        {
                            "API with Guid {" + id.ToString() + "} doesn't exist"
                        }
                });

            //If User doesn't have enough Balance in account
            if (model.price > user.Wallet.Balance)
                return BadRequest(new ResponseDTO()
                {
                    Success = false,
                    Errors = new List<string>
                        {
                            "User doesn't have enough Balance to buy this API"
                        }
                });

            var buyingAPI = new BoughtAPIs()
            {
                boughtOn = DateTime.UtcNow,
                expiresOn = DateTime.UtcNow.AddMonths(1),
                User = user,
                api = model,
            };
            try
            {
                //Add Bought API to DB
                await _dbContext.BoughtApis.AddAsync(buyingAPI);
                //Remove Balance from Account
                user.Wallet.Balance -= model.price;
                _dbContext.Users.Update(user);

                await _dbContext.SaveChangesAsync();

                return Ok(new ResponseDTO()
                {
                    Success = true,
                });
            }
            catch (DbUpdateException e)
            {
                Console.Write(e.Message);
                return BadRequest(new ResponseDTO()
                {
                    Success = false,
                    Errors = new List<string>{
                        "There was an error buying this API",
                        e.Message
                    }
                });
            }
        }
        [HttpGet]
        [Route("GetBoughtAPIs")]
        public async Task<IActionResult> GetBoughtAPIs()
        {
            var Token = Request.Headers["refreshToken"].ToString();
            var userJwtToken = await _dbContext.RefreshTokens.Where(x => x.Token == Token).Include(x => x.User).FirstOrDefaultAsync();
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
            var boughtAPIs = await _dbContext.BoughtApis.Where(b => b.User == user).Include(b => b.api).ToListAsync();

            if (boughtAPIs == null || boughtAPIs.Count == 0)
            {
                return BadRequest(new ResponseDTO()
                {
                    Success = false,
                    Errors = new List<string> { "User havent bought an API yet" }
                });
            }


            return Ok(boughtAPIs);
        }
    }
}
