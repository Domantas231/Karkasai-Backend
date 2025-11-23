namespace HabitTribe.Models;

public record UserDto(string UserName);

public record RegisterUserDto(string UserName, string Password, string Email);

public record LoginDto(string UserName, string Password);

public record SuccessfulLoginDto(string AccessToken);
