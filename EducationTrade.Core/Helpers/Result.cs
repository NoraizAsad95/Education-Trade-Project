using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EducationTrade.Core.Helpers
{
    public class Result                                 // For Operations With No Data
    {
        public bool IsSuccess { get; set; }
        public string? Error { get; set; }

        public static Result Success()
        {
            return new Result { IsSuccess = true };
        }

        public static Result Failure(string error)
        {
            return new Result { IsSuccess = false, Error = error };
        }
    }

    public class Result<T>                            // For Operations With Data
    {
        public bool IsSuccess { get; set;}
        public string? Error { get; set; }
        public T? Data { get; set;}

        public static Result<T> Success(T data)
        {
            return new Result<T> { IsSuccess = true, Data = data };
        }

        public static Result<T> Failure(string error)
        {
            return new Result<T> { IsSuccess = false, Error = error };
        }
    }

}
