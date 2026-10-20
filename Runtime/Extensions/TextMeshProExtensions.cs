using TMPro;
using UnityEngine.EventSystems;

namespace KOOP.Extensions
{
    public static class TextMeshProExtensions
    {
        /// <summary>
        /// Deactivates the given input field AND deselects it so that it does not immediately get re-selected
        /// if a submit or cancel input is invoked
        /// </summary>
        public static void DeactivateAndDeselectInputField(this TMP_InputField inputField)
        {
            inputField.DeactivateInputField();
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }
}