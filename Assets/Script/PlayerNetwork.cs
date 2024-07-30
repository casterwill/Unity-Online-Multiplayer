using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace Script
{
    public class PlayerNetwork : NetworkBehaviour
    {
        private readonly NetworkVariable<PlayerNetworkData> netState = new(writePerm: NetworkVariableWritePermission.Owner);
        private NetworkVariable<int> randomNumber = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private void Update()
        {
            Debug.Log(OwnerClientId + "; randomNumber: " + randomNumber.Value);

            if (!IsOwner) return;

                netState.Value = new PlayerNetworkData()
                {
                    Position = transform.position
                };

                if (Input.GetKeyDown(KeyCode.T)) 
                {
                    randomNumber.Value = Random.Range(0, 100);
                    Debug.Log(OwnerClientId + " ; randomNumber: " + randomNumber.Value);
                }

            else
            {
                //jangan dipake di industri game
                //transform.position = Vector2.SmoothDamp(transform.position, netState.Value.Position, ref vel, cheapInterpolationTime);
                
                transform.position = netState.Value.Position;
            } 
        }

        struct  PlayerNetworkData : INetworkSerializable
        {
            private float X, Y;

            internal Vector3 Position
            {
                get => new Vector3(X, Y, 0);
                set
                {
                    X = value.x;
                    Y = value.y;
                }   
            }
            
            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref X);
                serializer.SerializeValue(ref Y);
            }
        }
    }
}