using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GoogleDrive.REST.Model
{
    [DataContract]
    public class ApiError
    {
        [DataMember(Name = "statusCode", EmitDefaultValue = false)]
        public int StatusCode { get; set; }

        [DataMember(Name = "errorCode", EmitDefaultValue = false)]
        public string ErrorCode { get; set; }

        [DataMember(Name = "message", EmitDefaultValue = false)]
        public string Message { get; set; }

        [DataMember(Name = "requestId", EmitDefaultValue = false)]
        public string RequestId { get; set; }
    }

    [DataContract]
    public class ApiErrorRoot
    {
        [DataMember(Name = "error", EmitDefaultValue = false)]
        public ApiError Error { get; set; }
    }
}
