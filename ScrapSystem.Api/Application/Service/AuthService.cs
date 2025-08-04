using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using ScrapSystem.Api.Data.Repositories.IRepositories;
using ScrapSystem.Api.Repositories;
using ScrapSystem.Api.Application.Response;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using BC = BCrypt.Net.BCrypt;
using ScrapSystem.Api.Application.DTOs.UserDtos;
using ScrapSystem.Api.Domain.Models;
using ScrapSystem.Api.Application.Service.IServices;
using ScrapSystem.Api.Application.DTOs.UserDtos.Validators;
using Serilog;
using ScrapSystem.Api.Application.Common;
using Microsoft.Extensions.Caching.Memory;
using ScrapSystem.Api.Application.Response;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Extensions.Configuration;
using Azure.Core;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ScrapSystem.Api.Application.Service
{

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly JwtConfig _jwtConfig;
        private readonly IJwtService _jwtService;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;


        public AuthService(AppDbContext context, IOptions<JwtConfig> jwtConfig, IMapper mapper, IUnitOfWork unitOfWork, IMemoryCache cache, IJwtService jwtService)
        {
            _context = context;
            _jwtConfig = jwtConfig.Value;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _cache = cache;
            _jwtService = jwtService;
        }


        private bool VerifyPassword(User user, string password)
        {
            if (user == null || string.IsNullOrEmpty(password))

                return false;

            return BC.Verify(password, user.Password);
        }

        private Task BlacklistTokenAsync(string token)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var jsonToken = jwtHandler.ReadJwtToken(token);
            var exp = jsonToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp)?.Value;
            var expTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp ?? "0"));

            var cacheKey = $"blacklist_{token}";
            _cache.Set(cacheKey, true, expTime);

            return Task.CompletedTask;
        }

        private Task<bool> IsTokenBlacklistedAsync(string token)
        {
            var cacheKey = $"blacklist_{token}";
            var isBlacklisted = _cache.TryGetValue(cacheKey, out _);
            return Task.FromResult(isBlacklisted);
        }




        public async Task<ApiResult<UserDto>> RegisterAsync(UserRegisterDto model)
        {
            try
            {
                var validator = new UserRegisterDtoValidator();

                var validationResults = validator.Validate(model);

                if (!validationResults.IsValid)
                {
                    return new ApiResult<UserDto> { IsSuccess = false, Errors = validationResults.Errors.Select(e => e.ErrorMessage).ToList() };
                }

                model.Password = BC.HashPassword(model.Password);
                await _unitOfWork.UserRepository.Add(_mapper.Map<User>(model));
                await _unitOfWork.SaveChangesAsync();
                _unitOfWork.Dispose();
                return new ApiResult<UserDto>() { IsSuccess = true };
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw;
            }

        }

        public async Task<bool> LogoutAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new UnauthorizedAccessException("Token is required for logout");
            }
            var isAlreadyBlacklisted = await IsTokenBlacklistedAsync(token);
            if (isAlreadyBlacklisted)
            {
                return true;
            }

            try
            {
                await BlacklistTokenAsync(token);
                return true;
            }

            catch (SecurityTokenException ex)
            {
                throw new ArgumentException("Invalid or expired token", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Logout service is temporarily unavailable", ex);
            }
        }

        //code old

        //public async Task<AuthResponse> LoginAsync(UserLoginDto model)
        //{
        //    try
        //    {
        //        var validator = new UserLoginDtoValidator();
        //        var validationResults = validator.Validate(model);
        //        if (!validationResults.IsValid)
        //        {
        //            var errorMessages = string.Join("; ", validationResults.Errors.Select(e => e.ErrorMessage));
        //            throw new UnauthorizedAccessException(errorMessages);
        //        }

        //        var user = await _unitOfWork.UserRepository.GetUserByUserId(model.UserId);

        //        bool passwordValid = VerifyPassword(user, model.Password);

        //        if (!passwordValid)
        //            throw new UnauthorizedAccessException("Invalid username or password");

        //        var accessToken = _jwtService.GenerateAccessToken(user);
        //        var refreshToken = _jwtService.GenerateRefreshToken();

        //        var userRefreshToken = new UserRefreshToken
        //        {
        //            UserId = user.UserId,
        //            Token = refreshToken,
        //            ExpiryDate = DateTime.UtcNow.AddDays(_jwtConfig.RefreshTokenExpirationInDays),
        //            CreatedDate = DateTime.UtcNow,
        //            IsActive = true
        //        };
        //        await _unitOfWork.UserRefreshTokenRepository.Add(userRefreshToken);
        //        await _unitOfWork.SaveChangesAsync();

        //        Log.Information("User {Username} logged in successfully", model.UserId);
        //        Log.Error("User {Username} logged in successfully", model.UserId);
        //        return new AuthResponse
        //        {
        //            AccessToken = accessToken,
        //            RefreshToken = refreshToken,
        //            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpirationInMinutes),
        //            User = new UserDto
        //            {
        //                UserId = user.UserId,
        //                Section = user.Section,
        //            }
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex.Message, "An error occurred while logging in.");
        //        throw;
        //    }
        //}

        //new ket noi sql s
        public async Task<AuthResponse> LoginAsync(UserLoginDto model)
        {
            try
            {
                var validator = new UserLoginDtoValidator();
                var validationResults = validator.Validate(model);
                if (!validationResults.IsValid)
                {
                    var errorMessages = string.Join("; ", validationResults.Errors.Select(e => e.ErrorMessage));
                    throw new UnauthorizedAccessException(errorMessages);
                }

                // Gọi stored procedure thay vì EF
                var user = await LoginWithStoredProcedure(model.UserId, model.Password);

                if (user == null)
                {
                    throw new UnauthorizedAccessException("Invalid username or password");
                }
                // Tạo token như cũ
                //var accessToken = _jwtService.GenerateAccessToken(user);
                //var refreshToken = _jwtService.GenerateRefreshToken();
                var user1 = await _unitOfWork.UserRepository.GetUserByUserId(model.UserId);
                var accessToken = _jwtService.GenerateAccessToken(user1);
                var refreshToken = _jwtService.GenerateRefreshToken();

                var userRefreshToken = new UserRefreshToken
                {
                    UserId = user.UserId,
                    Token = refreshToken,
                    ExpiryDate = DateTime.UtcNow.AddDays(_jwtConfig.RefreshTokenExpirationInDays),
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };
                await _unitOfWork.UserRefreshTokenRepository.Add(userRefreshToken);

                // save refreshToken vào DB → dung ADO.NET add
                Log.Information("User {Username} logged in successfully", model.UserId);

                return new AuthResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpirationInMinutes),
                    User = user
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while logging in.");
                throw;
            }
        }

        //ket noi sql khong dung FE 03.08.2025
        public async Task<UserDto?> LoginWithStoredProcedure(string userId, string password)
        {
            //string _connectionString = "Server=10.92.186.30;Database=ScrapSystem;User Id=sa;Password=Psnvdb2013;MultipleActiveResultSets=true;Encrypt=True;TrustServerCertificate=True;";
            var _connectionString = _context.Database.GetDbConnection().ConnectionString;
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("sp_LoginUser", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@Password", password);

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new UserDto
                        {
                            //UserId = reader["UserId"].ToString(),
                            //Section = reader["Section"].ToString()
                            UserId = reader["UserId"] != DBNull.Value ? reader["UserId"].ToString() : string.Empty,
                            Section = reader["Section"] != DBNull.Value ? reader["Section"].ToString() : string.Empty
                        };
                    }
                }
            }

            return null;
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
            var userId = principal.FindFirst("uid")?.Value ?? "0";

            var storedRefreshToken = await _unitOfWork.UserRefreshTokenRepository.GetUserRefreshTokenByUserId(request, userId);

            if (storedRefreshToken == null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            var user = await _unitOfWork.UserRepository.GetUserByUserId(userId);

            if (user == null )
            {
                throw new UnauthorizedAccessException("User not found or inactive");
            }

            // Generate new tokens
            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            // Revoke old refresh token
            storedRefreshToken.IsActive = false;
            storedRefreshToken.RevokedDate = DateTime.UtcNow;

            // Create new refresh token
            var newUserRefreshToken = new UserRefreshToken
            {
                UserId = user.UserId,
                Token = newRefreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(_jwtConfig.RefreshTokenExpirationInDays),
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            _unitOfWork.UserRefreshTokenRepository.Add(newUserRefreshToken);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpirationInMinutes),
                User = new UserDto
                {
                    UserId = user.UserId,
                    Section = user.Section
                }
            };
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            var storedRefreshToken = await _unitOfWork.UserRefreshTokenRepository.GetUserRefreshTokenByToken(refreshToken);

            if (storedRefreshToken == null)
                return false;

            storedRefreshToken.IsActive = false;
            storedRefreshToken.RevokedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangePasswordAsync(string userId, UserChangePasswordDto request)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUserId(userId);
            if (user == null)
                return false;

            if (!BC.Verify(request.CurrentPassword, user.Password))
                return false;

            user.Password = BC.HashPassword(request.NewPassword);

            // Revoke all refresh tokens (force re-login)
            var refreshTokens = await _unitOfWork.UserRefreshTokenRepository.GetListToken(userId);
               

            foreach (var token in refreshTokens)
            {
                token.IsActive = false;
                token.RevokedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}