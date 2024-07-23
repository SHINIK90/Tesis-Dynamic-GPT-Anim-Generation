using UnityEngine;
using System.Text.RegularExpressions;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEditor.Animations;

//Classes For JSON Serialization

[System.Serializable]
public class SerializableAnimationClip
{
    public string name;
    public List<KeyframeData> keyframes;

    public SerializableAnimationClip(AnimationClip clip)
    {
        name = clip.name;
    }
}
[System.Serializable]
public class SerializableAnimationClipCompressed
{
    public string name;
    public List<KeyframeDataParallel> keyframes;

    public SerializableAnimationClipCompressed(AnimationClip clip)
    {
        name = clip.name;
    }
}
[System.Serializable]
public class KeyframeData
{
    public string path;
    public string propertyName;
    public SerializableKeyframe[] curve;
}
[System.Serializable]
public class KeyframeDataParallel
{
    public string path;
    public string propertyName;
    public List<string> time;
    public List<string> value;
    public KeyframeDataParallel(string path, string propertyName)
    {
        this.path = path;
        this.propertyName = propertyName;
        this.time = new List<string>(); // Instantiate the time list
        this.value = new List<string>(); // Instantiate the value list
    }

    // Method to add a keyframe
    public void AddKeyframe(string time, string value)
    {
        this.time.Add(time);
        this.value.Add(value);
    }
}
[System.Serializable]
public class KeyframeDataParallelVectors
{
    public string path;
    public string propertyName;
    public List<string> time;
    public List<string> x;
    public List<string> y;
    public List<string> z;
    public List<string> w;
    public KeyframeDataParallelVectors(string path, string propertyName)
    {
        this.path = path;
        this.propertyName = propertyName;
        this.time = new List<string>(); // Instantiate the time list
        this.x = new List<string>(); // Instantiate the value list
        this.y = new List<string>(); // Instantiate the value list
        this.z = new List<string>(); // Instantiate the value list
        this.w = new List<string>(); // Instantiate the value list
    }

}
[System.Serializable]
public class SerializableKeyframe
{
    public string time;
    public string value;

    public SerializableKeyframe(Keyframe keyframe)
    {
        time = keyframe.time.ToString("F2");//(Mathf.Round(keyframe.time * 1000)/1000).ToString("0.000"); //round to 2 decimals "100"
        value = keyframe.value.ToString("F6");;//(Mathf.Round(keyframe.value * 10000)/10000).ToString("0.00");
    }
}
[System.Serializable]
public class KeyframeDataLoad
{
    public string path;
    public string propertyName;
    public List<float> time;
    public List<float> x;
    public List<float> y;
    public List<float> z;
    public List<float> w;

    public KeyframeDataLoad(){
        time = new List<float>();
        x = new List<float>();
        y = new List<float>();
        z = new List<float>();
        w = new List<float>();
    }
}
[Serializable]
public class AnimationData
{
    public string name;
    public List<KeyframeDataLoad> keyframes;
}
[Serializable]
public class AnimationDataComp2048
{
    public string animationName;
    // public string[,] HipsP; //deprecated position changes
    public string[,] Hips;
    public string[,] LeftLegHipJoint;
    public string[,] LeftKnee;
    public string[,] LeftAnkle;
    public string[,] RightLegHipJoint;
    public string[,] RightKnee;
    public string[,] RightAnkle;
    public string[,] LowerBack;
    public string[,] MiddleBack;
    public string[,] UpperBack;
    public string[,] LeftScapula;
    public string[,] LeftShoulder;
    public string[,] LeftElbow;
    public string[,] LeftWrist;
    public string[,] Neck;
    public string[,] Head;
    public string[,] RightScapula;
    public string[,] RightShoulder;
    public string[,] RightElbow;
    public string[,] RightWrist;
}

public class GptAnimEditorWindow : EditorWindow{

    BiDictionary<string, string> BodyPartsToMixamo = new BiDictionary<string, string>();

    public enum Options
    {
        Full,
        Compressed,
        Tokens,
        FullDEG,
        CompressedDEG,
        TokensDEG
    }
    Dictionary<Options, string> versionPaths;
    // This will appear as a dropdown in the Unity Editor
    public Options JsonFormatVersion;

    private AnimationClip animationClip;
    private int SamplingNumber;
    private int VersionsPromptsNumber;
    private int RepetitionPromptsNumber;
    private int DifferentPromptsNumber;
    private string GeneratedJsonPath;
    // private bool DATASET_LOADED = false;
    private StringBuilder stringBuilder = new StringBuilder();
    // private const int MaxSizeBytes = 1.8 * 1024 * 1024; // 9MB in bytes
    private const int MaxSizeBytes = 2000000;
    private int fileCounter = 0;
    private Transform RigRoot;

    //Quaternion Transformation testing

    private float w = 1.0f; // Default identity quaternion
    private float x = 0.0f;
    private float y = 0.0f;
    private float z = 0.0f;

    //Value in range testing

    private float minRange = 0f;
    private float maxRange = 0f;
    private float angle = 0f;
    private string result = "";

    private List<string> propertiesNames = new List<string>();
    
    [MenuItem("Window/My Editor Window")]
    public static void ShowWindow()
    {
        GetWindow<GptAnimEditorWindow>("GPT Anim Editor Window");
    }
    void OnEnable(){    //Editor Initialization
        versionPaths = new Dictionary<Options, string>{
            { Options.Full, "GPTsFiles/Scoring/Full/" },
            { Options.Compressed, "GPTsFiles/Scoring/Compressed/" },
            { Options.Tokens, "GPTsFiles/Scoring/2048Tokens/" },
            { Options.FullDEG, "GPTsFiles/Scoring/FullDEG/" },
            { Options.CompressedDEG, "GPTsFiles/Scoring/CompressedDEG/" },
            { Options.TokensDEG, "GPTsFiles/Scoring/2048TokensDEG/" }
        };

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
    }
    void OnGUI(){       //Editor Window Management

        GUILayout.Label("Select an Animation Clip:", EditorStyles.boldLabel);
        animationClip = EditorGUILayout.ObjectField(animationClip, typeof(AnimationClip), false) as AnimationClip;
        if (GUILayout.Button("Run Function")){
            if (animationClip != null)
            {
                RunFunction();
            }
            else
            {
                Debug.LogWarning("Please select an Animation Clip first.");
            }
        }

        if (GUILayout.Button("Load Dataset")){
            List<AnimationClip> AnimsDataset = LoadAllFBXAnimations();
            if (AnimsDataset != null)
            {
                int fileNumber = 1;
                Debug.Log("Generating Json Anims");
                foreach(AnimationClip anim in AnimsDataset){
                    string jsonFilePath = Application.dataPath + "/JsonAnims3/"+anim.name+".json";
                    string file = SaveAnimationClipToJsonCompressedVectors(anim, jsonFilePath);
                    int sizeOfCurrentString = Encoding.UTF8.GetByteCount(file);
                    if(fileCounter == 0){
                        stringBuilder.Append(file);
                        fileCounter += sizeOfCurrentString;
                    }else if(fileCounter < MaxSizeBytes && fileCounter != 0){
                        stringBuilder.Append(","+file);
                        fileCounter += sizeOfCurrentString;
                    }else{
                        File.WriteAllText($"{Application.dataPath}/CompressedAnims/Examples_{fileNumber}.json", stringBuilder.ToString());
                        stringBuilder.Clear();
                        fileCounter = 0;
                        fileNumber++;
                    }
                    Debug.Log("Generating: " + anim.name);
                }
            }
            else
            {
                Debug.LogWarning("Error loading dataset");
            }
        }
        SamplingNumber = EditorGUILayout.IntField("Sampling Number", SamplingNumber);
        if (GUILayout.Button("Load 2048 Compressed Dataset") && SamplingNumber != 0){
            List<AnimationClip> AnimsDataset = LoadAllFBXAnimations();
            if (AnimsDataset != null){
                foreach(AnimationClip anim in AnimsDataset){
                    if(anim.length > 1.0f && anim.length < 1.2f){
                        string jsonFilePath = Application.dataPath + "/JsonAnims6-2048-DEG/"+anim.name+".json";
                        string file = SaveAnimationClipToJson2048TokensDEG(anim, jsonFilePath, SamplingNumber);
                    }
                }
            }
        }else{
            Debug.LogWarning("No sampling value set, it must be larger than 1");
        }

        GUILayout.Label("Select a Json and Format Version", EditorStyles.boldLabel);
        RigRoot = EditorGUILayout.ObjectField("Rig Root", RigRoot, typeof(Transform), true) as Transform;
        JsonFormatVersion = (Options)EditorGUILayout.EnumPopup("Select Option", JsonFormatVersion);
        GeneratedJsonPath = EditorGUILayout.TextField("Generated Json Path", GeneratedJsonPath);
        if (GUILayout.Button("Get Json Anim Score") && GeneratedJsonPath != ""){
            string dataAsJson = File.ReadAllText(Application.dataPath + "/" + GeneratedJsonPath);
            AnimationClip clip = new AnimationClip();
            GetScoreFromJson(GeneratedJsonPath, JsonFormatVersion);
            // switch(JsonFormatVersion){
            //     case Options.Full:
            //         AnimationDataJSON animationData = JsonConvert.DeserializeObject<AnimationDataJSON>(dataAsJson);
            //         Debug.Log(JsonConvert.SerializeObject(animationData));
            //         clip = CreateAnimationClip(animationData);
            //         Debug.Log("Score for: " + clip.name + " is = " + CalculateAnimationScore2(clip));
            //         break;
            //     case Options.Compressed:
            //         AnimationDataCompJSON animationDataComp = JsonConvert.DeserializeObject<AnimationDataCompJSON>(dataAsJson);
            //         Debug.Log(JsonConvert.SerializeObject(animationDataComp));
            //         clip = CreateAnimationClipComp(animationDataComp);
            //         Debug.Log("Score for: " + clip.name + " is = " + CalculateAnimationScore2(clip));
            //         break;
            //     case Options.Tokens:
            //         AnimationDataComp2048JSON animationData2048 = JsonConvert.DeserializeObject<AnimationDataComp2048JSON>(dataAsJson);
            //         Debug.Log(dataAsJson);
            //         Debug.Log(JsonConvert.SerializeObject(animationData2048));
            //         clip = CreateAnimationClipComp2048(animationData2048);
            //         Debug.Log("Score for: " + clip.name + " is = " + CalculateAnimationScore2(clip));
            //         break;
            //     case Options.FullDEG:
            //         AnimationDataJSON animationDataDEG = JsonConvert.DeserializeObject<AnimationDataJSON>(dataAsJson);
            //         Debug.Log(JsonConvert.SerializeObject(animationDataDEG));
            //         clip = CreateAnimationClipDEG(animationDataDEG);
            //         Debug.Log("Score for: " + clip.name + " is = " + CalculateAnimationScore2(clip));
            //         break;
            //     case Options.CompressedDEG:
            //         AnimationDataCompJSON animationDataCompDEG = JsonConvert.DeserializeObject<AnimationDataCompJSON>(dataAsJson);
            //         Debug.Log(JsonConvert.SerializeObject(animationDataCompDEG));
            //         clip = CreateAnimationClipCompDEG(animationDataCompDEG);
            //         Debug.Log("Score for: " + clip.name + " is = " + CalculateAnimationScore2(clip));
            //         break;
            //     case Options.TokensDEG:
            //         AnimationDataComp2048JSON animationData2048DEG = JsonConvert.DeserializeObject<AnimationDataComp2048JSON>(dataAsJson);
            //         Debug.Log(JsonConvert.SerializeObject(animationData2048DEG));
            //         clip = CreateAnimationClipComp2048DEG(animationData2048DEG);
            //         Debug.Log("Score for: " + clip.name + " is = " + CalculateAnimationScore2(clip));
            //         break;
            //     default:
            //         break;
            // }
        }else{
            Debug.LogWarning("No data path set");
        }

        GUILayout.Label("Generate All Versions Datasets", EditorStyles.boldLabel);
        SamplingNumber = EditorGUILayout.IntField("Sampling Number", SamplingNumber);
        if (GUILayout.Button("Start Dataset Generation") && SamplingNumber != 0){
            List<AnimationClip> AnimsDataset = LoadAllFBXAnimations();
            if (AnimsDataset != null){
                StringBuilder FullStringBuilder = new StringBuilder();
                StringBuilder compressedStringBuilder = new StringBuilder();
                StringBuilder TokensStringBuilder = new StringBuilder();
                StringBuilder FullDEGStringBuilder = new StringBuilder();
                StringBuilder compressedDEGStringBuilder = new StringBuilder();
                StringBuilder TokensDEGStringBuilder = new StringBuilder();

                int FullfileCounter = 0;
                int compressedfileCounter = 0;
                int TokensfileCounter = 0;
                int FullDEGfileCounter = 0;
                int compressedDEGfileCounter = 0;
                int TokensDEGfileCounter = 0;

                int fileNumber = 1;
                foreach(AnimationClip anim in AnimsDataset){
                    //Full
                    string jsonFilePath = Application.dataPath + "/JSON-Datasets/Full/Output/"+anim.name+".json";
                    string file = SaveAnimationClipToJson(anim, jsonFilePath);
                    int sizeOfCurrentString = Encoding.UTF8.GetByteCount(file);
                    if(FullfileCounter == 0){
                        FullStringBuilder.Append(file);
                        FullfileCounter += sizeOfCurrentString;
                    }else if(FullfileCounter < MaxSizeBytes && FullfileCounter != 0){
                        FullStringBuilder.Append(","+file);
                        FullfileCounter += sizeOfCurrentString;
                    }else{
                        File.WriteAllText($"{Application.dataPath}/JSON-Datasets/Full/CompressedAnims/Examples_{fileNumber}.json", FullStringBuilder.ToString());
                        FullStringBuilder.Clear();
                        FullfileCounter = 0;
                    }
                    //compressed
                    jsonFilePath = Application.dataPath + "/JSON-Datasets/Compressed/Output/"+anim.name+".json";
                    file = SaveAnimationClipToJsonCompressedVectors(anim, jsonFilePath);
                    sizeOfCurrentString = Encoding.UTF8.GetByteCount(file);
                    if(compressedfileCounter == 0){
                        compressedStringBuilder.Append(file);
                        compressedfileCounter += sizeOfCurrentString;
                    }else if(compressedfileCounter < MaxSizeBytes && compressedfileCounter != 0){
                        compressedStringBuilder.Append(","+file);
                        compressedfileCounter += sizeOfCurrentString;
                    }else{
                        File.WriteAllText($"{Application.dataPath}/JSON-Datasets/Compressed/CompressedAnims/Examples_{fileNumber}.json", compressedStringBuilder.ToString());
                        compressedStringBuilder.Clear();
                        compressedfileCounter = 0;
                    }
                    //2048Tokens
                    jsonFilePath = Application.dataPath + "/JSON-Datasets/2048Tokens/Output/"+anim.name+".json";
                    file = SaveAnimationClipToJson2048Tokens(anim, jsonFilePath, SamplingNumber);//2 should be changed    //add sampling values for dataset generation for this and possible compressed aswell
                    sizeOfCurrentString = Encoding.UTF8.GetByteCount(file);
                    if(TokensfileCounter == 0){
                        TokensStringBuilder.Append(file);
                        TokensfileCounter += sizeOfCurrentString;
                    }else if(TokensfileCounter < MaxSizeBytes && TokensfileCounter != 0){
                        TokensStringBuilder.Append(","+file);
                        TokensfileCounter += sizeOfCurrentString;
                    }else{
                        File.WriteAllText($"{Application.dataPath}/JSON-Datasets/2048Tokens/CompressedAnims/Examples_{fileNumber}.json", TokensStringBuilder.ToString());
                        TokensStringBuilder.Clear();
                        TokensfileCounter = 0;
                    }
                    //Full  DEG
                    jsonFilePath = Application.dataPath + "/JSON-Datasets/FullDEG/Output/"+anim.name+".json";
                    file = SaveAnimationClipToJsonDEG(anim, jsonFilePath);
                    sizeOfCurrentString = Encoding.UTF8.GetByteCount(file);
                    if(FullDEGfileCounter == 0){
                        FullDEGStringBuilder.Append(file);
                        FullDEGfileCounter += sizeOfCurrentString;
                    }else if(FullDEGfileCounter < MaxSizeBytes && FullDEGfileCounter != 0){
                        FullDEGStringBuilder.Append(","+file);
                        FullDEGfileCounter += sizeOfCurrentString;
                    }else{
                        File.WriteAllText($"{Application.dataPath}/JSON-Datasets/FullDEG/CompressedAnims/Examples_{fileNumber}.json", FullDEGStringBuilder.ToString());
                        FullDEGStringBuilder.Clear();
                        FullDEGfileCounter = 0;
                    }
                    //compressed DEG
                    jsonFilePath = Application.dataPath + "/JSON-Datasets/compressedDEG/Output/"+anim.name+".json";
                    file = SaveAnimationClipToJsonCompressedVectorsDEG(anim, jsonFilePath);
                    sizeOfCurrentString = Encoding.UTF8.GetByteCount(file);
                    if(compressedDEGfileCounter == 0){
                        compressedDEGStringBuilder.Append(file);
                        compressedDEGfileCounter += sizeOfCurrentString;
                    }else if(compressedDEGfileCounter < MaxSizeBytes && compressedDEGfileCounter != 0){
                        compressedDEGStringBuilder.Append(","+file);
                        compressedDEGfileCounter += sizeOfCurrentString;
                    }else{
                        File.WriteAllText($"{Application.dataPath}/JSON-Datasets/compressedDEG/CompressedAnims/Examples_{fileNumber}.json", compressedDEGStringBuilder.ToString());
                        compressedDEGStringBuilder.Clear();
                        compressedDEGfileCounter = 0;
                    }
                    //2048Tokens DEG
                    jsonFilePath = Application.dataPath + "/JSON-Datasets/2048TokensDEG/Output/"+anim.name+".json";
                    file = SaveAnimationClipToJson2048TokensDEG(anim, jsonFilePath, SamplingNumber);
                    sizeOfCurrentString = Encoding.UTF8.GetByteCount(file);
                    if(TokensDEGfileCounter == 0){
                        TokensDEGStringBuilder.Append(file);
                        TokensDEGfileCounter += sizeOfCurrentString;
                    }else if(TokensDEGfileCounter < MaxSizeBytes && TokensDEGfileCounter != 0){
                        TokensDEGStringBuilder.Append(","+file);
                        TokensDEGfileCounter += sizeOfCurrentString;
                    }else{
                        File.WriteAllText($"{Application.dataPath}/JSON-Datasets/2048TokensDEG/CompressedAnims/Examples_{fileNumber}.json", TokensDEGStringBuilder.ToString());
                        TokensDEGStringBuilder.Clear();
                        TokensDEGfileCounter = 0;
                    }


                    // if(anim.length > 1.0f && anim.length < 1.2f){
                    //     string jsonFilePath = Application.dataPath + "/JsonAnims6-2048-DEG/"+anim.name+".json";
                    //     string file = SaveAnimationClipToJson2048TokensDEG(anim, jsonFilePath, SamplingNumber);
                    // }

                    //increase file number for next file
                    fileNumber++;
                }
            }
        }else{
            Debug.LogWarning("No sampling value set, it must be larger than 1");
        }

        GUILayout.Label("Enter Quaternion Values", EditorStyles.boldLabel);
        // Input fields for quaternion components
        w = EditorGUILayout.FloatField("W", w);
        x = EditorGUILayout.FloatField("X", x);
        y = EditorGUILayout.FloatField("Y", y);
        z = EditorGUILayout.FloatField("Z", z);
        if (GUILayout.Button("Convert to Euler Angles")){
            Quaternion quaternion = new Quaternion(x, y, z, w);
            Vector3 eulerAngles = quaternion.eulerAngles; // Unity does the conversion
            Debug.Log("Euler Angles: " + eulerAngles.ToString());
        }

        // Generate Scores for all formats
        VersionsPromptsNumber = EditorGUILayout.IntField("Versions Prompts Number", VersionsPromptsNumber);
        if (GUILayout.Button("Generate Scores CSV to path") && VersionsPromptsNumber != 0){
            StringBuilder csvContent = new StringBuilder();
            csvContent.AppendLine("Animation,Full,Compressed,Tokens,FullDEG,CompressedDEG,TokensDEG");

            for(int i = 1; i<=VersionsPromptsNumber; i++){
                string line = i.ToString();
                foreach(KeyValuePair<Options, string> kvp in versionPaths){
                    float score = GetScoreFromJsonDataset(i,kvp.Key);
                    line += "," + score.ToString("F2");
                    Debug.Log(kvp.Key + ": " + i);
                }
                csvContent.AppendLine(line);
            }
            File.WriteAllText(Application.dataPath + "/GPTsFiles/Scoring/" + "Scores.csv", csvContent.ToString());
            Debug.Log("Scores written to CSV file.");
            
        }else{ 
            Debug.LogWarning("No number of prompts set, must be 1 or more");
        }

        // Generate scores for winner format
        RepetitionPromptsNumber = EditorGUILayout.IntField("Number of repetitions per prompt", RepetitionPromptsNumber);
        DifferentPromptsNumber = EditorGUILayout.IntField("Number of diferent prompts", DifferentPromptsNumber);
        if (GUILayout.Button("Generate FullDEG Scores CSV to path") && RepetitionPromptsNumber != 0){
            StringBuilder csvContent = new StringBuilder();
            csvContent.AppendLine("Repetition,1,2,3,4,5");

            for(int i = 1; i<=RepetitionPromptsNumber; i++){
                string line = i.ToString();

                for(int j = 1; j<=DifferentPromptsNumber; j++){
                    string repetitionsPath = "GPTsFiles/Scoring/PromptRepetition/" + j + "/" + i + ".json";
                    float score = GetScoreFromJson(repetitionsPath, Options.FullDEG);
                    line += "," + score.ToString("F2");
                }
                csvContent.AppendLine(line);
            }
            File.WriteAllText(Application.dataPath + "/" +"GPTsFiles/Scoring/PromptRepetition/" + "FullDEGScores.csv", csvContent.ToString());
            Debug.Log("FullDEG Scores written to CSV file.");
            
        }else{ 
            Debug.LogWarning("No number of prompts set, must be 1 or more");
        }

        // Angle in raange testing
        GUILayout.Label("Angle Range Checker", EditorStyles.boldLabel);
        minRange = EditorGUILayout.FloatField("Min Range", minRange);
        maxRange = EditorGUILayout.FloatField("Max Range", maxRange);
        angle = EditorGUILayout.Slider("Angle", angle, 0f, 360f);
        if(GUILayout.Button("Check Angle")){
            bool isInRange = IsAngleInRange(angle, minRange, maxRange);
            result = isInRange ? "The angle is within the range." : "The angle is outside the range.";
        }
        GUILayout.Label(result, EditorStyles.label);
    }
    void RunFunction(){ //test anim to JSON generation with a single file, selecting the desired version
        string jsonFilePath = Application.dataPath + "/YourAnimationClip4.json";
        SaveAnimationClipToJson(animationClip, jsonFilePath);
        // SaveAnimationClipToJsonCompressedVectors(animationClip, jsonFilePath);
        // SaveAnimationClipToJson2048Tokens(animationClip, jsonFilePath, 2);   //sampling value should be 2 or higher to see at minimum one change in animation
        // SaveAnimationClipToJsonCompressedVectorsDEG(animationClip, jsonFilePath);
        // SaveAnimationClipToJson2048TokensDEG(animationClip, jsonFilePath, 2);   //sampling value should be 2 or higher to see at minimum one change in animation
        Debug.Log("Function is running with Animation Clip: " + animationClip.name);
    }
    public static bool IsAngleInRange(float angle, float min, float max)
    {
        // angle = NormalizeAngle(angle);
        // min = NormalizeAngle(min);
        // max = NormalizeAngle(max);

        if (min < max)
        {
            return angle >= min && angle <= max;
        }
        else // This handles the wrap around 360 degrees
        {
            return angle >= min || angle <= max;
        }
    }
    
    //Functions to save Json from Clips
    string SaveAnimationClipToJson(AnimationClip clip, string filePath){

        string directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);  // Create the directory if it does not exist
        }
        // Create a serializable object to store animation clip data
        SerializableAnimationClip serializableClip = new SerializableAnimationClip(clip);
        serializableClip.keyframes = new List<KeyframeData>();

        // Extract keyframe data from the animation curve
        foreach (EditorCurveBinding curveBinding in AnimationUtility.GetCurveBindings(clip))
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, curveBinding);

            if(curve.keys.Length > 2 && propertiesNames.Contains(curveBinding.path)){
                KeyframeData keyframeData = new KeyframeData();
                keyframeData.path = BodyPartsToMixamo.GetKey(curveBinding.path);
                keyframeData.propertyName = curveBinding.propertyName;
                keyframeData.curve = new SerializableKeyframe[curve.keys.Length];

                for (int i = 0; i < curve.keys.Length; i++)
                {
                    keyframeData.curve[i] = new SerializableKeyframe(curve.keys[i]);
                }

                serializableClip.keyframes.Add(keyframeData);
            }

        }

        // Convert the serializable object to JSON format
        string jsonData = JsonUtility.ToJson(serializableClip, true).Replace(" ", "").Replace("\n", "");

        //convert float values
        var regex = new Regex("\"(-?[0-9]+\\.[0-9]+)\"");
        string convertedJson = regex.Replace(jsonData, match => match.Groups[1].Value);

        // Write the JSON data to a file
        File.WriteAllText(filePath, convertedJson);

        Debug.Log("Animation clip data saved to: " + filePath);
        // Debug.Log(jsonData);
        return convertedJson;
    }
    string SaveAnimationClipToJsonCompressedVectors(AnimationClip clip, string filePath){
        // Create a serializable object to store animation clip data
        SerializableAnimationClipCompressed serializableClip = new SerializableAnimationClipCompressed(clip);
        serializableClip.keyframes = new List<KeyframeDataParallel>();

        // Extract keyframe data from the animation curve
        foreach (EditorCurveBinding curveBinding in AnimationUtility.GetCurveBindings(clip))
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, curveBinding);

            if(curve.keys.Length > 2 && propertiesNames.Contains(curveBinding.path)){
                KeyframeDataParallel keyframeData = new KeyframeDataParallel(BodyPartsToMixamo.GetKey(curveBinding.path), curveBinding.propertyName);

                for (int i = 0; i < curve.keys.Length; i++){
                    keyframeData.AddKeyframe(curve.keys[i].time.ToString("F2"), curve.keys[i].value.ToString("F3"));
                }

                serializableClip.keyframes.Add(keyframeData);
            }

        }
        Dictionary<string,KeyframeDataParallelVectors> vectors = new Dictionary<string,KeyframeDataParallelVectors>();
        Dictionary<string, int> axisToIndex = new Dictionary<string, int>
        {
            { "x", 0 },
            { "y", 1 },
            { "z", 2 },
            { "w", 3 }
        };
        foreach(KeyframeDataParallel keyframe in serializableClip.keyframes){
            string pathProperty = keyframe.path + "#" + keyframe.propertyName.Split(".")[0];
            if(!vectors.ContainsKey(pathProperty)){
                vectors[pathProperty] = new KeyframeDataParallelVectors(keyframe.path,keyframe.propertyName.Split(".")[0]);
                vectors[pathProperty].time = keyframe.time;
            }
            switch (axisToIndex[keyframe.propertyName.Split(".")[1]]){
                case 0:
                    vectors[pathProperty].x = keyframe.value;
                    break;
                case 1:
                    vectors[pathProperty].y = keyframe.value;
                    break;
                case 2:
                    vectors[pathProperty].z = keyframe.value;
                    break;
                case 3:
                    vectors[pathProperty].w = keyframe.value;
                    break;
                default:
                    break;
            }

        }
        List<KeyframeDataParallelVectors> myList = vectors.Values.ToList();
        serializableClip.keyframes = new List<KeyframeDataParallel>();
        foreach(KeyframeDataParallelVectors vectorKeyframe in myList){
            KeyframeDataParallel keyframeData = new KeyframeDataParallel(vectorKeyframe.path, vectorKeyframe.propertyName);
            if(vectorKeyframe.propertyName.Equals("m_LocalRotation")){
                for (int i = 0; i < vectorKeyframe.time.Count; i++){
                    keyframeData.AddKeyframe(vectorKeyframe.time[i], $"[{vectorKeyframe.x[i]},{vectorKeyframe.y[i]},{vectorKeyframe.z[i]},{vectorKeyframe.w[i]}]");
                }
            }
            // else{
            //     for (int i = 0; i < vectorKeyframe.time.Count; i++){
            //         keyframeData.AddKeyframe(vectorKeyframe.time[i], $"[{vectorKeyframe.x[i]},{vectorKeyframe.y[i]},{vectorKeyframe.z[i]}]");// deprecated use of position changes
            //     }
            // }
            serializableClip.keyframes.Add(keyframeData);
        }

        // Convert the serializable object to JSON format
        string jsonData = JsonUtility.ToJson(serializableClip, true).Replace(" ", "").Replace("\n", "");

        //convert float values
        var regex = new Regex("\"(-?[0-9]+(?:\\.[0-9]+)?)\"");
        string convertedJson = regex.Replace(jsonData, match => match.Groups[1].Value);
        string pattern = "\"\\[(.*?)\\]\"";
        convertedJson = Regex.Replace(convertedJson, pattern, "[$1]");
        // string jsonCompact = Regex.Replace(convertedJson, @"\s*(\[\s*|\]\s*|,\s*)\s*", "$1");

        // Write the JSON data to a file
        File.WriteAllText(filePath, convertedJson);

        Debug.Log("Animation clip data saved to: " + filePath);
        // Debug.Log(jsonData);

        return convertedJson;
    }
    string SaveAnimationClipToJson2048Tokens(AnimationClip clip, string filePath, int sampling){
        int bodyPartCount = 20;
        // Create the list to hold all body parts' animations
        List<string[,]> bodyPartsAnimations = new List<string[,]>();

        // Initialize each body part with its keyframes and quaternion values
        for (int i = 0; i < bodyPartCount; i++){
            string[,] keyframes = new string[sampling, 4];  // Create a 2D array for keyframes and quaternion components
            bodyPartsAnimations.Add(keyframes);  // Add to the list
        }

        Dictionary<string, int> axisToIndex = new Dictionary<string, int>
        {
            { "x", 0 },
            { "y", 1 },
            { "z", 2 },
            { "w", 3 }
        };

        // Create a serializable object to store animation clip data
        AnimationDataComp2048 serializableClip = new AnimationDataComp2048();
        serializableClip.animationName = clip.name;
        float sampleInterval = 1.0f / sampling;

        // Extract keyframe data from the animation curve
        foreach (EditorCurveBinding curveBinding in AnimationUtility.GetCurveBindings(clip)){
            if(propertiesNames.Contains(curveBinding.path)){ 
                AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, curveBinding);
                for(int i = 0; i<sampling; i++){
                    float value = curve.Evaluate((float)i*sampleInterval);

                    int bodyIndex = propertiesNames.IndexOf(curveBinding.path);
                    if(bodyIndex == 0 && curveBinding.propertyName.Split(".")[0] == "m_LocalPosition"){
                        // HipsPosition[i, axisToIndex[curveBinding.propertyName.Split(".")[1]]] = value.ToString("F3");   //deprecated position changes
                    }else{
                        //set up the quaternion at the given index and keyframe as [i,axis]
                        Debug.Log(bodyPartsAnimations[bodyIndex] + "|index:" + bodyIndex);
                        bodyPartsAnimations[bodyIndex][i,axisToIndex[curveBinding.propertyName.Split(".")[1]]] = value.ToString("F3");
                    }
                }
            }
        }
        // serializableClip.HipsP = HipsPosition;   //deprecated position changes
        serializableClip.Hips = bodyPartsAnimations[0];
        serializableClip.LeftLegHipJoint = bodyPartsAnimations[1];
        serializableClip.LeftKnee = bodyPartsAnimations[2];
        serializableClip.LeftAnkle = bodyPartsAnimations[3];
        serializableClip.RightLegHipJoint = bodyPartsAnimations[4];
        serializableClip.RightKnee = bodyPartsAnimations[5];
        serializableClip.RightAnkle = bodyPartsAnimations[6];
        serializableClip.LowerBack = bodyPartsAnimations[7];
        serializableClip.MiddleBack = bodyPartsAnimations[8];
        serializableClip.UpperBack = bodyPartsAnimations[9];
        serializableClip.LeftScapula = bodyPartsAnimations[10];
        serializableClip.LeftShoulder = bodyPartsAnimations[11];
        serializableClip.LeftElbow = bodyPartsAnimations[12];
        serializableClip.LeftWrist = bodyPartsAnimations[13];
        serializableClip.Neck = bodyPartsAnimations[14];
        serializableClip.Head = bodyPartsAnimations[15];
        serializableClip.RightScapula = bodyPartsAnimations[16];
        serializableClip.RightShoulder = bodyPartsAnimations[17];
        serializableClip.RightElbow = bodyPartsAnimations[18];
        serializableClip.RightWrist = bodyPartsAnimations[19];

        // Convert the serializable object to JSON format
        string jsonData = JsonConvert.SerializeObject(serializableClip).Replace(" ", "").Replace("\n", "");
        // string jsonData = JsonUtility.ToJson(serializableClip, true).Replace(" ", "").Replace("\n", "");

        //convert float values
        var regex = new Regex("\"(-?[0-9]+\\.[0-9]+)\"");
        string convertedJson = regex.Replace(jsonData, match => match.Groups[1].Value);

        // Write the JSON data to a file
        File.WriteAllText(filePath, convertedJson);

        Debug.Log("Animation clip data saved to: " + filePath);
        // Debug.Log(jsonData);
        return convertedJson;
    }
    string SaveAnimationClipToJsonDEG(AnimationClip clip, string filePath){
        // Create a serializable object to store animation clip data
        Dictionary<string, KeyframeDataLoad> quaternionsData = new Dictionary<string, KeyframeDataLoad>();
        Dictionary<string, int> axisToIndex = new Dictionary<string, int>{
            { "x", 0 },
            { "y", 1 },
            { "z", 2 },
            { "w", 3 }
        };

        foreach (EditorCurveBinding curveBinding in AnimationUtility.GetCurveBindings(clip)){
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, curveBinding);
            
            if(curve.keys.Length > 2 && propertiesNames.Contains(curveBinding.path) && curveBinding.propertyName.Split(".")[0] == "m_LocalRotation"){
                string pathProperty = BodyPartsToMixamo.GetKey(curveBinding.path) + "#" + curveBinding.propertyName.Split(".")[0];
                if(!quaternionsData.ContainsKey(pathProperty)){
                    quaternionsData[pathProperty] = new KeyframeDataLoad();
                    quaternionsData[pathProperty].path = BodyPartsToMixamo.GetKey(curveBinding.path);
                    quaternionsData[pathProperty].propertyName = curveBinding.propertyName.Split(".")[0];
                    foreach(Keyframe key in curve.keys){
                        quaternionsData[pathProperty].time.Add(key.time);
                    }
                }
                switch (axisToIndex[curveBinding.propertyName.Split(".")[1]]){
                    case 0:
                        foreach(Keyframe key in curve.keys){
                            quaternionsData[pathProperty].x.Add(key.value);
                        }
                        break;
                    case 1:
                        foreach(Keyframe key in curve.keys){
                            quaternionsData[pathProperty].y.Add(key.value);
                        }
                        break;
                    case 2:
                        foreach(Keyframe key in curve.keys){
                            quaternionsData[pathProperty].z.Add(key.value);
                        }
                        break;
                    case 3:
                        foreach(Keyframe key in curve.keys){
                            quaternionsData[pathProperty].w.Add(key.value);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        SerializableAnimationClip serializableClip = new SerializableAnimationClip(clip);
        serializableClip.keyframes = new List<KeyframeData>();
        foreach(KeyValuePair<string, KeyframeDataLoad> kvp in quaternionsData){

            KeyframeData keyframeDataX = new KeyframeData();
            KeyframeData keyframeDataY = new KeyframeData();
            KeyframeData keyframeDataZ = new KeyframeData();

            keyframeDataX.path = kvp.Value.path;
            keyframeDataX.propertyName = kvp.Value.propertyName + ".x";
            keyframeDataY.path = kvp.Value.path;
            keyframeDataY.propertyName = kvp.Value.propertyName + ".y";
            keyframeDataZ.path = kvp.Value.path;
            keyframeDataZ.propertyName = kvp.Value.propertyName + ".z";

            keyframeDataX.curve = new SerializableKeyframe[kvp.Value.time.Count];
            keyframeDataY.curve = new SerializableKeyframe[kvp.Value.time.Count];
            keyframeDataZ.curve = new SerializableKeyframe[kvp.Value.time.Count];

            for(int i = 0; i<kvp.Value.time.Count; i++){
                Quaternion q = new Quaternion(kvp.Value.x[i],kvp.Value.y[i],kvp.Value.z[i],kvp.Value.w[i]);
                Vector3 rot = q.eulerAngles;
                keyframeDataX.curve[i] = new SerializableKeyframe(new Keyframe(kvp.Value.time[i], rot.x));
                keyframeDataY.curve[i] = new SerializableKeyframe(new Keyframe(kvp.Value.time[i], rot.y));
                keyframeDataZ.curve[i] = new SerializableKeyframe(new Keyframe(kvp.Value.time[i], rot.z));
            }

            serializableClip.keyframes.Add(keyframeDataX);
            serializableClip.keyframes.Add(keyframeDataY);
            serializableClip.keyframes.Add(keyframeDataZ);
        }

        // Convert the serializable object to JSON format
        string jsonData = JsonUtility.ToJson(serializableClip, true).Replace(" ", "").Replace("\n", "");

        //convert float values
        var regex = new Regex("\"(-?[0-9]+\\.[0-9]+)\"");
        string convertedJson = regex.Replace(jsonData, match => match.Groups[1].Value);

        // Write the JSON data to a file
        File.WriteAllText(filePath, convertedJson);

        Debug.Log("Animation clip data saved to: " + filePath);
        // Debug.Log(jsonData);
        return convertedJson;
    }
    string SaveAnimationClipToJsonCompressedVectorsDEG(AnimationClip clip, string filePath){
        // Create a serializable object to store animation clip data
        SerializableAnimationClipCompressed serializableClip = new SerializableAnimationClipCompressed(clip);
        serializableClip.keyframes = new List<KeyframeDataParallel>();

        // Extract keyframe data from the animation curve
        foreach (EditorCurveBinding curveBinding in AnimationUtility.GetCurveBindings(clip))
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, curveBinding);

            if(curve.keys.Length > 2 && propertiesNames.Contains(curveBinding.path)){
                KeyframeDataParallel keyframeData = new KeyframeDataParallel(BodyPartsToMixamo.GetKey(curveBinding.path), curveBinding.propertyName);

                for (int i = 0; i < curve.keys.Length; i++){
                    keyframeData.AddKeyframe(curve.keys[i].time.ToString("F2"), curve.keys[i].value.ToString("F3"));
                }

                serializableClip.keyframes.Add(keyframeData);
            }

        }
        Dictionary<string,KeyframeDataParallelVectors> vectors = new Dictionary<string,KeyframeDataParallelVectors>();
        Dictionary<string, int> axisToIndex = new Dictionary<string, int>
        {
            { "x", 0 },
            { "y", 1 },
            { "z", 2 },
            { "w", 3 }
        };
        foreach(KeyframeDataParallel keyframe in serializableClip.keyframes){
            string pathProperty = keyframe.path + "#" + keyframe.propertyName.Split(".")[0];
            if(!vectors.ContainsKey(pathProperty)){
                vectors[pathProperty] = new KeyframeDataParallelVectors(keyframe.path,keyframe.propertyName.Split(".")[0]);
                vectors[pathProperty].time = keyframe.time;
            }
            switch (axisToIndex[keyframe.propertyName.Split(".")[1]]){
                case 0:
                    vectors[pathProperty].x = keyframe.value;
                    break;
                case 1:
                    vectors[pathProperty].y = keyframe.value;
                    break;
                case 2:
                    vectors[pathProperty].z = keyframe.value;
                    break;
                case 3:
                    vectors[pathProperty].w = keyframe.value;
                    break;
                default:
                    break;
            }

        }
        List<KeyframeDataParallelVectors> myList = vectors.Values.ToList();
        serializableClip.keyframes = new List<KeyframeDataParallel>();
        foreach(KeyframeDataParallelVectors vectorKeyframe in myList){
            KeyframeDataParallel keyframeData = new KeyframeDataParallel(vectorKeyframe.path, vectorKeyframe.propertyName);
            if(vectorKeyframe.propertyName.Equals("m_LocalRotation")){
                for (int i = 0; i < vectorKeyframe.time.Count; i++){
                    Quaternion original = new Quaternion(float.Parse(vectorKeyframe.x[i]),float.Parse(vectorKeyframe.y[i]),float.Parse(vectorKeyframe.z[i]),float.Parse(vectorKeyframe.w[i]));
                    Vector3 converted = original.eulerAngles;
                    keyframeData.AddKeyframe(vectorKeyframe.time[i], $"[{converted.x},{converted.y},{converted.z}]");
                }
            }
            // else{
            //     for (int i = 0; i < vectorKeyframe.time.Count; i++){
            //         keyframeData.AddKeyframe(vectorKeyframe.time[i], $"[{vectorKeyframe.x[i]},{vectorKeyframe.y[i]},{vectorKeyframe.z[i]}]");// deprecated use of position changes
            //     }
            // }
            serializableClip.keyframes.Add(keyframeData);
        }

        // Convert the serializable object to JSON format
        string jsonData = JsonUtility.ToJson(serializableClip, true).Replace(" ", "").Replace("\n", "");

        //convert float values
        var regex = new Regex("\"(-?[0-9]+(?:\\.[0-9]+)?)\"");
        string convertedJson = regex.Replace(jsonData, match => match.Groups[1].Value);
        string pattern = "\"\\[(.*?)\\]\"";
        convertedJson = Regex.Replace(convertedJson, pattern, "[$1]");
        // string jsonCompact = Regex.Replace(convertedJson, @"\s*(\[\s*|\]\s*|,\s*)\s*", "$1");

        // Write the JSON data to a file
        File.WriteAllText(filePath, convertedJson);

        Debug.Log("Animation clip data saved to: " + filePath);
        // Debug.Log(jsonData);

        return convertedJson;
    }
    string SaveAnimationClipToJson2048TokensDEG(AnimationClip clip, string filePath, int sampling){
        int bodyPartCount = 20;

        // Create the list to hold all body parts' animations
        List<float[,]> bodyPartsAnimations = new List<float[,]>();

        // Initialize each body part with its keyframes and quaternion values
        for (int i = 0; i < bodyPartCount; i++)
        {
            float[,] keyframes = new float[sampling, 4];  // Create a 2D array for keyframes and quaternion components
            bodyPartsAnimations.Add(keyframes);  // Add to the list
        }

        Dictionary<string, int> axisToIndex = new Dictionary<string, int>
        {
            { "x", 0 },
            { "y", 1 },
            { "z", 2 },
            { "w", 3 }
        };

        // Create a serializable object to store animation clip data
        AnimationDataComp2048 serializableClip = new AnimationDataComp2048();
        serializableClip.animationName = clip.name;
        float sampleInterval = 1.0f / sampling;

        // Extract keyframe data from the animation curve
        foreach (EditorCurveBinding curveBinding in AnimationUtility.GetCurveBindings(clip)){
            if(propertiesNames.Contains(curveBinding.path)){ 
                AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, curveBinding);
                for(int i = 0; i<sampling; i++){
                    float value = curve.Evaluate((float)i*sampleInterval);

                    int bodyIndex = propertiesNames.IndexOf(curveBinding.path);
                    if(bodyIndex == 0 && curveBinding.propertyName.Split(".")[0] == "m_LocalPosition"){
                        // HipsPosition[i, axisToIndex[curveBinding.propertyName.Split(".")[1]]] = value.ToString("F3");   //deprecated position changes
                    }else{
                        //set up the quaternion at the given index and keyframe as [i,axis]
                        Debug.Log(bodyPartsAnimations[bodyIndex] + "|index:" + bodyIndex);
                        bodyPartsAnimations[bodyIndex][i,axisToIndex[curveBinding.propertyName.Split(".")[1]]] = value;//.ToString("F3");
                    }
                }
            }
        }

        // serializableClip.HipsP = HipsPosition;  //deprecated position changes
        serializableClip.Hips = ToEulerTransformString(bodyPartsAnimations[0]);
        serializableClip.LeftLegHipJoint = ToEulerTransformString(bodyPartsAnimations[1]);
        serializableClip.LeftKnee = ToEulerTransformString(bodyPartsAnimations[2]);
        serializableClip.LeftAnkle = ToEulerTransformString(bodyPartsAnimations[3]);
        serializableClip.RightLegHipJoint = ToEulerTransformString(bodyPartsAnimations[4]);
        serializableClip.RightKnee = ToEulerTransformString(bodyPartsAnimations[5]);
        serializableClip.RightAnkle = ToEulerTransformString(bodyPartsAnimations[6]);
        serializableClip.LowerBack = ToEulerTransformString(bodyPartsAnimations[7]);
        serializableClip.MiddleBack = ToEulerTransformString(bodyPartsAnimations[8]);
        serializableClip.UpperBack = ToEulerTransformString(bodyPartsAnimations[9]);
        serializableClip.LeftScapula = ToEulerTransformString(bodyPartsAnimations[10]);
        serializableClip.LeftShoulder = ToEulerTransformString(bodyPartsAnimations[11]);
        serializableClip.LeftElbow = ToEulerTransformString(bodyPartsAnimations[12]);
        serializableClip.LeftWrist = ToEulerTransformString(bodyPartsAnimations[13]);
        serializableClip.Neck = ToEulerTransformString(bodyPartsAnimations[14]);
        serializableClip.Head = ToEulerTransformString(bodyPartsAnimations[15]);
        serializableClip.RightScapula = ToEulerTransformString(bodyPartsAnimations[16]);
        serializableClip.RightShoulder = ToEulerTransformString(bodyPartsAnimations[17]);
        serializableClip.RightElbow = ToEulerTransformString(bodyPartsAnimations[18]);
        serializableClip.RightWrist = ToEulerTransformString(bodyPartsAnimations[19]);

        // Convert the serializable object to JSON format
        string jsonData = JsonConvert.SerializeObject(serializableClip).Replace(" ", "").Replace("\n", "");
        // string jsonData = JsonUtility.ToJson(serializableClip, true).Replace(" ", "").Replace("\n", "");

        //convert float values
        var regex = new Regex("\"(-?[0-9]+(?:\\.[0-9]+)?)\"");
        string convertedJson = regex.Replace(jsonData, match => match.Groups[1].Value);

        // Write the JSON data to a file
        File.WriteAllText(filePath, convertedJson);

        Debug.Log("Animation clip data saved to: " + filePath);
        // Debug.Log(jsonData);
        return convertedJson;
    }

    string[,] ToEulerTransformString(float[,] quaternion){
        string[,] result = new string[quaternion.GetLength(0), 3];
        for(int i = 0; i<quaternion.GetLength(0); i++){
            Quaternion q = new Quaternion(quaternion[i,0], quaternion[i,1], quaternion[i,2], quaternion[i,3]);
            Vector3 eulerAngles = q.eulerAngles;
            result[i,0] = eulerAngles.x.ToString("F0");
            result[i,1] = eulerAngles.y.ToString("F0");
            result[i,2] = eulerAngles.z.ToString("F0");
        }
        return result;
    }

    // void LoadAnimationClipFromJson(string filePath){
    //     // string filePath = Application.dataPath + "/YourAnimationClip3.json";
    //     if (File.Exists(filePath))
    //     {
    //         string dataAsJson = File.ReadAllText(filePath);
    //         AnimationData animationData = JsonUtility.FromJson<AnimationData>(dataAsJson);
    //         AnimationClip clip = CreateAnimationClip(animationData);
    //         // Apply the animation clip as needed...
            
    //     }
    //     else
    //     {
    //         Debug.LogError("Cannot find file at " + filePath);
    //     }
    // }

    List<AnimationClip> LoadAllFBXAnimations(){
        // Specify the folder path relative to the Assets directory
        string folderPath = "Assets/AnimDataset";

        // Get all FBX assets in the specified folder and its subfolders
        string[] guids = AssetDatabase.FindAssets("t:Model", new[] { folderPath });
        List<AnimationClip> animations = new List<AnimationClip>();

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            System.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            foreach (System.Object asset in assets)
            {
                if (asset is AnimationClip clip && !clip.legacy && clip.name != "__preview__mixamo.com" && !Regex.IsMatch(clip.name, @"\(\d+\)"))
                {
                    animations.Add(clip);
                    // Here you can access clip information, like clip.length, clip.name, etc.
                    Debug.Log($"Loaded animation clip: {clip.name} with length: {clip.length}");
                }
            }
        }
        return animations;
        //Turn them to json

    }

    List<List<float[]>> ClipToDegreesArray(AnimationClip clip){
        int bodyPartCount = 20;

        // Create the list to hold all body parts' animations
        List<List<float[]>> bodyPartsAnimations2 = new List<List<float[]>>();
        // Initialize each body part with its keyframes and quaternion values
        for (int i = 0; i < bodyPartCount; i++)
        {
            List<float[]> keyframesList = new List<float[]>();
            bodyPartsAnimations2.Add(keyframesList);
        }

        Dictionary<string, int> axisToIndex = new Dictionary<string, int>
        {
            { "x", 0 },
            { "y", 1 },
            { "z", 2 },
            { "w", 3 }
        };

        // Extract keyframe data from the animation curve
        foreach (EditorCurveBinding curveBinding in AnimationUtility.GetCurveBindings(clip)){
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, curveBinding);
            if(propertiesNames.Contains(curveBinding.path)){
                int bodyIndex = propertiesNames.IndexOf(curveBinding.path);
                if(bodyPartsAnimations2[bodyIndex].Count == 0){
                    for (int i = 0; i < curve.keys.Length; i++){
                        float[] quaternion = new float[4];
                        quaternion[axisToIndex[curveBinding.propertyName.Split(".")[1]]] = curve.keys[i].value;
                        bodyPartsAnimations2[bodyIndex].Add(quaternion);
                    }
                }else{
                    for (int i = 0; i < curve.keys.Length; i++){
                        bodyPartsAnimations2[bodyIndex][i][axisToIndex[curveBinding.propertyName.Split(".")[1]]] = curve.keys[i].value;
                    }
                }
            }
        }
        List<List<float[]>> OutRotations = new List<List<float[]>>();
        foreach(List<float[]> keyframeList in bodyPartsAnimations2){
            if(keyframeList.Count != 0){
                List<float[]> OutkeyframeList = new List<float[]>();
                foreach(float[] keyframe in keyframeList){
                    Quaternion q = new Quaternion(keyframe[0], keyframe[1], keyframe[2], keyframe[3]);
                    Vector3 eulerAngles = q.eulerAngles;
                    // float[] angles = new float[]{eulerAngles.x,eulerAngles.y,eulerAngles.z}; 
                    OutkeyframeList.Add(new float[]{eulerAngles.x,eulerAngles.y,eulerAngles.z});
                }
                OutRotations.Add(OutkeyframeList);
            }
        }

        return OutRotations;
    }
    Dictionary<string, List<float[]>> ClipToDegreesArray2(AnimationClip clip){
        Dictionary<string, int> axisToIndex = new Dictionary<string, int>
        {
            { "x", 0 },
            { "y", 1 },
            { "z", 2 },
            { "w", 3 }
        };

        Dictionary<string, List<float[]>> holder = new Dictionary<string, List<float[]>>();
        
        foreach (EditorCurveBinding curveBinding in AnimationUtility.GetCurveBindings(clip)){
            if(propertiesNames.Contains(curveBinding.path)){ 
                AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, curveBinding);
                if(!holder.ContainsKey(curveBinding.path)){
                    holder[curveBinding.path] = new List<float[]>();
                    for(int i = 0; i<curve.keys.Length; i++){
                        float[] vector = new float[]{0,0,0,0};
                        holder[curveBinding.path].Add(vector);
                    }
                }
                switch(axisToIndex[curveBinding.propertyName.Split(".")[1]]){
                    case 0:
                        for(int i = 0; i<curve.keys.Length; i++){
                            holder[curveBinding.path][i][0] = curve.keys[i].value;
                        }
                        break;
                    case 1:
                        for(int i = 0; i<curve.keys.Length; i++){
                            holder[curveBinding.path][i][1] = curve.keys[i].value;
                        }
                        break;
                    case 2:
                        for(int i = 0; i<curve.keys.Length; i++){
                            holder[curveBinding.path][i][2] = curve.keys[i].value;
                        }
                        break;
                    case 3:
                        for(int i = 0; i<curve.keys.Length; i++){
                            holder[curveBinding.path][i][3] = curve.keys[i].value;
                        }
                        break;
                    default:
                        break;
                }
                
            }
        }

        Dictionary<string, List<float[]>> OutRotations = new Dictionary<string, List<float[]>>();

        foreach(KeyValuePair<string,List<float[]>> kvp in holder){
            OutRotations[kvp.Key] = new List<float[]>();
            foreach(float[] quaternion in kvp.Value){
                Quaternion q = new Quaternion(quaternion[0], quaternion[1], quaternion[2], quaternion[3]);
                Vector3 eulerAngles = q.eulerAngles;
                Debug.Log(kvp.Key + " || " +eulerAngles + "|Quaternion| " + q);
                OutRotations[kvp.Key].Add(new float[]{eulerAngles.x,eulerAngles.y,eulerAngles.z});
            }
        }
        
        return OutRotations;
    }
    Dictionary<string, List<float[]>> ClipToDegreesArray3(AnimationClip clip){
        //get correct rotations from running the animation and copying the euler representaions
        //I can only get the rotations from a given game object, so i would need to iterate thorough the body parts that are being animated
        //I need to know which body parts are being animated
        //I also need the times for each body part animated

        //Get from the curves each path and save it to a dictionary of <path,times list>
        Dictionary<string, List<float>> pathTimes = new Dictionary<string, List<float>>();
        
        foreach (EditorCurveBinding curveBinding in AnimationUtility.GetCurveBindings(clip)){
            if(propertiesNames.Contains(curveBinding.path) && !pathTimes.ContainsKey(curveBinding.path)){ 
                AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, curveBinding);
                pathTimes[curveBinding.path] = new List<float>();
                for(int i = 0; i<curve.keys.Length; i++){
                    pathTimes[curveBinding.path].Add(curve.keys[i].time);
                }
            }
        }
        //Get a reference to the prent obj and find a way to get a reference to the correct body part from the path
        //Iterate throgh the times list for each path and get a dictionary of <path, vector3 rotations list> // I'll use float[] instead of vector 3 for compatibility
        Dictionary<string, List<float[]>> holder = new Dictionary<string, List<float[]>>();
        Animator animator = RigRoot.GetComponent<Animator>();
        EditorUtility.SetDirty(animator);
        AnimationMode.StartAnimationMode();
        foreach(KeyValuePair<string, List<float>> kvp in pathTimes){
            holder[kvp.Key] = new List<float[]>();
            Transform bodyPartTransform = RigRoot.Find(kvp.Key);
            if (bodyPartTransform == null){
                Debug.LogWarning($"Body part {kvp.Key} not found in the hierarchy.");
                continue;
            }
            foreach(float time in kvp.Value){
                // Set the animator to the correct time
                if (!AnimationMode.InAnimationMode()){
                    AnimationMode.StartAnimationMode();
                }
                animator.Play(clip.name, 0, 0.5f);
                animator.Update(0); // Update the animator to apply the time's pose
                EditorApplication.update += OnEditorUpdate;

                // if (!EditorApplication.isPlaying){
                //     EditorApplication.QueuePlayerLoopUpdate();
                //     UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                // }

                // Get the rotation in Euler angles and add it to the list
                // Vector3 eulerAngles = bodyPartTransform.eulerAngles;
                Vector3 eulerAngles = bodyPartTransform.localEulerAngles;
                float[] vector = new float[]{eulerAngles.x, eulerAngles.y, eulerAngles.z};
                holder[kvp.Key].Add(vector);
                Debug.Log(kvp.Key + " || " + eulerAngles + "||" + time);
            }
        }
        AnimationMode.StopAnimationMode();
        //return the dictionary
        return holder;
    }

    void OnEditorUpdate(){
        if (!AnimationMode.InAnimationMode())
        {
            // If animation mode is stopped externally, stop updating
            EditorApplication.update -= OnEditorUpdate;
            return;
        }

        // This will force the animator to update and render the frame in the Scene view
        if (true) //originally used (animator && clip)
        {
            RigRoot.GetComponent<Animator>().Update(Time.deltaTime);
            SceneView.RepaintAll();
        }
    }
    float CalculateAnimationScore(AnimationClip clip){
        string RangesFile = File.ReadAllText(Application.dataPath + "/" + "BodyMovementRanges.json");
        List<List<float[]>> rotations = ClipToDegreesArray(clip);
        Ranges ranges = JsonConvert.DeserializeObject<Ranges>(RangesFile);
        Debug.Log("Rotations: " + rotations);
        float totalScore = 0;

        for (int bodyPartIndex = 0; bodyPartIndex < rotations.Count; bodyPartIndex++)
        {
            for (int axisIndex = 0; axisIndex < 3; axisIndex++)
            {
                float rangeMin = ranges.BodyMovementRanges[bodyPartIndex][axisIndex][0];
                float rangeMax = ranges.BodyMovementRanges[bodyPartIndex][axisIndex][1];
                float permittedRange = Mathf.Abs(rangeMax - rangeMin);

                float maxError = (360 - permittedRange) / 2;
                float tierDistance = maxError / 3;
                float axisScores = 0;
                bool allWithinRange = true;

                for (int keyframeIndex = 0; keyframeIndex < rotations[bodyPartIndex].Count; keyframeIndex++)
                {
                    float keyframeValue = rotations[bodyPartIndex][keyframeIndex][axisIndex] % 360;
                    if (keyframeValue > 180)  // Normalize between -180 and 180
                    {
                        keyframeValue -= 360;
                    }

                    if (keyframeValue < rangeMin || keyframeValue > rangeMax)
                    {
                        allWithinRange = false;
                        float distanceToMin = Math.Abs(keyframeValue - rangeMin);
                        float distanceToMax = Math.Abs(keyframeValue - rangeMax);
                        float distance = Math.Min(distanceToMin, distanceToMax);

                        if (distance > 360){
                            distance -= 360;
                        }
                        float score = Math.Abs((distance/tierDistance)-3);
                        
                        if(score < axisScores){
                            axisScores = score;
                        }
                    }
                }

                if (allWithinRange){
                    totalScore += 3;
                }else{
                    totalScore += axisScores;
                }
            }
        }
        float finalScore = (totalScore / (rotations.Count*9))*100;  // divide by the total possible score obtained from the amount of body parts and turn to percentage
        return finalScore;
    }
    float CalculateAnimationScore2(AnimationClip clip){
        string RangesFile = File.ReadAllText(Application.dataPath + "/" + "BodyMovementRangesNormalized.json");
        string RangesNotNormFile = File.ReadAllText(Application.dataPath + "/" + "BodyMovementRanges.json");
        Dictionary<string, List<float[]>> rotations = ClipToDegreesArray2(clip);
        Ranges ranges = JsonConvert.DeserializeObject<Ranges>(RangesFile);
        Ranges rangesNotNorm = JsonConvert.DeserializeObject<Ranges>(RangesNotNormFile);
        if(rotations.Count == 0){
            Debug.Log("WARNING Empty rotations dictionary: " + rotations);
        }
        float totalScore = 0;
        float axisScores = 0;
        foreach(KeyValuePair<string, List<float[]>> bodyRotations in rotations){
            int bodyPartIndex = propertiesNames.IndexOf(bodyRotations.Key);
            for (int axisIndex = 0; axisIndex < 3; axisIndex++){
                float rangeMin = ranges.BodyMovementRanges[bodyPartIndex][axisIndex][0];
                float rangeMax = ranges.BodyMovementRanges[bodyPartIndex][axisIndex][1];
                float rangeMinNotNorm = rangesNotNorm.BodyMovementRanges[bodyPartIndex][axisIndex][0];
                float rangeMaxNotNorm = rangesNotNorm.BodyMovementRanges[bodyPartIndex][axisIndex][1];

                float permittedRange = Mathf.Abs(rangeMaxNotNorm - rangeMinNotNorm);
                float maxError = (360 - permittedRange) / 2;
                float tierDistance = maxError / 3;
                bool inRange = false;
                bool allWithinRange = true;
                float maxDistance = 0;
                axisScores = 0;

                foreach(float[] vector in bodyRotations.Value){
                    float axisValue = vector[axisIndex];
                    inRange = false;
                    if(rangeMin < rangeMax){
                        if(axisValue >= rangeMin && axisValue <= rangeMax){
                            inRange = true;
                        }
                    }
                    if(rangeMin > rangeMax){
                        if(axisValue <= rangeMax || axisValue >= rangeMin){
                            inRange = true;
                        }
                    }
                    if (!inRange){
                        allWithinRange = false;
                        float distanceToMin;
                        float distanceToMax;
                        if(true){
                            float distanceToMinDirect = Math.Abs(axisValue - rangeMin);
                            float distanceToMinWrap = 360 - distanceToMinDirect;
                            distanceToMin = Math.Min(distanceToMinDirect, distanceToMinWrap);

                            float distanceToMaxDirect = Math.Abs(axisValue - rangeMax);
                            float distanceToMaxWrap = 360 - distanceToMaxDirect;
                            distanceToMax = Math.Min(distanceToMaxDirect, distanceToMaxWrap);
                        }else{
                            distanceToMin = Math.Abs(axisValue - rangeMin);
                            distanceToMax = Math.Abs(axisValue - rangeMax);
                        }
                        
                        float distance = Math.Min(distanceToMin, distanceToMax);

                        // float score = distance/tierDistance;                                      //switch to score based on 3 tiers of distances from correct range
                        // float score = (1- NormalDistErrorPenalty(distance, permittedRange/2f))*3;    //swith to score based on normal dist with mean 0 and std based on movement range
                        float score = distance/permittedRange;                                      //switch to score based on distance proportion to total range (must change return to totalScore instead of finalScore)
                        if(score > 3f){
                            Debug.Log("SCORING > 3!!!! distance: " + distance + " TierDistance: " + tierDistance);
                            Debug.Log("AxisValue: " + axisValue + " rangeMin: " + rangeMin + " rangeMax: " + rangeMax);
                            Debug.Log("distanceToMin: " + distanceToMin + " distanceToMax: " + distanceToMax);
                        }
                        if(score > axisScores){
                            axisScores = score;
                            maxDistance = distance;
                        }
                    }
                }
                totalScore += axisScores;
                Debug.Log(BodyPartsToMixamo.GetKey(bodyRotations.Key) + " || Axis: " + axisIndex + " deducted: " + (axisScores) + " points" + " || rangeMin: " + rangeMin + " rangeMax: " + rangeMax + " || was in range: " + allWithinRange + " || distance: " + maxDistance);    //Turn on to track the points taken to calculate a given score, being able to observe how much was deducted per body part

            }
        }
        float maxPoints = rotations.Count * 9.0f;
        Debug.Log("Body parts count: " + rotations.Count);
        float finalScore = ((maxPoints - totalScore) / maxPoints) * 100;  // divide by the total possible score obtained from the amount of body parts and turn to percentage
        Debug.Log("total score: " + totalScore);
        // return finalScore;
        return totalScore;
    }
    float GetScoreFromJsonDataset(int promptNumber, Options version){
        float score = GetScoreFromJson(versionPaths[version] + promptNumber + ".json", version);
        return score;
    }
    float GetScoreFromJson(string path, Options version){
        Debug.Log("Path: " + path);
        Debug.Log("Version: " + version);
        string dataAsJson = File.ReadAllText(Application.dataPath + "/" + path);
        AnimationClip clip = new AnimationClip();
        float score = 0;
        switch(version){
            case Options.Full:
                AnimationDataJSON animationData = JsonConvert.DeserializeObject<AnimationDataJSON>(dataAsJson);
                Debug.Log(JsonConvert.SerializeObject(animationData));
                clip = CreateAnimationClip(animationData);
                score = CalculateAnimationScore2(clip);
                Debug.Log("Score for: " + clip.name + " is = " + score);
                break;
            case Options.Compressed:
                AnimationDataCompJSON animationDataComp = JsonConvert.DeserializeObject<AnimationDataCompJSON>(dataAsJson);
                Debug.Log(JsonConvert.SerializeObject(animationDataComp));
                clip = CreateAnimationClipComp(animationDataComp);
                score = CalculateAnimationScore2(clip);
                Debug.Log("Score for: " + clip.name + " is = " + score);
                break;
            case Options.Tokens:
                AnimationDataComp2048JSON animationData2048 = JsonConvert.DeserializeObject<AnimationDataComp2048JSON>(dataAsJson);
                Debug.Log(dataAsJson);
                Debug.Log(JsonConvert.SerializeObject(animationData2048));
                clip = CreateAnimationClipComp2048(animationData2048);
                score = CalculateAnimationScore2(clip);
                Debug.Log("Score for: " + clip.name + " is = " + score);
                break;
            case Options.FullDEG:
                AnimationDataJSON animationDataDEG = JsonConvert.DeserializeObject<AnimationDataJSON>(dataAsJson);
                Debug.Log(JsonConvert.SerializeObject(animationDataDEG));
                clip = CreateAnimationClipDEG(animationDataDEG);
                score = CalculateAnimationScore2(clip);
                Debug.Log("Score for: " + clip.name + " is = " + score);
                break;
            case Options.CompressedDEG:
                AnimationDataCompJSON animationDataCompDEG = JsonConvert.DeserializeObject<AnimationDataCompJSON>(dataAsJson);
                Debug.Log(JsonConvert.SerializeObject(animationDataCompDEG));
                clip = CreateAnimationClipCompDEG(animationDataCompDEG);
                score = CalculateAnimationScore2(clip);
                Debug.Log("Score for: " + clip.name + " is = " + score);
                break;
            case Options.TokensDEG:
                AnimationDataComp2048JSON animationData2048DEG = JsonConvert.DeserializeObject<AnimationDataComp2048JSON>(dataAsJson);
                Debug.Log(JsonConvert.SerializeObject(animationData2048DEG));
                clip = CreateAnimationClipComp2048DEG(animationData2048DEG);
                score = CalculateAnimationScore2(clip);
                Debug.Log("Score for: " + clip.name + " is = " + score);
                break;
            default:
                break;
        }
        return score;
    }
    //Normal Distribution
    public double NormalPDF(double x, double mean, double stdDev){
        double exponent = -Math.Pow(x - mean, 2) / (2 * Math.Pow(stdDev, 2));
        return (1 / (stdDev * Math.Sqrt(2 * Math.PI))) * Math.Exp(exponent);
    }
    public float NormalDistErrorPenalty(double distance, double stdDev){    //calculate penalty based on a normal distribution where 1 std is half of the permitted range, for this I divide the value at a given angle by the value at 0 distance to get a percentage
        return (float) (NormalPDF(distance, 0f, stdDev) / NormalPDF(0f, 0f, stdDev));
    }
    //Functions to get Clips from Json
    AnimationClip CreateAnimationClip(AnimationDataJSON data){    
        AnimationClip clip = new AnimationClip();
        clip.name = data.animationName;
        // Debug.Log("Animation overridden with clip: " + clip.name);
        foreach (KeyframeDataLoadJSON keyframeData in data.keyframes)
        {
            AnimationCurve curve = new AnimationCurve();
            foreach (CurvePoint point in keyframeData.curve)
            {
                curve.AddKey(new Keyframe(point.time, point.value));
                // Debug.Log(point.value);
            }
            clip.SetCurve(BodyPartsToMixamo.GetValue(keyframeData.path), typeof(Transform), keyframeData.propertyName, curve);
        }

        return clip;
    }
    AnimationClip CreateAnimationClipComp(AnimationDataCompJSON data){   
        AnimationClip clip = new AnimationClip();
        clip.name = data.animationName;
        Debug.Log("Animation overridden with clip: " + clip.name);
        foreach (KeyframeDataParallelJSON keyframeData in data.keyframes){
            AnimationCurve curvex = new AnimationCurve();
            AnimationCurve curvey = new AnimationCurve();
            AnimationCurve curvez = new AnimationCurve();
            AnimationCurve curvew = new AnimationCurve();
            //replace with vector3 and quaternion
            if(keyframeData.propertyName.Equals("m_LocalRotation")){
                for(int i = 0; i<keyframeData.time.Length; i++){
                    // Debug.Log(keyframeData.path + ": time[" +i+"]");
                    curvex.AddKey(new Keyframe(keyframeData.time[i], keyframeData.value[i][0]));
                    curvey.AddKey(new Keyframe(keyframeData.time[i], keyframeData.value[i][1]));
                    curvez.AddKey(new Keyframe(keyframeData.time[i], keyframeData.value[i][2]));
                    curvew.AddKey(new Keyframe(keyframeData.time[i], keyframeData.value[i][3]));
                }
                // Debug.Log(keyframeData);
                clip.SetCurve(BodyPartsToMixamo.GetValue(keyframeData.path), typeof(Transform), keyframeData.propertyName + ".x", curvex);
                clip.SetCurve(BodyPartsToMixamo.GetValue(keyframeData.path), typeof(Transform), keyframeData.propertyName + ".y", curvey);
                clip.SetCurve(BodyPartsToMixamo.GetValue(keyframeData.path), typeof(Transform), keyframeData.propertyName + ".z", curvez);
                clip.SetCurve(BodyPartsToMixamo.GetValue(keyframeData.path), typeof(Transform), keyframeData.propertyName + ".w", curvew);
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
    AnimationClip CreateAnimationClipComp2048(AnimationDataComp2048JSON data){   
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
        // float sampleInterval = 1/((float)data.Hips.Length);
        float sampleInterval = 0;
        foreach(float[][] property in properties){
            if(property != null){
                sampleInterval = 1/(float)property.Length;
                break;
            }
        }
        // for(int i = 0; i<data.HipsP.Length;i++){
        //     curvex.AddKey(new Keyframe((i*sampleInterval), data.HipsP[i][0]));
        //     curvey.AddKey(new Keyframe((i*sampleInterval), data.HipsP[i][1]));
        //     curvez.AddKey(new Keyframe((i*sampleInterval), data.HipsP[i][2]));
        // }

        // clip.SetCurve("mixamorig:Hips", typeof(Transform), "m_LocalPosition.x", curvex);
        // clip.SetCurve("mixamorig:Hips", typeof(Transform), "m_LocalPosition.y", curvey);
        // clip.SetCurve("mixamorig:Hips", typeof(Transform), "m_LocalPosition.z", curvez);
        int counter = 0;
        foreach(float[][] property in properties){
            if(property != null){
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
        }

        return clip;
    }
    AnimationClip CreateAnimationClipDEG(AnimationDataJSON data){    
        AnimationClip clip = new AnimationClip();
        clip.name = data.animationName;
        //join back vectors
        Dictionary<string, KeyframeDataLoad> qData = new Dictionary<string, KeyframeDataLoad>();
        Dictionary<string, int> axisToIndex = new Dictionary<string, int>{
            { "x", 0 },
            { "y", 1 },
            { "z", 2 },
            { "w", 3 }
        };
        foreach (KeyframeDataLoadJSON keyframeData in data.keyframes){
            if(keyframeData.propertyName.Split(".")[0].Equals("m_LocalRotation") && !qData.ContainsKey(keyframeData.path)){
                qData[keyframeData.path] = new KeyframeDataLoad();
                qData[keyframeData.path].path = keyframeData.path;
                qData[keyframeData.path].propertyName = "m_LocalRotation";
                foreach(CurvePoint key in keyframeData.curve){
                    qData[keyframeData.path].time.Add(key.time);
                    qData[keyframeData.path].x.Add(0);
                    qData[keyframeData.path].y.Add(0);
                    qData[keyframeData.path].z.Add(0);
                }
            }
            switch (axisToIndex[keyframeData.propertyName.Split(".")[1]]){
                case 0:
                    qData[keyframeData.path].x.Clear();
                    foreach(CurvePoint key in keyframeData.curve){
                        qData[keyframeData.path].x.Add(key.value);
                    }
                    break;
                case 1:
                    qData[keyframeData.path].y.Clear();
                    foreach(CurvePoint key in keyframeData.curve){
                        qData[keyframeData.path].y.Add(key.value);
                    }
                    break;
                case 2:
                    qData[keyframeData.path].z.Clear();
                    foreach(CurvePoint key in keyframeData.curve){
                        qData[keyframeData.path].z.Add(key.value);
                    }
                    break;
                default:
                    break;
            }
        }
        foreach(KeyValuePair<string, KeyframeDataLoad> kvp in qData){
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
    AnimationClip CreateAnimationClipCompDEG(AnimationDataCompJSON data){   
        AnimationClip clip = new AnimationClip();
        clip.name = data.animationName;
        Debug.Log("Animation overridden with clip: " + clip.name);
        foreach (KeyframeDataParallelJSON keyframeData in data.keyframes){
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
    AnimationClip CreateAnimationClipComp2048DEG(AnimationDataComp2048JSON data){   
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
        // for(int i = 0; i<data.HipsP.Length;i++){
        //     curvex.AddKey(new Keyframe((i*sampleInterval), data.HipsP[i][0]));
        //     curvey.AddKey(new Keyframe((i*sampleInterval), data.HipsP[i][1]));
        //     curvez.AddKey(new Keyframe((i*sampleInterval), data.HipsP[i][2]));
        // }

        // clip.SetCurve("mixamorig:Hips", typeof(Transform), "m_LocalPosition.x", curvex);
        // clip.SetCurve("mixamorig:Hips", typeof(Transform), "m_LocalPosition.y", curvey);
        // clip.SetCurve("mixamorig:Hips", typeof(Transform), "m_LocalPosition.z", curvez);
        int counter = 0;
        foreach(float[][] property in properties){
            if(property != null){
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
public class KeyframeDataLoadJSON
{
    public string path;
    public string propertyName;
    public CurvePoint[] curve;
}
[Serializable]
public class AnimationDataJSON
{
    public string animationName;
    public KeyframeDataLoadJSON[] keyframes;
}
[Serializable]
public class CurvePoint
{
    public float time;
    public float value;
}

[System.Serializable]
public class KeyframeDataParallelJSON
{
    public string path;
    public string propertyName;
    public float[] time;
    public float[][] value;
}
[Serializable]
public class AnimationDataCompJSON
{
    public string animationName;
    public KeyframeDataParallelJSON[] keyframes;
}

[Serializable]
public class AnimationDataComp2048JSON
{
    public string animationName;
    // public float[][] HipsP; //depreca public float[][] Hips;
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

[Serializable]
public class Ranges{
    public List<List<List<float>>> BodyMovementRanges;
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

