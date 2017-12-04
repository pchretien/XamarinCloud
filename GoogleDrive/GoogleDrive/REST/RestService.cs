
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

        private static RestService _me;
        private static RestService _meGoogle;
        private static RestService _meBox;

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

        public static RestService GetGooGle()
        {
            if (_meGoogle == null)
                _meGoogle = new RestService("https://www.googleapis.com", "drive/v3/files");

            return _meGoogle;
        }

        public static RestService GetBox()
        {
            if (_meGoogle == null)
                _meGoogle = new RestService("https://api.box.com", "2.0");

            return _meGoogle;
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
            var uri = BuildServiceUri(path, ids);

            DateTime inTime = DateTime.Now;
            var getResponse = await _client.GetAsync(uri);
            CalculateTimeSpent("GET/IMAGE", uri, inTime);

            if (getResponse.IsSuccessStatusCode)
            {
                if (getResponse.Content.Headers.ContentType.MediaType.Contains("png") ||
                    getResponse.Content.Headers.ContentType.MediaType.Contains("jpeg") ||
                    getResponse.Content.Headers.ContentType.MediaType.Contains("pdf"))
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

        public async Task<GoogleFileList> ListGoogleFiles(string folder)
        {
            return await Get<GoogleFileList>("/?q='#' in parents and (mimeType contains 'folder' or mimeType contains 'png' or mimeType contains 'jpeg' or mimeType contains 'pdf' )", new string[1] { folder });
        }

        public async Task<byte[]> GetGoogleFile(string fileId)
        {
            return await GetImage("/#?alt=media", new string[1] { fileId });
        }

        public async Task<BoxFileList> ListBoxFiles(string folderId)
        {
            return await Get<BoxFileList>("/folders/#/items?limit=1000&offset=0", new string[1] { folderId });
        }

        public async Task<byte[]> GetBoxFile(string fileId)
        {
            return await GetImage("/files/#/content", new string[1] { fileId });
        }

        //public async Task<Loan> GetLoan(string loanId)
        //{
        //    return await Get<Loan>("/loans/#", new string[] { loanId });
        //}

        //public async Task<ModelEvent> PostLoanEvent(string loanId, ModelEvent eventRequest)
        //{
        //    return await Post<ModelEvent, ModelEvent>("/loans/#/events", new string[] { loanId}, eventRequest);
        //}

        //public async Task<ToDoItem> PutToDoItem(string loanId, string todoItemId, ToDoItem toDoItem)
        //{
        //    return await Put<ToDoItem, ToDoItem>("/loans/#/todoitems/#", new string[] { loanId, todoItemId }, toDoItem);
        //}

        //public async Task<bool> DeleteArtifact(string loanId, string toDoItemId, string artifactId)
        //{
        //    return await Delete("/loans/#/todoitems/#/artifacts/#", new string[] { loanId, toDoItemId, artifactId });
        //}


    }
}
