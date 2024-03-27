using System.Net;
using System.Net.Mail;

namespace ShellShockers.Server.Components;

internal static class EmailSender
{
	private const string Email = "yonatan005ziv@gmail.com";
	private const string Password = "dtey faoq abon ndmr";

	private static readonly SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");

	static EmailSender()
	{
		smtpClient.UseDefaultCredentials = false;
		smtpClient.Port = 587;
		smtpClient.Credentials = new NetworkCredential(Email, Password);
		smtpClient.EnableSsl = true;
	}

	public static bool SendEmail(string email, string subject, string body)
	{
		try
		{
			smtpClient.Send(Email, email, subject, body);
			return true;
		}
		catch { return false; }
	}
}