﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Experimental.UI;
using Microsoft.MixedReality.Toolkit.Experimental.Utilities;
using Microsoft.MixedReality.Toolkit.UI;
using NUnit.Framework;
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Microsoft.MixedReality.Toolkit.Tests
{
    public class MigrationToolTest
    {
        private readonly MigrationTool migrationTool = new MigrationTool();

        /// <summary>
        /// Checks if MigrationTool can process migration on a GameObject containing a deprecated ManipulationHandler component.
        /// </summary>
        [Test]
        public void GameObjectCanBeMigrated()
        {
            Type oldType = typeof(ManipulationHandler);
            Type newType = typeof(ObjectManipulator);
            Type migrationHandlerType = typeof(ObjectManipulatorMigrationHandler);

            GameObject gameObject = SetUpGameObjectWithComponentOfType(oldType);

            migrationTool.TryAddObjectForMigration(gameObject);
            migrationTool.MigrateSelection(migrationHandlerType, false);

            Assert.IsNull(gameObject.GetComponent(oldType), $"Migrated Component of type {oldType.Name} could not be removed");
            Assert.IsNotNull(gameObject.GetComponent(newType), $"Migrated Component of type {newType.Name} could not be added");

            GameObject.DestroyImmediate(gameObject);
        }

        /// <summary>
        /// Checks if MigrationTool can process migration on a Prefab containing a deprecated ManipulationHandler component.
        /// </summary>
        [Test]
        public void PrefabCanBeMigrated()
        {
            Type oldType = typeof(ManipulationHandler);
            Type newType = typeof(ObjectManipulator);
            Type migrationHandlerType = typeof(ObjectManipulatorMigrationHandler);
            String prefabPath = "Assets/_migration.prefab";

            GameObject gameObject = SetUpGameObjectWithComponentOfType(oldType);
            PrefabUtility.SaveAsPrefabAsset(gameObject, prefabPath);

            migrationTool.TryAddObjectForMigration(AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)));
            migrationTool.MigrateSelection(migrationHandlerType, false);

            GameObject prefabGameObject = PrefabUtility.LoadPrefabContents(prefabPath);

            Assert.IsNull(prefabGameObject.GetComponent(oldType), $"Migrated Component of type {oldType.Name} could not be removed");
            Assert.IsNotNull(prefabGameObject.GetComponent(newType), $"Migrated Component of type {newType.Name} could not be added");

            PrefabUtility.UnloadPrefabContents(prefabGameObject);
            AssetDatabase.DeleteAsset(prefabPath);
            GameObject.DestroyImmediate(gameObject);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Checks if MigrationTool can process migration on a Scene root GameObject that contains a deprecated ManipulationHandler component.
        /// </summary>
        [Test]
        public void SceneCanBeMigrated()
        {
            Type oldType = typeof(ManipulationHandler);
            Type newType = typeof(ObjectManipulator);
            Type migrationHandlerType = typeof(ObjectManipulatorMigrationHandler);
            String scenePath = "Assets/_migration.unity";

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject gameObject = SetUpGameObjectWithComponentOfType(oldType);
            EditorSceneManager.SaveScene(scene, scenePath);

            migrationTool.TryAddObjectForMigration(AssetDatabase.LoadAssetAtPath(scenePath, typeof(SceneAsset)));
            migrationTool.MigrateSelection(migrationHandlerType, false);

            var openScene = EditorSceneManager.OpenScene(scenePath);
            foreach (var sceneGameObject in openScene.GetRootGameObjects())
            {
                Assert.IsNull(sceneGameObject.GetComponent(oldType), $"Migrated Component of type {oldType.Name} could not be removed");
                Assert.IsNotNull(sceneGameObject.GetComponent(newType), $"Migrated Component of type {newType.Name} could not be added");

                GameObject.DestroyImmediate(sceneGameObject);
            }

            AssetDatabase.DeleteAsset(scenePath);
            GameObject.DestroyImmediate(gameObject);
            AssetDatabase.Refresh();
        }

        private static GameObject SetUpGameObjectWithComponentOfType(Type type)
        {
            GameObject go = new GameObject();
            if (typeof(Component).IsAssignableFrom(type))
            {
                go.AddComponent(type);
            }

            Assert.IsNotNull(go.GetComponent(type), $"Component of type {type.Name} could not be added to GameObject");

            return go;
        }
    }
}

