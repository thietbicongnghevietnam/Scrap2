namespace ScrapSystem.Api.Application.DTOs.UserDtos
{
    public class UserLoginDto : IUserDto
    {
        public string UserId { get; set; }
        public string Password { get; set; }
    }
}
