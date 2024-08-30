using Firebase.Extensions;
using Firebase.RemoteConfig;
using UnityEngine;
using System.Collections.Generic;
using Firebase;
using System;

namespace com.playbux.firebaseservice
{
    public class FirebaseRemoteConfigManager
    {

        private static FirebaseRemoteConfigManager instance;



        private FirebaseRemoteConfig firebaseRemoteConfig;
        private string endPoint;

        public string EndPoint { get => endPoint; set => endPoint = value; }

        public static FirebaseRemoteConfigManager GetInstance()
        {

            if (instance == null)
            {
                instance = new FirebaseRemoteConfigManager();
            }

            return instance;

        }


        private FirebaseRemoteConfigManager()
        {
            firebaseRemoteConfig = FirebaseRemoteConfig.DefaultInstance;
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                if (task.Result == DependencyStatus.Available)
                {
                    FetchRemoteConfig();
                }
                else
                {
                    Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);
                }
            });
        }

        void FetchRemoteConfig()
        {


            //firebaseRemoteConfig.SetDefaultsAsync(defaults);

            firebaseRemoteConfig.FetchAsync(TimeSpan.Zero).ContinueWithOnMainThread(fetchTask =>
            {
                if (fetchTask.IsCompleted)
                {
                    firebaseRemoteConfig.ActivateAsync().ContinueWithOnMainThread(activateTask =>
                    {
                        if (activateTask.IsCompleted)
                        {
                            Debug.Log("Remote config values updated!");
                            string endPoint = "localhost";
                            string endPointPath = "localhost";

#if UNITY_EDITOR
                            endPoint = firebaseRemoteConfig.GetValue("local_endpoint").StringValue;
                            endPointPath = "localPath";
#endif
#if !PRODUCTION && !UNITY_EDITOR
                            endPoint = firebaseRemoteConfig.GetValue("staging_endpoint").StringValue;
                            endPointPath = "staging";
#endif
#if PRODUCTION && !UNITY_EDITOR
                            endPoint = firebaseRemoteConfig.GetValue("production_endpoint").StringValue;
                            endPointPath = "production";
#endif
#if PRODUCTION && UNITY_EDITOR
                            endPoint = firebaseRemoteConfig.GetValue("production_endpoint").StringValue;
                            endPointPath = "production";
#endif
                            Debug.Log("endpoint: " + endPoint + " : " + endPointPath);
                            EndPoint = endPoint;

                        }
                    });
                }
                else
                {
                    Debug.LogError("Fetch failed: " + fetchTask.Exception);
                }
            });
        }
    }
}