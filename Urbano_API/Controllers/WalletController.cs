using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Urbano_API.DTOs;
using Urbano_API.Interfaces;
using Urbano_API.Models;
using MongoDB.Bson;
using Urbano_API.Repositories;
namespace Urbano_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WalletController : ControllerBase
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IUserRepository _userRepository;

        public WalletController(IWalletRepository walletRepository, IUserRepository userRepository)
        {
            _walletRepository = walletRepository;
            _userRepository = userRepository;
        }

        [HttpPost("add-token")]
        public async Task<IActionResult> AddToken([FromBody] AddTokenRequest request)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(request.Token);
            var role = jwtSecurityToken.Claims.First(claim => claim.Type == ClaimTypes.Role).Value;

            if(role != Roles.ADMIN.ToString())
            {
                ModelState.AddModelError("Unauthorized", "Not authorized to access the API");
                return Unauthorized(ModelState);
            }

            var user = await _userRepository.GetUserAsync(request.UserName);
            if (user == null)
            {
                return NotFound("User not found");
            }

            if (!ObjectId.TryParse(user.Id, out _))
            {
                return BadRequest("UserId is not a valid ObjectId.");
            }       
            var wallet = await _walletRepository.GetWalletByUserIdAsync(user.Id);
            if (wallet == null)
            {
                return NotFound("Wallet not found for the provided UserId.");
            }

            await _walletRepository.AddTokenAsync(user.Id, request.TokenType, request.Quantity);
            return Ok("Token added successfully.");
        }

        [HttpPost("remove-token")]
        public async Task<IActionResult> RemoveToken([FromBody] RemoveTokenRequest request)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(request.Token);
            var role = jwtSecurityToken.Claims.First(claim => claim.Type == ClaimTypes.Role).Value;

            if(role != Roles.ADMIN.ToString())
            {
                ModelState.AddModelError("Unauthorized", "Not authorized to access the API");
                return Unauthorized(ModelState);
            }

            var user = await _userRepository.GetUserAsync(request.UserName);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var success = await _walletRepository.RemoveTokenAsync(user.Id, request.TokenType, request.Quantity);
            if (!success)
            {
                return BadRequest("Insufficient balance or token not found.");
            }

            return Ok("Token removed successfully.");
        }

        [HttpPost("verify-token")]
        public async Task<IActionResult> VerifyToken([FromBody] VerifyTokenRequest request)
        {
            var user = await _userRepository.GetUserAsync(request.UserName);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var isValid = await _walletRepository.VerifyTokenAsync(user.Id, request.TokenType, request.RequiredQuantity);
            if (!isValid)
            {
                return BadRequest("Insufficient tokens.");
            }

            return Ok("Tokens verified.");
        }

        [HttpPost("balance")]
        public async Task<IActionResult> GetBalance([FromBody] BalanceRequest request)
        {
            var user = await _userRepository.GetUserAsync(request.UserName);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var balance = await _walletRepository.GetBalanceAsync(user.Id);
            return Ok(balance);
        }
    }
}