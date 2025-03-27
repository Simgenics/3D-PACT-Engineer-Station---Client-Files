using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using LMS.API;
using LMS.Models;

namespace LMS.UI
{
    
    public class Login : MonoBehaviour
    {
        public static Action<bool> OnAutoAuth = delegate {};

        public static readonly List<string> WorkflowRoles = new List<string>() { "Roles", "Facilities", "Companies", "Asset Bundles", "Scenarios" };

        public TMP_InputField edtUsername;
        public TMP_InputField edtPassword;

        public NotificationBanner notificationBanner;
        public NotificationSnackBar notificationSnackBar;
        public GameObject loadingObject;

        public UserDataLib userDataLib;

        public UICompanies uiCompanies;
        public RectTransform loginPanel;

        private void Start()
        {
            ClearInputFields();
            uiCompanies.gameObject.SetActive(false);

            // Store username on endEdit
            edtUsername.onEndEdit.AddListener(SetUsername);
            // Set username from previous login
            edtUsername.text = PlayerPrefs.GetString("username");
            // focus on edtUsername
            edtUsername.Select();

            AutoLogin();
        }

        public void ClearInput()
        {
            edtUsername.text = "";
            edtPassword.text = "";
            edtUsername.Select();
        }

        /// <summary>
        /// Try to log the user in using the stored authToken/refreshToken
        /// </summary>
        private void AutoLogin()
        {
            // TODO: Replace this with AuthAdapter.instance.RefreshToken when endpoint works
            // Slight workaround to check if our authToken still works
            loadingObject.SetActive(true);
            ProfileAdapter.instance.GetProfilePicture(result =>
            {
                loadingObject.SetActive(false);
                if (result != null)
                {
                    Debug.Log("Successfully got user profile picture, authToken still works");
                    loginPanel.gameObject.SetActive(false);
                    OnAutoAuth.Invoke(true);
                }
                else
                {
                    Debug.LogWarning("User could not fetch profile picture, they need to log in");
                    loginPanel.gameObject.SetActive(true);
                    OnAutoAuth.Invoke(false);
                    const string message = "Session Expired";
                    notificationSnackBar.displayNotificationSnackbar(SnackbarTemplates.Information, message);
                }
                RestClient.onNonSuccessResponse += OnNonSuccessResponse;
            });
        }

        private static void SetUsername(string username) => PlayerPrefs.SetString("username", username);

        private void OnDestroy()
        {
            edtUsername.onEndEdit.RemoveListener(SetUsername);
            RestClient.onNonSuccessResponse -= OnNonSuccessResponse;
        }

        private void Update()
        {
            // Select password txt with Tab
            if (edtUsername.isFocused && Input.GetKeyDown(KeyCode.Tab))
            {
                edtPassword.Select();
            }

            // Login with Enter
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (edtUsername.text != "" && edtPassword.text != "")
                {
                    processLogin();
                }
            }
        }

        private void OnNonSuccessResponse(string obj)
        {
            loadingObject.SetActive(false);
            notificationBanner.displayNotificationBanner(MessageTemplate.Error, "CONNECTION ERROR", obj, "OKAY", null);
        }

        public void ExitApplication()
        {
            notificationBanner.displayNotificationBanner(MessageTemplate.Confirmation, "CONFIRM CLOSE", "Are you sure you wish to close 3DPact?", "CANCEL", null, "CLOSE", QuitApplication);
        }

        public void resetPasswordOnLMS()
        {
            Application.OpenURL("https://simgenics-web-dev.azurewebsites.net/account/forgot-password"); //temp
        }

        public void processLogin()
        {
            loadingObject.SetActive(true);
            notificationSnackBar.CloseNotificationSnackBar();

            // Validate input fields
            if (validateUserEntry(edtUsername.text, edtPassword.text) == false)
            {
                notificationSnackBar.displayNotificationSnackbar(SnackbarTemplates.Error, "Invalid Username or Password");
                loadingObject.SetActive(false);
            }
            else
            {
                // Attempt to login
                var userModel = new AuthenticateModel()
                {
                    userNameOrEmailAddress = edtUsername.text,
                    password = edtPassword.text,
                    tenantName = ConnectionStrings.Tenant
                };

                AuthAdapter.instance.AuthWithTenantName(userModel, (authResult) =>
                {
                    if (authResult != null)
                    {
                        CheckPermissions(WorkflowRoles, null);
                    }
                });
            }
        }

        public static void CheckPermissions(List<string> expectedPermissions, Action permissionsSuccess = null)
        {
            var permissions = new GetRolesInput() { Permissions = expectedPermissions };
            RolesAdapter.instance.GetRoles(permissions, result =>
            {
                if (result != null)
                {
                    var roles = result.items;
                    var hasPermissions = result.items.Count > 0;
                    if (hasPermissions)
                    {
                        Debug.Log($"Authorized Role: {roles.First().name}");
                        permissionsSuccess?.Invoke();
                    }
                    else
                    {
                        Debug.LogWarning("User does not have sufficient permissions.");
                    }
                }
            });
        }

        public void GetCompanies()
        {
            CompaniesAdapter.instance.GetTreeOfFacilitiesForUser(companiesResult =>
            {
                if (companiesResult != null)
                {
                    loadingObject.SetActive(false);
                    uiCompanies.SetCompanies(companiesResult.items);
                }
            });
        }

        private static bool validateUserEntry(string enteredUsername, string enteredPassword)
            => !string.IsNullOrWhiteSpace(enteredUsername) && !string.IsNullOrWhiteSpace(enteredPassword);

        private void ClearInputFields()
        {
            edtUsername.text = "";
            edtPassword.text = "";
        }

        private void QuitApplication()
        {
            Application.Quit();
        }
    }
}