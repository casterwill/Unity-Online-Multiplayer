using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Script
{
    [RequireComponent(typeof(Rigidbody2D))]

    
    public class PlayerAction : NetworkBehaviour
    {
        private NetworkVariable<MyCustomData> number = new NetworkVariable<MyCustomData>(
            new MyCustomData
            {
                _int = 56,
                _bool = true
            }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public struct MyCustomData : INetworkSerializable
        {
            public int _int;
            public bool _bool;
            public FixedString128Bytes message;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref _int);
                serializer.SerializeValue(ref _bool);
                serializer.SerializeValue(ref message);
            }
        }

        private Vector2 moveDirectionKeyb = Vector2.zero;
        private Vector2 moveDirectionJoyst = Vector2.zero;
        private float moveSpeed = 5.0f;
        [SerializeField] private Joystick joystick;

        private Rigidbody2D rb2d;

        public PlayerInputActions playerControls;
        private InputAction move;
        private InputAction fire;
        
        [SerializeField] private Animator animator;

        [SerializeField] private Transform SpawnedObjectPrefab;
        private Transform spawnedObjectTransform;

        private void Awake()
        {
            playerControls = new PlayerInputActions();
        }

        public override void OnNetworkSpawn()
        {
            Debug.Log(OwnerClientId + "; randomNumber: " + number.Value._int + "; " + number.Value._bool + "; " + number.Value.message);

            number.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) =>
            {
                Debug.Log(OwnerClientId + "; randomNumber: " + newValue._int + "; " + newValue._bool + "; " + newValue.message);
            };
        }

        private void OnEnable()
        {
            move = playerControls.Player.Move;
            move.Enable();

            //fire = playerControls.Player.Fire;
            //fire.Enable();

            //fire.performed += Fire;
        }

        private void OnDisable()
        {
            move.Disable();
            //fire.Disable()
        }

        private void Start()
        {
            rb2d = GetComponent<Rigidbody2D>();
        }
        
        void Update()
        {
            if (!IsOwner) return;

            MovementInput();

            if (Input.GetKeyDown(KeyCode.T))
            {
                spawnedObjectTransform = Instantiate(SpawnedObjectPrefab);
                spawnedObjectTransform.GetComponent<NetworkObject>().Spawn(true);
                //TestClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { 1 } } });
                //number.Value = new MyCustomData
                //{
                //    _int = Random.Range(0, 100),
                //    _bool = false,
                //    message = "Ok"
                //};
            }

            if(Input.GetKeyDown(KeyCode.Y))
            {
                spawnedObjectTransform.GetComponent<NetworkObject>().Despawn(true);
                Destroy(spawnedObjectTransform.gameObject);
            }
        }

        private void FixedUpdate()
        {
            DoMovement();
        }

        void MovementInput()
        {
            // float inputX = Input.GetAxis("Horizontal");
            // float inputY = Input.GetAxis("Vertical");

            //moveDirectionKeyb = move.ReadValue<Vector2>();

            if (moveDirectionKeyb != Vector2.zero || moveDirectionJoyst != Vector2.zero) 
            { 
                animator.SetBool("Move", true); 
            }
            else 
            { 
                animator.SetBool("Move", false); 
            }
        }

        void DoMovement()
        {
            if(joystick.Direction.y != 0)
            {
                rb2d.velocity = new Vector2(joystick.Direction.x * moveSpeed, joystick.Direction.y * moveSpeed);
            } else
            {
                rb2d.velocity = Vector2.zero;
            }
            //rb2d.velocity = new Vector2(moveDirectionJoyst.x * moveSpeed, moveDirectionJoyst.y * moveSpeed);
        }

        void Fire(InputAction.CallbackContext context)
        {
            //Debug.Log("Firee!!!");
        }

        [ServerRpc]
        private void TestServerRpc(ServerRpcParams serverRpcParams)
        {
            Debug.Log("TestServerRpc " + OwnerClientId + "; " + serverRpcParams.Receive.SenderClientId);
        }

        [ClientRpc]
        private void TestClientRpc(ClientRpcParams clientRpcParams)
        {
            Debug.Log("TestClientRpc");
        }
    }
}