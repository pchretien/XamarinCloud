using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GoogleDrive.REST.Model;
using System.Net.Http.Headers;

namespace GoogleDrive.REST
{
    public class RestService
    {
        private HttpClient _client;

        private static RestService _me = new RestService();

        private string _host = "https://dev-www.docmagic.com";
        private string _servicePath = "webservices/borrowermobile/api/v2";
        private string authenticationPath = "webservices/authentication/api/v1";

        private ApiError _lastApiError;
        public ApiError LastApiError
        {
            get
            {
                return _lastApiError;
            }

            set
            {
                _lastApiError = value;
            }
        }

        public RestService()
        {
            _client = new HttpClient();
        }

        public RestService(string host, string servicePath)
        {
            _host = host;
            _servicePath = servicePath;

            _client = new HttpClient();
        }

        private string _token;
        public string Token
        {
            set
            {
                _token = value;
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            }
        }

        private static void CalculateTimeSpent(string method, Uri uri, DateTime inTime)
        {
#if DEBUG
            DateTime outTime = DateTime.Now;
            var millis = (outTime - inTime).TotalMilliseconds;
            System.Diagnostics.Debug.WriteLine("{0}: {1} [{2}ms]", method, uri.AbsoluteUri, millis);
#endif
        }

        public static RestService GetInstance()
        {
            if(_me == null)
                _me = new RestService();

            return _me;
        }
        
        public Uri BuildServiceUri(string path, string[] ids)
        {
            return BuildUri(_servicePath, path, ids);
        }

        public Uri BuildAuthUri(string path, string[] ids)
        {
            return BuildUri(authenticationPath, path, ids);
        }

        public Uri BuildUri(string rootPath, string path, string[] ids)
        {
            if (path == null || path.Length == 0)
                throw new InvalidOperationException("PATH can't be null or empty");

            path = path.TrimEnd('/').TrimStart('/');

            if (ids == null || ids.Length == 0)
                return new Uri(string.Format("{0}/{1}/{2}", _host, rootPath, path));

            string finalpath = "";
            string[] tokens = path.Split('#');
            for(int i=0; i<tokens.Length; i++)
            {
                if (i < ids.Length)
                    finalpath += tokens[i] + ids[i];
                else
                    finalpath += tokens[i];
            }

            return new Uri(string.Format("{0}/{1}/{2}", _host, rootPath, finalpath));
        }

        private void SetApiLastError(string message)
        {
            LastApiError = new ApiError()
            {
                StatusCode = 600,
                Message = message
            };

            System.Diagnostics.Debug.WriteLine("API ERROR: " + message);
        }

        private async Task<T> SetApiLastError<T>(HttpResponseMessage getResponse)
        {
            try
            {
                var content = await getResponse.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine("API ERROR: " + content);

                var error = JsonConvert.DeserializeObject<ApiErrorRoot>(content);

                if (error != null)
                {
                    LastApiError = error.Error;
                }
            }
            catch (Exception ex)
            {
                SetApiLastError(ex.Message);
            }

            return default(T);
        }

        private async Task<byte[]> GetImage(string path, string[] ids)
        {
            App.LogActivity();

            var uri = BuildServiceUri(path, ids);

            DateTime inTime = DateTime.Now;
            var getResponse = await _client.GetAsync(uri);
            CalculateTimeSpent("GET/IMAGE", uri, inTime);

            if (getResponse.IsSuccessStatusCode)
            {
                if (getResponse.Content.Headers.ContentType.MediaType.StartsWith("image/"))
                {
                    byte[] content = await getResponse.Content.ReadAsByteArrayAsync();
                    return content;
                }
            }
            else
            {
                return await SetApiLastError<byte[]>(getResponse);
            }

            return null;
        }
        
        public async Task<T> Get<T>(string path, string[] ids)
        {   
            var uri = BuildServiceUri(path, ids);
            var result = await Get<T>(uri);

            return result;
        }

        public async Task<T> Get<T>(Uri uri)
        {
            App.LogActivity();

            try
            {
                DateTime inTime = DateTime.Now;
                var getResponse = await _client.GetAsync(uri);
                CalculateTimeSpent("GET", uri, inTime);

                if (getResponse.IsSuccessStatusCode)
                {
                    var content = await getResponse.Content.ReadAsStringAsync();

                    // PATCH !!!
                    content = content.Replace("\"null\"", "null");

                    try
                    {
                        var objResponse = JsonConvert.DeserializeObject<T>(content);
                        return objResponse;
                    }
                    catch (Exception ex)
                    {
                        SetApiLastError(ex.Message);
                    }
                }
                else
                {
                    return await SetApiLastError<T>(getResponse);
                }

                return default(T);
            }
            catch (Exception ex)
            {
                SetApiLastError(ex.Message);
            }

            return default(T);
        }

        public async Task<bool> Delete(string path, string[] ids)
        {
            var uri = BuildServiceUri(path, ids);
            return await Delete(uri);
        }

        public async Task<bool> Delete(Uri uri)
        {
            App.LogActivity();

            try
            {
                DateTime inTime = DateTime.Now;
                var deleteResponse = await _client.DeleteAsync(uri);
                CalculateTimeSpent("DELETE", uri, inTime);
                
                if (deleteResponse.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return await SetApiLastError<bool>(deleteResponse);
                }
            }
            catch (Exception ex)
            {
                SetApiLastError(ex.Message);
            }

            return false;
        }

        public async Task<R> Post<T,R>(string path, string[] ids, T data)
        {
            var uri = BuildServiceUri(path, ids);
            return await Post<T,R>(uri, data);
        }

        public async Task<R> Post<T,R>(Uri uri, T data)
        {
            App.LogActivity();

            try
            {
                var json = JsonConvert.SerializeObject(data);
                var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

                DateTime inTime = DateTime.Now;
                var postResponse = await _client.PostAsync(uri, stringContent);
                CalculateTimeSpent("POST", uri, inTime);

                if (postResponse.IsSuccessStatusCode)
                {
                    var content = await postResponse.Content.ReadAsStringAsync();
                    R objResponse = JsonConvert.DeserializeObject<R>(content);

                    return objResponse;
                }
                else
                {
                    return await SetApiLastError<R>(postResponse);
                }
            }
            catch (Exception ex)
            {
                SetApiLastError(ex.Message);
            }

            return default(R);
        }

        public async Task<R> Put<T, R>(string path, string[] ids, T data)
        {
            var uri = BuildServiceUri(path, ids);
            return await Put<T, R>(uri, data);
        }

        public async Task<R> Put<T, R>(Uri uri, T data)
        {
            App.LogActivity();

            try
            {
                var json = JsonConvert.SerializeObject(data);
                var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

                DateTime inTime = DateTime.Now;
                var putResponse = await _client.PutAsync(uri, stringContent);
                CalculateTimeSpent("PUT", uri, inTime);

                if (putResponse.IsSuccessStatusCode)
                {
                    var content = await putResponse.Content.ReadAsStringAsync();
                    R objResponse = JsonConvert.DeserializeObject<R>(content);

                    return objResponse;
                }
                else
                {
                    return await SetApiLastError<R>(putResponse);
                }
            }
            catch (Exception ex)
            {
                SetApiLastError(ex.Message);
            }

            return default(R);
        }

        public async Task<AuthenticationResponse> Login(string username, string password)
        {
            App.LogActivity();

            try
            {
                var uri = BuildAuthUri("/authenticate", null);

                var data = new LoginCredentials()
                {
                    AuthenticationType = "Identity",
                    UserName = username,
                    Password = password
                };

                string json = JsonConvert.SerializeObject(data);

                DateTime inTime = DateTime.Now;
                var postResponse = await _client.PostAsync(uri, new StringContent(json, Encoding.UTF8, "application/json"));
                CalculateTimeSpent("POST/LOGIN", uri, inTime);


                if (postResponse.IsSuccessStatusCode)
                {
                    var content = await postResponse.Content.ReadAsStringAsync();
                    var objResponse = JsonConvert.DeserializeObject<AuthenticationResponse>(content);

                    return objResponse;
                }
                else
                {
                    return await SetApiLastError<AuthenticationResponse>(postResponse);
                }
            }
            catch (Exception ex)
            {
                SetApiLastError(ex.Message);
            }

            return null;
        }

        public async Task<List<Loans>> ListLoans()
        {
            return await Get<List<Loans>>("/loans", new string[0]);
        }

        public async Task<Loan> GetLoan(string loanId)
        {
            return await Get<Loan>("/loans/#", new string[] { loanId });
        }

        public async Task<byte[]> GetCompanyLogoImage(string loanId, string imageId)
        {
            return await GetImage("/loans/#/theme/#", new string[] { loanId, imageId });
        }

        public async Task<List<Events>> ListLoanEvents(string loanId)
        {
            return await Get<List<Events>>("/loans/#/events", new string[] { loanId });
        }

        public async Task<ModelEvent> PostLoanEvent(string loanId, ModelEvent eventRequest)
        {
            return await Post<ModelEvent, ModelEvent>("/loans/#/events", new string[] { loanId}, eventRequest);
        }

        public async Task<Collateral> GetCollateral(string loanId)
        {
            return await Get<Collateral>("/loans/#/collateral", new string[] { loanId });
        }

        public async Task<List<Images>> ListImages(string loanId)
        {
            return await Get<List<Images>>("/loans/#/collateral/images", new string[] { loanId });
        }

        public async Task<byte[]> GetCollateralImage(string loanId, string imageId)
        {
            return await GetImage("/loans/#/collateral/images/#", new string[] { loanId, imageId });
        }

        public async Task<List<ToDoItems>> ListToDoItems(string loanId)
        {
            return await Get<List<ToDoItems>>("/loans/#/todoitems", new string[] { loanId });
        }

        public async Task<ToDoItem> GetToDoItem(string loanId, string todoItemId)
        {
            return await Get<ToDoItem>("/loans/#/todoitems/#", new string[] { loanId, todoItemId });
        }

        public async Task<ToDoItem> PutToDoItem(string loanId, string todoItemId, ToDoItem toDoItem)
        {
            return await Put<ToDoItem, ToDoItem>("/loans/#/todoitems/#", new string[] { loanId, todoItemId }, toDoItem);
        }

        public async Task<List<ToDoItemComment>> ListComments(string loanId, string toDoItemId)
        {
            return await Get<List<ToDoItemComment>>("/loans/#/todoitems/#/comments", new string[] { loanId, toDoItemId });
        }

        public async Task<ToDoItemComment> PostComment(string loanId, string todoItemId, ToDoItemComment comment)
        {
            return await Post<ToDoItemComment, ToDoItemComment>("/loans/#/todoitems/#/comments", new string[] { loanId, todoItemId }, comment);
        }

        public async Task<ToDoItemComment> PutComment(string loanId, string todoItemId, string commentId, ToDoItemComment comment)
        {
            return await Put<ToDoItemComment, ToDoItemComment>("/loans/#/todoitems/#/comments/#", new string[] { loanId, todoItemId, commentId }, comment);
        }

        public async Task<List<Artifacts>> ListArtifacts(string loanId, string toDoItemId)
        {
            return await Get<List<Artifacts>>("/loans/#/todoitems/#/artifacts", new string[] { loanId, toDoItemId });
        }

        public async Task<Artifact> GetArtifact(string loanId, string toDoItemId, string artifactId)
        {
            return await Get<Artifact>("/loans/#/todoitems/#/artifacts/#", new string[] { loanId, toDoItemId, artifactId });
        }

        public async Task<Artifact> PostArtifact(string loanId, string toDoItemId, Artifact artifact)
        {
            return await Post<Artifact, Artifact>("/loans/#/todoitems/#/artifacts", new string[] { loanId, toDoItemId }, artifact);
        }

        public async Task<bool> DeleteArtifact(string loanId, string toDoItemId, string artifactId)
        {
            return await Delete("/loans/#/todoitems/#/artifacts/#", new string[] { loanId, toDoItemId, artifactId });
        }

        public async Task<List<ESignDocuments>> ListESignDocuments(string loanId, string toDoItemId)
        {
            return await Get<List<ESignDocuments>>("/loans/#/todoitems/#/esign", new string[] { loanId, toDoItemId });
        }

        public async Task<ESignDocument> GetESignDocument(string loanId, string toDoItemId, string esignDocumentId)
        {
            return await Get<ESignDocument>("/loans/#/todoitems/#/esign/#", new string[] { loanId, toDoItemId, esignDocumentId });
        }

        public async Task<byte[]> GetESignDocumentImage(string loanId, string toDoItemId, string esignDocumentId, int page)
        {
                return await GetImage("/loans/#/todoitems/#/esign/#/image?page=#", new string[] { loanId, toDoItemId, esignDocumentId, page.ToString() });
        }

        public async Task<List<ESignDocumentMark>> ListESignDocumentMarks(string loanId, string toDoItemId, string esignDocumentId)
        {
            return await Get< List<ESignDocumentMark>>("/loans/#/todoitems/#/esign/#/documentmarks", new string[] { loanId, toDoItemId, esignDocumentId });
        }

        public async Task<ESignDocumentMark> GetESignDocumentMark(string loanId, string toDoItemId, string esignDocumentId, string esignDocumentMarkId)
        {
            return await Get<ESignDocumentMark>("/loans/#/todoitems/#/esign/#/documentmarks/#", new string[] { loanId, toDoItemId, esignDocumentId, esignDocumentMarkId });
        }

        public async Task<List<Parties>> ListParties(string loanId)
        {
            return await Get<List<Parties>>("/loans/#/parties", new string[] { loanId });
        }

        public async Task<Party> GetParty(string loanId, string partyId)
        {
            return await Get<Party>("/loans/#/parties/#", new string[] { loanId, partyId });
        }

        public async Task<byte[]> GetContactImage(string loanId, string partyId, string representativeId, string representativeImageId)
        {
            return await GetImage("/loans/#/parties/#/partyrepresentatives/#/#", new string[] { loanId, partyId, representativeId, representativeImageId });
        }

        public async Task<List<LoanStage>> ListLoanStages(string loanId)
        {
            return await Get<List<LoanStage>>("/loans/#/loanstages", new string[] { loanId });
        }

        public async Task<Profile> GetProfile()
        {
            return await Get<Profile>("/profile", new string[0]);
        }

        public async Task<Profile> PutProfile(Profile profile)
        {
            return await Put<Profile, Profile>("/profile", new string[0], profile);
        }

        public async Task<Registration> Register(Registration registration)
        {
            return await Post<Registration, Registration>("/register", new string[0], registration);
        }

        public async Task<Registration> Setup(SetupRequest setup)
        {
            return await Put<SetupRequest, Registration>("/setup", new string[0], setup);
        }

        public async Task<string> RegisterDevice(Device device)
        {
            return await Post<Device, string>("/registerdevice", new string[0], device);
        }
    }
}
