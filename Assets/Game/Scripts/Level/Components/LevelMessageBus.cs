using System;
using UnityEngine;

// This cass is used to broadcast level events to out world
public class LevelMessageBus : MonoBehaviour
{
    public MonoBehaviour Listener { get; set; }

    private void OnLevelTransitionMessage(LevelTransitionMessage message)
        => Listener.SendMessage(LevelTransitionMessage.Name, message);

    private void OnPlayerRecoveryMessage(PlayerRecoveryMessage message)
        => Listener.SendMessage(PlayerRecoveryMessage.Name, message);

    private void OnPlayerSaveMessage(PlayerSaveMessage message)
        => Listener.SendMessage(PlayerSaveMessage.Name, message);

    private void OnPlayerDeathMessage(PlayerDeathMessage message)
        => Listener.SendMessage(PlayerDeathMessage.Name, message);
}