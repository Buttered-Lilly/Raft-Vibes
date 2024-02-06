using BepInEx;
//using HarmonyLib;
using UnityEngine;
using UnityEditor;
using Buttplug;
using System;
//using System.Threading.Tasks;

namespace Raft_Vibes
{
    [BepInPlugin("Lilly.Raft_Vibes", "Raft Vibes", "1.0.0")]
    [BepInProcess("Raft.exe")]

    public class Raft_Vibes : BaseUnityPlugin
    {
        async void Awake()
        {
            await Run();
        }


        private async System.Threading.Tasks.Task Run()
        {
            var connector = new ButtplugEmbeddedConnectorOptions();
            var client = new ButtplugClient("Cult Of The Vibe Client");

            try
            {
                await client.ConnectAsync(connector);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Can't connect, exiting!");
                Debug.LogWarning($"Message: {ex.InnerException.Message}");
                return;
            }
            Debug.LogWarning("Client Connected");

        devicelost:
            await client.StartScanningAsync();
            while (client.Devices.Length == 0)
                await System.Threading.Tasks.Task.Delay(5000);
            await client.StopScanningAsync();
            Debug.LogWarning("Client currently knows about these devices:");
            foreach (var device in client.Devices)
            {
                Debug.LogWarning($"- {device.Name}");
            }

            foreach (var device in client.Devices)
            {
                Debug.LogWarning($"{device.Name} supports these messages:");
                foreach (var msgInfo in device.AllowedMessages)
                {
                    Debug.LogWarning($"- {msgInfo.Key.ToString()}");
                    if (msgInfo.Value.FeatureCount != 0)
                    {
                        Debug.LogWarning($" - Features: {msgInfo.Value.FeatureCount}");
                    }
                }
            }
            var testClientDevice = client.Devices;
            Debug.LogWarning("Sending commands");

            GameObject Player_Health = null;
            GameObject Player_Hunger = null;
            GameObject Player_Thirst = null;
            while (true)
            {
                if (Player_Health != null)
                {
                    try
                    {
                        float power_Health = (1 - (float)Player_Health.GetComponent<Stat_Health>().Value / (float)Player_Health.GetComponent<Stat_Health>().Max);
                        float power_Hunger = (1 - (float)Player_Hunger.GetComponent<Stat>().Value / (float)Player_Hunger.GetComponent<Stat>().Max);
                        float power_Thirst = (1 - (float)Player_Thirst.GetComponent<Stat>().Value / (float)Player_Thirst.GetComponent<Stat>().Max);
                        await System.Threading.Tasks.Task.Delay(500);
                        for (int i = 0; i < testClientDevice.Length; i++)
                        {
                            await testClientDevice[i].SendVibrateCmd(Math.Max(power_Health, Math.Max(power_Hunger, power_Thirst)));
                            //Debug.LogWarning("Power health = " + power_Health + " Power hunger = " + power_Hunger + " Power thirst = " + power_Thirst);
                            //Debug.LogWarning("Power = " + Math.Max(power_Health, Math.Max(power_Hunger, power_Thirst)));
                        }
                    }
                    catch (ButtplugDeviceException)
                    {
                        Debug.LogWarning("device lost");
                        goto devicelost;
                    }
                    catch (Exception)
                    {
                        Debug.LogWarning("player lost");
                        Player_Health = null;
                        for (int i = 0; i < testClientDevice.Length; i++)
                        {
                            await testClientDevice[i].SendVibrateCmd(0);
                        }
                    }
                }
                else
                {
                    await System.Threading.Tasks.Task.Delay(1000);
                    Player_Health = GameObject.Find("Health_Normal");
                    Player_Hunger = GameObject.Find("Hunger_Normal");
                    Player_Thirst = GameObject.Find("Thirst_Normal");
                    if (Player_Health != null && Player_Health.activeSelf && Player_Hunger != null && Player_Hunger.activeSelf && Player_Thirst != null && Player_Thirst.activeSelf)
                        Debug.LogWarning("player found");
                }
            }
        }
    }
}
