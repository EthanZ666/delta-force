// using UnityEngine;

// public class SongSearch
// {
       // gets the song names from the list then sorts the names alphabetically
//     public static List<string> GetSongNamesList(List<Song> songNames)
//     {
//         List<string> songNamesList = new List<string>();
//         foreach (Song song in songNames)
//         {
//             songNamesList.Add(song.name);
//         }
      
//         songNamesList.Sort(StringComparer.OrdinalIgnoreCase);
      
//         return songNamesList;
//     }

       // binary search to find the keyword the user searches within the song names
//     public static int BinarySearch(List<string> sortedList, string keyword)
//     {

//         int left = 0;
//         int right = sortedList.Count - 1;

//         while (left <= right)
//         {
//             int mid = left + (right - left) / 2;

//             int comparison = string.Compare(sortedList[mid], keyword, StringComparison.OrdinalIgnoreCase);

//             if (comparison == 0)
//             {
//                 return mid;
//             }
//             else if (comparison < 0)
//             {
//                 left = mid + 1; 
//             }
//             else
//             {
//                 right = mid - 1; 
//             }
//         }

//         return -1; 
//     }
// }
