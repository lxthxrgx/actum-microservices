using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLibraries.Factory
{
    public interface IResponse
   {
        bool Success { get; set; }
        string Message { get; set; }
   }

    public interface IOk<T> : IResponse
    {
        List<T>? Data { get; set; }
        int Count { get; set; }
    }

    public interface IError : IResponse {}

    public class OkResponse<T> : IOk<T>
    {
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
        public List<T>? Data { get; set; }
        public int Count { get; set; }
    }

    public class ErrorResponse : IError
    {
        public bool Success { get; set; } = false;
        public string? Message { get; set; }
    }

    public static class ResponseFactory
    {

        public static IOk<T> Ok<T>(List<T> data, string? message = null)
        {
            return new OkResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Count = data?.Count ?? 0
            };
        }

        public static IOk<T> Ok<T>(T item, string? message = null)
        {
            return new OkResponse<T>
            {
                Success = true,
                Message = message,
                Data = new List<T> { item },
                Count = 1
            };
        }

        public static IOk<T> Ok<T>(string? message = null)
        {
            return new OkResponse<T>
            {
                Success = true,
                Message = message,
                Data = null,
                Count = 0
            };
        }

        public static IError Error(string message)
        {
            return new ErrorResponse
            {
                Success = false,
                Message = message,
            };
        }
    }
}
