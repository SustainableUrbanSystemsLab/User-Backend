using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc.Infrastructure;

using Urbano_API.DTOs;
using Urbano_API.Controllers;
using Urbano_API.Models;
using Urbano_API.Interfaces;
using Urbano_API.Services;
using Microsoft.Extensions.Configuration;

namespace Urbano_API.Tests.Controllers;

public class AuthControllerTests
{
    private readonly IConfiguration configuration;
    //private readonly IUserRepository _userRepositoryMock = A.Fake<IUserRepository>();
    //private readonly IVerificationRepository _verificationRepositoryMock = A.Fake<IVerificationRepository>();
    //private readonly IAuthService _authServiceMock = A.Fake<IAuthService>();
    //private readonly IVerificationService _verificationServiceMock = A.Fake<IVerificationService>();

    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IVerificationRepository> _verificationRepositoryMock = new();
    private readonly Mock<IVerificationService> _verificationServiceMock = new();
    private readonly IAuthService _authService = new AuthService();

    public AuthControllerTests()
    {
        var inMemorySettings = new Dictionary<string, string> {
            {"TopLevelKey", "TopLevelValue"},
            {"SectionName:SomeKey", "SectionValue"},
        };

        configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
    }

    private UserDTO CreateValidUser()
    {
        UserDTO userDTO = new UserDTO();
        userDTO.FirstName = "random";
        userDTO.LastName = "name";
        userDTO.UserName = "abc@gmail.com";
        userDTO.Password = "passoword@test";
        
        return userDTO;
    }

    private UserDTO CreateInvalidUser()
    {
        UserDTO userDTO = new UserDTO();
        userDTO.FirstName = "random";
        userDTO.LastName = "name";
        userDTO.UserName = "abdfsdfs";
        userDTO.Password = "passoword@test";

        return userDTO;
    }

    [Fact]
    public void Register_Success()
    {
        // Arrange
        UserDTO userDTO = CreateValidUser();
        User user = userDTO.GetUser();

        _userRepositoryMock.Setup(x => x.CreateAsync(user)).Returns(Task.CompletedTask).Verifiable();
        _verificationServiceMock.Setup(x => x.SendVerificationMail(user.UserName, user.FirstName + " " + user.LastName)).Verifiable();

        AuthController _authController = new AuthController(configuration, _authService, _verificationServiceMock.Object, _userRepositoryMock.Object, _verificationRepositoryMock.Object);

        // Act
        var result = _authController.Register(userDTO);

        // Assert
        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
        _verificationServiceMock.Verify(x => x.SendVerificationMail(It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        Assert.NotNull(result);
        Assert.Equal(200, ((IStatusCodeActionResult)result.Result).StatusCode);
    }


    [Fact]
    public void Register_Failure()
    {
        // Arrange
        UserDTO userDTO = CreateInvalidUser();
        User user = userDTO.GetUser();

        _userRepositoryMock.Setup(x => x.CreateAsync(user)).Returns(Task.CompletedTask).Verifiable();
        _verificationServiceMock.Setup(x => x.SendVerificationMail(user.UserName, user.FirstName + " " + user.LastName)).Verifiable();

        AuthController _authController = new AuthController(configuration, _authService, _verificationServiceMock.Object, _userRepositoryMock.Object, _verificationRepositoryMock.Object);

        // Act
        var result = _authController.Register(userDTO);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, ((IStatusCodeActionResult)result.Result).StatusCode);
    }


    [Fact]
    public void Login_Success()
    {
        // Arrange (move this logic to seperate fn)
        UserDTO userDTO = CreateValidUser();
        User user = userDTO.GetUser();
        user.Verified = true;

        LoginDTO loginDTO = new LoginDTO();
        loginDTO.UserName = userDTO.UserName;
        loginDTO.Password = userDTO.Password;

        _userRepositoryMock.Setup(x => x.GetUserAsync(user.UserName)).Returns(Task.FromResult<User>(user)).Verifiable();

        AuthController _authController = new AuthController(configuration, _authService, _verificationServiceMock.Object, _userRepositoryMock.Object, _verificationRepositoryMock.Object);

        // Act
        var result = _authController.Login(loginDTO);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, ((IStatusCodeActionResult)result.Result).StatusCode);
        // check for accessToken Field
    }


    [Fact]
    public void Login_Failure_UserNotVerified()
    {
        // Arrange (move this logic to seperate fn)
        UserDTO userDTO = CreateValidUser();
        User user = userDTO.GetUser();

        LoginDTO loginDTO = new LoginDTO();
        loginDTO.UserName = userDTO.UserName;
        loginDTO.Password = userDTO.Password;

        _userRepositoryMock.Setup(x => x.GetUserAsync(user.UserName)).Returns(Task.FromResult<User>(user)).Verifiable();

        AuthController _authController = new AuthController(configuration, _authService, _verificationServiceMock.Object, _userRepositoryMock.Object, _verificationRepositoryMock.Object);

        // Act
        var result = _authController.Login(loginDTO);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(401, ((IStatusCodeActionResult)result.Result).StatusCode);
    }


    [Fact]
    public void Login_Failure_UserNotExists()
    {
        // Arrange (move this logic to seperate fn)
        UserDTO userDTO = CreateValidUser();
        User user = userDTO.GetUser();

        LoginDTO loginDTO = new LoginDTO();
        loginDTO.UserName = userDTO.UserName;
        loginDTO.Password = userDTO.Password;

        _userRepositoryMock.Setup(x => x.GetUserAsync(user.UserName)).Returns(Task.FromResult<User>(null)).Verifiable();

        AuthController _authController = new AuthController(configuration, _authService, _verificationServiceMock.Object, _userRepositoryMock.Object, _verificationRepositoryMock.Object);

        // Act
        var result = _authController.Login(loginDTO);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(401, ((IStatusCodeActionResult)result.Result).StatusCode);
    }


    [Fact]
    public void OTP_Generate_Success()
    {
        // Arrange (move this logic to seperate fn)
        UserDTO userDTO = CreateValidUser();
        User user = userDTO.GetUser();

        OTPDTO oTPDTO = new OTPDTO();
        oTPDTO.UserName = userDTO.UserName;

        _userRepositoryMock.Setup(x => x.GetUserAsync(user.UserName)).Returns(Task.FromResult<User>(user)).Verifiable();
        _verificationServiceMock.Setup(x => x.SendOTP(user.UserName, user.FirstName)).Verifiable();

        AuthController _authController = new AuthController(configuration, _authService, _verificationServiceMock.Object, _userRepositoryMock.Object, _verificationRepositoryMock.Object);

        // Act
        var result = _authController.GenerateOTP(oTPDTO);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, ((IStatusCodeActionResult)result.Result).StatusCode);
        //write a test case to verify otp generation algo (service class)
    }


    [Fact]
    public void OTP_Generate_Failure_UserNotExists()
    {
        // Arrange (move this logic to seperate fn)
        UserDTO userDTO = CreateValidUser();
        User user = userDTO.GetUser();

        OTPDTO oTPDTO = new OTPDTO();
        oTPDTO.UserName = userDTO.UserName;

        _userRepositoryMock.Setup(x => x.GetUserAsync(user.UserName)).Returns(Task.FromResult<User>(null)).Verifiable();
        _verificationServiceMock.Setup(x => x.SendOTP(user.UserName, user.FirstName)).Verifiable();

        AuthController _authController = new AuthController(configuration, _authService, _verificationServiceMock.Object, _userRepositoryMock.Object, _verificationRepositoryMock.Object);

        // Act
        var result = _authController.GenerateOTP(oTPDTO);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, ((IStatusCodeActionResult)result.Result).StatusCode);
    }


    [Fact]
    public void OTP_Generate_Failure_UserNotValid()
    {
        // Arrange (move this logic to seperate fn)
        UserDTO userDTO = CreateInvalidUser();
        User user = userDTO.GetUser();

        OTPDTO oTPDTO = new OTPDTO();
        oTPDTO.UserName = userDTO.UserName;

        _userRepositoryMock.Setup(x => x.GetUserAsync(user.UserName)).Returns(Task.FromResult<User>(null)).Verifiable();
        _verificationServiceMock.Setup(x => x.SendOTP(user.UserName, user.FirstName)).Verifiable();

        AuthController _authController = new AuthController(configuration, _authService, _verificationServiceMock.Object, _userRepositoryMock.Object, _verificationRepositoryMock.Object);

        // Act
        var result = _authController.GenerateOTP(oTPDTO);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, ((IStatusCodeActionResult)result.Result).StatusCode);
    }

    //update password
    //verify token
    //otp verify 
}


 