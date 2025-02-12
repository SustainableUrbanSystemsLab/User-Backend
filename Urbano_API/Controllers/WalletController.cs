using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Urbano_API.DTOs;
using Urbano_API.Interfaces;
using Urbano_API.Models;
using MongoDB.Bson;
namespace Urbano_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WalletController : ControllerBase
    {
        private readonly IWalletRepository _walletRepository;

        public WalletController(IWalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
        }

        [HttpPost("add-token")]
        public async Task<IActionResult> AddToken([FromBody] AddTokenRequest request)
        {
            
            if (string.IsNullOrEmpty(request.UserId))
            {
                return BadRequest("UserId is required.");
            } 
            if (!ObjectId.TryParse(request.UserId, out _))
            {
                return BadRequest("UserId is not a valid ObjectId.");
            }       
            var wallet = await _walletRepository.GetWalletByUserIdAsync(request.UserId);
            if (wallet == null)
            {
                return NotFound("Wallet not found for the provided UserId.");
            }

            await _walletRepository.AddTokenAsync(request.UserId, request.TokenType, request.Quantity);
            return Ok("Token added successfully.");
        }

        [HttpPost("remove-token")]
        public async Task<IActionResult> RemoveToken([FromBody] RemoveTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId))
            {
                return BadRequest("UserId is required.");
            }

            var success = await _walletRepository.RemoveTokenAsync(request.UserId, request.TokenType, request.Quantity);
            if (!success)
            {
                return BadRequest("Insufficient balance or token not found.");
            }

            return Ok("Token removed successfully.");
        }

        [HttpPost("verify-token")]
        public async Task<IActionResult> VerifyToken([FromBody] VerifyTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId))
            {
                return BadRequest("UserId is required.");
            }

            var isValid = await _walletRepository.VerifyTokenAsync(request.UserId, request.TokenType, request.RequiredQuantity);
            if (!isValid)
            {
                return BadRequest("Insufficient tokens.");
            }

            return Ok("Tokens verified.");
        }

        [HttpPost("balance")]
        public async Task<IActionResult> GetBalance([FromBody] BalanceRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId))
            {
                return BadRequest("UserId is required.");
            }

            var balance = await _walletRepository.GetBalanceAsync(request.UserId);
            return Ok(balance);
        }
    }
}

