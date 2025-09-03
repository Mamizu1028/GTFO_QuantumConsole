using Il2CppInterop.Runtime.Attributes;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Hikaria.QC
{
    //[Il2CppImplements(typeof(IPointerClickHandler))]
    public class SuggestionDisplay : MonoBehaviour
    {
        private QuantumConsole _quantumConsole = null;
        private TextMeshProUGUI _textArea = null;

        public void OnPointerClick(PointerEventData eventData)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(_textArea, eventData.position, null);
            if (linkIndex >= 0)
            {
                TMP_LinkInfo link = _textArea.textInfo.linkInfo[linkIndex];
                if (int.TryParse(link.GetLinkID(), out int suggestionIndex))
                {
                    _quantumConsole.SetSuggestion(suggestionIndex);
                }
            }
        }

        [HideFromIl2Cpp]
        internal void Setup(QuantumConsole console, TextMeshProUGUI textArea)
        {
            _quantumConsole = console;
            _textArea = textArea;

            var eventTrigger = gameObject.AddComponent<EventTrigger>();

            var onPointerClickEntry = new EventTrigger.Entry();
            onPointerClickEntry.eventID = EventTriggerType.PointerClick;
            onPointerClickEntry.callback.AddListener(new Action<BaseEventData>((data) => { OnPointerClick(data.Cast<PointerEventData>()); }));
            eventTrigger.triggers.Add(onPointerClickEntry);
        }
    }
}