using System;
using System.Threading.Tasks;
using QFSW.QC;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Network.Services
{
    [DisallowMultipleComponent]
    public class NetworkAuthenticationManager : MonoBehaviour
    {

        [Command("auth.signin")]
        public static async Task SignInAnonymously(string profileName)
        {
            try
            {

                SwitchProfileIfNecessary(profileName);

                await InitialiseUnityServices(profileName);

                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                Debug.Log($"Profile: {profileName}");
                Debug.Log($"PlayerId: {AuthenticationService.Instance.PlayerId}");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static void SwitchProfileIfNecessary(string profileName)
        {
            try
            {
                if (UnityServices.State == ServicesInitializationState.Initialized)
                {
                    if (AuthenticationService.Instance.IsSignedIn)
                    {
                        Debug.Log("Signing out of current profile.");
                        AuthenticationService.Instance.SignOut();
                    }

                    Debug.Log($"Switching profile to {profileName}");
                    AuthenticationService.Instance.SwitchProfile(profileName);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [Command("auth.signout")]
        private static void SignOut()
        {
            try
            {
                if (UnityServices.State == ServicesInitializationState.Initialized)
                {
                    if (AuthenticationService.Instance.IsSignedIn)
                    {
                        Debug.Log("Signing out of current profile.");
                        AuthenticationService.Instance.SignOut();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static async Task InitialiseUnityServices(string profileName)
        {
            try
            {
                var unityAuthenticationInitOptions = new InitializationOptions();
                unityAuthenticationInitOptions.SetProfile(profileName);
                await UnityServices.InitializeAsync(unityAuthenticationInitOptions);
                
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}