// using UnityEngine;

// public class Song
// {
//     public string name;
//     public string genre;
//     public int duration;
// }

// public class Program
// {
//     static List<Song> songNames = new List<Song>();

//     static void Main()
//     {
//         
//         Song luna = new Song();
//         luna.name = "Luna";
//         luna.genre = "Ethereal";
//         luna.duration = 206;

//         Song gotYou = new Song();
//         gotYou.name = "Got You";
//         gotYou.genre = "Electronic";
//         gotYou.duration = 204;

//         Song dianYuZhang = new Song();
//         dianYuZhang.name = "Dian Yu Zhang";
//         dianYuZhang.genre = "Dramatic";
//         dianYuZhang.duration = 138;

//         Song dawn = new Song();
//         dawn.name = "Dawn";
//         dawn.genre = "Rock";
//         dawn.duration = 236;

//         Song backgroundMusic = new Song();
//         backgroundMusic.name = "Background Music";
//         backgroundMusic.genre = "Rock";
//         backgroundMusic.duration = 176;

//         songNames.Add(luna);
//         songNames.Add(gotYou);
//         songNames.Add(dianYuZhang);
//         songNames.Add(dawn);
//         songNames.Add(backgroundMusic);

//         CombinationSort();



// // public void MusicBubbleSort()
// {
//     int n = songNames.Count;

//     for (int i = 0; i < n - 1; i++)
//     {
//         for (int j = 0; j < n - i - 1; j++)
//         {
//              if (string.Compare(songNames[j].genre, songNames[j + 1].genre) > 0)
    //             {
    //                 Song temp = songNames[j];
    //                 songNames[j] = songNames[j + 1];
    //                 songNames[j + 1] = temp;
    //             }
    //         }
    //     }
    // }


// public void MusicExchangeSort()
// {
//     int n = songNames.Count;

//     for (int i = 0; i < n - 1; i++)
//     {
//         for (int j = i + 1; j < n; j++)
//         {
//            
//              if (songNames[i].duration > songNames[j].duration)
                // {
                //     Song temp = songNames[i];
                //     songNames[i] = songNames[j];
                //     songNames[j] = temp;
//                 }
//         }
//      }
// }
//     public static void CombinationSort()
//     {
//         MusicBubbleSort(); 

//         List<string> genreList = new List<string>();
//         List<Song> sorted = new List<Song>();

//         for (int i = 0; i < songNames.Count; i++)
//         {
//             if (!genreList.Contains(songNames[i].genre))
//             {
//                 genreList.Add(songNames[i].genre);

//                
//                 List<Song> currentGenreSongs = new List<Song>();

//                 for (int j = 0; j < songNames.Count; j++)
//                 {
//                     if (songNames[j].genre == songNames[i].genre)
//                     {
//                         currentGenreSongs.Add(songNames[j]);
//                     }
//                 }
//                 for (int a = 0; a < currentGenreSongs.Count - 1; a++)

//                 {
//                     for (int b = a + 1; b < currentGenreSongs.Count; b++)
//                     {
//                         if (currentGenreSongs[a].duration > currentGenreSongs[b].duration)
//                         {
//                             Song temp = currentGenreSongs[a];
//                             currentGenreSongs[a] = currentGenreSongs[b];
//                             currentGenreSongs[b] = temp;
//                         }
//                     }
//                 }

//               
//                 sorted.AddRange(currentGenreSongs);
//             }
//         }

//         songNames = sorted; 
//     }
// }





