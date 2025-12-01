using UnityEngine;

public class TutorialBridge_Moncho : MonoBehaviour
{
    [Header("Refs opcionales (si existen en tu escena)")]
    [SerializeField] private PlateContainer plate;
    [SerializeField] private CraftingManager crafting;
    [SerializeField] private LoteriaService loteria;

    bool _mostroPrimerIngrediente = false;

    void OnEnable()
    {
        if (plate != null) plate.OnContentChanged += OnPlateChanged;

        if (crafting != null)
        {
            crafting.OnDishCrafted += OnDishCrafted;
            crafting.OnDishDelivered += OnDishDelivered;
        }

        if (loteria != null) loteria.OnBoardChanged += OnBoardChanged;
    }

    void OnDisable()
    {
        if (plate != null) plate.OnContentChanged -= OnPlateChanged;

        if (crafting != null)
        {
            crafting.OnDishCrafted -= OnDishCrafted;
            crafting.OnDishDelivered -= OnDishDelivered;
        }

        if (loteria != null) loteria.OnBoardChanged -= OnBoardChanged;
    }

    void OnPlateChanged()
    {
        TutorialEvents.Raise("PlatoCambio");

        if (!_mostroPrimerIngrediente)
        {
            _mostroPrimerIngrediente = true;
            TutorialEvents.Raise("PrimerIngredienteColocado");
        }
    }

    void OnDishCrafted(RecipeSO.DishData dish, RecipeSO recipe)
    {
        TutorialEvents.Raise("PlatilloCrafteado");
    }

    void OnDishDelivered(CraftingManager.DeliveryPayload payload)
    {
        TutorialEvents.Raise("PedidoEntregado");
        if (payload.matched) TutorialEvents.Raise("PedidoCorrecto");
        else TutorialEvents.Raise("PedidoIncorrecto");
    }

    void OnBoardChanged()
    {
        TutorialEvents.Raise("LoteriaActualizada");
    }
}