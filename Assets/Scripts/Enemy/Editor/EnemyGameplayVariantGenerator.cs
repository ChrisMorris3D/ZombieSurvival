using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CrispyCube.EditorTools
{
    public static class EnemyGameplayVariantGenerator
    {
        const string EnemyListPath = "Assets/Data/EnemyPrefabList.asset";
        const string GameplayTemplatePath = "Assets/Prefab/Enemy/Zombie_Enemy.prefab";
        const string VariantFolderPath = "Assets/Prefab/Enemy/Variants";

        static bool isGenerating;

        [InitializeOnLoadMethod]
        static void ScheduleGeneration()
        {
            EditorApplication.delayCall += GenerateIfNeeded;
        }

        [MenuItem("Tools/Zombie Survival/Generate Enemy Gameplay Variants")]
        public static void GenerateVariants()
        {
            Generate(false);
        }

        static void GenerateIfNeeded()
        {
            Generate(true);
        }

        static void Generate(bool onlyIfNeeded)
        {
            if (isGenerating || EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += GenerateIfNeeded;
                return;
            }

            EnemyPrefabList enemyList = AssetDatabase.LoadAssetAtPath<EnemyPrefabList>(EnemyListPath);
            GameObject gameplayTemplate = AssetDatabase.LoadAssetAtPath<GameObject>(GameplayTemplatePath);
            if (enemyList == null || gameplayTemplate == null)
            {
                Debug.LogError("Enemy gameplay variants could not be generated because the prefab list or gameplay template is missing.");
                return;
            }

            SerializedObject listObject = new SerializedObject(enemyList);
            SerializedProperty prefabProperty = listObject.FindProperty("enemyPrefabs");
            List<GameObject> sourcePrefabs = ReadPrefabs(prefabProperty);

            bool needsGeneration = sourcePrefabs.Exists(prefab =>
                prefab != null &&
                (!AssetDatabase.GetAssetPath(prefab).StartsWith(VariantFolderPath + "/", StringComparison.Ordinal) ||
                 !SpawnEffectMatchesTemplate(prefab, gameplayTemplate)));

            if (onlyIfNeeded && !needsGeneration)
            {
                return;
            }

            EnsureVariantFolderExists();
            isGenerating = true;

            GameObject templateContents = null;
            Scene previewScene = default;
            try
            {
                templateContents = PrefabUtility.LoadPrefabContents(GameplayTemplatePath);
                previewScene = EditorSceneManager.NewPreviewScene();

                List<GameObject> resultingPrefabs = new List<GameObject>(sourcePrefabs.Count);
                for (int i = 0; i < sourcePrefabs.Count; i++)
                {
                    GameObject sourcePrefab = sourcePrefabs[i];
                    if (sourcePrefab == null)
                    {
                        resultingPrefabs.Add(null);
                        continue;
                    }

                    string sourcePath = AssetDatabase.GetAssetPath(sourcePrefab);
                    if (sourcePath.StartsWith(VariantFolderPath + "/", StringComparison.Ordinal))
                    {
                        SyncSpawnEffect(templateContents, sourcePath);
                        resultingPrefabs.Add(sourcePrefab);
                        continue;
                    }

                    string variantPath = $"{VariantFolderPath}/{sourcePrefab.name}_Gameplay.prefab";
                    GameObject variant = CreateVariant(sourcePrefab, variantPath, templateContents, previewScene);
                    resultingPrefabs.Add(variant != null ? variant : sourcePrefab);
                }

                WritePrefabs(prefabProperty, resultingPrefabs);
                listObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(enemyList);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log($"Generated gameplay variants for {resultingPrefabs.Count} enemy prefabs.", enemyList);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                if (templateContents != null)
                {
                    PrefabUtility.UnloadPrefabContents(templateContents);
                }

                if (previewScene.IsValid())
                {
                    EditorSceneManager.ClosePreviewScene(previewScene);
                }

                isGenerating = false;
            }
        }

        static GameObject CreateVariant(GameObject sourcePrefab, string variantPath, GameObject template, Scene previewScene)
        {
            GameObject existingVariant = AssetDatabase.LoadAssetAtPath<GameObject>(variantPath);
            if (existingVariant != null)
            {
                SyncSpawnEffect(template, variantPath);
                return existingVariant;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(sourcePrefab, previewScene) as GameObject;
            if (instance == null)
            {
                Debug.LogError($"Could not instantiate enemy prefab '{sourcePrefab.name}'.", sourcePrefab);
                return null;
            }

            try
            {
                ConfigureGameplayComponents(instance, template);
                return PrefabUtility.SaveAsPrefabAsset(instance, variantPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        static void ConfigureGameplayComponents(GameObject target, GameObject template)
        {
            ZombieEnemy templateEnemy = template.GetComponent<ZombieEnemy>();
            AudioSource templateAudio = template.GetComponent<AudioSource>();
            EnemyHealthController templateHealth = template.GetComponent<EnemyHealthController>();
            EnemyAnimationStateController templateAnimation = template.GetComponent<EnemyAnimationStateController>();
            CapsuleCollider templateHurtbox = template.GetComponent<CapsuleCollider>();

            Animator targetAnimator = target.GetComponent<Animator>();
            if (targetAnimator == null)
            {
                targetAnimator = target.GetComponentInChildren<Animator>(true);
            }

            Animator templateAnimator = templateEnemy.anim;
            if (targetAnimator != null && templateAnimator != null)
            {
                targetAnimator.runtimeAnimatorController = templateAnimator.runtimeAnimatorController;
                targetAnimator.applyRootMotion = templateAnimator.applyRootMotion;
                targetAnimator.cullingMode = templateAnimator.cullingMode;
                targetAnimator.updateMode = templateAnimator.updateMode;
            }

            AudioSource targetAudio = target.AddComponent<AudioSource>();
            EditorUtility.CopySerialized(templateAudio, targetAudio);

            CapsuleCollider targetHurtbox = target.AddComponent<CapsuleCollider>();
            EditorUtility.CopySerialized(templateHurtbox, targetHurtbox);

            CapsuleCollider targetAttackCollider = CreateTriggerCollider(target.transform, templateEnemy.attackCollider);
            CapsuleCollider targetActivationCollider = CreateTriggerCollider(target.transform, templateEnemy.triggerCollider);

            ZombieEnemy targetEnemy = target.AddComponent<ZombieEnemy>();
            EditorUtility.CopySerialized(templateEnemy, targetEnemy);
            targetEnemy.anim = targetAnimator;
            targetEnemy.audioSource = targetAudio;
            targetEnemy.attackCollider = targetAttackCollider;
            targetEnemy.triggerCollider = targetActivationCollider;

            EnemyHealthController targetHealth = target.AddComponent<EnemyHealthController>();
            EditorUtility.CopySerialized(templateHealth, targetHealth);
            SetObjectReference(targetHealth, "enemy", targetEnemy);

            EnemyAnimationStateController targetAnimation = target.AddComponent<EnemyAnimationStateController>();
            EditorUtility.CopySerialized(templateAnimation, targetAnimation);
            SetObjectReference(targetAnimation, "enemy", targetEnemy);
            SetObjectReference(targetAnimation, "animator", targetAnimator);
        }

        static CapsuleCollider CreateTriggerCollider(Transform targetRoot, CapsuleCollider templateCollider)
        {
            GameObject triggerObject = new GameObject(templateCollider.gameObject.name);
            triggerObject.layer = templateCollider.gameObject.layer;
            triggerObject.tag = templateCollider.gameObject.tag;
            triggerObject.transform.SetParent(targetRoot, false);
            triggerObject.transform.localPosition = templateCollider.transform.localPosition;
            triggerObject.transform.localRotation = templateCollider.transform.localRotation;
            triggerObject.transform.localScale = templateCollider.transform.localScale;

            CapsuleCollider targetCollider = triggerObject.AddComponent<CapsuleCollider>();
            EditorUtility.CopySerialized(templateCollider, targetCollider);
            return targetCollider;
        }

        static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            SerializedObject serializedTarget = new SerializedObject(target);
            serializedTarget.FindProperty(propertyName).objectReferenceValue = value;
            serializedTarget.ApplyModifiedPropertiesWithoutUndo();
        }

        static bool SpawnEffectMatchesTemplate(GameObject variant, GameObject template)
        {
            ZombieEnemy variantEnemy = variant.GetComponent<ZombieEnemy>();
            ZombieEnemy templateEnemy = template.GetComponent<ZombieEnemy>();
            if (variantEnemy == null || templateEnemy == null)
            {
                return false;
            }

            SerializedObject variantObject = new SerializedObject(variantEnemy);
            SerializedObject templateObject = new SerializedObject(templateEnemy);
            return variantObject.FindProperty("spawnEffectPrefab").objectReferenceValue ==
                   templateObject.FindProperty("spawnEffectPrefab").objectReferenceValue &&
                   Mathf.Approximately(
                       variantObject.FindProperty("spawnEffectDuration").floatValue,
                       templateObject.FindProperty("spawnEffectDuration").floatValue);
        }

        static void SyncSpawnEffect(GameObject template, string variantPath)
        {
            GameObject contents = PrefabUtility.LoadPrefabContents(variantPath);
            try
            {
                ZombieEnemy variantEnemy = contents.GetComponent<ZombieEnemy>();
                ZombieEnemy templateEnemy = template.GetComponent<ZombieEnemy>();
                if (variantEnemy == null || templateEnemy == null)
                {
                    return;
                }

                SerializedObject variantObject = new SerializedObject(variantEnemy);
                SerializedObject templateObject = new SerializedObject(templateEnemy);
                variantObject.FindProperty("spawnEffectPrefab").objectReferenceValue =
                    templateObject.FindProperty("spawnEffectPrefab").objectReferenceValue;
                variantObject.FindProperty("spawnEffectDuration").floatValue =
                    templateObject.FindProperty("spawnEffectDuration").floatValue;
                variantObject.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(contents, variantPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        static List<GameObject> ReadPrefabs(SerializedProperty prefabProperty)
        {
            List<GameObject> prefabs = new List<GameObject>(prefabProperty.arraySize);
            for (int i = 0; i < prefabProperty.arraySize; i++)
            {
                prefabs.Add(prefabProperty.GetArrayElementAtIndex(i).objectReferenceValue as GameObject);
            }

            return prefabs;
        }

        static void WritePrefabs(SerializedProperty prefabProperty, List<GameObject> prefabs)
        {
            prefabProperty.arraySize = prefabs.Count;
            for (int i = 0; i < prefabs.Count; i++)
            {
                prefabProperty.GetArrayElementAtIndex(i).objectReferenceValue = prefabs[i];
            }
        }

        static void EnsureVariantFolderExists()
        {
            if (!AssetDatabase.IsValidFolder(VariantFolderPath))
            {
                AssetDatabase.CreateFolder("Assets/Prefab/Enemy", "Variants");
            }
        }
    }
}
