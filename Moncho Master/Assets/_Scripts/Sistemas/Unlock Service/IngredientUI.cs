using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IngredientUI : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private UnlockService unlockService;
    [SerializeField] private IngredientSO ingredient;
    [SerializeField] private bool hideWhenLocked = false;
    [SerializeField] private float lockedAlpha = 0.4f;

    [Header("UI Components")]
    [SerializeField] private Selectable uiSelectable;
    [SerializeField] private Image ingredientImage;

    [Header("Particle System")]
    [SerializeField] private ParticleSystem unlockParticleSystem;

    private Color _originalImageColor;
    private bool _isInitialized = false;
    private bool _hasValidIngredient = false;
    private bool _wasUnlocked = false;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        if (_isInitialized) return;

        if (uiSelectable == null)
            uiSelectable = GetComponent<Selectable>();

        if (ingredientImage == null)
            ingredientImage = GetComponent<Image>();

        if (ingredientImage != null)
            _originalImageColor = ingredientImage.color;

        _hasValidIngredient = ingredient != null && !string.IsNullOrEmpty(ingredient.Id);
        _isInitialized = true;
    }

    private void OnEnable()
    {
        if (!_isInitialized) InitializeComponents();

        unlockService.OnUnlocksChanged += RefreshVisuals;
        StartCoroutine(RefreshAfterDelay());
    }

    private IEnumerator RefreshAfterDelay()
    {
        yield return null;
        RefreshVisuals();
    }

    private void OnDisable()
    {
        if (unlockService != null)
            unlockService.OnUnlocksChanged -= RefreshVisuals;
    }

    private void Start()
    {
        RefreshVisuals();
    }

    public void RefreshVisuals()
    {
        if (!_isInitialized) InitializeComponents();

        if (!_hasValidIngredient || unlockService == null)
            return;

        bool isUnlocked = unlockService.IsUnlocked(ingredient);

        if (!_wasUnlocked && isUnlocked && unlockParticleSystem != null)
        {
            unlockParticleSystem.Play();
        }

        _wasUnlocked = isUnlocked;
        UpdateVisualState(isUnlocked);
    }

    private void UpdateVisualState(bool isUnlocked)
    {
        if (uiSelectable != null)
        {
            uiSelectable.interactable = isUnlocked;
        }

        if (ingredientImage != null)
        {
            Color newColor = _originalImageColor;
            newColor.a = isUnlocked ? _originalImageColor.a : lockedAlpha;
            ingredientImage.color = newColor;
        }

        if (hideWhenLocked)
        {
            gameObject.SetActive(isUnlocked);
        }
    }
}