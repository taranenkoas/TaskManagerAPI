﻿namespace TaskManagerAPI.Application.DTO.Auth;

public class LoginDTO
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
