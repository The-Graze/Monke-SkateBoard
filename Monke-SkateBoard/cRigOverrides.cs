using System;
using UnityEngine;
namespace Monke_SkateBoard
{

    // Token: 0x02000093 RID: 147
    public class cRigOverrides : MonoBehaviour
    {
        // Token: 0x17000035 RID: 53
        // (get) Token: 0x060003E3 RID: 995 RVA: 0x0001AE09 File Offset: 0x00019009
        public Transform NameDefaultAnchor
        {
            get
            {
                return this.nameDefaultAnchor;
            }
        }

        // Token: 0x17000036 RID: 54
        // (get) Token: 0x060003E4 RID: 996 RVA: 0x0001AE11 File Offset: 0x00019011
        public Transform NameTransform
        {
            get
            {
                return this.nameTransform;
            }
        }

        // Token: 0x060003E5 RID: 997 RVA: 0x0001AE1C File Offset: 0x0001901C
        public void Awake()
        {
            for (int i = 0; i < 8; i++)
            {
                this.overrideAnchors[i] = null;
            }
        }

        // Token: 0x060003E6 RID: 998 RVA: 0x0001AE3E File Offset: 0x0001903E
        public void OnEnable()
        {
            this.nameTransform.parent = this.nameDefaultAnchor.parent;
        }

        // Token: 0x060003E7 RID: 999 RVA: 0x0001AE58 File Offset: 0x00019058
        public int MapPositionToIndex(CustomHoldable.PositionState pos)
        {
            int num = (int)pos;
            int num2 = 0;
            while ((num >>= 1) != 0)
            {
                num2++;
            }
            return num2;
        }

        // Token: 0x060003E8 RID: 1000 RVA: 0x0001AE78 File Offset: 0x00019078
        public void OverrideAnchor(CustomHoldable.PositionState pos, Transform anchor)
        {
            int num = this.MapPositionToIndex(pos);
            if (this.overrideAnchors[num] != null)
            {
                foreach (object obj in this.overrideAnchors[num])
                {
                    ((Transform)obj).parent = null;
                }
            }
            this.overrideAnchors[num] = anchor;
        }

        // Token: 0x060003E9 RID: 1001 RVA: 0x0001AEF4 File Offset: 0x000190F4
        public Transform AnchorOverride(CustomHoldable.PositionState pos, Transform fallback)
        {
            int num = this.MapPositionToIndex(pos);
            Transform transform = this.overrideAnchors[num];
            if (!(transform != null))
            {
                return fallback;
            }
            return transform;
        }

        // Token: 0x04000478 RID: 1144
        [SerializeField]
        protected Transform nameDefaultAnchor;

        // Token: 0x04000479 RID: 1145
        [SerializeField]
        protected Transform nameTransform;

        // Token: 0x0400047A RID: 1146
        public readonly Transform[] overrideAnchors = new Transform[8];

        // Token: 0x0400047B RID: 1147
        public GameObject nameLastObjectToAttach;
    }
}

