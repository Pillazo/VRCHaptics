using MelonLoader;
using UnityEngine;
using VRC.SDKBase;
using Il2CppSystem;
using UnhollowerBaseLib;
using UnityEngine.Animations;
using System.Windows;

[assembly: MelonInfo(typeof(VRCHaptics2.VRCHaptics2 ), "VRCHaptics", "1.0.0", "Pillazo")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace VRCHaptics2
{
    public class VRCHaptics2 : MelonMod
    {
        private float ElapsedTime = 0;
        private string PosString = "";
        private string PlayerString = "";
        private bool first = false;
        private Vector3 SelfPos = Vector3.zero;
         public override void OnUpdate()
         {
            ElapsedTime = ElapsedTime + Time.deltaTime;
            if (ElapsedTime > 0.1)
            {
                ElapsedTime = 0;
                PosString = "";
                first = false;
                var Players = new Il2CppSystem.Collections.Generic.List<VRCPlayerApi>();
                Players = VRC.SDKBase.VRCPlayerApi.AllPlayers;
                foreach (VRCPlayerApi p in Players)
                {
                    PlayerString = (p.GetBonePosition(HumanBodyBones.Head) * 10).ToString() +
                        (p.GetBonePosition(HumanBodyBones.Hips) * 10).ToString() +
                        (p.GetBonePosition(HumanBodyBones.Chest) * 10).ToString() +
                        (p.GetBonePosition(HumanBodyBones.RightShoulder) * 10).ToString() +
                        (p.GetBonePosition(HumanBodyBones.LeftShoulder) * 10).ToString() +
                        (p.GetBonePosition(HumanBodyBones.RightLowerArm) * 10).ToString() +
                        (p.GetBonePosition(HumanBodyBones.LeftLowerArm) * 10).ToString() +
                        (p.GetBonePosition(HumanBodyBones.RightHand) * 10).ToString() +
                        (p.GetBonePosition(HumanBodyBones.LeftHand) * 10).ToString() +
                        (p.GetBonePosition(HumanBodyBones.RightUpperLeg) * 10).ToString() +
                        (p.GetBonePosition(HumanBodyBones.LeftUpperLeg) * 10).ToString() +
                        (p.GetBonePosition(HumanBodyBones.RightLowerLeg) * 10).ToString() +
                        (p.GetBonePosition(HumanBodyBones.LeftLowerLeg) * 10).ToString() +
                        (p.GetBonePosition(HumanBodyBones.RightFoot) * 10).ToString() +
                        (p.GetBonePosition(HumanBodyBones.LeftFoot) * 10).ToString() +
                        "(" + p.GetBoneRotation(HumanBodyBones.Head).x.ToString("F7") + "," + p.GetBoneRotation(HumanBodyBones.Head).y.ToString("F7") + "," + p.GetBoneRotation(HumanBodyBones.Head).z.ToString("F7") + "," + p.GetBoneRotation(HumanBodyBones.Head).w.ToString("F7") + ")" +
                        "(" + p.GetBoneRotation(HumanBodyBones.RightHand).x.ToString("F7") + "," + p.GetBoneRotation(HumanBodyBones.RightHand).y.ToString("F7") + "," + p.GetBoneRotation(HumanBodyBones.RightHand).z.ToString("F7") + "," + p.GetBoneRotation(HumanBodyBones.RightHand).w.ToString("F7") + ")" +
                        "(" + p.GetBoneRotation(HumanBodyBones.LeftHand).x.ToString("F7") + "," + p.GetBoneRotation(HumanBodyBones.LeftHand).y.ToString("F7") + "," + p.GetBoneRotation(HumanBodyBones.LeftHand).z.ToString("F7") + "," + p.GetBoneRotation(HumanBodyBones.LeftHand).w.ToString("F7") + ")" +
                        "(" + p.GetBoneRotation(HumanBodyBones.Chest).x.ToString("F7") + "," + p.GetBoneRotation(HumanBodyBones.Chest).y.ToString("F7") + "," + p.GetBoneRotation(HumanBodyBones.Chest).z.ToString("F7") + "," + p.GetBoneRotation(HumanBodyBones.Chest).w.ToString("F7") + ")" +
                        "(" + p.GetBoneRotation(HumanBodyBones.Hips).x.ToString("F7") + "," + p.GetBoneRotation(HumanBodyBones.Hips).y.ToString("F7") + "," + p.GetBoneRotation(HumanBodyBones.Hips).z.ToString("F7") + "," + p.GetBoneRotation(HumanBodyBones.Hips).w.ToString("F7") + ")";

                    if (first == false)
                    {
                        SelfPos = p.GetBonePosition(HumanBodyBones.Hips);
                        PosString = "Haptics:V1" + PlayerString + "^";
                        first = true;
                    }
                    else
                    {
                        if (Vector3.Distance(SelfPos, p.GetBonePosition(HumanBodyBones.Hips)) < 2.0)
                        {
                            PosString = PosString + "^/^" + PlayerString + p.displayName;
                        }

                    }
                }
                MelonLogger.Msg(PosString);
            }     
         }
    }
}
