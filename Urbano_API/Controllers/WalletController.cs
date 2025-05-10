using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Urbano_API.DTOs;
using Urbano_API.Interfaces;

namespace Urbano_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "AdminOnly")]
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
        public async Task<IActionResult> AddToken([FromBody] AddTokenRequestDTO request)
        {
            try
            {
                // Get user by username
                var user = await _userRepository.GetUserAsync(request.UserName);
                if (user == null)
                {
                    //return NotFound if user does not exist
                    return NotFound("User not found");
                }

                if (!ObjectId.TryParse(user.Id, out _))
                {
                    return BadRequest("UserId is not a valid ObjectId.");
                }
                // Get user's wallet
                var wallet = await _walletRepository.GetWalletByUserIdAsync(user.Id!);

                if (wallet == null)
                {
                    // return NotFound if wallet does not exist
                    return NotFound("Wallet not found for the provided UserId.");
                }

                await _walletRepository.AddTokenAsync(user.Id!, request.TokenType, request.Quantity);
                return Ok("Token added successfully.");
            }
            catch (Exception ex)
            {
                // Optionally log the exception here.
                return StatusCode(500, "An error occurred while adding the token: " + ex.Message);
            }
        }

        [HttpPost("remove-token")]
        public async Task<IActionResult> RemoveToken([FromBody] RemoveTokenRequestDTO request)
        {
            try
            {
                // Get user by username
                var user = await _userRepository.GetUserAsync(request.UserName);
                if (user == null)
                {
                    //return NotFound if user does not exist
                    return NotFound("User not found");
                }

                var success = await _walletRepository.RemoveTokenAsync(user.Id!, request.TokenType, request.Quantity);
                if (!success)
                {
                    // return BadRequest if insufficient balance or token not found
                    return BadRequest("Insufficient balance or token not found.");
                }

                return Ok("Token removed successfully.");
            }
            catch (Exception ex)
            {
                // Optionally log the exception here.
                return StatusCode(500, "An error occurred while removing the token: " + ex.Message);
            }
        }

        [HttpPost("verify-token")]
        public async Task<IActionResult> VerifyToken([FromBody] VerifyTokenRequestDTO request)
        {
            try
            {
                // Get user by username
                var user = await _userRepository.GetUserAsync(request.UserName);
                if (user == null)
                {
                    //return NotFound if user does not exist
                    return NotFound("User not found");
                }

                var isValid = await _walletRepository.VerifyTokenAsync(user.Id!, request.TokenType, request.RequiredQuantity);
                if (!isValid)
                {
                    // return BadRequest if not insufficient tokens
                    return BadRequest("Insufficient tokens.");
                }

                return Ok("Tokens verified.");
            }
            catch (Exception ex)
            {
                // Optionally log the exception here.
                return StatusCode(500, "An error occurred while verifying the token: " + ex.Message);
            }
        }

        [HttpPost("balance")]
        public async Task<IActionResult> GetBalance([FromBody] BalanceRequestDTO request)
        {
            try
            {
                // Get user by username
                var user = await _userRepository.GetUserAsync(request.UserName);
                if (user == null)
                {
                    //return NotFound if user does not exist
                    return NotFound("User not found");
                }

                var balance = await _walletRepository.GetBalanceAsync(user.Id!);
                return Ok(balance);
            }
            catch (Exception ex)
            {
                // Log Error for exception handling
                return StatusCode(500, "An error occurred while retrieving the balance: " + ex.Message);
            }
        }
    }
}