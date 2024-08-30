using System;
using Mirror;
using Zenject;
using UnityEngine;
using System.Linq;
using com.playbux.input;
using com.playbux.motor;
using com.playbux.utilis;
using System.Collections.Generic;
using com.playbux.networking.mirror.message;
using com.playbux.networking.mirror.snapshot;
using Random = UnityEngine.Random;

namespace com.playbux.networking.mirror.client
{
    public class AutoInputWorker : IAFKWorker, ITickable, ILateDisposable
    {
        public bool IsAFK => isAfk;
        public event Action OnAFK;
        
        private bool isAfk;
        private bool hasInput;
        private bool autoMove;
        private bool isInitialized;
        
        private float afkCounter;
        private int stopCounter;
        private int processCounter;

        private Vector2 autoInput = Vector2.zero;
        private SortedList<double, InputSnapshot> inputSnapshots;

        private readonly IMotor motor;
        private readonly NetworkIdentity networkIdentity;
        private readonly InternalUpdateWorker updateWorker;
        private readonly IInputController<Vector2> inputController;
        private readonly IClientMessageSender<PlayerMoveInputMessage> clientMessageSender;

        private const float COOLDOWN = 5f;

        public AutoInputWorker(
            IMotor motor,
            NetworkIdentity networkIdentity,
            InternalUpdateWorker updateWorker,
            IInputController<Vector2> inputController,
            IClientMessageSender<PlayerMoveInputMessage> clientMessageSender)
        {
            this.motor = motor;
            this.updateWorker = updateWorker;
            this.networkIdentity = networkIdentity;
            this.inputController = inputController;
            this.clientMessageSender = clientMessageSender;
            
            inputSnapshots = new SortedList<double, InputSnapshot>(NetworkClient.snapshotSettings.bufferLimit);
        }

        public void Initialize()
        {
            updateWorker.OnTick += AutoMove;
            
            inputController.OnHold += OnPerformed;
            inputController.OnPressed += OnPerformed;
            inputController.OnReleased += OnReleased;
            
            clientMessageSender.Message += SendInput;
            clientMessageSender.SendCondition += SendCondition;

            isInitialized = true;
        }

        public void Dispose()
        {
            isInitialized = false;
            
            updateWorker.OnTick -= AutoMove;
            
            inputController.OnHold -= OnPerformed;
            inputController.OnPressed -= OnPerformed;
            inputController.OnReleased -= OnReleased;
            
            clientMessageSender.Message -= SendInput;
            clientMessageSender.SendCondition -= SendCondition;
        }
        
        public void LateDispose()
        {
            Dispose();
        }
        
        public void Tick()
        {
            PerformATK();
        }

        public void PerformATK()
        {
            if (!isInitialized)
                return;
            
            if (hasInput)
            {
                isAfk = false;
                autoMove = false;
                afkCounter = 0;
                stopCounter = 30;
                processCounter = 0;
                return;
            }
            
            afkCounter += Time.deltaTime;
            
            if (afkCounter < COOLDOWN)
                return;

            isAfk = true;
        }

        private void AutoMove()
        {
            if (!isAfk)
                return;
            
            if (autoMove)
            {
                if (processCounter > 15)
                {
                    stopCounter = 0;
                    autoMove = false;
                    autoInput = Vector2.zero;
                    motor.Stop();
                    var stopShot = new InputSnapshot(NetworkTime.time, NetworkTime.localTime, autoInput);
                    SnapshotInterpolation.InsertIfNotExists(inputSnapshots, NetworkClient.snapshotSettings.bufferLimit, stopShot);
                    return;
                }
                
                motor.Move(autoInput.ToDirection());
                var snapshot = new InputSnapshot(NetworkTime.time, NetworkTime.localTime, autoInput);
                SnapshotInterpolation.InsertIfNotExists(inputSnapshots, NetworkClient.snapshotSettings.bufferLimit, snapshot);
                processCounter ++;
                return;
            }

            stopCounter++;
            
            if (stopCounter < 30)
                return;

            processCounter = 0;
            autoInput.x = Random.Range(-1, 2);
            autoInput.y = Random.Range(-1, 2);
            autoMove = true;
        }

        private bool SendCondition()
        {
            return afkCounter >= COOLDOWN && inputSnapshots.Count > 0;
        }

        private void OnReleased()
        {
            hasInput = false;
        }
        
        private void OnPerformed(Vector2 inputDirection)
        {
            hasInput = inputDirection != Vector2.zero;
        }

        private PlayerMoveInputMessage SendInput()
        {
            var timestamps = inputSnapshots.Keys.ToArray();
            var inputs = inputSnapshots.Values.Select(value => value.Input).ToArray();
            inputSnapshots.Clear();
            return new PlayerMoveInputMessage(timestamps, inputs);
        }
    }
}