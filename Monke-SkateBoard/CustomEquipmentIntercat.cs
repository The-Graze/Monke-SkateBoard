using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Monke_SkateBoard
{


    public class CustomEquipmentIntercat : MonoBehaviour
    {
        // Token: 0x06000273 RID: 627 RVA: 0x0001188C File Offset: 0x0000FA8C
        private void Awake()
        {
            if (CustomEquipmentIntercat.instance == null)
            {
                CustomEquipmentIntercat.instance = this;
            }
            else if (CustomEquipmentIntercat.instance != this)
            {
                UnityEngine.Object.Destroy(base.gameObject);
            }
            this.leftHandSet = false;
            this.rightHandSet = false;
            this.autoGrabLeft = true;
            this.autoGrabRight = true;
            this.gorillaInteractableLayerMask = LayerMask.GetMask(new string[]
            {
            "GorillaInteractable"
            });
        }

        // Token: 0x06000274 RID: 628 RVA: 0x00011904 File Offset: 0x0000FB04
        public void ReleaseRightHand()
        {
            if (this.rightHandHeldEquipment != null)
            {
                this.rightHandHeldEquipment.OnRelease(null, this.rightHand);
            }
            if (this.leftHandHeldEquipment != null)
            {
                this.leftHandHeldEquipment.OnRelease(null, this.rightHand);
            }
            this.autoGrabRight = true;
        }

        // Token: 0x06000275 RID: 629 RVA: 0x00011958 File Offset: 0x0000FB58
        public void ReleaseLeftHand()
        {
            if (this.rightHandHeldEquipment != null)
            {
                this.rightHandHeldEquipment.OnRelease(null, this.leftHand);
            }
            if (this.leftHandHeldEquipment != null)
            {
                this.leftHandHeldEquipment.OnRelease(null, this.leftHand);
            }
            this.autoGrabLeft = true;
        }

        // Token: 0x06000276 RID: 630 RVA: 0x000119AC File Offset: 0x0000FBAC
        private void LateUpdate()
        {
            this.CheckInputValue(true);
            this.isLeftGrabbing = ((this.wasLeftGrabPressed && this.grabValue > this.grabThreshold - this.grabHysteresis) || (!this.wasLeftGrabPressed && this.grabValue > this.grabThreshold + this.grabHysteresis));
            this.CheckInputValue(false);
            this.isRightGrabbing = ((this.wasRightGrabPressed && this.grabValue > this.grabThreshold - this.grabHysteresis) || (!this.wasRightGrabPressed && this.grabValue > this.grabThreshold + this.grabHysteresis));
            this.FireHandInteractions(this.leftHand, true);
            this.FireHandInteractions(this.rightHand, false);
            if (!this.isRightGrabbing && this.wasRightGrabPressed)
            {
                this.ReleaseRightHand();
            }
            if (!this.isLeftGrabbing && this.wasLeftGrabPressed)
            {
                this.ReleaseLeftHand();
            }
            this.wasLeftGrabPressed = this.isLeftGrabbing;
            this.wasRightGrabPressed = this.isRightGrabbing;
        }

        // Token: 0x06000277 RID: 631 RVA: 0x00011AB4 File Offset: 0x0000FCB4
        private void FireHandInteractions(GameObject interactingHand, bool isLeftHand)
        {
            if (isLeftHand)
            {
                this.justGrabbed = ((this.isLeftGrabbing && !this.wasLeftGrabPressed) || (this.isLeftGrabbing && this.autoGrabLeft));
                this.justReleased = (this.leftHandHeldEquipment != null && !this.isLeftGrabbing && this.wasLeftGrabPressed);
            }
            else
            {
                this.justGrabbed = ((this.isRightGrabbing && !this.wasRightGrabPressed) || (this.isRightGrabbing && this.autoGrabRight));
                this.justReleased = (this.rightHandHeldEquipment != null && !this.isRightGrabbing && this.wasRightGrabPressed);
            }
            foreach (InteractionPoint interactionPoint in (isLeftHand ? this.overlapInteractionPointsLeft : this.overlapInteractionPointsRight))
            {
                bool flag = isLeftHand ? (this.leftHandHeldEquipment != null) : (this.rightHandHeldEquipment != null);
                bool flag2 = isLeftHand ? this.disableLeftGrab : this.disableRightGrab;
                if (!flag && !flag2 && interactionPoint != null)
                {
                    if (this.justGrabbed)
                    {
                        interactionPoint.parentTransferrableObject.OnGrab(interactionPoint, interactingHand);
                    }
                    else
                    {
                        interactionPoint.parentTransferrableObject.OnHover(interactionPoint, interactingHand);
                    }
                }
                if (this.justReleased)
                {
                    this.tempZone = interactionPoint.GetComponent<DropZone>();
                    if (this.tempZone != null)
                    {
                        if (interactingHand == this.leftHand)
                        {
                            if (this.leftHandHeldEquipment != null)
                            {
                                this.leftHandHeldEquipment.OnRelease(this.tempZone, interactingHand);
                            }
                        }
                        else if (this.rightHandHeldEquipment != null)
                        {
                            this.rightHandHeldEquipment.OnRelease(this.tempZone, interactingHand);
                        }
                    }
                }
            }
        }

        // Token: 0x06000278 RID: 632 RVA: 0x00011C8C File Offset: 0x0000FE8C
        public void UpdateHandEquipment(CustomHoldable newEquipment, bool forLeftHand)
        {
            if (forLeftHand)
            {
                if (newEquipment == this.rightHandHeldEquipment)
                {
                    this.rightHandHeldEquipment = null;
                }
                if (this.leftHandHeldEquipment != null)
                {
                    this.leftHandHeldEquipment.DropItemCleanup();
                }
                this.leftHandHeldEquipment = newEquipment;
                this.autoGrabLeft = false;
                return;
            }
            if (newEquipment == this.leftHandHeldEquipment)
            {
                this.leftHandHeldEquipment = null;
            }
            if (this.rightHandHeldEquipment != null)
            {
                this.rightHandHeldEquipment.DropItemCleanup();
            }
            this.rightHandHeldEquipment = newEquipment;
            this.autoGrabRight = false;
        }

        // Token: 0x06000279 RID: 633 RVA: 0x00011D18 File Offset: 0x0000FF18
        public void CheckInputValue(bool isLeftHand)
        {
            if (isLeftHand)
            {
                this.grabValue = ControllerInputPoller.GripFloat(XRNode.LeftHand);
                this.tempValue = ControllerInputPoller.TriggerFloat(XRNode.LeftHand);
            }
            else
            {
                this.grabValue = ControllerInputPoller.GripFloat(XRNode.RightHand);
                this.tempValue = ControllerInputPoller.TriggerFloat(XRNode.RightHand);
            }
            this.grabValue = Mathf.Max(this.grabValue, this.tempValue);
        }

        // Token: 0x04000249 RID: 585
        public static volatile CustomEquipmentIntercat instance;

        // Token: 0x0400024A RID: 586
        public CustomHoldable leftHandHeldEquipment;

        // Token: 0x0400024B RID: 587
        public CustomHoldable rightHandHeldEquipment;

        // Token: 0x0400024C RID: 588
        public Transform leftHandTransform;

        // Token: 0x0400024D RID: 589
        public Transform rightHandTransform;

        // Token: 0x0400024E RID: 590
        public Transform chestTransform;

        // Token: 0x0400024F RID: 591
        public Transform leftArmTransform;

        // Token: 0x04000250 RID: 592
        public Transform rightArmTransform;

        // Token: 0x04000251 RID: 593
        public GameObject rightHand;

        // Token: 0x04000252 RID: 594
        public GameObject leftHand;

        // Token: 0x04000253 RID: 595
        private bool leftHandSet;

        // Token: 0x04000254 RID: 596
        private bool rightHandSet;

        // Token: 0x04000255 RID: 597
        public InputDevice leftHandDevice;

        // Token: 0x04000256 RID: 598
        public InputDevice rightHandDevice;

        // Token: 0x04000257 RID: 599
        public List<InteractionPoint> overlapInteractionPointsLeft = new List<InteractionPoint>();

        // Token: 0x04000258 RID: 600
        public List<InteractionPoint> overlapInteractionPointsRight = new List<InteractionPoint>();

        // Token: 0x04000259 RID: 601
        private int gorillaInteractableLayerMask;

        // Token: 0x0400025A RID: 602
        public float grabRadius;

        // Token: 0x0400025B RID: 603
        public float grabThreshold = 0.7f;

        // Token: 0x0400025C RID: 604
        public float grabHysteresis = 0.05f;

        // Token: 0x0400025D RID: 605
        public bool wasLeftGrabPressed;

        // Token: 0x0400025E RID: 606
        public bool wasRightGrabPressed;

        // Token: 0x0400025F RID: 607
        public bool isLeftGrabbing;

        // Token: 0x04000260 RID: 608
        public bool isRightGrabbing;

        // Token: 0x04000261 RID: 609
        public bool justReleased;

        // Token: 0x04000262 RID: 610
        public bool justGrabbed;

        // Token: 0x04000263 RID: 611
        public bool disableLeftGrab;

        // Token: 0x04000264 RID: 612
        public bool disableRightGrab;

        // Token: 0x04000265 RID: 613
        public bool autoGrabLeft;

        // Token: 0x04000266 RID: 614
        public bool autoGrabRight;

        // Token: 0x04000267 RID: 615
        private float grabValue;

        // Token: 0x04000268 RID: 616
        private float tempValue;

        // Token: 0x04000269 RID: 617
        private InteractionPoint tempPoint;

        // Token: 0x0400026A RID: 618
        private DropZone tempZone;
    }
}
