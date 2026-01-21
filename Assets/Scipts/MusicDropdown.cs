using UnityEngine;
using TMPro;

public class MusicDropdownTMP : MonoBehaviour
{
    private const string PREF_INDEX = "music_index";

    [Header("References")]
    [SerializeField] private TMP_Dropdown dropdown;

    private MusicPlayer player;

    private void Awake()
    {
        if (dropdown == null) dropdown = GetComponent<TMP_Dropdown>();

        player = MusicPlayer.Instance != null ? MusicPlayer.Instance : FindFirstObjectByType<MusicPlayer>();

        if (dropdown != null)
        {
            dropdown.onValueChanged.RemoveListener(OnChanged);
            dropdown.onValueChanged.AddListener(OnChanged);
        }

        Refresh();
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        player = MusicPlayer.Instance != null ? MusicPlayer.Instance : FindFirstObjectByType<MusicPlayer>();

        if (player == null)
        {
            Debug.LogWarning("[MusicDropdownTMP] MusicPlayer not found.");
            return;
        }

        if (dropdown == null) return;

        dropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        for (int i = 0; i < player.SongCount; i++)
            options.Add(player.GetSongName(i));

        dropdown.AddOptions(options);

        int index = PlayerPrefs.GetInt(PREF_INDEX, 0);
        index = Mathf.Clamp(index, 0, Mathf.Max(0, dropdown.options.Count - 1));
        dropdown.SetValueWithoutNotify(index);
    }

    private void OnChanged(int index)
    {
        PlayerPrefs.SetInt(PREF_INDEX, index);
        PlayerPrefs.Save();

        player = MusicPlayer.Instance != null ? MusicPlayer.Instance : FindFirstObjectByType<MusicPlayer>();

        if (player != null)
            player.PlayIndex(index);
        else
            Debug.LogWarning("[MusicDropdownTMP] MusicPlayer not found when selecting song.");
    }
}
