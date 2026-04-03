using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using AuthAPI.Models;
using AuthAPI.Data;
using AuthAPI.Services;

namespace AuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;
        private readonly EmailService _emailService;

        public AuthController(ApplicationDbContext context, JwtService jwtService, EmailService emailService)
        {
            _context = context;
            _jwtService = jwtService;
            _emailService = emailService;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                return BadRequest(new AuthResponse 
                { 
                    Success = false, 
                    Message = "Email already registered. Please login." 
                });
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = passwordHash,
                Role = "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            string token = _jwtService.GenerateToken(user);

            return Ok(new AuthResponse
            {
                Success = true,
                Message = "Registration successful! Welcome aboard! 🎉",
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                }
            });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new AuthResponse
                {
                    Success = false,
                    Message = "Invalid email or password. Please try again."
                });
            }

            string token = _jwtService.GenerateToken(user);

            return Ok(new AuthResponse
            {
                Success = true,
                Message = $"Welcome back, {user.Name}! 👋",
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                }
            });
        }

        // GET: api/auth/profile
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            return Ok(new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            });
        }

        // POST: api/auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return Ok(new { Success = true, Message = "If email exists, OTP has been sent." });
            }

            var otpCode = new Random().Next(100000, 999999).ToString();

            var oldOtps = _context.PasswordResets.Where(p => p.Email == request.Email);
            _context.PasswordResets.RemoveRange(oldOtps);

            var passwordReset = new PasswordReset
            {
                Email = request.Email,
                OtpCode = otpCode,
                ExpiryTime = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };
            _context.PasswordResets.Add(passwordReset);
            await _context.SaveChangesAsync();

            var emailSent = await _emailService.SendOtpEmailAsync(request.Email, otpCode);

            if (emailSent)
            {
                return Ok(new { Success = true, Message = "OTP sent to your email address. Please check your inbox." });
            }
            else
            {
                return StatusCode(500, new { Success = false, Message = "Failed to send OTP. Please try again." });
            }
        }

        // POST: api/auth/verify-otp
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var resetRecord = await _context.PasswordResets
                .FirstOrDefaultAsync(p => p.Email == request.Email && p.OtpCode == request.OtpCode);

            if (resetRecord == null)
            {
                return BadRequest(new { Success = false, Message = "Invalid OTP code." });
            }

            if (resetRecord.IsUsed)
            {
                return BadRequest(new { Success = false, Message = "OTP has already been used." });
            }

            if (resetRecord.ExpiryTime < DateTime.UtcNow)
            {
                return BadRequest(new { Success = false, Message = "OTP has expired. Please request a new one." });
            }

            return Ok(new { Success = true, Message = "OTP verified successfully. You can now reset your password." });
        }

        // POST: api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var resetRecord = await _context.PasswordResets
                .FirstOrDefaultAsync(p => p.Email == request.Email && p.OtpCode == request.OtpCode);

            if (resetRecord == null)
            {
                return BadRequest(new { Success = false, Message = "Invalid OTP code." });
            }

            if (resetRecord.IsUsed)
            {
                return BadRequest(new { Success = false, Message = "OTP has already been used." });
            }

            if (resetRecord.ExpiryTime < DateTime.UtcNow)
            {
                return BadRequest(new { Success = false, Message = "OTP has expired. Please request a new one." });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return BadRequest(new { Success = false, Message = "User not found." });
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            resetRecord.IsUsed = true;
            
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Password reset successfully! Please login with your new password. 🎉" });
        }
    }
}