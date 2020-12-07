﻿using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorClient.ComponentsLibrary
{
    public static class LocalStorage
    {
        public static ValueTask<T> GetAsync<T>(IJSRuntime jsRuntime, string key)
            => jsRuntime.InvokeAsync<T>("blazorLocalStorage.get", key);

        public static ValueTask<object> SetAsync(IJSRuntime jsRuntime, string key, object value)
            => jsRuntime.InvokeAsync<object>("blazorLocalStorage.set", key, value);

        public static ValueTask<object> DeleteAsync(IJSRuntime jsRuntime, string key)
            => jsRuntime.InvokeAsync<object>("blazorLocalStorage.delete", key);
    }
}
