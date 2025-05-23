﻿namespace Urbano_API.Interfaces;

public interface IAuthService
{
    public bool IsValidUserName(string userName);

    public String GeneratePasswordHash(String password);
}