using Mirror;
using Zenject;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Threading;
using com.playbux.networking.mirror.message;
using com.playbux.networking.mirror.snapshot;
using Cysharp.Threading.Tasks;

namespace com.playbux.networking.mirror.server.fakeplayer
{
    public class MoveSanpoFakePlayerState : IFakePlayerState, ILateTickable
    {
        private class InterruptChance
        {
            public int maxInterruptCeling = 50;
            public int interruptChanceFloor = 0;
            public int interruptChanceCeiling = 5;
            public float preventInterruptTime = 5f;

            public bool IsNeededToInterrupt()
            {
                int chance = Random.Range(interruptChanceFloor, 100);

                if (chance > interruptChanceCeiling)
                    interruptChanceCeiling += interruptChanceCeiling >= maxInterruptCeling ? 0 : 1;

                return chance < interruptChanceCeiling;
            }
        }

        public FakePlayerStateEnum StateEnum { get; }
        private int BufferSize => NetworkClient.snapshotSettings.bufferLimit;

        private readonly NavMeshAgent navMeshAgent;
        private readonly NetworkIdentity networkIdentity;
        private readonly IServerMessageSender<FakePlayerPositionMessage> positionSender;
        private readonly Vector2[] wayPoints;

        private bool isMoving;
        private bool isPerforming;
        private bool hasInterrupted;
        private float stopCount;
        private float interruptCount;
        private Vector2 lastDirection;
        private InterruptChance interruptChance;
        private CancellationTokenSource cancellationTokenSource;
        private SortedList<double, PositionStateSnapshot> positionBuffer;

        public MoveSanpoFakePlayerState(
            NavMeshAgent navMeshAgent,
            FakePlayerStateEnum stateEnum,
            NetworkIdentity networkIdentity,
            IServerMessageSender<FakePlayerPositionMessage> positionSender,
            Vector2[] wayPoints)
        {
            StateEnum = stateEnum;
            this.wayPoints = wayPoints;
            this.navMeshAgent = navMeshAgent;
            this.positionSender = positionSender;
            this.networkIdentity = networkIdentity;
            interruptChance = new InterruptChance();
            positionBuffer = new SortedList<double, PositionStateSnapshot>(BufferSize);
        }

        public void Perform()
        {
            cancellationTokenSource = new CancellationTokenSource();
            positionSender.MessageToObserver += SendStatesMessage;
            positionSender.SendCondition += SendStateMessageCondition;
            isPerforming = true;
            ChooseWaypoint(cancellationTokenSource.Token).Forget();
        }

        public void Exit()
        {
            cancellationTokenSource.Cancel();
            positionSender.MessageToObserver -= SendStatesMessage;
            positionSender.SendCondition -= SendStateMessageCondition;
            isPerforming = isMoving = false;
            positionBuffer.Clear();
        }

        public void LateTick()
        {
            if (!isPerforming)
                return;

            if (!isMoving)
                return;

            Record();
            interruptCount += Time.unscaledDeltaTime;

            if (interruptCount > interruptChance.preventInterruptTime)
                hasInterrupted = interruptChance.IsNeededToInterrupt();

            if (hasInterrupted)
            {
                isMoving = false;
                navMeshAgent.isStopped = true;
                ChooseWaypoint(cancellationTokenSource.Token).Forget();
                hasInterrupted = false;
                interruptCount = 0;
                interruptChance = new InterruptChance();
            }

            if (navMeshAgent.isStopped)
                return;

            if (Vector2.Distance(navMeshAgent.destination, networkIdentity.transform.position) > 1)
                return;

            isMoving = false;
            navMeshAgent.isStopped = true;
            ChooseWaypoint(cancellationTokenSource.Token).Forget();
        }

        private async UniTaskVoid ChooseWaypoint(CancellationToken cancellationToken)
        {
            await UniTask.WaitForSeconds(Random.Range(5, 15), false, PlayerLoopTiming.Update, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return;

            int randomIndex = Random.Range(0, wayPoints.Length);
            navMeshAgent.SetDestination(wayPoints[randomIndex] + Vector2.one * Random.Range(0.5f, 2f));
            isMoving = true;
            navMeshAgent.isStopped = false;
        }

        private void Record()
        {
            Vector2 direction = navMeshAgent.isStopped || !isMoving ? Vector2.zero : navMeshAgent.desiredVelocity.normalized;
            direction.x *= -1;
            RecordForOther(direction, networkIdentity.transform.position);
            lastDirection = direction;
        }

        private void RecordForOther(Vector2 direction, Vector2 position)
        {
            var snapshot = new PositionStateSnapshot(NetworkTime.localTime, NetworkTime.localTime, direction, position);
            switch (positionBuffer.Count)
            {
                case 0:
                    SnapshotInterpolation.InsertIfNotExists(positionBuffer, BufferSize, snapshot);
                    return;
                case > 0 when positionBuffer.Values[^1].Input == Vector2.zero && lastDirection == Vector2.zero:
                    return;
                default:
                    SnapshotInterpolation.InsertIfNotExists(positionBuffer, BufferSize, snapshot);
                    break;
            }
        }

        private bool SendStateMessageCondition()
        {
            return positionBuffer.Count > 0;
        }

        private ServerMessageToObserver<FakePlayerPositionMessage> SendStatesMessage()
        {
            double[] timestamps = positionBuffer.Keys.ToArray();
            var inputs = positionBuffer.Values.Select(value => value.Input).ToArray();
            var positions = positionBuffer.Values.Select(value => value.Position).ToArray();
            positionBuffer.Clear();
            var toOtherMessage = new FakePlayerPositionMessage(networkIdentity.netId, timestamps, inputs, positions);
            return new ServerMessageToObserver<FakePlayerPositionMessage>(false, networkIdentity, toOtherMessage);
        }
    }
}