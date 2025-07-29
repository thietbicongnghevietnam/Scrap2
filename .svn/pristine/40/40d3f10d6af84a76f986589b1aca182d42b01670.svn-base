using AutoMapper;
using ScrapSystem.Application.Service.IServices;
using ScrapSystem.Data.Repositories.IRepositories;
using ScrapSystem.Domain.Models;
using ScrapSystem.Services.DTOs.UserDtos;
using ScrapSystem.Services.DTOs.Validators;
using ScrapSystem.Services.Response;
using Serilog;

namespace ScrapSystem.Application.Service
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<UserDto> GetUserById(int id)
        {
            var user = await _unitOfWork.UserRepository.GetUserById(id);
            if (user == null)
            {
                return null;
            }

            return _mapper.Map<UserDto>(user);
        }

        public async Task<ApiResponse<UserDto>> AddUserAsync(CreateUserDto userDto)
        {
            try
            {
                var validator = new CreateUserDtoValidator();
                var validationResults = validator.Validate(userDto);
                if (!validationResults.IsValid)
                {
                    return new ApiResponse<UserDto>(validationResults.Errors
                        .Select(e => e.ErrorMessage)
                        .ToList());
                }

                var user = _mapper.Map<User>(userDto);
                await _unitOfWork.UserRepository.Add(user);
                await _unitOfWork.SaveChangesAsync();

                return new ApiResponse<UserDto>(_mapper.Map<UserDto>(user));

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw;
            }
        }
    }
}


