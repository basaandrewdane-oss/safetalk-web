using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Shared
{
    public class ApiResponse<T>
    {
        public bool success { get; set; }
        public string message { get; set; }
        public T data { get; set; }
        public static ApiResponse<T> Ok(T data, string message = null)
        {
            return new ApiResponse<T>
            {
                success = true,
                message = message,
                data = data
            };
        }
        public static ApiResponse<T> Fail(string message)
        {
            return new ApiResponse<T>
            {
                success = false,
                message = message,
                data = default
            };
        }
        public static ApiResponse<T> Fail(string message, T data)
        {
            return new ApiResponse<T>
            {
                success = false,
                message = message,
                data = data
            };
        }
    }
}