using System.Collections.Generic;
using UnityEngine;

public class DangerHighlighter2D : MonoBehaviour
{
    [Header("Label")]
    [SerializeField] private DangerLabel2D labelPrefab;
    [SerializeField] private Vector3 labelOffset = new Vector3(0f, 0.8f, 0f);
    [SerializeField] private Transform labelParent;

    [Header("Selection")]
    [SerializeField, Min(1)] private int topCount = 3;
    [SerializeField, Min(0.05f)] private float refreshInterval = 0.25f;

    private readonly Dictionary<EnemyBase, DangerLabel2D> labels = new();
    private float nextRefreshTime;

    private struct Entry
    {
        public EnemyBase enemy;
        public int level;
        public float score01;
    }

    private void Update()
    {
        if (Time.time < nextRefreshTime) return;
        nextRefreshTime = Time.time + refreshInterval;

        RefreshTopDangerLabel2Ds();
    }

    private void RefreshTopDangerLabel2Ds()
    {
        if (labelPrefab == null) return;

        // 1) Collect alive enemies
        EnemyBase[] allEnemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        List<Entry> temp = new List<Entry>(allEnemies.Length);

        foreach (var e in allEnemies)
        {
            if (e == null) continue;
            if (e.IsDead) continue;

            temp.Add(new Entry
            {
                enemy = e,
                level = e.DangerLevel,
                score01 = e.DangerScore01
            });
        }

        // 2) Put danger levels in an array
        int[] dangerLevels = new int[temp.Count];
        for (int i = 0; i < temp.Count; i++)
            dangerLevels[i] = temp[i].level;

        // 3) Bubble sort entries by danger (highest first)
        Entry[] entries = temp.ToArray();
        BubbleSortEntries(entries);

        // 4) Pick top N enemies
        HashSet<EnemyBase> topSet = new HashSet<EnemyBase>();
        int count = Mathf.Min(topCount, entries.Length);

        for (int i = 0; i < count; i++)
            topSet.Add(entries[i].enemy);

        // 5) Show labels for top enemies
        for (int i = 0; i < count; i++)
        {
            EnemyBase e = entries[i].enemy;
            if (e == null) continue;

            if (!labels.TryGetValue(e, out DangerLabel2D label) || label == null)
            {
                label = Instantiate(labelPrefab, labelParent);
                labels[e] = label;
            }

            label.AttachTo(e.transform, labelOffset);
            label.SetValue(entries[i].level);
        }

        // 6) Hide labels for non-top + cleanup dead
        var keys = new List<EnemyBase>(labels.Keys);
        foreach (var e in keys)
        {
            if (e == null || e.IsDead)
            {
                if (labels[e] != null) Destroy(labels[e].gameObject);
                labels.Remove(e);
                continue;
            }

            if (!topSet.Contains(e))
            {
                if (labels[e] != null) labels[e].Hide();
            }
        }
    }

    private void BubbleSortEntries(Entry[] arr)
    {
        int n = arr.Length;
        if (n <= 1) return;

        for (int pass = 0; pass < n - 1; pass++)
        {
            bool swapped = false;

            for (int i = 0; i < n - 1 - pass; i++)
            {
                if (ShouldSwap(arr[i], arr[i + 1]))
                {
                    Entry tmp = arr[i];
                    arr[i] = arr[i + 1];
                    arr[i + 1] = tmp;
                    swapped = true;
                }
            }

            if (!swapped) break;
        }
    }

    // swap if "a" should come after "b" (descending order)
    private bool ShouldSwap(Entry a, Entry b)
    {
        if (a.level != b.level)
            return a.level < b.level;       // higher level first

        return a.score01 < b.score01;       // tie-breaker: higher score first
    }
}
