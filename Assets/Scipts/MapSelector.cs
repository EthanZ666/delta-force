// using UnityEngine;

// public class MapSelector : MonoBehaviour
// {

//     public MapData[] maps;
//     public Button[] mapButtons;
    
//     public class MapSettings
//     {
//         public string mapName;
//         public string mapDescription;
//         public Sprite mapImage;
//     }
//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {
//         DisplayButtons();
//     }
    
//     void DisplayMap (string mapName)
//     {
//         SceneManager.LoadScene(mapName);
//     }

//         void SetupButtons()
//     {
//         for (int i = 0; i < mapButtons.Length; i++)
//         {
//             int index = i;

//             mapButtons[i].interactable = true;

//             mapButtons[i].onClick.RemoveAllListeners();
//             mapButtons[i].onClick.AddListener(() =>
//             {
//                 DisplayMap(maps[index].mapName);
//             });
