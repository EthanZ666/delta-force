// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class RuntimeUIUpdater : MonoBehaviour
// {
//     private static readonly List<(float t, Action a)> _timers = new();

//     private EnemyBase _boss;
//     private bool _bossAnnounced;

//     private void Start()
//     {
//         // 进关卡先来个 Ready
//         RuntimeUIGenerator.ShowCenter("READY", 0.9f);
//         RunAfter(1.0f, () => RuntimeUIGenerator.ShowCenter("WAVE 1", 1.0f));
//     }

//     private void Update()
//     {
//         UpdateTimers();
//         UpdateHUD();
//         UpdateBossDetection();
//         UpdateReturnHotkeyWhenResult();
//     }

//     private void UpdateHUD()
//     {
//         int money = -1;
//         var mm = FindObjectOfType<MoneyManager>();
//         if (mm != null) money = mm.Money; // MoneyManager.Money :contentReference[oaicite:2]{index=2}

//         // 生命/波数你们目前没现成系统，我先显示占位（后面你加 BaseHealth/WaveManager 再接）
//         string hp = "HP: (todo)";
//         string wave = "Wave: 1";

//         var scene = SceneManager.GetActiveScene().name;
//         RuntimeUIGenerator.SetHUDText(
//             $"Scene: {scene}\nMoney: {(money >= 0 ? money.ToString() : "N/A")}\n{hp}\n{wave}\n\nTips: Esc pause, +/- volume"
//         );
//     }

//     private void UpdateBossDetection()
//     {
//         // 找一个“血量最高”的 EnemyBase 当 Boss（不需要你在 Unity 里打 Tag）
//         // EnemyBase.MaxHealth 可读 :contentReference[oaicite:3]{index=3}
//         if (_boss == null)
//         {
//             var enemies = FindObjectsOfType<EnemyBase>();
//             float best = -1f;
//             EnemyBase pick = null;

//             foreach (var e in enemies)
//             {
//                 if (e == null || e.IsDead) continue;
//                 if (e.MaxHealth > best)
//                 {
//                     best = e.MaxHealth;
//                     pick = e;
//                 }
//             }

//             _boss = pick;
//             _bossAnnounced = false;

//             if (_boss != null)
//             {
//                 _boss.Died += OnBossDied; // EnemyBase.Died :contentReference[oaicite:4]{index=4}
//             }
//         }

//         if (_boss != null && !_bossAnnounced)
//         {
//             // 给 boss 出场一点“仪式感”
//             _bossAnnounced = true;
//             RuntimeUIGenerator.ShowCenter("BOSS APPROACHING", 1.2f);
//         }
//     }

//     private void OnBossDied(EnemyBase boss)
//     {
//         // 胜利结算
//         RuntimeUIGenerator.ShowResult("VICTORY!");
//         Time.timeScale = 0f;
//     }

//     private void UpdateReturnHotkeyWhenResult()
//     {
//         // 结算界面统一用 Backspace 回主菜单（不改 Unity）
//         if (Input.GetKeyDown(KeyCode.Backspace))
//         {
//             // 如果你们用 GameHotkeys 已经做了 Backspace 回主菜单，这里也不会出大事
//             // 但为了稳，先恢复时间
//             Time.timeScale = 1f;

//             // 这里写你主菜单 scene 名（你截图里是 MainMenuScene）
//             SceneManager.LoadScene("MainMenuScene");
//         }
//     }

//     // --------- Timer helper ----------
//     public static void RunAfter(float seconds, Action action)
//     {
//         if (action == null) return;
//         _timers.Add((Time.unscaledTime + seconds, action));
//     }

//     private void UpdateTimers()
//     {
//         float now = Time.unscaledTime;
//         for (int i = _timers.Count - 1; i >= 0; i--)
//         {
//             if (now >= _timers[i].t)
//             {
//                 var a = _timers[i].a;
//                 _timers.RemoveAt(i);
//                 try { a?.Invoke(); } catch { }
//             }
//         }
//     }

//     private void OnDestroy()
//     {
//         if (_boss != null) _boss.Died -= OnBossDied;
//     }
// }
