using System;

namespace KK.Core.Texts
{
    // Serializable из System, а не из UnityEngine: Core без ссылок на движок, а Unity всё равно покажет поля в инспекторе
    [Serializable]
    public class TextFragmentOptions
    {
        // TODO: брать из настроек игрока
        public int MinLength = 300;
        public int MaxLength = 600;
    }
}
