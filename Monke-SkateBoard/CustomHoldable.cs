using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.XR;

namespace Monke_SkateBoard
{


    public class CustomHoldable : MonoBehaviourPunCallbacks
    {
        // Token: 0x060003A9 RID: 937 RVA: 0x00019C92 File Offset: 0x00017E92
        public virtual void Awake()
        {
            latched = false;
            initOffset = base.transform.localPosition;
            initRotation = base.transform.localRotation;
        }

        // Token: 0x060003AA RID: 938 RVA: 0x0000352D File Offset: 0x0000172D
        public virtual void Start()
        {
        }

        // Token: 0x060003AB RID: 939 RVA: 0x00019CC0 File Offset: 0x00017EC0
        public override void OnEnable()
        {
            base.OnEnable();
            if (myRig != null && myRig.isOfflineVRRig)
            {
                if (currentState == CustomHoldable.PositionState.OnLeftArm)
                {
                    storedZone = BodyDockPositions.DropPositions.LeftArm;
                }
                else if (currentState == CustomHoldable.PositionState.OnRightArm)
                {
                    storedZone = BodyDockPositions.DropPositions.RightArm;
                }
                else if (currentState == CustomHoldable.PositionState.OnLeftShoulder)
                {
                    storedZone = BodyDockPositions.DropPositions.LeftBack;
                }
                else if (currentState == CustomHoldable.PositionState.OnRightShoulder)
                {
                    storedZone = BodyDockPositions.DropPositions.RightBack;
                }
                else
                {
                    storedZone = BodyDockPositions.DropPositions.Chest;
                }
            }
            if (currentState == CustomHoldable.PositionState.OnLeftArm && flipOnXForLeftArm)
            {
                Transform transform = GetAnchor(currentState);
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
            initState = currentState;
            enabledOnFrame = Time.frameCount;
        }
        public override void OnDisable()
        {
            base.OnDisable();
            enabledOnFrame = -1;
        }

        public Transform DefaultAnchor()
        {
            if (!(anchor == null))
            {
                return anchor;
            }
            return base.transform;
        }

        // Token: 0x060003B6 RID: 950 RVA: 0x00019FC1 File Offset: 0x000181C1
        public Transform GetAnchor(CustomHoldable.PositionState pos)
        {
            if (grabAnchor == null)
            {
                return DefaultAnchor();
            }
            if (InHand())
            {
                return grabAnchor;
            }
            return DefaultAnchor();
        }

        // Token: 0x060003B7 RID: 951 RVA: 0x00019FF0 File Offset: 0x000181F0
        public bool Attached()
        {
            bool flag = InHand() && detatchOnGrab;
            return !Dropped() && !flag;
        }

        // Token: 0x060003B8 RID: 952 RVA: 0x0001A020 File Offset: 0x00018220
        public void UpdateFollowXform()
        {
            if (targetRig == null)
            {
                return;
            }
            if (targetDock == null)
            {
                targetDock = targetRig.GetComponent<BodyDockPositions>();
            }
            if (anchorOverrides == null)
            {
                anchorOverrides = targetRig.GetComponent<cRigOverrides>();
            }
            Transform transform = GetAnchor(currentState);
            Transform transform2 = transform;
            CustomHoldable.PositionState positionState = currentState;
            if (positionState <= CustomHoldable.PositionState.InRightHand)
            {
                switch (positionState)
                {
                    case CustomHoldable.PositionState.OnLeftArm:
                        transform2 = anchorOverrides.AnchorOverride(currentState, targetDock.leftArmTransform);
                        break;
                    case CustomHoldable.PositionState.OnRightArm:
                        transform2 = anchorOverrides.AnchorOverride(currentState, targetDock.rightArmTransform);
                        break;
                    case CustomHoldable.PositionState.OnLeftArm | CustomHoldable.PositionState.OnRightArm:
                        break;
                    case CustomHoldable.PositionState.InLeftHand:
                        transform2 = anchorOverrides.AnchorOverride(currentState, targetDock.leftHandTransform);
                        break;
                    default:
                        if (positionState == CustomHoldable.PositionState.InRightHand)
                        {
                            transform2 = anchorOverrides.AnchorOverride(currentState, targetDock.rightHandTransform);
                        }
                        break;
                }
            }
            else if (positionState != CustomHoldable.PositionState.OnChest)
            {
                if (positionState != CustomHoldable.PositionState.OnLeftShoulder)
                {
                    if (positionState == CustomHoldable.PositionState.OnRightShoulder)
                    {
                        transform2 = anchorOverrides.AnchorOverride(currentState, targetDock.rightBackTransform);
                    }
                }
                else
                {
                    transform2 = anchorOverrides.AnchorOverride(currentState, targetDock.leftBackTransform);
                }
            }
            else
            {
                transform2 = anchorOverrides.AnchorOverride(currentState, targetDock.chestTransform);
            }
            CustomHoldable.InterpolateState interpolateState = interpState;
            if (interpolateState != CustomHoldable.InterpolateState.None)
            {
                if (interpolateState != CustomHoldable.InterpolateState.Interpolating)
                {
                    return;
                }
                float t = Mathf.Clamp((interpTime - interpDt) / interpTime, 0f, 1f);
                transform.transform.position = Vector3.Lerp(interpStartPos, transform2.transform.position, t);
                transform.transform.rotation = Quaternion.Slerp(interpStartRot, transform2.transform.rotation, t);
                interpDt -= Time.deltaTime;
                if (interpDt <= 0f)
                {
                    transform.parent = transform2;
                    interpState = CustomHoldable.InterpolateState.None;
                    transform.localPosition = Vector3.zero;
                    transform.localRotation = Quaternion.identity;
                    transform.localScale = Vector3.one;
                    if (flipOnXForLeftHand && currentState == CustomHoldable.PositionState.InLeftHand)
                    {
                        transform.localScale = new Vector3(-1f, 1f, 1f);
                    }
                    if (flipOnYForLeftHand && currentState == CustomHoldable.PositionState.InLeftHand)
                    {
                        transform.localScale = new Vector3(1f, -1f, 1f);
                    }
                }
            }
            else if (transform2 != transform.parent)
            {
                if (Time.frameCount == enabledOnFrame)
                {
                    transform.parent = transform2;
                    transform.localPosition = Vector3.zero;
                    transform.localRotation = Quaternion.identity;
                    return;
                }
                interpState = CustomHoldable.InterpolateState.Interpolating;
                interpDt = interpTime;
                interpStartPos = transform.transform.position;
                interpStartRot = transform.transform.rotation;
                return;
            }
        }

        // Token: 0x060003B9 RID: 953 RVA: 0x0001A345 File Offset: 0x00018545
        public void DropItem()
        {
            base.transform.parent = null;
        }

        // Token: 0x060003BB RID: 955 RVA: 0x0001A3FC File Offset: 0x000185FC
        public void ResetXf()
        {
            if (canDrop)
            {
                Transform transform = DefaultAnchor();
                if (base.transform != transform && base.transform.parent != transform)
                {
                    base.transform.parent = transform;
                }
                base.transform.localPosition = initOffset;
                base.transform.localRotation = initRotation;
            }
        }

        // Token: 0x060003BC RID: 956 RVA: 0x0001A467 File Offset: 0x00018667
        public void ReDock()
        {
            if (IsMyItem())
            {
                currentState = initState;
            }
            ResetXf();
        }

        // Token: 0x060003BD RID: 957 RVA: 0x0001A484 File Offset: 0x00018684
        public void HandleLocalInput()
        {
            GameObject[] array;
            if (!InHand())
            {
                array = gameObjectsActiveOnlyWhileHeld;
                for (int i = 0; i < array.Length; i++)
                {
                    array[i].SetActive(false);
                }
                return;
            }
            array = gameObjectsActiveOnlyWhileHeld;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].SetActive(true);
            }
            XRNode node = (currentState == CustomHoldable.PositionState.InLeftHand) ? XRNode.LeftHand : XRNode.RightHand;
            indexTrigger = ControllerInputPoller.TriggerFloat(node);
            bool flag = !latched && indexTrigger >= myThreshold;
            bool flag2 = latched && indexTrigger < myThreshold - hysterisis;
            if (flag || testActivate)
            {
                testActivate = false;
                if (CanActivate())
                {
                    OnActivate();
                    return;
                }
            }
            else if (flag2 || testDeactivate)
            {
                testDeactivate = false;
                if (CanDeactivate())
                {
                    OnDeactivate();
                }
            }
        }
        public virtual void ResetToDefaultState()
        {
            canAutoGrabLeft = true;
            canAutoGrabRight = true;
            wasHover = false;
            isHover = false;
            ResetXf();
        }
        public virtual void OnGrab(InteractionPoint pointGrabbed, GameObject grabbingHand)
        {
            if (!IsMyItem())
            {
                return;
            }
            if (!(grabbingHand == interactor.leftHand) || currentState == CustomHoldable.PositionState.OnLeftArm)
            {
                if (grabbingHand == interactor.rightHand && currentState != CustomHoldable.PositionState.OnRightArm)
                {
                    if (currentState == CustomHoldable.PositionState.InLeftHand && disableStealing)
                    {
                        return;
                    }
                    canAutoGrabRight = false;
                    currentState = CustomHoldable.PositionState.InRightHand;
                    CustomEquipmentIntercat.instance.UpdateHandEquipment(this, false);
                    GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tapHapticStrength / 8f, GorillaTagger.Instance.tapHapticDuration * 0.5f);
                }
                return;
            }
            if (currentState == CustomHoldable.PositionState.InRightHand && disableStealing)
            {
                return;
            }
            canAutoGrabLeft = false;
            currentState = CustomHoldable.PositionState.InLeftHand;
            CustomEquipmentIntercat.instance.UpdateHandEquipment(this, true);
            GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.tapHapticStrength / 8f, GorillaTagger.Instance.tapHapticDuration * 0.5f);
        }

        // Token: 0x060003C2 RID: 962 RVA: 0x0001A7BC File Offset: 0x000189BC
        public virtual void OnRelease(DropZone zoneReleased, GameObject releasingHand)
        {
            if (!IsMyItem())
            {
                return;
            }
            if (!CanDeactivate())
            {
                return;
            }
            if (IsHeld() && ((releasingHand == CustomEquipmentIntercat.instance.rightHand && this == CustomEquipmentIntercat.instance.rightHandHeldEquipment) || (releasingHand == CustomEquipmentIntercat.instance.leftHand && this == CustomEquipmentIntercat.instance.leftHandHeldEquipment)))
            {
                if (releasingHand == CustomEquipmentIntercat.instance.leftHand)
                {
                    canAutoGrabLeft = true;
                }
                else
                {
                    canAutoGrabRight = true;
                }
                if (zoneReleased != null)
                {
                    bool flag = currentState == CustomHoldable.PositionState.InLeftHand && zoneReleased.dropPosition == BodyDockPositions.DropPositions.LeftArm;
                    bool flag2 = currentState == CustomHoldable.PositionState.InRightHand && zoneReleased.dropPosition == BodyDockPositions.DropPositions.RightArm;
                    if (flag || flag2)
                    {
                        return;
                    }
                    if (targetDock.DropZoneStorageUsed(zoneReleased.dropPosition) == -1 && zoneReleased.forBodyDock == targetDock && (zoneReleased.dropPosition & dockPositions) != BodyDockPositions.DropPositions.None)
                    {
                        storedZone = zoneReleased.dropPosition;
                    }
                }
                DropItemCleanup();
                CustomEquipmentIntercat.instance.UpdateHandEquipment(null, releasingHand == CustomEquipmentIntercat.instance.leftHand);
            }
        }

        // Token: 0x060003C3 RID: 963 RVA: 0x0001A900 File Offset: 0x00018B00
        public virtual void DropItemCleanup()
        {
            if (canDrop)
            {
                currentState = CustomHoldable.PositionState.Dropped;
                return;
            }
            BodyDockPositions.DropPositions dropPositions = storedZone;
            switch (dropPositions)
            {
                case BodyDockPositions.DropPositions.LeftArm:
                    currentState = CustomHoldable.PositionState.OnLeftArm;
                    return;
                case BodyDockPositions.DropPositions.RightArm:
                    currentState = CustomHoldable.PositionState.OnRightArm;
                    return;
                case BodyDockPositions.DropPositions.LeftArm | BodyDockPositions.DropPositions.RightArm:
                    break;
                case BodyDockPositions.DropPositions.Chest:
                    currentState = CustomHoldable.PositionState.OnChest;
                    return;
                default:
                    if (dropPositions == BodyDockPositions.DropPositions.LeftBack)
                    {
                        currentState = CustomHoldable.PositionState.OnLeftShoulder;
                        return;
                    }
                    if (dropPositions != BodyDockPositions.DropPositions.RightBack)
                    {
                        return;
                    }
                    currentState = CustomHoldable.PositionState.OnRightShoulder;
                    break;
            }
        }

        // Token: 0x060003C4 RID: 964 RVA: 0x0001A974 File Offset: 0x00018B74
        public virtual void OnHover(InteractionPoint pointHovered, GameObject hoveringHand)
        {
            if (!IsMyItem())
            {
                return;
            }
            if (!wasHover)
            {
                GorillaTagger.Instance.StartVibration(hoveringHand == CustomEquipmentIntercat.instance.leftHand, GorillaTagger.Instance.tapHapticStrength / 8f, GorillaTagger.Instance.tapHapticDuration * 0.5f);
            }
            isHover = true;
        }

        // Token: 0x060003C5 RID: 965 RVA: 0x0001A9D8 File Offset: 0x00018BD8
        public void ActivateItemFX(float hapticStrength, float hapticDuration, int soundIndex, float soundVolume)
        {
            bool flag = currentState == CustomHoldable.PositionState.InLeftHand;
            if (myOnlineRig != null)
            {
                PhotonView.Get(myOnlineRig).RPC("PlayHandTap", RpcTarget.Others, new object[]
                {
                soundIndex,
                flag,
                0.1f
                });
            }
            myRig.PlayHandTapLocal(soundIndex, flag, soundVolume);
            GorillaTagger.Instance.StartVibration(flag, hapticStrength, hapticDuration);
        }

        // Token: 0x060003C6 RID: 966 RVA: 0x0000352D File Offset: 0x0000172D
        public virtual void PlayNote(int note, float volume)
        {
        }

        // Token: 0x060003C7 RID: 967 RVA: 0x0001AA54 File Offset: 0x00018C54
        public virtual bool AutoGrabTrue(bool leftGrabbingHand)
        {
            if (!leftGrabbingHand)
            {
                return canAutoGrabRight;
            }
            return canAutoGrabLeft;
        }

        // Token: 0x060003C8 RID: 968 RVA: 0x00012E45 File Offset: 0x00011045
        public virtual bool CanActivate()
        {
            return true;
        }

        // Token: 0x060003C9 RID: 969 RVA: 0x00012E45 File Offset: 0x00011045
        public virtual bool CanDeactivate()
        {
            return true;
        }

        // Token: 0x060003CA RID: 970 RVA: 0x0001AA66 File Offset: 0x00018C66
        public virtual void OnActivate()
        {
            latched = true;
        }

        // Token: 0x060003CB RID: 971 RVA: 0x0001AA6F File Offset: 0x00018C6F
        public virtual void OnDeactivate()
        {
            latched = false;
        }

        // Token: 0x060003CC RID: 972 RVA: 0x0001AA78 File Offset: 0x00018C78
        public virtual bool IsMyItem()
        {
            return myRig != null && myRig.isOfflineVRRig;
        }

        // Token: 0x060003CD RID: 973 RVA: 0x0001AA95 File Offset: 0x00018C95
        public virtual bool IsHeld()
        {
            return CustomEquipmentIntercat.instance.leftHandHeldEquipment == this || CustomEquipmentIntercat.instance.rightHandHeldEquipment == this;
        }

        // Token: 0x060003CE RID: 974 RVA: 0x0001AABF File Offset: 0x00018CBF
        public bool InHand()
        {
            return currentState == CustomHoldable.PositionState.InLeftHand || currentState == CustomHoldable.PositionState.InRightHand;
        }

        // Token: 0x060003CF RID: 975 RVA: 0x0001AAD5 File Offset: 0x00018CD5
        public bool Dropped()
        {
            return currentState == CustomHoldable.PositionState.Dropped;
        }

        // Token: 0x060003D0 RID: 976 RVA: 0x0001AAE4 File Offset: 0x00018CE4
        public bool InLeftHand()
        {
            return currentState == CustomHoldable.PositionState.InLeftHand;
        }

        // Token: 0x060003D1 RID: 977 RVA: 0x0001AAEF File Offset: 0x00018CEF
        public bool InRightHand()
        {
            return currentState == CustomHoldable.PositionState.InRightHand;
        }

        // Token: 0x060003D2 RID: 978 RVA: 0x0001AAFA File Offset: 0x00018CFA
        public bool OnChest()
        {
            return currentState == CustomHoldable.PositionState.OnChest;
        }

        // Token: 0x060003D3 RID: 979 RVA: 0x0001AB06 File Offset: 0x00018D06
        public bool OnShoulder()
        {
            return currentState == CustomHoldable.PositionState.OnLeftShoulder || currentState == CustomHoldable.PositionState.OnRightShoulder;
        }

        // Token: 0x060003D4 RID: 980 RVA: 0x0001AB1E File Offset: 0x00018D1E
        public Player OwningPlayer()
        {
            if (myRig == null)
            {
                return myOnlineRig.photonView.Owner;
            }
            return PhotonNetwork.LocalPlayer;
        }

        // Token: 0x04000443 RID: 1091
        public CustomEquipmentIntercat interactor;

        // Token: 0x04000444 RID: 1092
        public VRRig myRig;

        // Token: 0x04000445 RID: 1093
        public VRRig myOnlineRig;

        // Token: 0x04000446 RID: 1094
        public bool latched;

        // Token: 0x04000447 RID: 1095
        public float indexTrigger;

        // Token: 0x04000448 RID: 1096
        public bool testActivate;

        // Token: 0x04000449 RID: 1097
        public bool testDeactivate;

        // Token: 0x0400044A RID: 1098
        public float myThreshold = 0.8f;

        // Token: 0x0400044B RID: 1099
        public float hysterisis = 0.05f;

        // Token: 0x0400044C RID: 1100
        public bool flipOnXForLeftHand;

        // Token: 0x0400044D RID: 1101
        public bool flipOnYForLeftHand;

        // Token: 0x0400044E RID: 1102
        public bool flipOnXForLeftArm;

        // Token: 0x0400044F RID: 1103
        public bool disableStealing;

        // Token: 0x04000450 RID: 1104
        public CustomHoldable.PositionState initState;

        // Token: 0x04000451 RID: 1105
        public CustomHoldable.ItemStates itemState;

        // Token: 0x04000452 RID: 1106
        public InteractionPoint gripInteractor;

        // Token: 0x04000453 RID: 1107
        public BodyDockPositions.DropPositions storedZone;

        // Token: 0x04000454 RID: 1108
        public CustomHoldable.PositionState previousState;

        // Token: 0x04000455 RID: 1109
        public CustomHoldable.PositionState currentState;

        // Token: 0x04000456 RID: 1110
        public BodyDockPositions.DropPositions dockPositions;

        // Token: 0x04000457 RID: 1111
        public VRRig targetRig;

        // Token: 0x04000458 RID: 1112
        public BodyDockPositions targetDock;

        // Token: 0x04000459 RID: 1113
        public cRigOverrides anchorOverrides;

        // Token: 0x0400045A RID: 1114
        public bool canAutoGrabLeft;

        // Token: 0x0400045B RID: 1115
        public bool canAutoGrabRight;

        // Token: 0x0400045C RID: 1116
        public int objectIndex;

        // Token: 0x0400045D RID: 1117
        [Tooltip("In Holdables.prefab, assign to the parent of this transform.\nExample: 'Holdables/YellowHandBootsRight' is the anchor of 'Holdables/YellowHandBootsRight/YELLOW HAND BOOTS'")]
        public Transform anchor;

        // Token: 0x0400045E RID: 1118
        [Tooltip("(Optional) Use this to override the transform used when the object is in the hand.\nExample: 'GHOST BALLOON' uses child 'grabPtAnchor' which is the end of the balloon's string.")]
        public Transform grabAnchor;

        // Token: 0x0400045F RID: 1119
        public int myIndex;

        // Token: 0x04000460 RID: 1120
        [Tooltip("(Optional)")]
        public GameObject[] gameObjectsActiveOnlyWhileHeld;

        // Token: 0x04000461 RID: 1121
        public GameObject worldShareableInstance;

        // Token: 0x04000462 RID: 1122
        public float interpTime = 0.1f;

        // Token: 0x04000463 RID: 1123
        public float interpDt;

        // Token: 0x04000464 RID: 1124
        public Vector3 interpStartPos;

        // Token: 0x04000465 RID: 1125
        public Quaternion interpStartRot;

        // Token: 0x04000466 RID: 1126
        public int enabledOnFrame = -1;

        // Token: 0x04000467 RID: 1127
        public Vector3 initOffset;

        // Token: 0x04000468 RID: 1128
        public Quaternion initRotation;

        // Token: 0x04000469 RID: 1129
        public bool canDrop;

        // Token: 0x0400046A RID: 1130
        public bool shareable;

        // Token: 0x0400046B RID: 1131
        public bool detatchOnGrab;

        // Token: 0x0400046C RID: 1132
        public bool wasHover;

        // Token: 0x0400046D RID: 1133
        public bool isHover;

        // Token: 0x0400046E RID: 1134
        public bool disableItem;

        // Token: 0x0400046F RID: 1135
        public CustomHoldable.InterpolateState interpState;

        // Token: 0x0200021F RID: 543
        public enum ItemStates
        {
            // Token: 0x04000EFA RID: 3834
            State0 = 1,
            // Token: 0x04000EFB RID: 3835
            State1,
            // Token: 0x04000EFC RID: 3836
            State2 = 4,
            // Token: 0x04000EFD RID: 3837
            State3 = 8,
            // Token: 0x04000EFE RID: 3838
            State4 = 16
        }

        // Token: 0x02000220 RID: 544
        [Flags]
        public enum PositionState
        {
            // Token: 0x04000F00 RID: 3840
            OnLeftArm = 1,
            // Token: 0x04000F01 RID: 3841
            OnRightArm = 2,
            // Token: 0x04000F02 RID: 3842
            InLeftHand = 4,
            // Token: 0x04000F03 RID: 3843
            InRightHand = 8,
            // Token: 0x04000F04 RID: 3844
            OnChest = 16,
            // Token: 0x04000F05 RID: 3845
            OnLeftShoulder = 32,
            // Token: 0x04000F06 RID: 3846
            OnRightShoulder = 64,
            // Token: 0x04000F07 RID: 3847
            Dropped = 128,
            // Token: 0x04000F08 RID: 3848
            Count = 8,
            // Token: 0x04000F09 RID: 3849
            None = 0
        }

        // Token: 0x02000221 RID: 545
        public enum InterpolateState
        {
            // Token: 0x04000F0B RID: 3851
            None,
            // Token: 0x04000F0C RID: 3852
            Interpolating
        }
    }
}
