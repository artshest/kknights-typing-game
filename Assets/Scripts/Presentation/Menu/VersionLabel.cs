using TMPro;
using UnityEngine;

namespace KK.Presentation.Menu
{
    public class VersionLabel : MonoBehaviour
    {
        // Файл кладёт в сборку тулза KK/Build (BuildRunner) только на время сборки,
        // поэтому в редакторе и в сборках не через тулзу хэша нет.
        private const string CommitResourceName = "BuildCommit";

        [SerializeField]
        private TextMeshProUGUI _text;

        private void Awake()
        {
            var commit = Resources.Load<TextAsset>(CommitResourceName);
            _text.text = VersionFormatter.Format(Application.version, commit != null ? commit.text : null, Debug.isDebugBuild);
        }
    }
}
