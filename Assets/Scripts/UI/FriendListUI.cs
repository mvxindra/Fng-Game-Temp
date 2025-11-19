using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using FnGMafia.Social;

/// <summary>
/// UI controller for friend list and friend system
/// </summary>
public class FriendListUI : MonoBehaviour
{
    [Header("Friend List")]
    public Transform friendListContent;
    public GameObject friendEntryPrefab;

    [Header("Friend Requests")]
    public Transform requestListContent;
    public GameObject requestEntryPrefab;
    public TextMeshProUGUI pendingRequestCount;

    [Header("Friend Shop")]
    public Transform shopListContent;
    public GameObject shopItemPrefab;
    public TextMeshProUGUI friendPointsDisplay;

    [Header("Support Hero")]
    public Transform supportHeroListContent;
    public GameObject supportHeroPrefab;
    public TextMeshProUGUI dailyRentalStatus;

    [Header("Search/Add Friend")]
    public TMP_InputField searchInput;
    public Button searchButton;
    public Button sendRequestButton;

    [Header("Panels")]
    public GameObject friendListPanel;
    public GameObject requestsPanel;
    public GameObject shopPanel;
    public GameObject supportHeroPanel;

    private List<GameObject> spawnedFriendEntries = new List<GameObject>();
    private List<GameObject> spawnedRequestEntries = new List<GameObject>();
    private List<GameObject> spawnedShopEntries = new List<GameObject>();

    private void Start()
    {
        // Setup button listeners
        if (searchButton != null)
            searchButton.onClick.AddListener(OnSearchFriend);
        if (sendRequestButton != null)
            sendRequestButton.onClick.AddListener(OnSendRequest);

        ShowFriendList();
    }

    #region Panel Navigation

    public void ShowFriendList()
    {
        SetActivePanel(friendListPanel);
        RefreshFriendList();
    }

    public void ShowRequests()
    {
        SetActivePanel(requestsPanel);
        RefreshRequestList();
    }

    public void ShowShop()
    {
        SetActivePanel(shopPanel);
        RefreshShop();
    }

    public void ShowSupportHeroes()
    {
        SetActivePanel(supportHeroPanel);
        RefreshSupportHeroes();
    }

    private void SetActivePanel(GameObject activePanel)
    {
        if (friendListPanel != null) friendListPanel.SetActive(false);
        if (requestsPanel != null) requestsPanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
        if (supportHeroPanel != null) supportHeroPanel.SetActive(false);

        if (activePanel != null) activePanel.SetActive(true);
    }

    #endregion

    #region Friend List

    public void RefreshFriendList()
    {
        // Clear existing entries
        foreach (var entry in spawnedFriendEntries)
            Destroy(entry);
        spawnedFriendEntries.Clear();

        if (FriendManager.Instance == null) return;

        var friends = FriendManager.Instance.GetFriends();

        foreach (var friend in friends)
        {
            if (friendEntryPrefab != null && friendListContent != null)
            {
                GameObject entry = Instantiate(friendEntryPrefab, friendListContent);
                SetupFriendEntry(entry, friend);
                spawnedFriendEntries.Add(entry);
            }
        }

        Debug.Log($"[FriendListUI] Displaying {friends.Count} friends");
    }

    private void SetupFriendEntry(GameObject entry, FriendEntry friend)
    {
        // Setup friend entry UI elements
        var nameText = entry.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null)
            nameText.text = $"{friend.playerName} (Lv.{friend.playerLevel})";

        var statusText = entry.transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
        if (statusText != null)
            statusText.text = friend.isOnline ? "Online" : "Offline";

        var supportBtn = entry.transform.Find("UseSupportButton")?.GetComponent<Button>();
        if (supportBtn != null)
            supportBtn.onClick.AddListener(() => OnUseSupportHero(friend.playerId));

        var removeBtn = entry.transform.Find("RemoveButton")?.GetComponent<Button>();
        if (removeBtn != null)
            removeBtn.onClick.AddListener(() => OnRemoveFriend(friend.playerId));

        var favoriteBtn = entry.transform.Find("FavoriteButton")?.GetComponent<Button>();
        if (favoriteBtn != null)
            favoriteBtn.onClick.AddListener(() => OnToggleFavorite(friend.playerId));
    }

    #endregion

    #region Friend Requests

    public void RefreshRequestList()
    {
        // Clear existing entries
        foreach (var entry in spawnedRequestEntries)
            Destroy(entry);
        spawnedRequestEntries.Clear();

        if (FriendManager.Instance == null) return;

        var requests = FriendManager.Instance.GetPendingRequests();

        foreach (var request in requests)
        {
            if (requestEntryPrefab != null && requestListContent != null)
            {
                GameObject entry = Instantiate(requestEntryPrefab, requestListContent);
                SetupRequestEntry(entry, request);
                spawnedRequestEntries.Add(entry);
            }
        }

        if (pendingRequestCount != null)
            pendingRequestCount.text = requests.Count.ToString();

        Debug.Log($"[FriendListUI] Displaying {requests.Count} friend requests");
    }

    private void SetupRequestEntry(GameObject entry, FriendRequest request)
    {
        var nameText = entry.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null)
            nameText.text = $"{request.fromPlayerName} (Lv.{request.fromPlayerLevel})";

        var messageText = entry.transform.Find("MessageText")?.GetComponent<TextMeshProUGUI>();
        if (messageText != null)
            messageText.text = request.message;

        var acceptBtn = entry.transform.Find("AcceptButton")?.GetComponent<Button>();
        if (acceptBtn != null)
            acceptBtn.onClick.AddListener(() => OnAcceptRequest(request.requestId));

        var rejectBtn = entry.transform.Find("RejectButton")?.GetComponent<Button>();
        if (rejectBtn != null)
            rejectBtn.onClick.AddListener(() => OnRejectRequest(request.requestId));
    }

    #endregion

    #region Friend Shop

    public void RefreshShop()
    {
        // Clear existing entries
        foreach (var entry in spawnedShopEntries)
            Destroy(entry);
        spawnedShopEntries.Clear();

        if (FriendManager.Instance == null) return;

        // Update friend points display
        if (friendPointsDisplay != null)
        {
            int points = FriendManager.Instance.GetFriendPoints();
            friendPointsDisplay.text = $"Friend Points: {points}";
        }

        var shopItems = FriendManager.Instance.GetFriendShopItems();

        foreach (var item in shopItems)
        {
            if (shopItemPrefab != null && shopListContent != null)
            {
                GameObject entry = Instantiate(shopItemPrefab, shopListContent);
                SetupShopEntry(entry, item);
                spawnedShopEntries.Add(entry);
            }
        }

        Debug.Log($"[FriendListUI] Displaying {shopItems.Count} shop items");
    }

    private void SetupShopEntry(GameObject entry, FriendShopItem item)
    {
        var nameText = entry.transform.Find("ItemNameText")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null)
            nameText.text = item.itemName;

        var costText = entry.transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();
        if (costText != null)
            costText.text = $"{item.friendPointCost} FP";

        var quantityText = entry.transform.Find("QuantityText")?.GetComponent<TextMeshProUGUI>();
        if (quantityText != null)
            quantityText.text = $"x{item.quantity}";

        var stockText = entry.transform.Find("StockText")?.GetComponent<TextMeshProUGUI>();
        if (stockText != null)
        {
            if (item.stock < 0)
                stockText.text = "Unlimited";
            else
                stockText.text = $"Stock: {item.stock}";
        }

        var buyBtn = entry.transform.Find("BuyButton")?.GetComponent<Button>();
        if (buyBtn != null)
            buyBtn.onClick.AddListener(() => OnPurchaseItem(item.itemId));
    }

    #endregion

    #region Support Heroes

    public void RefreshSupportHeroes()
    {
        if (FriendManager.Instance == null) return;

        var friends = FriendManager.Instance.GetFriends();

        // Display available support heroes
        // Implementation similar to friend list but focused on support hero availability

        Debug.Log($"[FriendListUI] Refreshed support hero list");
    }

    #endregion

    #region Button Handlers

    private void OnSearchFriend()
    {
        if (searchInput == null || string.IsNullOrEmpty(searchInput.text))
        {
            Debug.Log("[FriendListUI] Enter a player name or ID to search");
            return;
        }

        string searchTerm = searchInput.text;
        Debug.Log($"[FriendListUI] Searching for: {searchTerm}");

        // In real implementation, would query server for player
    }

    private void OnSendRequest()
    {
        if (searchInput == null || string.IsNullOrEmpty(searchInput.text))
        {
            Debug.Log("[FriendListUI] Enter a player ID to send request");
            return;
        }

        string playerId = searchInput.text;
        bool success = FriendManager.Instance.SendFriendRequest(playerId);

        if (success)
        {
            Debug.Log("[FriendListUI] Friend request sent!");
            searchInput.text = "";
        }
    }

    private void OnAcceptRequest(string requestId)
    {
        bool success = FriendManager.Instance.AcceptFriendRequest(requestId);
        if (success)
        {
            RefreshRequestList();
            RefreshFriendList();
            Debug.Log("[FriendListUI] Friend request accepted!");
        }
    }

    private void OnRejectRequest(string requestId)
    {
        bool success = FriendManager.Instance.RejectFriendRequest(requestId);
        if (success)
        {
            RefreshRequestList();
            Debug.Log("[FriendListUI] Friend request rejected");
        }
    }

    private void OnRemoveFriend(string playerId)
    {
        bool success = FriendManager.Instance.RemoveFriend(playerId);
        if (success)
        {
            RefreshFriendList();
            Debug.Log("[FriendListUI] Friend removed");
        }
    }

    private void OnToggleFavorite(string playerId)
    {
        FriendManager.Instance.ToggleFavorite(playerId);
        RefreshFriendList();
    }

    private void OnUseSupportHero(string friendId)
    {
        var supportHero = FriendManager.Instance.RentSupportHero(friendId);
        if (supportHero != null)
        {
            Debug.Log($"[FriendListUI] Rented support hero: {supportHero.heroName}");
            // In real implementation, would add hero to party for next battle
        }
    }

    private void OnPurchaseItem(string itemId)
    {
        bool success = FriendManager.Instance.PurchaseFriendShopItem(itemId);
        if (success)
        {
            RefreshShop();
            Debug.Log("[FriendListUI] Item purchased!");
        }
    }

    #endregion
}
