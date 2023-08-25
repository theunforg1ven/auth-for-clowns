namespace StudyAuthApp.WebApi.DTOs;

public class ChangePasswordDto
{
    public string Token { get; set; }

    public string OldPassword { get; set; }

    public string NewPassword { get; set; }

    public string ConfirmNewPassword { get; set; }
}
