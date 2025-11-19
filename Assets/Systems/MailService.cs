using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Mail
{
    public string id;
    public string title;
    public string body;
    public string reward;  // e.g., "GOLD:1000" or "GEMS:50"
    public bool claimed;
}

public class MailService : MonoBehaviour
{
    public static MailService Instance { get; private set; }

    public List<Mail> inbox = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SendMail(string id, string title, string body, string reward = "")
    {
        inbox.Add(new Mail
        {
            id = id,
            title = title,
            body = body,
            reward = reward,
            claimed = false
        });

        Debug.Log($"[MailService] New mail: {title}");
    }

    public void ClaimReward(string mailId)
    {
        var mail = inbox.Find(m => m.id == mailId);
        if (mail == null)
        {
            Debug.LogWarning($"[MailService] Mail not found: {mailId}");
            return;
        }

        if (mail.claimed)
        {
            Debug.LogWarning($"[MailService] Mail already claimed: {mailId}");
            return;
        }

        // Grant the reward
        if (!string.IsNullOrEmpty(mail.reward))
        {
            var distributor = GetComponent<RewardDistributor>();
            if (distributor != null)
            {
                distributor.GrantReward(mail.reward, "MAIL", mailId);
            }
        }

        mail.claimed = true;
        Debug.Log($"[MailService] Reward claimed for: {mail.title}");
    }
}

