using Mirror;
using Zenject;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using com.playbux.networking.mirror.message;
using com.playbux.networking.mirror.snapshot;

namespace com.playbux.networking.mirror.server.fakeplayer
{
    public class MoveAndExitFakePlayerState : IFakePlayerState, ILateTickable
    {
        public FakePlayerStateEnum StateEnum { get; }

        private int BufferSize => NetworkClient.snapshotSettings.bufferLimit;

        private readonly NavMeshAgent navMeshAgent;
        private readonly NetworkIdentity networkIdentity;
        private readonly IServerMessageSender<FakePlayerPositionMessage> positionSender;
        private readonly Vector2[] wayPoints;

        private bool isMoving;
        private bool isPerforming;
        private Vector2 lastDirection;
        private SortedList<double, PositionStateSnapshot> positionBuffer;

        public MoveAndExitFakePlayerState(
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
            positionBuffer = new SortedList<double, PositionStateSnapshot>(BufferSize);
        }

        public void Perform()
        {
            positionSender.MessageToObserver += SendStatesMessage;
            positionSender.SendCondition += SendStateMessageCondition;
            isPerforming = true;
            ChooseWaypoint();
        }

        public void Exit()
        {
            positionSender.MessageToObserver -= SendStatesMessage;
            positionSender.SendCondition -= SendStateMessageCondition;
            isPerforming = false;
            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
            positionBuffer.Clear();
        }

        public void LateTick()
        {
            if (!isPerforming)
                return;

            if (!isMoving)
                return;

            Record();

            if (navMeshAgent.isStopped)
                return;

            if (Vector2.Distance(navMeshAgent.destination, networkIdentity.transform.position) < 1)
                Exit();
        }

        private void ChooseWaypoint()
        {
            int randomIndex = Random.Range(0, wayPoints.Length);
            navMeshAgent.SetDestination(wayPoints[randomIndex]);
            isMoving = true;
            navMeshAgent.isStopped = false;
        }

        private void Record()
        {
            Vector2 direction = navMeshAgent.desiredVelocity.normalized;
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