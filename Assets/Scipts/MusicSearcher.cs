using UnityEngine;

public AudioClip BinarySearch(string searchName)
{
    int left = 0;
    int right = songs.Length - 1;

    searchName = searchName.ToLower();

    while (left <= right)
    {
        int mid = (left + right) / 2;
        string midName = songs[mid].name.ToLower();

        int comparison = string.Compare(midName, searchName);

        if (comparison == 0)
            return songs[mid]; 
        else if (comparison < 0)
            left = mid + 1;
        else
            right = mid - 1;
    }

    return null; 
}
