using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
    [Route("Admin")]
    class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;

        private readonly ApiDbContext _dbContext;

        public AdminController(
            ILogger<AdminController> logger,
            ApiDbContext _context)
        {
            _logger = logger;
            _dbContext = _context;
        }
        [Route("Index")]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}