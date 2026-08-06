using UnityEngine;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.UI.Headquarters;

namespace GuildMaster.Runtime.UI.Shell
{
    /// <summary>
    /// Phase 4: Headquarters Hub — populates the 6 building cards with live backend data and
    /// opens the implemented headquarters dialogs or a minimal placeholder popup through
    /// <see cref="AppShellController"/>'s PopupRoot when a card is clicked. Reads existing
    /// services only — never writes, never touches SaveData directly.
    /// </summary>
    public class HeadquartersHubController : MonoBehaviour
    {
        [SerializeField] private BuildingCardView _quartersCard;
        [SerializeField] private BuildingCardView _tavernCard;
        [SerializeField] private BuildingCardView _storageCard;
        [SerializeField] private BuildingCardView _marketCard;
        [SerializeField] private BuildingCardView _workshopCard;
        [SerializeField] private BuildingCardView _shelterCard;

        [SerializeField] private GameObject _popupPrefabSource; // template instance, cloned per open

        [Header("Phase 5: Dialog Prefabs")]
        [SerializeField] private GameObject _quartersDialogPrefab;
        [SerializeField] private GameObject _tavernDialogPrefab;
        [SerializeField] private GameObject _storageDialogPrefab;
        [SerializeField] private GameObject _workshopDialogPrefab;
        [SerializeField] private GameObject _shelterDialogPrefab;
        [SerializeField] private GameObject _marketDialogPrefab;

        private ServiceContainer _services;
        private AppShellController _shell;
        private GameObject _activePopupInstance;
        private string _activePopupFeatureId;

        public void Initialize(ServiceContainer services, AppShellController shell)
        {
            _services = services;
            _shell = shell;

            WireCard(_quartersCard, "quarters");
            WireCard(_tavernCard, "tavern");
            WireCard(_storageCard, "storage");
            WireCard(_marketCard, "market");
            WireCard(_workshopCard, "workshop");
            WireCard(_shelterCard, "shelter");

            RefreshCards();
        }

        private void WireCard(BuildingCardView card, string featureId)
        {
            if (card == null || card.Button == null) return;
            card.Button.onClick.RemoveAllListeners();
            card.Button.onClick.AddListener(() => OpenBuildingPopup(featureId));
        }

        /// <summary>Re-reads backend state into all 6 cards. Safe to call any time after Initialize.</summary>
        public void RefreshCards()
        {
            if (_services == null) return;

            if (_quartersCard != null)
            {
                int current = _services.Character.GetAllCharacters().Count;
                int cap = _services.Tavern.GetQuartersCapacity();
                _quartersCard.SetData("Quarters", $"{current}/{cap}", GetSign("sign_quarters"), false);
            }

            if (_tavernCard != null)
            {
                int current = _services.Tavern.GetGuests().Count;
                int cap = _services.Tavern.GetTavernCapacity();
                _tavernCard.SetData("Tavern", $"{current}/{cap}", GetSign("sign_tavern"), false);
            }

            if (_storageCard != null)
            {
                int current = _services.Inventory.GetAllItems().Count;
                int cap = _services.Inventory.GetCapacity();
                _storageCard.SetData("Storage", $"{current}/{cap}", GetSign("sign_storage"), false);
            }

            if (_marketCard != null)
            {
                // Phase 5F: IMerchantService has a real Selling/Sold flow — show live counts
                // instead of the earlier "Coming soon" placeholder.
                int selling = _services.Merchant.GetMarketListings().Count;
                int sold = _services.Merchant.GetSoldMarketItems().Count;
                _marketCard.SetData("Market", $"Selling {selling} • Sold {sold}", GetSign("sign_market"), false);
            }

            if (_workshopCard != null)
            {
                int current = _services.Craft.GetQueue().Count;
                int cap = _services.Craft.GetQueueCapacity();
                _workshopCard.SetData("Workshop", $"{current}/{cap}", GetSign("sign_workshop"), false);
            }

            if (_shelterCard != null)
            {
                // IPetService exposes no capacity getter — count-only, no denominator invented.
                int current = _services.Pet.GetAllPets().Count;
                _shelterCard.SetData("Shelter", $"{current} pet(s)", GetSign("sign_shelter"), false);
            }
        }

        private Sprite GetSign(string legacyName) => Legacy.LegacySpriteRegistry.GetSprite(legacyName);

        private void OpenBuildingPopup(string featureId)
        {
            if (_shell == null) return;

            // Singleton guard: refuse if this exact feature's popup is already open.
            if (_shell.IsPopupOpen && _activePopupFeatureId == featureId)
            {
                Debug.LogWarning($"[HeadquartersHubController] '{featureId}' popup is already open.");
                return;
            }

            GameObject instance = null;

            if (featureId == "quarters" && _quartersDialogPrefab != null)
            {
                instance = Instantiate(_quartersDialogPrefab);
                instance.name = $"Popup_{featureId}";
                var dialog = instance.GetComponent<QuartersDialog>();
                dialog.Setup(_services, 
                    onClose: () =>
                    {
                        _shell.ClosePopup();
                        _activePopupInstance = null;
                        _activePopupFeatureId = null;
                    },
                    onStateChanged: () =>
                    {
                        RefreshCards();
                        _shell.RefreshHud();
                    });
            }
            else if (featureId == "tavern" && _tavernDialogPrefab != null)
            {
                instance = Instantiate(_tavernDialogPrefab);
                instance.name = $"Popup_{featureId}";
                var dialog = instance.GetComponent<TavernDialog>();
                if (dialog == null)
                {
                    Destroy(instance);
                    Debug.LogError("[HeadquartersHubController] Tavern dialog prefab is missing TavernDialog.");
                    return;
                }

                dialog.Setup(_services,
                    onClose: () =>
                    {
                        _shell.ClosePopup();
                        _activePopupInstance = null;
                        _activePopupFeatureId = null;
                    },
                    onStateChanged: () =>
                    {
                        RefreshCards();
                        _shell.RefreshHud();
                    });
            }
            else if (featureId == "storage" && _storageDialogPrefab != null)
            {
                instance = Instantiate(_storageDialogPrefab);
                instance.name = $"Popup_{featureId}";
                var dialog = instance.GetComponent<StorageDialog>();
                if (dialog == null)
                {
                    Destroy(instance);
                    Debug.LogError("[HeadquartersHubController] Storage dialog prefab is missing StorageDialog.");
                    return;
                }

                dialog.Setup(_services,
                    onClose: () =>
                    {
                        _shell.ClosePopup();
                        _activePopupInstance = null;
                        _activePopupFeatureId = null;
                    },
                    onStateChanged: () =>
                    {
                        RefreshCards();
                        _shell.RefreshHud();
                    });
            }
            else if (featureId == "workshop" && _workshopDialogPrefab != null)
            {
                instance = Instantiate(_workshopDialogPrefab);
                instance.name = $"Popup_{featureId}";
                var dialog = instance.GetComponent<WorkshopDialog>();
                if (dialog == null)
                {
                    Destroy(instance);
                    Debug.LogError("[HeadquartersHubController] Workshop dialog prefab is missing WorkshopDialog.");
                    return;
                }

                dialog.Setup(_services,
                    onClose: () =>
                    {
                        _shell.ClosePopup();
                        _activePopupInstance = null;
                        _activePopupFeatureId = null;
                    },
                    onStateChanged: () =>
                    {
                        RefreshCards();
                        _shell.RefreshHud();
                    });
            }
            else if (featureId == "shelter" && _shelterDialogPrefab != null)
            {
                instance = Instantiate(_shelterDialogPrefab);
                instance.name = $"Popup_{featureId}";
                var dialog = instance.GetComponent<ShelterDialog>();
                if (dialog == null)
                {
                    Destroy(instance);
                    Debug.LogError("[HeadquartersHubController] Shelter dialog prefab is missing ShelterDialog.");
                    return;
                }

                dialog.Setup(_services,
                    onClose: () =>
                    {
                        _shell.ClosePopup();
                        _activePopupInstance = null;
                        _activePopupFeatureId = null;
                    },
                    onStateChanged: () =>
                    {
                        RefreshCards();
                        _shell.RefreshHud();
                    });
            }
            else if (featureId == "market" && _marketDialogPrefab != null)
            {
                instance = Instantiate(_marketDialogPrefab);
                instance.name = $"Popup_{featureId}";
                var dialog = instance.GetComponent<MarketDialog>();
                if (dialog == null)
                {
                    Destroy(instance);
                    Debug.LogError("[HeadquartersHubController] Market dialog prefab is missing MarketDialog.");
                    return;
                }

                dialog.Setup(_services,
                    onClose: () =>
                    {
                        _shell.ClosePopup();
                        _activePopupInstance = null;
                        _activePopupFeatureId = null;
                    },
                    onStateChanged: () =>
                    {
                        RefreshCards();
                        _shell.RefreshHud();
                    });
            }
            else
            {
                // Fallback for unimplemented dialogs (Phase 4 placeholder)
                if (_popupPrefabSource == null) return;
                instance = Instantiate(_popupPrefabSource);
                instance.name = $"Popup_{featureId}";
                var panel = instance.GetComponent<SimplePopupPanel>();
                string title = char.ToUpperInvariant(featureId[0]) + featureId.Substring(1);
                panel.Setup(featureId, title, () =>
                {
                    _shell.ClosePopup();
                    _activePopupInstance = null;
                    _activePopupFeatureId = null;
                });
            }

            bool opened = _shell.OpenPopup(instance);
            if (!opened)
            {
                Destroy(instance);
                return;
            }

            _activePopupInstance = instance;
            _activePopupFeatureId = featureId;
        }
    }
}
