// using UnityEngine;

// public void MusicBubbleSort()
// {
//     int n = songs.Length;

//     for (int i = 0; i < n - 1; i++)
//     {
//         for (int j = 0; j < n - i - 1; j++)
//         {
//             string genreA = songs[j].genre.ToLower();
//             string genreB = songs[j + 1].genre.ToLower();

//             if (string.Compare(genreA, genreB) > 0)
//             {
//                 MusicList temp = songs[j];
//                 songs[j] = songs[j + 1];
//                 songs[j + 1] = temp;
//             }
//         }
//  }
// public void MusicExchangeSort()
// {
//     int n = songs.Length;

//     for (int i = 0; i < n - 1; i++)
//     {
//         for (int j = i + 1; j < n; j++)
//         {
//            
//             if (songs[i].length > songs[j].length)
//             {
//             
//                 AudioClip temp = songs[i];
//                 songs[i] = songs[j];
//                 songs[j] = temp;
//             }
//         }
//     }
// }
