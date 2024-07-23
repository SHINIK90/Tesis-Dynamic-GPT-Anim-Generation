using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using Newtonsoft.Json;

public class LoadAnimFromJson : MonoBehaviour
{
    private List<string> propertiesNames = new List<string>();
    BiDictionary<string, string> BodyPartsToMixamo = new BiDictionary<string, string>();
    public string pathName;
    public enum Options
    {
        Full,
        Compressed,
        Compressed2048,
        FullDEG,
        CompressedDEG,
        Compressed2048DEG
    }
    // This will appear as a dropdown in the Unity Editor
    public Options JsonFormatVersion;

    void Start(){
        propertiesNames.Add("mixamorig:Hips");
        propertiesNames.Add("mixamorig:Hips/mixamorig:LeftUpLeg");
        propertiesNames.Add("mixamorig:Hips/mixamorig:LeftUpLeg/mixamorig:LeftLeg");
        propertiesNames.Add("mixamorig:Hips/mixamorig:LeftUpLeg/mixamorig:LeftLeg/mixamorig:LeftFoot");
        propertiesNames.Add("mixamorig:Hips/mixamorig:RightUpLeg");
        propertiesNames.Add("mixamorig:Hips/mixamorig:RightUpLeg/mixamorig:RightLeg");
        propertiesNames.Add("mixamorig:Hips/mixamorig:RightUpLeg/mixamorig:RightLeg/mixamorig:RightFoot");
        propertiesNames.Add("mixamorig:Hips/mixamorig:Spine");
        propertiesNames.Add("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1");
        propertiesNames.Add("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2");
        propertiesNames.Add("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder");
        propertiesNames.Add("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm");
        propertiesNames.Add("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm");
        propertiesNames.Add("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm/mixamorig:LeftHand");
        propertiesNames.Add("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck");
        propertiesNames.Add("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head");
        propertiesNames.Add("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder");
        propertiesNames.Add("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm");
        propertiesNames.Add("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm");
        propertiesNames.Add("mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm/mixamorig:RightHand");
        
        BodyPartsToMixamo.Add("Hips", "mixamorig:Hips");
        BodyPartsToMixamo.Add("LeftLegHipJoint", "mixamorig:Hips/mixamorig:LeftUpLeg");
        BodyPartsToMixamo.Add("LeftKnee", "mixamorig:Hips/mixamorig:LeftUpLeg/mixamorig:LeftLeg");
        BodyPartsToMixamo.Add("LeftAnkle", "mixamorig:Hips/mixamorig:LeftUpLeg/mixamorig:LeftLeg/mixamorig:LeftFoot");
        BodyPartsToMixamo.Add("RightLegHipJoint", "mixamorig:Hips/mixamorig:RightUpLeg");
        BodyPartsToMixamo.Add("RightKnee", "mixamorig:Hips/mixamorig:RightUpLeg/mixamorig:RightLeg");
        BodyPartsToMixamo.Add("RightAnkle", "mixamorig:Hips/mixamorig:RightUpLeg/mixamorig:RightLeg/mixamorig:RightFoot");
        BodyPartsToMixamo.Add("LowerBack", "mixamorig:Hips/mixamorig:Spine");
        BodyPartsToMixamo.Add("MiddleBack", "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1");
        BodyPartsToMixamo.Add("UpperBack", "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2");
        BodyPartsToMixamo.Add("LeftScapula", "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder");
        BodyPartsToMixamo.Add("LeftShoulder", "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm");
        BodyPartsToMixamo.Add("LeftElbow", "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm");
        BodyPartsToMixamo.Add("LeftWrist", "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:LeftShoulder/mixamorig:LeftArm/mixamorig:LeftForeArm/mixamorig:LeftHand");
        BodyPartsToMixamo.Add("Neck", "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck");
        BodyPartsToMixamo.Add("Head", "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head");
        BodyPartsToMixamo.Add("RightScapula", "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder");
        BodyPartsToMixamo.Add("RightShoulder", "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm");
        BodyPartsToMixamo.Add("RightElbow", "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm");
        BodyPartsToMixamo.Add("RightWrist", "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:RightShoulder/mixamorig:RightArm/mixamorig:RightForeArm/mixamorig:RightHand");

        LoadAnimationClipFromJson();
    }
    void LoadAnimationClipFromJson(){
        // string filePath = Application.dataPath + "/GPTAnim4.json";
        string filePath = Application.dataPath + "/" + pathName;
        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            AnimationClip clip = new AnimationClip();
            switch(JsonFormatVersion){
                case Options.Full:
                    AnimationData animationData = JsonConvert.DeserializeObject<AnimationData>(dataAsJson);
                    clip = CreateAnimationClip(animationData);
                    break;
                case Options.Compressed:
                    AnimationDataComp animationDataComp = JsonConvert.DeserializeObject<AnimationDataComp>(dataAsJson);
                    clip = CreateAnimationClipComp(animationDataComp);
                    break;
                case Options.Compressed2048:
                    AnimationDataComp2048 animationData2048 = JsonConvert.DeserializeObject<AnimationDataComp2048>(dataAsJson);
                    Debug.Log(JsonConvert.SerializeObject(animationData2048));
                    clip = CreateAnimationClipComp2048(animationData2048);
                    break;
                case Options.FullDEG:  //review
                    AnimationData animationDataDEG = JsonConvert.DeserializeObject<AnimationData>(dataAsJson);
                    clip = CreateAnimationClipDEG(animationDataDEG);
                    break;
                case Options.CompressedDEG:    //review
                    AnimationDataComp animationDataCompDEG = JsonConvert.DeserializeObject<AnimationDataComp>(dataAsJson);
                    clip = CreateAnimationClipCompDEG(animationDataCompDEG);
                    break;
                case Options.Compressed2048DEG:
                    AnimationDataComp2048 animationData2048DEG = JsonConvert.DeserializeObject<AnimationDataComp2048>(dataAsJson);
                    Debug.Log(JsonConvert.SerializeObject(animationData2048DEG));
                    clip = CreateAnimationClipComp2048DEG(animationData2048DEG);
                    break;
                default:
                    break;
            }

            Animator animator = GetComponent<Animator>();
            AnimatorOverrideController overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
            animator.runtimeAnimatorController = overrideController;
            if(clip != null){
                // clip.SetLoopTime(true);
                overrideController["Walking"] = clip;
                Debug.Log("Animation overridden with clip: " + clip.name);
                // overrideController.ApplyOverrides(new List<KeyValuePair<AnimationClip, AnimationClip>>(overrideController.overrides));
                animator.Play("TestClip",-1,0f);
                
                // foreach (var a in overrideController.overrides){
                //     Debug.Log("Override: " + a.Key + " -> " + a.Value.name);
                // }
            }
            
        }
        else
        {
            Debug.LogError("Cannot find file at " + filePath);
        }
    }
    AnimationClip CreateAnimationClip(AnimationData data){    
        AnimationClip clip = new AnimationClip();
        clip.name = data.animationName;
        clip.frameRate = 24;
        Debug.Log("Animation overridden with clip: " + clip.name);
        foreach (KeyframeDataLoad keyframeData in data.keyframes)
        {
            AnimationCurve curve = new AnimationCurve();
            foreach (CurvePoint point in keyframeData.curve)
            {
                curve.AddKey(new Keyframe(point.time, point.value));
                Debug.Log(point.value);
            }
            // clip.SetCurve(BodyPartsToMixamo.GetValue(keyframeData.path), typeof(Transform), keyframeData.propertyName, curve);
            clip.SetCurve(keyframeData.path, typeof(Transform), keyframeData.propertyName, curve);  //use this for backward compatibility with uncompressed path names
        }

        return clip;
    }
    AnimationClip CreateAnimationClipComp(AnimationDataComp data){   
        AnimationClip clip = new AnimationClip();
        clip.name = data.animationName;
        Debug.Log("Animation overridden with clip: " + clip.name);
        foreach (KeyframeDataParallel keyframeData in data.keyframes)
        {
            AnimationCurve curvex = new AnimationCurve();
            AnimationCurve curvey = new AnimationCurve();
            AnimationCurve curvez = new AnimationCurve();
            AnimationCurve curvew = new AnimationCurve();
            //replace with vector3 and quaternion
            if(keyframeData.propertyName.Equals("m_LocalRotation")){
                for(int i = 0; i<keyframeData.time.Length; i++){
                    Debug.Log(keyframeData.path + ": time[" +i+"]");
                    curvex.AddKey(new Keyframe(keyframeData.time[i], keyframeData.value[i][0]));
                    curvey.AddKey(new Keyframe(keyframeData.time[i], keyframeData.value[i][1]));
                    curvez.AddKey(new Keyframe(keyframeData.time[i], keyframeData.value[i][2]));
                    curvew.AddKey(new Keyframe(keyframeData.time[i], keyframeData.value[i][3]));
                }
                // Debug.Log(keyframeData);
                clip.SetCurve(keyframeData.path + "", typeof(Transform), keyframeData.propertyName + ".x", curvex);
                clip.SetCurve(keyframeData.path, typeof(Transform), keyframeData.propertyName + ".y", curvey);
                clip.SetCurve(keyframeData.path, typeof(Transform), keyframeData.propertyName + ".z", curvez);
                clip.SetCurve(keyframeData.path, typeof(Transform), keyframeData.propertyName + ".w", curvew);
            }else if(keyframeData.propertyName.Equals("m_LocalPosition")){
                for(int i = 0; i<keyframeData.time.Length; i++){
                    Debug.Log(keyframeData.value);
                    Debug.Log(keyframeData.value[i]);
                    Debug.Log(keyframeData.value[i][2]);
                    curvex.AddKey(new Keyframe(keyframeData.time[i], keyframeData.value[i][0]));
                    curvey.AddKey(new Keyframe(keyframeData.time[i], keyframeData.value[i][1]));
                    curvez.AddKey(new Keyframe(keyframeData.time[i], keyframeData.value[i][2]));
                }
                clip.SetCurve(BodyPartsToMixamo.GetValue(keyframeData.path), typeof(Transform), keyframeData.propertyName + ".x", curvex);
                clip.SetCurve(BodyPartsToMixamo.GetValue(keyframeData.path), typeof(Transform), keyframeData.propertyName + ".y", curvey);
                clip.SetCurve(BodyPartsToMixamo.GetValue(keyframeData.path), typeof(Transform), keyframeData.propertyName + ".z", curvez);
            }
        }

        return clip;
    }
    AnimationClip CreateAnimationClipComp2048(AnimationDataComp2048 data){   
        AnimationClip clip = new AnimationClip();
        clip.name = data.animationName;
        Debug.Log("Animation overridden with clip: " + clip.name);

        List<float[][]> properties = new List<float[][]>();
        properties.Add(data.Hips);
        properties.Add(data.LeftLegHipJoint);
        properties.Add(data.LeftKnee);
        properties.Add(data.LeftAnkle);
        properties.Add(data.RightLegHipJoint);
        properties.Add(data.RightKnee);
        properties.Add(data.RightAnkle);
        properties.Add(data.LowerBack);
        properties.Add(data.MiddleBack);
        properties.Add(data.UpperBack);
        properties.Add(data.LeftScapula);
        properties.Add(data.LeftShoulder);
        properties.Add(data.LeftElbow);
        properties.Add(data.LeftWrist);
        properties.Add(data.Neck);
        properties.Add(data.Head);
        properties.Add(data.RightScapula);
        properties.Add(data.RightShoulder);
        properties.Add(data.RightElbow);
        properties.Add(data.RightWrist);

        AnimationCurve curvex = new AnimationCurve();
        AnimationCurve curvey = new AnimationCurve();
        AnimationCurve curvez = new AnimationCurve();
        AnimationCurve curvew = new AnimationCurve();
        float sampleInterval = 0;
        foreach(float[][] property in properties){
            if(property != null){
                sampleInterval = 1/(float)property.Length;
                break;
            }
        }
        for(int i = 0; i<data.HipsP.Length;i++){
            curvex.AddKey(new Keyframe((i*sampleInterval), data.HipsP[i][0]));
            curvey.AddKey(new Keyframe((i*sampleInterval), data.HipsP[i][1]));
            curvez.AddKey(new Keyframe((i*sampleInterval), data.HipsP[i][2]));
        }

        clip.SetCurve("mixamorig:Hips", typeof(Transform), "m_LocalPosition.x", curvex);
        clip.SetCurve("mixamorig:Hips", typeof(Transform), "m_LocalPosition.y", curvey);
        clip.SetCurve("mixamorig:Hips", typeof(Transform), "m_LocalPosition.z", curvez);
        int counter = 0;
        foreach(float[][] property in properties){
            curvex = new AnimationCurve();
            curvey = new AnimationCurve();
            curvez = new AnimationCurve();
            curvew = new AnimationCurve();
            Debug.Log("Property float[][] length is: "+property.Length);
            for(int i = 0; i<property.Length;i++){
                curvex.AddKey(new Keyframe((i*sampleInterval), property[i][0]));
                curvey.AddKey(new Keyframe((i*sampleInterval), property[i][1]));
                curvez.AddKey(new Keyframe((i*sampleInterval), property[i][2]));
                curvew.AddKey(new Keyframe((i*sampleInterval), property[i][3]));
            }

            clip.SetCurve(propertiesNames[counter], typeof(Transform), "m_LocalRotation.x", curvex);
            clip.SetCurve(propertiesNames[counter], typeof(Transform), "m_LocalRotation.y", curvey);
            clip.SetCurve(propertiesNames[counter], typeof(Transform), "m_LocalRotation.z", curvez);
            clip.SetCurve(propertiesNames[counter], typeof(Transform), "m_LocalRotation.w", curvew);

            counter++;
        }

        return clip;
    }
    AnimationClip CreateAnimationClipDEG(AnimationData data){    
        AnimationClip clip = new AnimationClip();
        clip.name = data.animationName;
        //join back vectors
        Dictionary<string, KeyframeDataLoadDEG> qData = new Dictionary<string, KeyframeDataLoadDEG>();
        Dictionary<string, int> axisToIndex = new Dictionary<string, int>{
            { "x", 0 },
            { "y", 1 },
            { "z", 2 },
            { "w", 3 }
        };
        foreach (KeyframeDataLoad keyframeData in data.keyframes){
            if(keyframeData.propertyName.Split(".")[0].Equals("m_LocalRotation") && !qData.ContainsKey(keyframeData.path)){
                qData[keyframeData.path] = new KeyframeDataLoadDEG();
                qData[keyframeData.path].path = keyframeData.path;
                qData[keyframeData.path].propertyName = "m_LocalRotation";
                foreach(CurvePoint key in keyframeData.curve){
                    qData[keyframeData.path].time.Add(key.time);
                }
            }
            switch (axisToIndex[keyframeData.propertyName.Split(".")[1]]){
                case 0:
                    foreach(CurvePoint key in keyframeData.curve){
                        qData[keyframeData.path].x.Add(key.value);
                    }
                    break;
                case 1:
                    foreach(CurvePoint key in keyframeData.curve){
                        qData[keyframeData.path].y.Add(key.value);
                    }
                    break;
                case 2:
                    foreach(CurvePoint key in keyframeData.curve){
                        qData[keyframeData.path].z.Add(key.value);
                    }
                    break;
                default:
                    break;
            }
        }
        foreach(KeyValuePair<string, KeyframeDataLoadDEG> kvp in qData){
            AnimationCurve curvex = new AnimationCurve();
            AnimationCurve curvey = new AnimationCurve();
            AnimationCurve curvez = new AnimationCurve();
            AnimationCurve curvew = new AnimationCurve();

            Quaternion pastQuaternion = Quaternion.Euler(kvp.Value.x[0], kvp.Value.y[0], kvp.Value.z[0]);
            for(int i = 0; i< kvp.Value.time.Count; i++){
                Quaternion rotation = Quaternion.Euler(kvp.Value.x[i], kvp.Value.y[i], kvp.Value.z[i]);
                if(i != 0){
                    rotation = AdjustQuaternionForShortestPath(pastQuaternion, rotation);
                    pastQuaternion = rotation;
                }
                curvex.AddKey(new Keyframe(kvp.Value.time[i], rotation.x));
                curvey.AddKey(new Keyframe(kvp.Value.time[i], rotation.y));
                curvez.AddKey(new Keyframe(kvp.Value.time[i], rotation.z));
                curvew.AddKey(new Keyframe(kvp.Value.time[i], rotation.w));
            }
            clip.SetCurve(BodyPartsToMixamo.GetValue(kvp.Value.path), typeof(Transform), kvp.Value.propertyName + ".x", curvex);
            clip.SetCurve(BodyPartsToMixamo.GetValue(kvp.Value.path), typeof(Transform), kvp.Value.propertyName + ".y", curvey);
            clip.SetCurve(BodyPartsToMixamo.GetValue(kvp.Value.path), typeof(Transform), kvp.Value.propertyName + ".z", curvez);
            clip.SetCurve(BodyPartsToMixamo.GetValue(kvp.Value.path), typeof(Transform), kvp.Value.propertyName + ".w", curvew);
        }
        return clip;

    }
    AnimationClip CreateAnimationClipCompDEG(AnimationDataComp data){   
        AnimationClip clip = new AnimationClip();
        clip.name = data.animationName;
        Debug.Log("Animation overridden with clip: " + clip.name);
        foreach (KeyframeDataParallel keyframeData in data.keyframes){
            AnimationCurve curvex = new AnimationCurve();
            AnimationCurve curvey = new AnimationCurve();
            AnimationCurve curvez = new AnimationCurve();
            AnimationCurve curvew = new AnimationCurve();

            if(keyframeData.propertyName.Equals("m_LocalPosition")){
                for(int i = 0; i<keyframeData.time.Length; i++){
                    curvex.AddKey(new Keyframe(keyframeData.time[i], keyframeData.value[i][0]));
                    curvey.AddKey(new Keyframe(keyframeData.time[i], keyframeData.value[i][1]));
                    curvez.AddKey(new Keyframe(keyframeData.time[i], keyframeData.value[i][2]));
                }
                clip.SetCurve(keyframeData.path, typeof(Transform), keyframeData.propertyName + ".x", curvex);
                clip.SetCurve(keyframeData.path, typeof(Transform), keyframeData.propertyName + ".y", curvey);
                clip.SetCurve(keyframeData.path, typeof(Transform), keyframeData.propertyName + ".z", curvez);
            }else{
                Quaternion pastQuaternion = Quaternion.Euler(keyframeData.value[0][0], keyframeData.value[0][1], keyframeData.value[0][2]);
                for(int i = 0; i< keyframeData.time.Length; i++){
                    Quaternion rotation = Quaternion.Euler(keyframeData.value[i][0], keyframeData.value[i][1], keyframeData.value[i][2]);
                    if(i != 0){
                        rotation = AdjustQuaternionForShortestPath(pastQuaternion, rotation);
                        pastQuaternion = rotation;
                    }
                    curvex.AddKey(new Keyframe(keyframeData.time[i], rotation.x));
                    curvey.AddKey(new Keyframe(keyframeData.time[i], rotation.y));
                    curvez.AddKey(new Keyframe(keyframeData.time[i], rotation.z));
                    curvew.AddKey(new Keyframe(keyframeData.time[i], rotation.w));
                }
                clip.SetCurve(BodyPartsToMixamo.GetValue(keyframeData.path), typeof(Transform), keyframeData.propertyName + ".x", curvex);
                clip.SetCurve(BodyPartsToMixamo.GetValue(keyframeData.path), typeof(Transform), keyframeData.propertyName + ".y", curvey);
                clip.SetCurve(BodyPartsToMixamo.GetValue(keyframeData.path), typeof(Transform), keyframeData.propertyName + ".z", curvez);
                clip.SetCurve(BodyPartsToMixamo.GetValue(keyframeData.path), typeof(Transform), keyframeData.propertyName + ".w", curvew);
            }
        }

        return clip;
    }
    AnimationClip CreateAnimationClipComp2048DEG(AnimationDataComp2048 data){   
        AnimationClip clip = new AnimationClip();
        clip.name = data.animationName;
        Debug.Log("Animation overridden with clip: " + clip.name);

        List<float[][]> properties = new List<float[][]>();
        properties.Add(data.Hips);
        properties.Add(data.LeftLegHipJoint);
        properties.Add(data.LeftKnee);
        properties.Add(data.LeftAnkle);
        properties.Add(data.RightLegHipJoint);
        properties.Add(data.RightKnee);
        properties.Add(data.RightAnkle);
        properties.Add(data.LowerBack);
        properties.Add(data.MiddleBack);
        properties.Add(data.UpperBack);
        properties.Add(data.LeftScapula);
        properties.Add(data.LeftShoulder);
        properties.Add(data.LeftElbow);
        properties.Add(data.LeftWrist);
        properties.Add(data.Neck);
        properties.Add(data.Head);
        properties.Add(data.RightScapula);
        properties.Add(data.RightShoulder);
        properties.Add(data.RightElbow);
        properties.Add(data.RightWrist);

        AnimationCurve curvex = new AnimationCurve();
        AnimationCurve curvey = new AnimationCurve();
        AnimationCurve curvez = new AnimationCurve();
        AnimationCurve curvew = new AnimationCurve();
        float sampleInterval = 0;
        foreach(float[][] property in properties){
            if(property != null){
                sampleInterval = 1/(float)property.Length;
                break;
            }
        }
        for(int i = 0; i<data.HipsP.Length;i++){
            curvex.AddKey(new Keyframe((i*sampleInterval), data.HipsP[i][0]));
            curvey.AddKey(new Keyframe((i*sampleInterval), data.HipsP[i][1]));
            curvez.AddKey(new Keyframe((i*sampleInterval), data.HipsP[i][2]));
        }

        clip.SetCurve("mixamorig:Hips", typeof(Transform), "m_LocalPosition.x", curvex);
        clip.SetCurve("mixamorig:Hips", typeof(Transform), "m_LocalPosition.y", curvey);
        clip.SetCurve("mixamorig:Hips", typeof(Transform), "m_LocalPosition.z", curvez);
        int counter = 0;
        foreach(float[][] property in properties){
            curvex = new AnimationCurve();
            curvey = new AnimationCurve();
            curvez = new AnimationCurve();
            curvew = new AnimationCurve();
            Quaternion pastQuaternion = Quaternion.Euler(property[0][0], property[0][1], property[0][2]);
            for(int i = 0; i<property.Length;i++){
                Quaternion rotation = Quaternion.Euler(property[i][0], property[i][1], property[i][2]);

                if(i != 0){
                    rotation = AdjustQuaternionForShortestPath(pastQuaternion, rotation);
                    pastQuaternion = rotation;
                }

                curvex.AddKey(new Keyframe((i*sampleInterval), rotation.x));
                curvey.AddKey(new Keyframe((i*sampleInterval), rotation.y));
                curvez.AddKey(new Keyframe((i*sampleInterval), rotation.z));
                curvew.AddKey(new Keyframe((i*sampleInterval), rotation.w));
            }

            clip.SetCurve(propertiesNames[counter], typeof(Transform), "m_LocalRotation.x", curvex);
            clip.SetCurve(propertiesNames[counter], typeof(Transform), "m_LocalRotation.y", curvey);
            clip.SetCurve(propertiesNames[counter], typeof(Transform), "m_LocalRotation.z", curvez);
            clip.SetCurve(propertiesNames[counter], typeof(Transform), "m_LocalRotation.w", curvew);

            counter++;
        }

        return clip;
    }

    Quaternion AdjustQuaternionForShortestPath(Quaternion from, Quaternion to) {
        if (Quaternion.Dot(from, to) < 0) {
            to = new Quaternion(-to.x, -to.y, -to.z, -to.w);
        }
        return to;
    }
}

[Serializable]
public class KeyframeDataLoad
{
    public string path;
    public string propertyName;
    public CurvePoint[] curve;
}
[Serializable]
public class AnimationData
{
    public string animationName;
    public KeyframeDataLoad[] keyframes;
}
[Serializable]
public class CurvePoint
{
    public float time;
    public float value;
}
[System.Serializable]
public class KeyframeDataLoadDEG
{
    public string path;
    public string propertyName;
    public List<float> time;
    public List<float> x;
    public List<float> y;
    public List<float> z;
    public List<float> w;
    public KeyframeDataLoadDEG(){
        time = new List<float>();
        x = new List<float>();
        y = new List<float>();
        z = new List<float>();
        w = new List<float>();
    }
}
[Serializable]
public class AnimationDataDEG
{
    public string animationName;
    public List<KeyframeDataLoadDEG> keyframes;
}
[System.Serializable]
public class KeyframeDataParallel
{
    public string path;
    public string propertyName;
    public float[] time;
    public float[][] value;
}
[Serializable]
public class AnimationDataComp
{
    public string animationName;
    public KeyframeDataParallel[] keyframes;
}

[Serializable]
public class AnimationDataComp2048
{
    public string animationName;
    public float[][] HipsP;
    public float[][] Hips;
    public float[][] LeftLegHipJoint;
    public float[][] LeftKnee;
    public float[][] LeftAnkle;
    public float[][] RightLegHipJoint;
    public float[][] RightKnee;
    public float[][] RightAnkle;
    public float[][] LowerBack;
    public float[][] MiddleBack;
    public float[][] UpperBack;
    public float[][] LeftScapula;
    public float[][] LeftShoulder;
    public float[][] LeftElbow;
    public float[][] LeftWrist;
    public float[][] Neck;
    public float[][] Head;
    public float[][] RightScapula;
    public float[][] RightShoulder;
    public float[][] RightElbow;
    public float[][] RightWrist;
}

public class BiDictionary<T1, T2>{
    private Dictionary<T1, T2> keyToValue = new Dictionary<T1, T2>();
    private Dictionary<T2, T1> valueToKey = new Dictionary<T2, T1>();

    public void Add(T1 key, T2 value){
        if (keyToValue.ContainsKey(key) || valueToKey.ContainsKey(value)){
            Debug.Log(keyToValue[key]);
            Debug.Log(valueToKey[value]);
            throw new ArgumentException("Duplicate key or value.");
        }
        keyToValue.Add(key, value);
        valueToKey.Add(value, key);
    }

    public T2 GetValue(T1 key){
        return keyToValue[key];
    }

    public T1 GetKey(T2 value){
        return valueToKey[value];
    }

    // Optionally, provide methods to handle updates, removals, etc.
}