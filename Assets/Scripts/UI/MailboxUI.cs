using UnityEngine;
using TMPro;

public class MailboxUI : MonoBehaviour
{
    public MailService mailService;
    public TextMeshProUGUI inboxLabel;

    public void Refresh()
    {
        if (mailService == null || inboxLabel == null) return;

        System.Text.StringBuilder sb = new();
        foreach (var mail in mailService.inbox)
        {
            string status = mail.claimed ? "[CLAIMED]" : "[NEW]";
            sb.AppendLine($"{status} {mail.title}");
        }

        inboxLabel.text = sb.ToString();
    }
}
