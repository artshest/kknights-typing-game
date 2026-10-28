using System;
using System.Collections.Generic;
using UnityEngine;

namespace KK.Infrastructure.Texts
{
    [CreateAssetMenu(fileName = "TextLibrary", menuName = "KeyboardKnights/Text Library")]
    public class TextLibrary : ScriptableObject
    {
        [SerializeField]
        [TextArea(5, 15)]
        private string[] _texts = Array.Empty<string>();

        public IReadOnlyList<string> Texts => _texts;
    }
}
