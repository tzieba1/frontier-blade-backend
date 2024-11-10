namespace FbsApi.Settings
{
  public class EmailSenderOptions
  {
    required public string SendGridKey { get; set; }
    required public string SendGridSenderEmail { get; set; }
    required public string SendGridSenderName { get; set; }
    required public string GmailAppPassword { get; set; }
    required public string SmtpServer { get; set; }
    required public int SmtpPort { get; set; }
    required public string GmailSenderEmail { get; set; }
    required public string GmailSenderName { get; set; }
  }
}